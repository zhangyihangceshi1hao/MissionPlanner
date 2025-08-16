using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Org.BouncyCastle.Bcpg;

namespace MissionPlanner.GCSViews
{
    public partial class controllerTCP : Form
    {
        // ========= 新增：TCP 相关字段 =========
        private TcpClient _client;
        private Thread tcpThread;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _receiveTask;
      
        // 可选：默认地址和端口（若没有 textBoxIP/textBoxPort 会使用）
        private const string DefaultHost = "127.0.0.1";
        private const int DefaultPort = 9000;

        public controllerTCP()
        {
            InitializeComponent();
           
        }

        


        private void myButton1_Click(object sender, EventArgs e)
        {
            MainV2.comPort.sendPacket(new MAVLink.mavlink_pps_tcp_t
            {
                frequency = int.Parse(textBox1.Text),
                enable = (byte)1
            }, MainV2.comPort.sysidcurrent, MainV2.comPort.compidcurrent);
        }

        private void myButton2_Click(object sender, EventArgs e)
        {
            MainV2.comPort.sendPacket(new MAVLink.mavlink_pps_tcp_t
            {
                frequency = int.Parse(textBox1.Text),
                enable = (byte)0
            }, MainV2.comPort.sysidcurrent, MainV2.comPort.compidcurrent);
        }

        private async void myButton3_Click(object sender, EventArgs e)
        {
            if (myButton3.Text == "开始侦察")
            {
                    myButton3.Text = "结束侦察";
                 StartClientAsync();


            }
            else
            {
                myButton3.Text = "开始侦察";
                 StopClient();
            }
        }

        private async Task StartClientAsync()
        {
            try
            {
                _client = new TcpClient();
                _cancellationTokenSource = new CancellationTokenSource();
                string serverIp = FindTextBoxText("textBox2") ?? DefaultHost;
                int port = ParsePort(FindTextBoxText("textBox3")) ?? DefaultPort;
                await _client.ConnectAsync(serverIp, port);
             

                var stream = _client.GetStream();
                var buffer = new byte[1024];

                _receiveTask = Task.Run(async () =>
                {
                    try
                    {
                        while (!_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            if (_client.Connected && stream.DataAvailable)
                            {
                                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                                if (bytesRead <= 0)
                                    break;

                                // 从字节数组转换为结构体
                                PacketData packet = ByteArrayToStructure<PacketData>(buffer);
                                if (packet.MagicNum == 0x00B20E00) // 假设这里是 MagicNum
                                {

                                    Console.WriteLine("=== 收到数据包 (PacketData) ===");
                                    Console.WriteLine($"MagicNum     : 0x{packet.MagicNum:X8}");
                                    Console.WriteLine($"DataSize     : {packet.DataSize}");
                                    Console.WriteLine($"DataType     : 0x{packet.DataType:X8}");
                                    Console.WriteLine($"LenPack      : {packet.LenPack}");
                                    Console.WriteLine($"ZeroByte     : {packet.ZeroByte}");
                                    Console.WriteLine($"Version      : {packet.Version}");
                                    Console.WriteLine($"SequenceNum  : {packet.SequenceNum}");
                                    Console.WriteLine($"StateInfo    : 0x{packet.StateInfo:X4}");
                                    Console.WriteLine($"SerialNum    : {packet.SerialNum?.TrimEnd('\0') ?? "null"}");
                                    Console.WriteLine($"Longitude    : {packet.Longitude}");
                                    Console.WriteLine($"Latitude     : {packet.Latitude}");
                                    Console.WriteLine($"Altitude     : {packet.Altitude}");
                                    Console.WriteLine($"Height       : {packet.Height}");
                                    Console.WriteLine($"VNorth       : {packet.VNorth}");
                                    Console.WriteLine($"VEast        : {packet.VEast}");
                                    Console.WriteLine($"VUp          : {packet.VUp}");
                                    Console.WriteLine($"Yaw          : {packet.Yaw}");
                                    Console.WriteLine($"GpsTime      : {packet.GpsTime}");
                                    Console.WriteLine($"RcLatitude   : {packet.RcLatitude}");
                                    Console.WriteLine($"RcLongitude  : {packet.RcLongitude}");
                                    Console.WriteLine($"HomeLongitude: {packet.HomeLongitude}");
                                    Console.WriteLine($"HomeLatitude : {packet.HomeLatitude}");
                                    Console.WriteLine($"DeviceType   : {packet.DeviceType}");
                                    Console.WriteLine($"UuidLen      : {packet.UuidLen}");

                                    // 打印 Uuid（十六进制格式）
                                    if (packet.Uuid != null && packet.UuidLen > 0)
                                    {
                                        string uuidHex = string.Join(" ", packet.Uuid.Take(packet.UuidLen).Select(b => b.ToString("X2")));
                                        Console.WriteLine($"Uuid         : {uuidHex}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Uuid         : (null or zero length)");
                                    }

                                    Console.WriteLine($"Crc          : 0x{packet.Crc:X4}");
                                    Console.WriteLine("==============================");

                                }

                            }
                            else
                            {
                                await Task.Delay(10);
                            }
                        }
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                       
                    }
                }, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
         
            }
        }
        private void StopClient()
        {
            _cancellationTokenSource?.Cancel();
            _client?.Close();
            _client?.Dispose();
            _client = null;
           
        }
       

        // ========= 新增：PacketData 结构体 =========
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PacketData
        {
            public uint MagicNum;
            public uint DataSize;
            public uint DataType;
            public int LenPack;
            public int ZeroByte;
            public int Version;
            public ushort SequenceNum;
            public ushort StateInfo;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
            public string SerialNum;
            public int Longitude;
            public int Latitude;
            public short Altitude;
            public short Height;
            public short VNorth;
            public short VEast;
            public short VUp;
            public short Yaw;
            public ulong GpsTime;
            public int RcLatitude;
            public int RcLongitude;
            public int HomeLongitude;
            public int HomeLatitude;
            public int DeviceType;
            public int UuidLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] Uuid;
            public ushort Crc;
        }

