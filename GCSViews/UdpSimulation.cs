using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using Xamarin.Essentials;
using System.Timers;
using MissionPlanner.Utilities;


namespace MissionPlanner.GCSViews
{
    public class UdpSimulation
    {
        const string UDP_IP = "192.168.228.219";  // 目标IP
        //const string UDP_IP = "127.0.0.1";  // 目标IP
        const int UDP_PORT = 15006;           // 目标端口
        //const int UDP_PORT = 24583;           // 目标端口

        IPEndPoint endPoint;
        IPEndPoint endPointudp;
        UdpClient udpClient;
        public static Boolean is_true = false;
        private Thread workerThread;
        private CancellationTokenSource cancellationTokenSource;
        //private bool is_true = false;

        public UdpSimulation(){
            one.Elapsed += receivemessage;
        }
        public void UDPlink(Boolean isLink)
        {
            
            //Console.WriteLine("这是一个日志信息");
            if (isLink)
            {
                Settings.Instance.IsSimulation = "true";
                is_true = true;
                endPoint = new IPEndPoint(IPAddress.Parse(UDP_IP), UDP_PORT);
                udpClient = new UdpClient(16018);
               
                // 启动发送数据的工作线程
                //StartWorker();
                ThreadPool.QueueUserWorkItem(sendmessage);
                one.Start();
            }
            else
            {
                Settings.Instance.IsSimulation = "false";
                is_true = false;
                // 关闭UDP客户端并停止线程
                if (udpClient != null)
                {
                    udpClient.Close();
                }

                one.Stop();
            }

        }
        System.Timers.Timer one = new System.Timers.Timer { Interval = 1000, AutoReset = true };
        private void sensorsend()
        { 
            
        
        }
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PdxpPacket
        {
            public byte VER;              // 协议版本号 (1字节)
            public Int16 MID;              // 任务代号 (1字节)
            public Int32 SID;            // 发送方地址 (2字节)
            public Int32 DID;            // 接收方地址 (2字节)          
            public Int32 No;               // 包序号 (4字节)         
            public Int32 DATE;             //2000年1月1日累计时间戳0.1ms          
            public UInt32 L;              // 数据域长度 (2字节)
            public Int16 UAVId;           // 无人机编号 (2字节)
            public Int32 Longitude;         // 经度 (4字节)
            public Int32 Latitude;          // 纬度 (4字节)
            public Int16 RelativeHeight;  // 相对高度 (2字节)
            public Int16 Altitude;        // 海拔高度 (2字节)
            public Int32 Roll;           // 横滚角 (4字节)
            public Int32 Pitch;           // 横滚角 (4字节)
            public Int32 Yaw;           // 横滚角 (4字节)
            public Int16 EastVelocity;    // 东向速度 (2字节)
            public Int16 NorthVelocity;   // 北向速度 (2字节)
            public Int16 VerticalVelocity;// 垂向速度 (2字节)           

        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PdxpPacket2
        {
            public byte VER;              // 协议版本号 (1字节)
            public Int16 MID;              // 任务代号 (1字节)
            public Int32 SID;            // 发送方地址 (2字节)
            public Int32 DID;            // 接收方地址 (2字节)          
            public Int32 No;               // 包序号 (4字节)         
            public Int32 DATE;             //2000年1月1日累计时间戳0.1ms          
            public UInt16 L;              // 数据域长度 (2字节)
            public Int16 UAVId;           // 无人机编号 (2字节)           
        }
       
        private void sendmessage(object nothing)
        {
            int sequence = 0;
            while (is_true)
            {
                // 执行任务，可以替换为实际的代码逻辑
                //Console.WriteLine("线程正在执行...");
                //Thread.Sleep(1000); // 模拟工作1秒钟

                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        sequence++;
                        //DateTime gpsEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                       

                        // 设置基准时间：2000年1月1日 UTC
                        DateTime baseTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                        // 获取当前 UTC 时间
                        DateTime nowUtc = DateTime.UtcNow;

                        // 计算时间差（以 ticks 为单位，1 tick = 100 ns）
                        long totalTicks = (nowUtc - baseTime).Ticks;

                        // 1 毫秒 = 10,000 ticks → 0.1 毫秒 = 1,000 ticks
                        // 所以我们可以用 ticks 直接除以 1000 来得到 0.1ms 的整数值
                        ulong timestampIn0_1ms = (ulong)(totalTicks / 1000);
                        //TimeSpan elapsed = mav.cs.gpstime.ToUniversalTime() - gpsEpoch;
                        // 获取总天数（整数天）
                        //int daysSince2000 = (int)difference.TotalDays;

                        //Int32 timestamp_0_1ms = (Int32)(DateTime.UtcNow.TimeOfDay.TotalMilliseconds * 10);
                        //Int32 ticks = (Int32)(elapsed.TotalMilliseconds * 10); // 转换为 0.1 毫秒单位
                        //Int32 gpstimestamp_0_1ms = (Int32)(mav.cs.gpstime.TimeOfDay.TotalSeconds * 10000);
                        //mav.cs.gpstime

                        // 初始化PDXP数据包并填充默认值
                        PdxpPacket pdxpPacket = new PdxpPacket
                        {
                            VER = 0x30,                           // 协议版本
                            MID = 17902,                        // 任务代号
                            SID = BitConverter.ToInt32(new byte[4] { 0x4F, 0x10, 0x01, 0x01 }, 0),                      // 发送方地址
                            DID = BitConverter.ToInt32(new byte[4] { 0x01, 0x01, 0x01, 0x4F }, 0),                      // 接收方地址                         
                            No = (Int32)sequence,                            // 初始包序号
                            DATE = (ushort)timestampIn0_1ms,
                            L = 32,                            // 数据域长度（32字节）
                            UAVId = (Int16)mav.sysid,                         // 无人机编号
                            Longitude = (Int32)(mav.cs.lng * 1e6),     // 经度：东经116.4度
                            Latitude = (Int32)(mav.cs.lat * 1e6),       // 纬度：北纬40.0度
                            RelativeHeight = (Int16)(mav.cs.alt * 10),                // 相对高度（0.1m单位）
                            Altitude = ConvertVelocity(mav.cs.altasl),                   // 海拔高度（0.1m单位）                                          
                            EastVelocity = ConvertVelocity(mav.cs.vy),                // 东向速度（0.1单位）
                            NorthVelocity = ConvertVelocity(mav.cs.vx),                // 北向速度（0.1单位）
                            VerticalVelocity = ConvertVelocity(mav.cs.vz),             // 垂向速度（0.1单位）


                        };
                      

                        // 将结构体转换为字节数组
                        byte[] packetBytes = StructToBytes(pdxpPacket);

                        // 发送UDP数据包
                        udpClient.Send(packetBytes, packetBytes.Length, endPoint);
                    }
                }


                System.Threading.Thread.Sleep(200);  // 每 100ms 发送一次

            }
            // 线程停止时的清理工作
            Console.WriteLine("线程已停止.");
        }

