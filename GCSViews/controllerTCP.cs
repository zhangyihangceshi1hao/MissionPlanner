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
        private CancellationTokenSource _cts;
        private Task _readerTask;

        // 可选：默认地址和端口（若没有 textBoxIP/textBoxPort 会使用）
        private const string DefaultHost = "127.0.0.1";
        private const int DefaultPort = 9000;

        public controllerTCP()
        {
            InitializeComponent();
            InitializeDataGridView();
            // 确保按钮文字初始状态
            if (myButton3 != null) myButton3.Text = "开始侦察";

            // 窗体关闭时清理网络资源
            this.FormClosing += async (s, e) =>
            {
                try { await StopAsync(); } catch { /* 忽略 */ }
            };
        }
        // 初始化 DataGridView 控件
        private void InitializeDataGridView()
        {
            // 添加列到 DataGridView
            dataGridView1.Columns.Add("MagicNum", "MagicNum");
            dataGridView1.Columns.Add("DataSize", "DataSize");
            dataGridView1.Columns.Add("DataType", "DataType");
            dataGridView1.Columns.Add("LenPack", "LenPack");
            dataGridView1.Columns.Add("ZeroByte", "ZeroByte");
            dataGridView1.Columns.Add("Version", "Version");
            dataGridView1.Columns.Add("SequenceNum", "SequenceNum");
            dataGridView1.Columns.Add("StateInfo", "StateInfo");
            dataGridView1.Columns.Add("SerialNum", "SerialNum");
            dataGridView1.Columns.Add("Longitude", "Longitude");
            dataGridView1.Columns.Add("Latitude", "Latitude");
            dataGridView1.Columns.Add("Altitude", "Altitude");
            dataGridView1.Columns.Add("Height", "Height");
            dataGridView1.Columns.Add("VNorth", "VNorth");
            dataGridView1.Columns.Add("VEast", "VEast");
            dataGridView1.Columns.Add("VUp", "VUp");
            dataGridView1.Columns.Add("Yaw", "Yaw");
            dataGridView1.Columns.Add("GpsTime", "GpsTime");
            dataGridView1.Columns.Add("RcLatitude", "RcLatitude");
            dataGridView1.Columns.Add("RcLongitude", "RcLongitude");
            dataGridView1.Columns.Add("HomeLongitude", "HomeLongitude");
            dataGridView1.Columns.Add("HomeLatitude", "HomeLatitude");
            dataGridView1.Columns.Add("DeviceType", "DeviceType");
            dataGridView1.Columns.Add("UuidLen", "UuidLen");
            dataGridView1.Columns.Add("Uuid", "UUID");
            dataGridView1.Columns.Add("Crc", "CRC");
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
                myButton3.Enabled = false;
                try
                {
                    await StartAsync();
                    myButton3.Text = "结束侦察";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("连接或启动失败：\n" + ex.Message);
                }
                finally
                {
                    myButton3.Enabled = true;
                }
            }
            else
            {
                myButton3.Enabled = false;
                try
                {
                    await StopAsync();
                    myButton3.Text = "开始侦察";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("停止失败：\n" + ex.Message);
                }
                finally
                {
                    myButton3.Enabled = true;
                }
            }
        }
        private async Task StartAsync()
        {
            if (_client != null) return; // 已在运行

            string host = FindTextBoxText("textBox2") ?? DefaultHost;
            int port = ParsePort(FindTextBoxText("textBox3")) ?? DefaultPort;

            _cts = new CancellationTokenSource();
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            NetworkStream ns = _client.GetStream();
            ns.ReadTimeout = 30000;
            ns.WriteTimeout = 30000;

            _readerTask = Task.Run(() => ReadLoopAsync(ns, _cts.Token));
            AppendLog($"[INFO] 已连接到 {host}:{port}\r\n");
        }

        private async Task StopAsync()
        {
            if (_client == null) return;

            try
            {
                _cts?.Cancel();
                try { _client.Close(); } catch { /* 忽略 */ }

                if (_readerTask != null)
                {
                    await Task.WhenAny(_readerTask, Task.Delay(2000));
                }
            }
            finally
            {
                _readerTask = null;
                _client = null;
                _cts?.Dispose();
                _cts = null;
                AppendLog($"[INFO] 已停止侦察并断开连接\r\n");
            }
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
                while (!ct.IsCancellationRequested)
                {
                    int bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length, ct);
                    if (bytesRead <= 0)
                        break;

                    // 从字节数组转换为结构体
                    PacketData packet = ByteArrayToStructure<PacketData>(buffer);
                    if (packet.MagicNum == 0x00B20E00) // 假设这里是 MagicNum
                    {
                       

                        // 将数据包填充到 DataGridView
                        FillDataGridView(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog($"读取数据包异常: {ex.Message}\r\n");
            }
        }
        // 填充 DataGridView
        private void FillDataGridView(PacketData packet)
        {
            // 清空现有数据
            dataGridView1.Rows.Clear();

            // 添加新的一行
            dataGridView1.Rows.Add(
                packet.MagicNum.ToString("X8"),
                packet.DataSize,
                packet.DataType,
                packet.LenPack,
                packet.ZeroByte,
                packet.Version,
                packet.SequenceNum,
                packet.StateInfo,
                packet.SerialNum,
                packet.Longitude,
                packet.Latitude,
                packet.Altitude,
                packet.Height,
                packet.VNorth,
                packet.VEast,
                packet.VUp,
                packet.Yaw,
                packet.GpsTime,
                packet.RcLatitude,
                packet.RcLongitude,
                packet.HomeLongitude,
                packet.HomeLatitude,
                packet.DeviceType,
                packet.UuidLen,
                BitConverter.ToString(packet.Uuid),
                packet.Crc.ToString("X4")
            );
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

        // ========= 新增：UI 辅助 =========
        private void AppendLog(string text)
        {
            try
            {
                Control c = this.Controls.Find("txtLog", true).FirstOrDefault();
                if (c is TextBox txt && !txt.IsDisposed)
                {
                    if (txt.InvokeRequired)
                        txt.BeginInvoke(new Action(() => txt.AppendText(text)));
                    else
                        txt.AppendText(text);
                }
            }
            catch { /* 忽略所有 UI 日志异常 */ }
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
