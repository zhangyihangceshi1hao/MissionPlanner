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

        //private async Task StartClientAsync()
        //{
        //    try
        //    {
        //        _client = new TcpClient();
        //        _cancellationTokenSource = new CancellationTokenSource();
        //        string serverIp = FindTextBoxText("textBox2") ?? DefaultHost;
        //        int port = ParsePort(FindTextBoxText("textBox3")) ?? DefaultPort;
        //        await _client.ConnectAsync(serverIp, port);
             

        //        var stream = _client.GetStream();
        //        var buffer = new byte[1024];

        //        _receiveTask = Task.Run(async () =>
        //        {
        //            try
        //            {
        //                while (!_cancellationTokenSource.Token.IsCancellationRequested)
        //                {
        //                    if (_client.Connected && stream.DataAvailable)
        //                    {
        //                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        //                        if (bytesRead <= 0)
        //                            break;

        //                        // 从字节数组转换为结构体
        //                        PacketData packet = ByteArrayToStructure<PacketData>(buffer);
        //                        if (packet.MagicNum == 0x00B20E00) // 假设这里是 MagicNum
        //                        {

        //                            Console.WriteLine("=== 收到数据包 (PacketData) ===");
        //                            //Console.WriteLine($"MagicNum     : 0x{packet.MagicNum:X8}");
        //                            //Console.WriteLine($"DataSize     : {packet.DataSize}");
        //                            //Console.WriteLine($"DataType     : 0x{packet.DataType:X8}");
        //                            //Console.WriteLine($"LenPack      : {packet.LenPack}");
        //                            //Console.WriteLine($"ZeroByte     : {packet.ZeroByte}");
        //                            //Console.WriteLine($"Version      : {packet.Version}");
        //                            //Console.WriteLine($"SequenceNum  : {packet.SequenceNum}");
        //                            //Console.WriteLine($"StateInfo    : 0x{packet.StateInfo:X4}");
        //                            //Console.WriteLine($"SerialNum    : {packet.SerialNum?.TrimEnd('\0') ?? "null"}");
        //                            //Console.WriteLine($"Longitude    : {packet.Longitude}");
        //                            //Console.WriteLine($"Latitude     : {packet.Latitude}");
        //                            //Console.WriteLine($"Altitude     : {packet.Altitude}");
        //                            //Console.WriteLine($"Height       : {packet.Height}");
        //                            //Console.WriteLine($"VNorth       : {packet.VNorth}");
        //                            //Console.WriteLine($"VEast        : {packet.VEast}");
        //                            //Console.WriteLine($"VUp          : {packet.VUp}");
        //                            //Console.WriteLine($"Yaw          : {packet.Yaw}");
        //                            //Console.WriteLine($"GpsTime      : {packet.GpsTime}");
        //                            //Console.WriteLine($"RcLatitude   : {packet.RcLatitude}");
        //                            //Console.WriteLine($"RcLongitude  : {packet.RcLongitude}");
        //                            //Console.WriteLine($"HomeLongitude: {packet.HomeLongitude}");
        //                            //Console.WriteLine($"HomeLatitude : {packet.HomeLatitude}");
        //                            //Console.WriteLine($"DeviceType   : {packet.DeviceType}");
        //                            //Console.WriteLine($"UuidLen      : {packet.UuidLen}");

        //                            //// 打印 Uuid（十六进制格式）
        //                            //if (packet.Uuid != null && packet.UuidLen > 0)
        //                            //{
        //                            //    string uuidHex = string.Join(" ", packet.Uuid.Take(packet.UuidLen).Select(b => b.ToString("X2")));
        //                            //    Console.WriteLine($"Uuid         : {uuidHex}");
        //                            //}
        //                            //else
        //                            //{
        //                            //    Console.WriteLine($"Uuid         : (null or zero length)");
        //                            //}

        //                            Console.WriteLine($"Crc          : 0x{packet.Crc:X4}");
        //                            Console.WriteLine("==============================");

        //                        }

        //                    }
        //                    //else
        //                    //{
        //                    //    await Task.Delay(10);
        //                    //}
        //                }
        //            }
        //            catch (Exception ex) when (!(ex is OperationCanceledException))
        //            {
                       
        //            }
        //        }, _cancellationTokenSource.Token);
        //    }
        //    catch (Exception ex)
        //    {
         
        //    }
        //}
        private void StopClient()
        {
            _cancellationTokenSource?.Cancel();
            _client?.Close();
            _client?.Dispose();
            _client = null;
           
        }
       

        //// ========= 新增：PacketData 结构体 =========
        //[StructLayout(LayoutKind.Sequential, Pack = 1)]
        //public struct PacketData
        //{
        //    public uint MagicNum;
        //    public uint DataSize;
        //    public uint DataType;
        //    public int LenPack;
        //    public int ZeroByte;
        //    public int Version;
        //    public ushort SequenceNum;
        //    public ushort StateInfo;
        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        //    public string SerialNum;
        //    public int Longitude;
        //    public int Latitude;
        //    public short Altitude;
        //    public short Height;
        //    public short VNorth;
        //    public short VEast;
        //    public short VUp;
        //    public short Yaw;
        //    public ulong GpsTime;
        //    public int RcLatitude;
        //    public int RcLongitude;
        //    public int HomeLongitude;
        //    public int HomeLatitude;
        //    public int DeviceType;
        //    public int UuidLen;
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        //    public byte[] Uuid;
        //    public ushort Crc;
        //}
        // 保留作为数据容器，但不再用于 Marshal 解析
        public class PacketData
        {
            public uint MagicNum;
            public uint DataSize;
            public uint DataType;
            public int LenPack;
            public int ZeroByte;
            public int Version;
            public ushort SequenceNum;
            public ushort StateInfo;
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
            public byte[] Uuid;
            public ushort Crc;
        }

        private static readonly byte[] MagicBytes = { 0x00, 0x0E, 0xB2, 0x00 }; // 0x00B20E00 小端

        private PacketData ParsePacket(byte[] data, int offset, int length)
        {
            if (length - offset < 4) return null;

            // 检查 MagicNum
            if (data[offset + 0] != MagicBytes[0] ||
                data[offset + 1] != MagicBytes[1] ||
                data[offset + 2] != MagicBytes[2] ||
                data[offset + 3] != MagicBytes[3])
                return null;

            int pos = offset + 4;

            if (pos + 4 > length) return null;
            uint dataSize = BitConverter.ToUInt32(data, pos); pos += 4;
            if (dataSize > 4096) return null; // 防止过大包

            if (offset + 4 + 4 + (int)dataSize > length) return null; // 数据不完整

            // 开始解析
            var packet = new PacketData();
            packet.MagicNum = 0x00B20E00;
            packet.DataSize = dataSize;

            pos += (int)dataSize; // 跳过 DataSize 字段本身？不，DataSize 是从 DataType 开始的长度

            // 重新定位：DataSize 是从 DataType 开始的长度
            pos = offset + 8; // Magic(4) + DataSize(4)

            //if (pos + 130 > length) return null; // 最小包长

            packet.DataType = BitConverter.ToUInt32(data, pos); pos += 4;
            packet.LenPack = BitConverter.ToInt32(data, pos); pos += 4;
            packet.ZeroByte = BitConverter.ToInt32(data, pos); pos += 4;
            packet.Version = BitConverter.ToInt32(data, pos); pos += 4;

            packet.SequenceNum = BitConverter.ToUInt16(data, pos); pos += 2;
            packet.StateInfo = BitConverter.ToUInt16(data, pos); pos += 2;

            // serial_num (17)
            packet.SerialNum = Encoding.ASCII.GetString(data, pos, 17).Split('\0')[0];
            pos += 17;

            packet.Longitude = BitConverter.ToInt32(data, pos); pos += 4;
            packet.Latitude = BitConverter.ToInt32(data, pos); pos += 4;
            packet.Altitude = BitConverter.ToInt16(data, pos); pos += 2;
            packet.Height = BitConverter.ToInt16(data, pos); pos += 2;
            packet.VNorth = BitConverter.ToInt16(data, pos); pos += 2;
            packet.VEast = BitConverter.ToInt16(data, pos); pos += 2;
            packet.VUp = BitConverter.ToInt16(data, pos); pos += 2;
            packet.Yaw = BitConverter.ToInt16(data, pos); pos += 2;
            packet.GpsTime = BitConverter.ToUInt64(data, pos); pos += 8;

            packet.RcLatitude = BitConverter.ToInt32(data, pos); pos += 4;
            packet.RcLongitude = BitConverter.ToInt32(data, pos); pos += 4;
            packet.HomeLongitude = BitConverter.ToInt32(data, pos); pos += 4;
            packet.HomeLatitude = BitConverter.ToInt32(data, pos); pos += 4;
            packet.DeviceType = BitConverter.ToInt32(data, pos); pos += 4;
            packet.UuidLen = BitConverter.ToInt32(data, pos); pos += 4;

            packet.Uuid = new byte[20];
            Array.Copy(data, pos, packet.Uuid, 0, 20);
            pos += 20;

            packet.Crc = BitConverter.ToUInt16(data, pos); pos += 2;

            return packet;
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
        private List<byte> _receiveBuffer = new List<byte>(); // 缓冲区

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

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (_client.Connected)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            // 添加到缓冲区
                            _receiveBuffer.AddRange(new ArraySegment<byte>(buffer, 0, bytesRead));

                            // 尝试解析
                            while (TryParsePacketFromBuffer())
                            {
                                // 继续解析下一个包
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录错误
            }
            finally
            {
                StopClient();
            }
        }

        private bool TryParsePacketFromBuffer()
        {
            if (_receiveBuffer.Count < 8) return false; // 至少要有 Magic + DataSize

            // 查找 MagicNum
            int magicIndex = -1;
            for (int i = 0; i <= _receiveBuffer.Count - 4; i++)
            {
                if (_receiveBuffer[i] == 0x00 && _receiveBuffer[i + 1] == 0x0E &&
                    _receiveBuffer[i + 2] == 0xB2 && _receiveBuffer[i + 3] == 0x00)
                {
                    magicIndex = i;
                    break;
                }
            }

            if (magicIndex == -1)
            {
                _receiveBuffer.Clear();
                return false;
            }

            if (magicIndex > 0)
            {
                _receiveBuffer.RemoveRange(0, magicIndex);
            }

            if (_receiveBuffer.Count < 8) return false;

            uint dataSize = BitConverter.ToUInt32(_receiveBuffer.ToArray(), 4);
            if (dataSize < 80 || dataSize > 4096) // 你的协议最小包 ~105 字节
            {
                _receiveBuffer.RemoveRange(0, 1);
                return false;
            }

            int totalPacketLength = 8 + (int)dataSize; // Magic(4) + DataSize(4) + DataSize
            if (_receiveBuffer.Count < totalPacketLength)
                return false; // 数据不完整

            // 解析
            var packet = ParsePacket(_receiveBuffer.ToArray(), 0, totalPacketLength);
            if (packet != null)
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
                //// ✅ 成功解析，更新 UI（跨线程）
                //this.Invoke((MethodInvoker)delegate
                //{
                //    UpdateUI(packet); // 你自己实现的 UI 更新方法
                //});
            }

            // 移除已解析数据
            _receiveBuffer.RemoveRange(0, totalPacketLength);
            return true;
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