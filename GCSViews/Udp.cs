using Microsoft.Scripting.Utils;
using MissionPlanner.ArduPilot;
using MissionPlanner.Swarm.Sequence;
using MissionPlanner.Utilities;
using netDxf.Entities;
using Newtonsoft.Json;
using SharpDX.Mathematics.Interop;
using SharpKml.Dom;
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
            httpListener.Prefixes.Add("http://192.168.1.20:5031/uav_control/api/");
            //httpListener.Prefixes.Add("http://localhost:5031/uav_control/api/");
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


                if (parts.Length >= 1 && parts[0] == "FHYUploadWayPoint")
                {
                    string uavid = parts[1];

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
                                //ProcessWaypoints(uavid, "1", waypoints);
                                foreach (var port in MainV2.Comports)
                                {
                                    foreach (var mav in port.MAVlist)
                                    {
                                        if (mav.sysid + "" == uavid)
                                        {


                                            List<Locationwp> commands = new List<Locationwp>();

                                            Locationwp temp1 = new Locationwp();
                                            // 清除当前任务
                                            mav.wps.Clear();

                                            Locationwp home = new Locationwp();
                                            try
                                            {
                                                home.frame = (byte)MAVLink.MAV_FRAME.GLOBAL;
                                                home.id = (ushort)MAVLink.MAV_CMD.WAYPOINT;
                                                home.lat = (mav.cs.PlannedHomeLocation.Lat);
                                                home.lng = (mav.cs.PlannedHomeLocation.Lng);
                                                home.alt = ((float)mav.cs.PlannedHomeLocation.Alt); // use saved home
                                            }
                                            catch
                                            {
                                                throw new Exception("Your home location is invalid");
                                            }
                                            commands.Insert(0, home);

                                                
                                                Locationwp temp = new Locationwp();

                                                temp.id = (ushort)84;

                                                temp.p1 = 0;

                                                temp.alt = float.Parse(waypoints[0].Height);

                                                temp.lat = double.Parse(waypoints[0].latitude);

                                                temp.lng = double.Parse(waypoints[0].longitude);

                                                temp.p2 = 0;

                                                temp.p3 = 0;

                                                temp.p4 = 0;

                                                temp.Tag = "0";

                                                temp.frame = 3;
                                                //MainV2.comPort.MAV.wps.Add(item);
                                                commands.Add(temp);

                                                Locationwp temp2 = new Locationwp();

                                                temp2.id = (ushort)17;

                                                temp2.p1 = 0;

                                                temp2.alt = float.Parse(waypoints[1].Height);

                                                temp2.lat = double.Parse(waypoints[1].latitude);

                                                temp2.lng = double.Parse(waypoints[1].longitude);

                                                temp2.p2 = 0;

                                                temp2.p3 = 0;

                                                temp2.p4 = 0;

                                                temp2.Tag = "0";

                                                temp2.frame = 3;
                                                //MainV2.comPort.MAV.wps.Add(item);
                                                commands.Add(temp2);


                                                Console.WriteLine($"已加载 {waypoints.Count} 个航点到任务列表");

                                                // 如果你想自动上传任务到飞控：
                                                mav_mission.upload(port, mav.sysid,
                                                                            mav.compid, 0,
                                                                             commands,
                                                                             (percent, status) =>
                                                                             {
                                                                             }).ConfigureAwait(false);

                                            Thread.Sleep(1000);
                                            port.setMode(mav.sysid, mav.compid, "Auto");
                                            Thread.Sleep(500);
                                            port.doARM(mav.sysid, mav.compid, true, true);

                                        }

                                            

                                        
                                    }
                                }

                                SendJsonResponse(context, new
                                {
                                    status = "Success",
                                    message = $"垂起成功接收到 {waypoints.Count} 个航点任务。",
                                    count = waypoints.Count
                                }, HttpStatusCode.OK);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("解析JSON失败：" + ex.Message);
                                SendJsonResponse(context, new
                                {
                                    status = "Failure",
                                    error = "垂起无效的JSON格式",
                                    detail = ex.Message
                                }, HttpStatusCode.BadRequest);
                            }
                        }
                    }
                    else
                    {
                        SendJsonResponse(context, new { error = "不支持的命令或方法。" }, HttpStatusCode.MethodNotAllowed);
                    }
                }else if (parts.Length >= 1 && parts[0] == "UploadWayPoint")
                {
                    string uavid = parts[1];

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
                                ProcessWaypoints(uavid, "1", waypoints);

                                SendJsonResponse(context, new
                                {
                                    status = "Success",
                                    message = $"成功接收到 {waypoints.Count} 个航点任务。",
                                    count = waypoints.Count
                                }, HttpStatusCode.OK);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("解析JSON失败：" + ex.Message);
                                SendJsonResponse(context, new
                                {
                                    status = "Failure",
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
                else if (parts.Length >= 1 && parts[0] == "set_mode")
                {
                    try
                    {
                        string uavid = parts[1];
                        string mode = parts[2];
                        foreach (var port in MainV2.Comports)
                        {
                            foreach (var mav in port.MAVlist)
                            {
                                if (mav.sysid + "" == uavid)
                                {
                                    port.setMode(mav.sysid, mav.compid, mode);
                                }
                            }
                        }

                        Thread.Sleep(1000);
                        foreach (var port in MainV2.Comports)
                        {
                            foreach (var mav in port.MAVlist)
                            {
                                if (mav.sysid + "" == uavid)
                                {
                                    if (mav.cs.mode == mode)
                                    {

                                        SendJsonResponse(context, new
                                        {
                                            status = "Success",
                                            message = "模式切换成功。",
                                            detail = "成功"
                                        });
                                    }
                                    else
                                    {
                                        SendJsonResponse(context, new
                                        {
                                            status = "Failure",
                                            message = "模式切换失败。",
                                            detail = "失败"
                                        });
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                }
                if (parts.Length >= 1 && (parts[1] == "UnArm"))
                {

                    string uavid = parts[0];
                    if (context.Request.HttpMethod == "POST")
                    {
                        using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                        {
                            string requestBody = reader.ReadToEnd();
                            Console.WriteLine("请求体内容：" + requestBody);
                            int count = 0;
                            try
                            {

                                foreach (var port in MainV2.Comports)
                                {
                                    foreach (var mav in port.MAVlist)
                                    {
                                        if (mav.sysid + "" == uavid)
                                        {
                                            port.doARM(mav.sysid, mav.compid, false);
                                        }
                                    }
                                }
                                Thread.Sleep(1000);
                                foreach (var port in MainV2.Comports)
                                {
                                    foreach (var mav in port.MAVlist)
                                    {
                                        if (mav.sysid + "" == uavid)
                                        {
                                            if (!mav.cs.armed) {

                                                SendJsonResponse(context, new
                                                {
                                                    status = "Success",
                                                    message = "上锁成功。",
                                                    detail = "成功"
                                                }, HttpStatusCode.OK);
                                            }
                                            else
                                            {
                                                SendJsonResponse(context, new
                                                {
                                                    status = "Failure",
                                                    message = "上锁失败。",
                                                    detail = "失败"
                                                }, HttpStatusCode.OK);
                                            }
                                        }
                                    }
                                }


                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("解析JSON失败：" + ex.Message);
                                SendJsonResponse(context, new
                                {
                                    status = "Failure",
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
                if (parts.Length >= 1 && (parts[1] == "Arm"))
                {

                    string uavid = parts[0];
                    if (context.Request.HttpMethod == "POST")
                    {
                        using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                        {
                            string requestBody = reader.ReadToEnd();
                            Console.WriteLine("请求体内容：" + requestBody);
                          

                            try
                            {

                                foreach (var port in MainV2.Comports)
                                {
                                    foreach (var mav in port.MAVlist)
                                    {
                                        if (mav.sysid + "" == uavid)
                                        {
                                            port.doARM(mav.sysid, mav.compid, true, true);
                                        }
                                    }
                                }
                                Thread.Sleep(1000);
                                foreach (var port in MainV2.Comports)
                                {
                                    foreach (var mav in port.MAVlist)
                                    {
                                        if (mav.sysid + "" == uavid)
                                        {
                                            if (mav.cs.armed)
                                            {
                                                SendJsonResponse(context, new
                                                {
                                                    status = "Success",
                                                    message = "解锁锁成功。",
                                                    detail = "成功"
                                                }, HttpStatusCode.OK);
                                            }
                                            else
                                            {
                                                SendJsonResponse(context, new
                                                {
                                                    status = "Failure",
                                                    message = "解锁失败。",
                                                    detail = "失败"
                                                }, HttpStatusCode.OK);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("解析JSON失败：" + ex.Message);
                                SendJsonResponse(context, new
                                {
                                    status = "Failure",
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
                if (parts.Length >= 1 && parts[1] == "TakeOff")
                {

                    string uavid = parts[0];
                    if (context.Request.HttpMethod == "POST")
                    {
                        using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                        {
                            string requestBody = reader.ReadToEnd();
                            Console.WriteLine("请求体内容：" + requestBody);

                            try
                            {
                                var command_params = JsonConvert.DeserializeObject<command_type_obj>(requestBody);
                                foreach (var port in MainV2.Comports)
                                {
                                    foreach (var mav in port.MAVlist)
                                    {
                                        if (mav.sysid + "" == uavid)
                                        {
                                            port.setMode(mav.sysid, mav.compid, "GUIDED");

                                            port.doCommand(mav.sysid, mav.compid, MAVLink.MAV_CMD.TAKEOFF, 0, 0, 0, 0, 0, 0, int.Parse(command_params.takeOff));
                                            SendJsonResponse(context, new
                                            {
                                                status = "Success",
                                                message = $"指点飞行。",
                                                detail = "成功"
                                            }, HttpStatusCode.OK);
                                        }
                                    }
                                }


                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("解析JSON失败：" + ex.Message);
                                SendJsonResponse(context, new
                                {
                                    status = "Failure",
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
                if (parts.Length >= 1 && parts[1] == "PointFlight")
                {

                    string uavid = parts[0];
                    if (context.Request.HttpMethod == "POST")
                    {
                        using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                        {
                            string requestBody = reader.ReadToEnd();
                            Console.WriteLine("请求体内容：" + requestBody);

                            try
                            {
                                foreach (var port in MainV2.Comports)
                                {
                                    foreach (var mav in port.MAVlist)
                                    {
                                        if (mav.sysid + "" == uavid)
                                        {
                                            var command_params = JsonConvert.DeserializeObject<command_type_obj>(requestBody);

                                            Locationwp gotohere = new Locationwp();

                                            gotohere.id = (ushort)MAVLink.MAV_CMD.WAYPOINT;
                                            gotohere.alt = float.Parse(command_params.height); // back to m
                                            gotohere.lat = double.Parse(command_params.latitude);
                                            gotohere.lng = double.Parse(command_params.longitude);

                                            port.setGuidedModeWP(mav.sysid, mav.compid, gotohere, true);



                                            SendJsonResponse(context, new
                                            {
                                                status = "Success",
                                                message = $"起飞 {command_params.takeOff} 高度。",
                                                detail = "成功"
                                            }, HttpStatusCode.OK);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("解析JSON失败：" + ex.Message);
                                SendJsonResponse(context, new
                                {
                                    status = "Failure",
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
            }
            else
            {
                SendJsonResponse(context, new { error = "路径错误。" }, HttpStatusCode.NotFound);
            }
        }
        public class command_type_obj
        {
            public string takeOff { get; set; }
            public string longitude { get; set; }
            public string latitude { get; set; }
            public string height { get; set; }
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

            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav.sysid + "" == uavId)
                    {


                        List<Locationwp> commands = new List<Locationwp>();

                        Locationwp temp1 = new Locationwp();
                        // 清除当前任务
                        mav.wps.Clear();

                        Locationwp home = new Locationwp();
                        try
                        {
                            home.frame = (byte)MAVLink.MAV_FRAME.GLOBAL;
                            home.id = (ushort)MAVLink.MAV_CMD.WAYPOINT;
                            home.lat = (mav.cs.PlannedHomeLocation.Lat);
                            home.lng = (mav.cs.PlannedHomeLocation.Lng);
                            home.alt = ((float)mav.cs.PlannedHomeLocation.Alt); // use saved home
                        }
                        catch
                        {
                            throw new Exception("Your home location is invalid");
                        }
                        commands.Insert(0, home);

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


                            Locationwp temp = new Locationwp();

                            temp.id = (ushort)16;

                            temp.p1 = 0;

                            temp.alt = (float)alt;

                            temp.lat = (double)lat;

                            temp.lng = (double)lon;

                            temp.p2 = 0;

                            temp.p3 = 0;

                            temp.p4 = 0;

                            temp.Tag = "0";

                            temp.frame = 3;
                            //MainV2.comPort.MAV.wps.Add(item);
                            commands.Add(temp);
                        }

                        Console.WriteLine($"已加载 {waypoints.Count} 个航点到任务列表");

                        // 如果你想自动上传任务到飞控：
                        mav_mission.upload(port, mav.sysid,
                                                    mav.compid, 0,
                                                     commands,
                                                     (percent, status) =>
                                                     {
                                                     }).ConfigureAwait(false);

                    }
                }
            }
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
            public string uavId { get; set; }
            public string longitude { get; set; }
            public string latitude { get; set; }
            public string altitude { get; set; }
            public string relativeHeight { get; set; }
            public string airSpeed { get; set; }
            public string groundSpeed { get; set; }
            public string pitch { get; set; }
            public string roll { get; set; }
            public string yaw { get; set; }
            public string batteryLevel { get; set; }
            public string linkQualityGcs { get; set; }
            public string satCount { get; set; }
            public string timeInAir { get; set; }
            public string dateTime { get; set; }
        }


        private readonly HttpClient _httpClient = new HttpClient();


        /// <summary>
        /// 发送无人机基本信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void sendMassage(object sender, ElapsedEventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    // 创建要发送的数据
                    var droneData = new DroneStatus
                    {
                        uavId = mav.sysid + "",
                        longitude = mav.cs.lng + "",
                        latitude = mav.cs.lat + "",
                        altitude = mav.cs.altasl + "",
                        relativeHeight = mav.cs.alt + "",
                        airSpeed = mav.cs.airspeed + "",
                        groundSpeed = mav.cs.groundspeed + "",
                        pitch = mav.cs.pitch + "",
                        roll = mav.cs.roll + "",
                        yaw = mav.cs.yaw + "",
                        batteryLevel = mav.cs.battery_voltage + "",
                        linkQualityGcs = mav.cs.linkqualitygcs + "",
                        satCount = mav.cs.satcount + "",
                        timeInAir = mav.cs.timeInAir + "",
                        dateTime = DateTime.Now.ToString()
                    };

                    // 将对象序列化为 JSON 字符串
                    string json = JsonConvert.SerializeObject(droneData);

                    // 构建 HTTP 请求内容
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // 创建 HttpClient 并发送 POST 请求

                    try
                    {
                        var response = await _httpClient.PostAsync("http://192.168.1.108:9085/uav_status/api/data", content);
                        //var response = await _httpClient.PostAsync("http://localhost:9085/uav_status/api/data", content);

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



    }
}