using Microsoft.Scripting.Utils;
using MissionPlanner.ArduPilot;
using MissionPlanner.Swarm.Sequence;
using MissionPlanner.Utilities;
using netDxf.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

     

        private void myButton2_Click(object sender, EventArgs e)
        {
            if (myButton2.Text == "连接")
            {

                countdown.Start();


                myButton2.Text = "断开连接";
            }
            else
            {
              
                myButton2.Text = "连接";
                countdown.Close();
            }
        }







        private async void receivemessage()
        {
            while (true)
            {
                try
                {
                  

                }
                catch
                {

                }
            }
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
                    Satcount = "12"
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