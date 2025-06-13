using Microsoft.Scripting.Utils;
using MissionPlanner.ArduPilot;
using MissionPlanner.Swarm.Sequence;
using MissionPlanner.Utilities;
using netDxf.Entities;
using Newtonsoft.Json;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Interop;
using static Community.CsharpSqlite.Sqlite3;
using static IronPython.Modules._ast;
using static MAVLink;
using static MissionPlanner.GCSViews.Udp;
using DateTime = System.DateTime;


namespace MissionPlanner.GCSViews
{
    public partial class Udp : Form
    {
        public Udp()
        {
            InitializeComponent();
            countdown.Elapsed += sendMassage;

          
        }

       
        Thread receive;

        System.Timers.Timer countdown = new System.Timers.Timer { Interval = 1000, AutoReset = true };
        private HttpListener httpListener;
        private Thread httpThread;


        private void myButton2_Click(object sender, EventArgs e)
        {
            if (myButton2.Text == "连接")
            {

                countdown.Start();
                // 启动 HTTP 服务
                StartHttpServer();

                myButton2.Text = "断开连接";
            }
            else
            {
                // 停止 HTTP 服务
                StopHttpServer();
                myButton2.Text = "连接";
                countdown.Close();
            }
        }






        private void StartHttpServer()
        {
            if (httpListener != null && httpListener.IsListening)
                return;

            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:5031/uav_control/api/");
            httpListener.Start();

            httpThread = new Thread(ListenLoop)
            {
                IsBackground = true
            };
            httpThread.Start();
        }
        private void StopHttpServer()
        {
            try
            {
                // 停止 HTTP Listener
                if (httpListener != null && httpListener.IsListening)
                {
                    httpListener.Stop();
                    httpListener.Close();
                    httpListener = null;
                }

                // 等待线程结束
                if (httpThread != null && httpThread.IsAlive)
                {
                    httpThread.Join(1000); // 最多等待1秒
                }

                httpThread = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("停止HTTP服务出错：" + ex.Message);
            }
        }