        public static Int16 ConvertVelocity(double vx)
        {
            double scaled = vx * 10; // 转换为 0.1m/s 单位
            //return (Int16)(scaled >= 0 ? Math.Floor(scaled + 0.5) : Math.Ceiling(scaled - 0.5));
            return (Int16)Math.Round(scaled, MidpointRounding.AwayFromZero);
            //return (Int16)(scaled >= 0 ? Math.Floor(scaled) : Math.Ceiling(scaled));

        }
        //{"类型“MissionPlanner.Udp+Register”不能作为非托管结构进行封送处理；无法计算有意义的大小或偏移量。"}
        public static byte[] StructToBytes(object structObj)
        {
            int size = Marshal.SizeOf(structObj);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structObj, buffer, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        private void receivemessage(object sender, ElapsedEventArgs e)
        {
            while (true)
            {
                try
                {

                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = new byte[4096];
                    data = udpClient.Receive(ref remoteEndPoint);//此方法把数据来源ip、port放到第二个参数中

                    if (data[0] == 0x30 && data[1] == 0xEE && data[2] == 0x46 )
                    {
                        PdxpPacket2 packet = ByteArrayToStructure<PdxpPacket2>(data);
                        short uavId = packet.UAVId;
                        //我想要在这处解析UAVId的数据
                        if (uavId > 0)
                        {

                            Settings.SetBadUavId(uavId - 1, 1);

                            foreach (var port in MainV2.Comports)
                            {
                                foreach (var mav in port.MAVlist)
                                {
                                    if (mav.sysid == uavId)
                                    {
                                        port.setMode(mav.sysid, mav.compid, "Brake");
                                    }


                                }
                            }
                        }


                    }

                }
                catch
                {

                }
            }
        }
        private T ByteArrayToStructure<T>(byte[] data) where T : struct
        {
            IntPtr ptr = Marshal.AllocHGlobal(data.Length);
            try
            {
                
                Marshal.Copy(data, 0, ptr, data.Length);
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }


}