        // ========= 新增：解析数据包并转为结构体 =========
        private async Task ReadLoopAsync(NetworkStream ns, CancellationToken ct)
        {
            try
            {
                byte[] buffer = new byte[4096]; // 假设最大字节数
                while (true)
                {
                    int bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length, ct);
                    if (bytesRead <= 0)
                        break;

                    // 从字节数组转换为结构体
                    PacketData packet = ByteArrayToStructure<PacketData>(buffer);
                    if (packet.MagicNum == 0x00B20E00) // 假设这里是 MagicNum
                    {

                        Console.WriteLine("=== 收到数据包 (PacketData) ===");
                        Console.WriteLine($"MagicNum     : 0x{packet.MagicNum:X8}");
                        Console.WriteLine($"DataSize     : {packet.DataSize}");
                        Console.WriteLine($"DataType     : 0x{packet.DataType:X8}");
                        Console.WriteLine($"LenPack      : {packet.LenPack}");
                        Console.WriteLine($"ZeroByte     : {packet.ZeroByte}");
                        Console.WriteLine($"Version      : {packet.Version}");
                        Console.WriteLine($"SequenceNum  : {packet.SequenceNum}");
                        Console.WriteLine($"StateInfo    : 0x{packet.StateInfo:X4}");
                        Console.WriteLine($"SerialNum    : {packet.SerialNum?.TrimEnd('\0') ?? "null"}");
                        Console.WriteLine($"Longitude    : {packet.Longitude}");
                        Console.WriteLine($"Latitude     : {packet.Latitude}");
                        Console.WriteLine($"Altitude     : {packet.Altitude}");
                        Console.WriteLine($"Height       : {packet.Height}");
                        Console.WriteLine($"VNorth       : {packet.VNorth}");
                        Console.WriteLine($"VEast        : {packet.VEast}");
                        Console.WriteLine($"VUp          : {packet.VUp}");
                        Console.WriteLine($"Yaw          : {packet.Yaw}");
                        Console.WriteLine($"GpsTime      : {packet.GpsTime}");
                        Console.WriteLine($"RcLatitude   : {packet.RcLatitude}");
                        Console.WriteLine($"RcLongitude  : {packet.RcLongitude}");
                        Console.WriteLine($"HomeLongitude: {packet.HomeLongitude}");
                        Console.WriteLine($"HomeLatitude : {packet.HomeLatitude}");
                        Console.WriteLine($"DeviceType   : {packet.DeviceType}");
                        Console.WriteLine($"UuidLen      : {packet.UuidLen}");

                        // 打印 Uuid（十六进制格式）
                        if (packet.Uuid != null && packet.UuidLen > 0)
                        {
                            string uuidHex = string.Join(" ", packet.Uuid.Take(packet.UuidLen).Select(b => b.ToString("X2")));
                            Console.WriteLine($"Uuid         : {uuidHex}");
                        }
                        else
                        {
                            Console.WriteLine($"Uuid         : (null or zero length)");
                        }

                        Console.WriteLine($"Crc          : 0x{packet.Crc:X4}");
                        Console.WriteLine("==============================");

                    }
                }
            }
            catch (Exception ex)
            {
               
            }
        }

        // 辅助方法：将字节数组转换为结构体
        public static T ByteArrayToStructure<T>(byte[] byteArray) where T : struct
        {
            T obj = default(T);
            int size = Marshal.SizeOf(obj);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(byteArray, 0, ptr, byteArray.Length);
                obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return obj;
        }


        private string FindTextBoxText(string name)
        {
            var c = this.Controls.Find(name, true).FirstOrDefault() as TextBox;
            return c?.Text?.Trim();
        }

        private int? ParsePort(string s)
        {
            if (int.TryParse(s, out int p) && p > 0 && p < 65536) return p;
            return null;
        }


    }
}