        private async void ListenLoop()
        {
            while (httpListener != null && httpListener.IsListening)
            {
                try
                {
                    var context = httpListener.GetContext();
                    HandleRequest(context); // 处理请求的方法
                }
                catch (Exception ex)
                {
                    Console.WriteLine("HTTP监听异常：" + ex.Message);
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            string url = context.Request.Url.AbsolutePath;
            Console.WriteLine($"收到请求: {url}");

            if (url.StartsWith("/uav_control/api/"))
            {
                string[] parts = url.Substring("/uav_control/api/".Length).Split('/');
                if (parts.Length >= 1 && parts[0] == "UploadWayPoint")
                {
             

                    if (context.Request.HttpMethod == "POST")
                    {
                        using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                        {
                            string requestBody = reader.ReadToEnd();
                            Console.WriteLine("请求体内容：" + requestBody);

                            try
                            {
                                var waypoints = JsonConvert.DeserializeObject<List<Waypoint>>(requestBody);

                                // 处理上传的航点数据
                                ProcessWaypoints("1", "1", waypoints);

                                SendJsonResponse(context, new
                                {
                                    message = $"成功接收到 {waypoints.Count} 个航点任务。",
                                    count = waypoints.Count
                                }, HttpStatusCode.OK);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("解析JSON失败：" + ex.Message);
                                SendJsonResponse(context, new
                                {
                                    error = "无效的JSON格式",
                                    detail = ex.Message
                                }, HttpStatusCode.BadRequest);
                            }
                        }
                    }
                    else
                    {
                        SendJsonResponse(context, new { error = "不支持的命令或方法。" }, HttpStatusCode.MethodNotAllowed);
                    }
                }
                else
                {
                    SendJsonResponse(context, new { error = "未找到指定接口。" }, HttpStatusCode.NotFound);
                }
            }
            else
            {
                SendJsonResponse(context, new { error = "路径错误。" }, HttpStatusCode.NotFound);
            }
        }

        public class Waypoint
        {
            public string id { get; set; }
            public string longitude { get; set; }
            public string latitude { get; set; }
            public string Height { get; set; }
        }
        private void ProcessWaypoints(string uavId, string platform, List<Waypoint> waypoints)
        {
            if (MainV2.comPort == null || !MainV2.comPort.BaseStream.IsOpen)
            {
                Console.WriteLine("飞控未连接");
                return;
            }
            List<Locationwp> commands = new List<Locationwp>();

            Locationwp temp1 = new Locationwp();
            // 清除当前任务
            MainV2.comPort.MAV.wps.Clear();

            foreach (var wp in waypoints)
            {
                double lat, lon, alt;

                if (!double.TryParse(wp.latitude, out lat) ||
                    !double.TryParse(wp.longitude, out lon) ||
                    !double.TryParse(wp.Height, out alt))
                {
                    Console.WriteLine($"跳过无效航点：{wp.id}");
                    continue;
                }

                // 创建任务项（MAV_CMD.WAYPOINT）
                var item = new MAVLink.mavlink_mission_item_int_t
                {
                    x = (int)(lon * 1e7),
                    y = (int)(lat * 1e7),
                    z = (float)alt,
                    seq = (ushort)MainV2.comPort.MAV.wps.Count,
                    command = (ushort)MAVLink.MAV_CMD.WAYPOINT,
                    frame = (byte)MAVLink.MAV_FRAME.MISSION,
                    param1 = 0,  // Hold time
                    param2 = 0,  // Accept radius
                    param3 = 0,  // Pass radius
                    param4 = 0,  // Yaw angle
                    autocontinue = 1
                };

                //MainV2.comPort.MAV.wps.Add(item);
            }

            Console.WriteLine($"已加载 {waypoints.Count} 个航点到任务列表");

            // 如果你想自动上传任务到飞控：
            mav_mission.upload(MainV2.comPort, MainV2.comPort.MAV.sysid,
                                        MainV2.comPort.MAV.compid, 0,
                                         commands,
                                         (percent, status) =>
                                         {
                                         }).ConfigureAwait(false);
        }

        private void SendJsonResponse(HttpListenerContext context, object obj, HttpStatusCode code = HttpStatusCode.OK)
        {
            string jsonResponse = JsonConvert.SerializeObject(obj);
            byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

            context.Response.StatusCode = (int)code;
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = buffer.Length;

            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }





        // 定义无人机状态类
        public class DroneStatus
        {
            public string UAVId { get; set; }
            public string Longitude { get; set; }
            public string Latitude { get; set; }
            public string Altitude { get; set; }
            public string RelativeHeight { get; set; }
            public string AirSpeed { get; set; }
            public string Groundspeed { get; set; }
            public string Pitch { get; set; }
            public string Roll { get; set; }
            public string Yaw { get; set; }
            public string Batterylevel { get; set; }
            public string Linkqualitygcs { get; set; }
            public string Satcount { get; set; }
            public string TimeInAir { get; set; }
            public string DateTime { get; set; }
        }


        private readonly HttpClient _httpClient = new HttpClient();


        /// <summary>
        /// 发送无人机基本信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void sendMassage(object sender, ElapsedEventArgs e)
        {
           
                // 创建要发送的数据
                var droneData = new DroneStatus
                {
                    UAVId = "1",
                    Longitude = "40.1234567",
                    Latitude = "85.1234567",
                    Altitude = "150",
                    RelativeHeight = "10",
                    AirSpeed = "10",
                    Groundspeed = "20",
                    Pitch = "30",
                    Roll = "15",
                    Yaw = "20",
                    Batterylevel = "15.23",
                    Linkqualitygcs = "100",
                    Satcount = "12",
                    TimeInAir="100",
                    DateTime ="2025/6/13 09:07:56"
                };

                // 将对象序列化为 JSON 字符串
                string json = JsonConvert.SerializeObject(droneData);

                // 构建 HTTP 请求内容
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 创建 HttpClient 并发送 POST 请求
                
                    try
                    {
                        var response = await _httpClient.PostAsync("http://localhost:5030/uav_status/api/data", content);

                        Console.WriteLine($"HTTP Status Code: {response.StatusCode}");

                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            Console.WriteLine("Response Body: " + responseBody);
                        }
                        else
                        {
                            Console.WriteLine("Error: " + await response.Content.ReadAsStringAsync());
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("发送失败：" + ex.Message);
                    }
                
            }
        



    }
}