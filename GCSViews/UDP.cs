using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using MissionPlanner.Controls;
using System.Runtime.InteropServices;
using MissionPlanner.Swarm.Sequence;
using System.Text;
using Onvif.Core.Client;
using BitMiracle.LibTiff.Classic;
using Xamarin.Forms;
using System.Runtime.ConstrainedExecution;
using System.Collections.Generic;

namespace MissionPlanner.GCSViews
{
    public class UDP
    {
        const string UDP_IP = "192.168.6.203";  // 目标IP
        //const string UDP_IP = "127.0.0.1";  // 目标IP

        const int UDP_PORT = 24584;           // 目标端口

        IPEndPoint endPoint;
        IPEndPoint endPointudp;
        UdpClient udpClient;
        public static Boolean is_true = false;
        private Thread workerThread;
        private CancellationTokenSource cancellationTokenSource;
        //private bool is_true = false;
        public void UDPlink(Boolean isLink)
        {
            
            //Console.WriteLine("这是一个日志信息");
            if (isLink)
            {
                is_true = true;
                endPoint = new IPEndPoint(IPAddress.Parse(UDP_IP), UDP_PORT);
                udpClient = new UdpClient(24576);
                // 启动发送数据的工作线程
                //StartWorker();
                ThreadPool.QueueUserWorkItem(sendmessage);
            }
            else {
                is_true = false ;
                // 关闭UDP客户端并停止线程
                if (udpClient != null)
                {
                    udpClient.Close();
                }

               
            }
            
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PdxpPacket
        {
            public byte VER;              // 协议版本号 (1字节)
            public Int16 MID;              // 任务代号 (1字节)
            public Int32 SID;            // 发送方地址 (2字节)
            public Int32 DID;            // 接收方地址 (2字节)
            public Int32 BID;              // 数据包类型标识 (4字节)
            public Int32 No;               // 包序号 (4字节)
            public byte FLAG;
            public Int32 R;
            public ushort DATE;             //2000年1月1日累计天数
            public Int32 TIME;             //北京时间，单位0.1ms（时间戳）
            public ushort L;              // 数据域长度 (2字节)
            public Int16 UAVId;           // 无人机编号 (2字节)
            public Int32 Longitude;         // 经度 (4字节)
            public Int32 Latitude;          // 纬度 (4字节)
            public Int16 RelativeHeight;  // 相对高度 (2字节)
            public Int16 Altitude;        // 海拔高度 (2字节)
            public Int32 GPSTime;           // GPS时间 (8字节)
            public Int32 Heading;           // 方向角 (4字节)
            public Int16 EastVelocity;    // 东向速度 (2字节)
            public Int16 NorthVelocity;   // 北向速度 (2字节)
            public Int16 VerticalVelocity;// 垂向速度 (2字节)
            public sbyte GPSSatellites;   // GPS搜星数量 (1字节)		_battery_voltage	12.587000937804632	double
            public Int16 Batteruy_V;      //电压(2字节)
            public byte Failsafe;          //是否进入故障安全模式（当前未触发）(1字节)
            
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PdxpPacket2
        {
            public byte VER;              // 协议版本号 (1字节)
            public Int16 MID;              // 任务代号 (1字节)
            public Int32 SID;            // 发送方地址 (2字节)
            public Int32 DID;            // 接收方地址 (2字节)
            public Int32 BID;              // 数据包类型标识 (4字节)
            public Int32 No;               // 包序号 (4字节)
            public byte FLAG;
            public Int32 R;
            public ushort DATE;             //2000年1月1日累计天数
            public Int32 TIME;             //北京时间，单位0.1ms（时间戳）
            public ushort L;              // 数据域长度 (2字节)




            public byte TelemetrySynch1;   //遥测帧同步字1
            public byte TelemetrySynch2;   //遥测帧同步字2
            public byte DestinationAddressHigh;   //目的地址高字节
            public byte DestinationAddressLow;   //目的地址高字节
            public byte SourceAddressHigh;   //源地址高字节
            public byte SourceAddressLow;   //源地址低字节
            public byte platType;   //平台/数据类型
            public Int32 TelemetryNo_SecretKey;   //帧编号_密钥备用
            public Int16 CRC; //CRC校验
            public byte FrameHeader1; //帧头1
            public byte FrameHeader2; //帧头2
            public byte FrameLength; //帧长度
            public byte UAVTYPE; //无人机型号
            public Int16 UAVId;           // 无人机编号 (2字节)
            public byte CommandIdentifier; //命令标识符
            public Int32 Latitude;          // 纬度 (4字节)
            public Int32 Longitude;         // 经度 (4字节)
            public Int16 MagneticHeadingAngle;    //磁航向角
            public Int16 Pitch;           // 俯仰角 (2字节)
            public Int16 Roll;           // 横滚角 (2字节)
            public Int16 RelativeHeight;  // 相对高度 (2字节)
            public Int16 AirSpeed; //空速
            public Int16 ElevatorAngle; //升降舵角
            public Int16 RudderAngle ; //方向舵角
            public Int16 SatelliteTrajectoryAngle; //卫星航迹角
            public Int16 ThrottleVolume; //油门量
            public Int16 TransmissionAtAltitude; //海拔高度下传
            public byte NightNavigationLightStatus; //夜航灯状态
            public byte FLAG2; //预留
            public Int16 EastVelocity;    // 东向速度 (2字节)
            public Int16 NorthVelocity;   // 北向速度 (2字节)
            
            public Int16 FlyTime;// 飞行时间 (2字节)
            public UInt16 GPSLostStarTime;// GPS丢星时间 (2字节)
            public byte FLAG3; //预留
            public byte CabinTemperature; //舱温
            public Int16 courseOfTheTarget;        // 目标航向
            public byte TargetWaypoint;        // 目标航点
            public byte Batteruy_V;      //电压
            public Int16 RateOfClimb; //爬升率
            public byte InstructionExecuted;      //已执行指令
            public byte AirplaneMode;      //飞行模式
            public byte Year;          // 年（UTC时间）
            public byte Month;         // 月
            public byte Day;           // 日
            public byte Hour;          // 时（UTC）
            public byte Minute;        // 分
            public byte Second;        // 秒
            public byte GpsHdop;       // GPS水平定位精度（HDOP值）
            public byte DeviceStatus;  // 飞机/载荷开关指令回报（设备状态）
            public byte SatelliteCount; // GPS搜星数量
            public Int16 FaultStatus;  // 故障状态
            public byte WarningFlag;   // 警告标识
            public Int16 CRC2;          // CRC校验
            public byte EndFlag;       // 结束标志（固定值，如0x55）
            public Int32 X;            // X坐标分量（单位：毫米 或 其他统一单位）
            public Int32 Y;            // Y坐标分量
            public Int32 Z;            // Z坐标分量
            public Int32 Vx;           // X方向速度Vx（单位：毫米/秒）
            public Int32 Vy;           // Y方向速度Vy
            public Int32 Vz;           // Z方向速度Vz


        }
        private static Dictionary<int, DateTime?> gpsFailureStartTime = new Dictionary<int, DateTime?>();

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

                        // 示例：北京某点 BLH 值
                        double latitude = mav.cs.lat;   // 纬度（北纬）
                        double longitude = mav.cs.lng; // 经度（东经）
                        double altitude = mav.cs.alt;         // 海拔高度（米）

                        var (X, Y, Z) = Program.BlhToXyz(latitude, longitude, altitude);

                        sequence++;
                            //DateTime gpsEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                            
                           


                            // 设置基准时间：2000年1月1日 UTC
                            DateTime baseTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                            // 获取当前 UTC 时间
                            //DateTime nowUtc = DateTime.UtcNow;time_usec
                        DateTime nowUtc = DateTime.Now;

                        // 计算时间差
                        TimeSpan difference = nowUtc - baseTime;
                            //TimeSpan elapsed = mav.cs.gpstime.ToUniversalTime() - gpsEpoch;
                            // 获取总天数（整数天）
                            int daysSince2000 = (int)difference.TotalDays;

                        Int16 timestamp_s = (Int16)(mav.cs.timeInAir);
                        
                        //Int32 ticks = (Int32)(elapsed.TotalMilliseconds * 10); // 转换为 0.1 毫秒单位
                        Int16 gpstimestamp_s = (Int16)((DateTime.UtcNow - mav.lastvalidpacket).TotalSeconds);
                        int modeType = 0;
                        if (mav.cs.mode == "Stabilize")
                        {
                            modeType = 0;

                        }
                        else if (mav.cs.mode == "Auto")
                        {
                            modeType = 2;

                        }
                        else if (mav.cs.mode == "Loiter" || mav.cs.mode == "Circle")
                        {
                            modeType = 3;

                        }
                        else if (mav.cs.mode == "RTL"  )
                        {
                            modeType = 4;
                        } else if (mav.cs.mode == "|| mav.cs.mode == \"Auto RTL\"")
                        {
                            modeType = 6;
                        }else if (mav.cs.mode == "AltHold"  )
                        {
                            modeType = 7;
                        }else if (mav.cs.mode == "Brake")
                        {
                            modeType = 8;
                        }
                        if (mav.cs.satcount < 3)
                        {
                            //gpsFailureStartTime[mav.sysid] = DateTime.Now;
                        }
                        else {
                            gpsFailureStartTime[mav.sysid] = nowUtc;
                        }
                        TimeSpan elapsed =  nowUtc - gpsFailureStartTime[mav.sysid].Value;

                            PdxpPacket2 pdxpPacket2 = new PdxpPacket2
                            {
                                VER = 0x80,                    // 协议版本号
                                MID = 20521,               // 任务代号
                                SID = BitConverter.ToInt32(new byte[4] { 0x01, 0x02, 0x11, 0xee }, 0),           // 发送方地址
                                DID = BitConverter.ToInt32(new byte[4] { 0x01, 0x01, 0x01, 0x21 }, 0),           // 接收方地址
                                BID = BitConverter.ToInt32(new byte[4] { mav.sysid, 0x02, 0x2b, 0xee }, 0),           // 数据包类型标识
                                No = (Int32)sequence,                  // 包序号
                                FLAG = 0,                // 标志位
                                R = 0,                      // 预留或使用字段
                                DATE = (ushort)((DateTime.Now - new DateTime(2000, 1, 1)).Days + 1), // 从2000年累计天数
                                TIME = (Int32)(DateTime.Now.TimeOfDay.TotalMilliseconds * 10), // 时间戳（0.1ms单位）
                                L = 107,                      // 数据域长度（可后续计算）



                                TelemetrySynch1 = 0xEB,     // 遥测帧同步字1
                                TelemetrySynch2 = 0x90,     // 遥测帧同步字2
                                DestinationAddressHigh = 0,
                                DestinationAddressLow = 0,
                                SourceAddressHigh = 0,
                                SourceAddressLow = 0,
                                platType = 0x03,            // 平台/数据类型
                                TelemetryNo_SecretKey = 0, // 帧编号_密钥备用
                                CRC = 0,               // CRC校验（可后期计算）
                                FrameHeader1 = 0,
                                FrameHeader2 = 0,
                                FrameLength = 0,         // 帧长度（可后期计算）
                                UAVTYPE = 0x03,             // 无人机型号
                                UAVId = (Int16)mav.sysid,                // 无人机编号
                                CommandIdentifier = 0xB0,     // 命令标识符
                                Latitude = (Int32)(mav.cs.lat * 1e7),       // 纬度：北纬40.0度
                                Longitude = (Int32)(mav.cs.lng * 1e7),     // 经度：东经116.4度
                                MagneticHeadingAngle = (Int16)(mav.cs.yaw * 10), // 磁航向角（单位：0.01度）
                                Pitch = (Int16)(mav.cs.pitch * 100),                  // 俯仰角
                                Roll = (Int16)(mav.cs.roll * 100),                   // 横滚角
                                RelativeHeight = (Int16)(mav.cs.alt * 10),      // 相对高度（单位：厘米）
                                AirSpeed = (Int16)(mav.cs.airspeed * 100),             // 空速（单位：0.1m/s）
                                ElevatorAngle = 0,          // 升降舵角
                                RudderAngle = 0,            // 方向舵角
                                SatelliteTrajectoryAngle = 0, // 卫星航迹角
                                ThrottleVolume = 0,        // 油门量（百分比）
                                TransmissionAtAltitude = (Int16)(mav.cs.altasl * 10), // 海拔高度下传（单位：米 × 100）
                                NightNavigationLightStatus = 0xD1, // 夜航灯状态
                                FLAG2 = 0x00,              // 预留
                                EastVelocity = ConvertVelocity(mav.cs.vy),                // 东向速度（0.1单位）
                                NorthVelocity = ConvertVelocity(mav.cs.vx),                // 北向速度（0.1单位）

                                FlyTime = timestamp_s,            // 飞行时间（单位：秒）
                                GPSLostStarTime = (UInt16)elapsed.TotalSeconds,       // GPS丢星时间（单位：秒）
                                FLAG3 = 0x00,              // 预留
                                CabinTemperature = 0,      // 舱温（单位：摄氏度）
                                courseOfTheTarget = 0,   // 目标航向（单位：0.01度）
                                TargetWaypoint = 0,          // 目标航点编号
                                Batteruy_V = (byte)(mav.cs.battery_voltage * 10),            // 电压（单位：V）
                                RateOfClimb = 0,           // 爬升率（单位：cm/s）
                                InstructionExecuted = 0x00,  // 已执行指令
                                AirplaneMode = (byte)modeType,        // 飞行模式
                                Year = (byte)(nowUtc.Year - 2000),                 // UTC年份（如 2024）
                                Month = (byte)nowUtc.Month,                  // UTC月份
                                Day = (byte)nowUtc.Day,                  // UTC日期
                                Hour = (byte)nowUtc.Hour,                 // UTC小时
                                Minute = (byte)nowUtc.Minute,               // UTC分钟
                                Second = (byte)nowUtc.Second,               // UTC秒
                                GpsHdop = (byte)(mav.cs.gpshdop * 10),             // HDOP值（×10）
                                DeviceStatus = 0,        // 设备状态
                                SatelliteCount = (byte)(mav.cs.satcount),        // GPS搜星数量
                                FaultStatus = 0,      // 故障状态
                                WarningFlag = 0,        // 警告标识
                                CRC2 = 0,             // CRC校验
                                EndFlag = 0xAA,            // 结束标志
                                X = ConvertVelocity2(X),                    // X坐标分量（单位：毫米）
                                Y = ConvertVelocity2(Y),                    // Y坐标分量
                                Z = ConvertVelocity2(Z),                    // Z坐标分量
                                Vx = ConvertVelocity1(mav.cs.vx),                   // X方向速度Vx
                                Vy = ConvertVelocity1(mav.cs.vy),                   // Y方向速度Vy
                                Vz = ConvertVelocity1(mav.cs.vz)                    // Z方向速度Vz
                            };
                        // 打印参数日志
                        //    Console.WriteLine("📊 PDXP 数据包字段解析：");
                        //    Console.WriteLine($"VER: {pdxpPacket.VER}        // 协议版本");
                        //    Console.WriteLine($"MID: 0x{pdxpPacket.MID:X2}   // 任务代号");
                        //    Console.WriteLine($"SID: 0x{pdxpPacket.SID:X4}   // 发送方地址");
                        //    Console.WriteLine($"DID: 0x{pdxpPacket.DID:X4}   // 接收方地址");
                        //    Console.WriteLine($"BID: 0x{pdxpPacket.BID:X8}   // 数据包类型标识");
                        //    Console.WriteLine($"No: {pdxpPacket.No}          // 包序号");
                        //    Console.WriteLine($"L: {pdxpPacket.L}            // 数据域长度（字节）");
                        //    Console.WriteLine($"UAVId: {pdxpPacket.UAVId}    // 无人机编号（sysid={mav.sysid}）");
                        //    Console.WriteLine($"Longitude: {pdxpPacket.Longitude}  // 经度 ×1e6 → {mav.cs.lng:F6}°");
                        //    Console.WriteLine($"Latitude: {pdxpPacket.Latitude}   // 纬度 ×1e6 → {mav.cs.lat:F6}°");
                        //    Console.WriteLine($"RelativeHeight: {pdxpPacket.RelativeHeight}  // 相对高度 ×10 → {mav.cs.alt:F2}m");
                        //    Console.WriteLine($"Altitude: {pdxpPacket.Altitude}              // 海拔高度 ×10 → {mav.cs.altasl:F2}m");
                        //    Console.WriteLine($"GPSTime: {pdxpPacket.GPSTime}               // GPS 时间戳（0.1ms）");
                        //    Console.WriteLine($"Heading: {pdxpPacket.Heading}               // 方向角 ×10 → {mav.cs.yaw:F1}°");
                        //    Console.WriteLine($"EastVelocity: {pdxpPacket.EastVelocity}     // 东向速度 ×10 → {mav.cs.vy:F1}m/s");
                        //    Console.WriteLine($"NorthVelocity: {pdxpPacket.NorthVelocity}   // 北向速度 ×10 → {mav.cs.vx:F1}m/s");
                        //    Console.WriteLine($"VerticalVelocity: {pdxpPacket.VerticalVelocity}  // 垂向速度 ×10 → {mav.cs.vz:F1}m/s");
                        //    Console.WriteLine($"GPSSatellites: {pdxpPacket.GPSSatellites}   // GPS 搜星数量（satcount={mav.cs.satcount}）");

                        //    Console.WriteLine($"Batteruy_V: {pdxpPacket.Batteruy_V}   // 电池电压 ×100 → {mav.cs.battery_voltage:F2}V");
                        //    Console.WriteLine($"Failsafe: {pdxpPacket.Failsafe}   // 是否进入故障安全模式（{mav.cs.failsafe}）");
                        //    Console.WriteLine($"Gpsstatus: {pdxpPacket.Gpsstatus}   // GPS 状态（{mav.cs.gpsstatus:F0}，≥3 表示正常）");
                        //Console.WriteLine("--------------------------------------------------");

                        // 将结构体转换为字节数组
                        byte[] packetBytes = StructToBytes(pdxpPacket2);

                            // 发送UDP数据包
                            udpClient.Send(packetBytes, packetBytes.Length, endPoint);
                        }
                    }
                   

                    System.Threading.Thread.Sleep(1000);  // 每 100ms 发送一次
               
            }
            // 线程停止时的清理工作
            Console.WriteLine("线程已停止.");
        }
        
        public static Int16 ConvertVelocity(double vx)
        {
            double scaled = vx * 100; // 转换为 0.1m/s 单位
            //return (Int16)(scaled >= 0 ? Math.Floor(scaled + 0.5) : Math.Ceiling(scaled - 0.5));
            return (Int16)Math.Round(scaled, MidpointRounding.AwayFromZero);
            //return (Int16)(scaled >= 0 ? Math.Floor(scaled) : Math.Ceiling(scaled));

        }

        public static Int32 ConvertVelocity1(double vx)
        {
            double scaled = vx * 100; // 转换为 0.1m/s 单位
            //return (Int16)(scaled >= 0 ? Math.Floor(scaled + 0.5) : Math.Ceiling(scaled - 0.5));
            return (Int32)Math.Round(scaled, MidpointRounding.AwayFromZero);
            //return (Int16)(scaled >= 0 ? Math.Floor(scaled) : Math.Ceiling(scaled));

        }

        public static Int32 ConvertVelocity2(double vx)
        {
            double scaled = vx * 10; // 转换为 0.1m/s 单位
            //return (Int16)(scaled >= 0 ? Math.Floor(scaled + 0.5) : Math.Ceiling(scaled - 0.5));
            return (Int32)Math.Round(scaled, MidpointRounding.AwayFromZero);
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
        private void receivemessage()
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

        class Program
        {
            // GRS80 椭球参数（用于 CGCS2000 坐标系）
            private const double A = 6378137.0;           // 长半轴
            private const double E2 = 0.00669438002290058; // 第一偏心率平方

            /// <summary>
            /// 将经纬度和海拔转换为 CGCS2000 地心坐标（X, Y, Z）
            /// </summary>
            /// <param name="lat">纬度（度）</param>
            /// <param name="lon">经度（度）</param>
            /// <param name="height">海拔高度（米）</param>
            /// <returns>包含X,Y,Z的元组</returns>
            public static (double X, double Y, double Z) BlhToXyz(double lat, double lon, double height)
            {
                // 将角度转为弧度
                double latRad = lat * Math.PI / 180.0;
                double lonRad = lon * Math.PI / 180.0;

                // 计算卯酉圈曲率半径 N
                double sinLat = Math.Sin(latRad);
                double cosLat = Math.Cos(latRad);
                double cosLon = Math.Cos(lonRad);
                double sinLon = Math.Sin(lonRad);

                double N = A / Math.Sqrt(1 - E2 * sinLat * sinLat);

                // 计算X、Y、Z
                double X = (N + height) * cosLat * cosLon;
                double Y = (N + height) * cosLat * sinLon;
                double Z = (N * (1 - E2) + height) * sinLat;

                return (X, Y, Z);
            }

            
        }
    }

   
}
