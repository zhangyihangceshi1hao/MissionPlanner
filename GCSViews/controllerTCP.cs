using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Org.BouncyCastle.Bcpg;
using GMap.NET; // PointLatLngAlt 用到
using MissionPlanner; // MainV2
using MissionPlanner.Utilities;
using MissionPlanner.Controls;

namespace MissionPlanner.GCSViews
{
    public partial class controllerTCP : Form
    {
        // ========= TCP Server 相关字段 =========
        private TcpListener _listener;
        private TcpClient _client;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _acceptTask;
        private List<byte> _receiveBuffer = new List<byte>();

        private const string DefaultHost = "0.0.0.0"; // 监听所有网卡
        private const int DefaultPort = 9000;

        // ========= UI 控件 =========
        private GroupBox groupBoxTCP;
        private GroupBox groupBoxFlight;
        private Button buttonArmTakeoff;
        private Button buttonGuided;
        private TextBox textBoxLat;
        private TextBox textBoxLng;
        private TextBox textBoxAlt;
        private Label labelLat;
        private Label labelLng;
        private Label labelAlt;

        // 新增：用于显示解析结果的表格
        private DataGridView dataGridView;

        public controllerTCP()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            this.Text = "TCP 控制器";
            this.Size = new Size(1020, 500);
            this.BackColor = Color.WhiteSmoke;

            // ===== TCP 控件分组 =====
            groupBoxTCP = new GroupBox
            {
                Text = "干扰控制 (TCP Server)",
                Left = 10,
                Top = 10,
                Width = 350,
                Height = 150,
                Font = new Font("微软雅黑", 10, FontStyle.Bold)
            };
            this.Controls.Add(groupBoxTCP);

            var tcpControls = new Control[] { textBox1, myButton1, myButton2, myButton3, textBox2, textBox3 };
            foreach (var c in tcpControls)
            {
                if (c != null) c.Parent = groupBoxTCP;
            }

            // ===== 飞行控制分组 =====
            groupBoxFlight = new GroupBox
            {
                Text = "飞行控制",
                Left = 10,
                Top = 180,
                Width = 350,
                Height = 250,
                Font = new Font("微软雅黑", 10, FontStyle.Bold)
            };
            this.Controls.Add(groupBoxFlight);

            buttonArmTakeoff = CreateStyledButton("解锁并起飞", 20, 30);
            buttonArmTakeoff.Click += but_armandtakeoff_Click;
            groupBoxFlight.Controls.Add(buttonArmTakeoff);

            labelLat = new Label { Text = "纬度:", Left = 20, Top = 80, Width = 50, Font = new Font("微软雅黑", 9) };
            textBoxLat = new TextBox { Left = 80, Top = 80, Width = 150 };

            labelLng = new Label { Text = "经度:", Left = 20, Top = 110, Width = 50, Font = new Font("微软雅黑", 9) };
            textBoxLng = new TextBox { Left = 80, Top = 110, Width = 150 };

            labelAlt = new Label { Text = "高度:", Left = 20, Top = 140, Width = 50, Font = new Font("微软雅黑", 9) };
            textBoxAlt = new TextBox { Left = 80, Top = 140, Width = 150 };

            groupBoxFlight.Controls.Add(labelLat);
            groupBoxFlight.Controls.Add(textBoxLat);
            groupBoxFlight.Controls.Add(labelLng);
            groupBoxFlight.Controls.Add(textBoxLng);
            groupBoxFlight.Controls.Add(labelAlt);
            groupBoxFlight.Controls.Add(textBoxAlt);

            buttonGuided = CreateStyledButton("指点飞行", 20, 180);
            buttonGuided.Click += buttonGuided_Click;
            groupBoxFlight.Controls.Add(buttonGuided);

            // ===== 解析结果表格 =====
            dataGridView = new DataGridView
            {
                Left = 380,
                Top = 10,
                Width = 600,
                Height = 420,
                ReadOnly = true,
                AllowUserToAddRows = false,
                ColumnCount = 2,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };

            dataGridView.Columns[0].Name = "字段";
            dataGridView.Columns[1].Name = "值";
            dataGridView.Columns[0].Width = 200;
            dataGridView.Columns[1].Width = 380;

            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9, FontStyle.Bold);
            dataGridView.RowHeadersVisible = false;
            dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            this.Controls.Add(dataGridView);
        }

        private Button CreateStyledButton(string text, int left, int top)
        {
            return new Button
            {
                Text = text,
                Left = left,
                Top = top,
                Width = 120,
                Height = 35,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9, FontStyle.Bold)
            };
        }

        // ========= 解锁起飞 =========
        private void but_armandtakeoff_Click(object sender, EventArgs e)
        {
            try
            {
                MainV2.comPort.setMode("GUIDED");

                if (MainV2.comPort.doARM(true))
                {
                    MainV2.comPort.setMode("GUIDED");

                    Thread.Sleep(5000);

                    float takeoffAlt = 10;
                    MainV2.comPort.doCommand(
                        (byte)MainV2.comPort.sysidcurrent,
                        (byte)MainV2.comPort.compidcurrent,
                        MAVLink.MAV_CMD.TAKEOFF,
                        0, 0, 0, 0,
                        0, 0, takeoffAlt
                    );

                    CustomMessageBox.Show("已发送解锁并起飞命令");
                }
                else
                {
                    CustomMessageBox.Show("解锁失败");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("起飞命令异常: " + ex.Message);
            }
        }

        // ========= 定点飞行 =========
        private void buttonGuided_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBoxLat.Text) ||
                    string.IsNullOrWhiteSpace(textBoxLng.Text) ||
                    string.IsNullOrWhiteSpace(textBoxAlt.Text))
                {
                    CustomMessageBox.Show("请先输入经纬度和高度");
                    return;
                }

                double lat = double.Parse(textBoxLat.Text);
                double lng = double.Parse(textBoxLng.Text);
                float alt = float.Parse(textBoxAlt.Text);

                Locationwp target = new Locationwp
                {
                    id = (ushort)MAVLink.MAV_CMD.WAYPOINT,
                    lat = lat,
                    lng = lng,
                    alt = alt
                };

                MainV2.comPort.setMode("GUIDED");
                MainV2.comPort.setGuidedModeWP(
                    (byte)MainV2.comPort.sysidcurrent,
                    (byte)MainV2.comPort.compidcurrent,
                    target
                );

                CustomMessageBox.Show($"已发送定点飞行: Lat={lat}, Lng={lng}, Alt={alt}");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("定点飞行异常: " + ex.Message);
            }
        }

        // ========= TCP Server 功能 =========
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
            if (myButton3.Text == "启动服务")
            {
                myButton3.Text = "停止服务";
                await StartServerAsync();
            }
            else
            {
                myButton3.Text = "启动服务";
                StopServer();
            }
        }

        private async Task StartServerAsync()
        {
            try
            {
                string hostIp = FindTextBoxText("textBox2") ?? DefaultHost;
                int port = ParsePort(FindTextBoxText("textBox3")) ?? DefaultPort;

                _listener = new TcpListener(IPAddress.Parse(hostIp), port);
                _listener.Start();
                _cancellationTokenSource = new CancellationTokenSource();

                _acceptTask = Task.Run(async () =>
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var client = await _listener.AcceptTcpClientAsync();
                            _client = client;
                            _ = HandleClientAsync(client, _cancellationTokenSource.Token);
                        }
                        catch (ObjectDisposedException)
                        {
                            break; // listener 停止
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("启动服务异常: " + ex.Message);
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            try
            {
                var stream = client.GetStream();
                var buffer = new byte[1024];

                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead <= 0) break;

                    _receiveBuffer.AddRange(new ArraySegment<byte>(buffer, 0, bytesRead));
                    while (TryParsePacketFromBuffer()) { }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("客户端处理异常: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        private void StopServer()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _client?.Close();
                _listener?.Stop();
                _listener = null;
                _client = null;
            }
            catch { }
        }

        // ========= 数据包解析 =========
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

            public double Longitude;
            public double Latitude;
            public double Altitude;
            public double Height;
            public double VNorth;
            public double VEast;
            public double VUp;
            public double Yaw;
            public double GpsTime;

            public double RcLatitude;
            public double RcLongitude;
            public double HomeLongitude;
            public double HomeLatitude;

            public int DeviceType;
            public int UuidLen;
            public byte[] Uuid;
            public ushort Crc;
        }

        private static readonly byte[] MagicBytes = { 0x00, 0xB2, 0x0E, 0x00 };

        private PacketData ParsePacket(byte[] data, int offset, int length)
        {
            if (length - offset < 4) return null;

            if (data[offset + 0] != MagicBytes[0] ||
                data[offset + 1] != MagicBytes[1] ||
                data[offset + 2] != MagicBytes[2] ||
                data[offset + 3] != MagicBytes[3])
                return null;

            int pos = offset + 8;
            uint dataSize = BitConverter.ToUInt32(data, offset + 4);
            if (dataSize > 4096) return null;

            var packet = new PacketData();
            packet.MagicNum = 0x00B20E00;
            packet.DataSize = dataSize;

            packet.DataType = BitConverter.ToUInt32(data, pos); pos += 4;
            packet.LenPack = BitConverter.ToInt32(data, pos); pos += 4;
            packet.ZeroByte = BitConverter.ToInt32(data, pos); pos += 4;
            packet.Version = BitConverter.ToInt32(data, pos); pos += 4;
            packet.SequenceNum = BitConverter.ToUInt16(data, pos); pos += 2;
            packet.StateInfo = BitConverter.ToUInt16(data, pos); pos += 2;
            packet.SerialNum = Encoding.ASCII.GetString(data, pos, 17).Split('\0')[0];
            pos += 17;

            int rawLon = BitConverter.ToInt32(data, pos); pos += 4;
            int rawLat = BitConverter.ToInt32(data, pos); pos += 4;
            packet.Longitude = rawLon / 1e7;
            packet.Latitude = rawLat / 1e7;

            short rawAlt = BitConverter.ToInt16(data, pos); pos += 2;
            packet.Altitude = rawAlt / 100.0;
            short rawHeight = BitConverter.ToInt16(data, pos); pos += 2;
            packet.Height = rawHeight / 100.0;

            short rawVNorth = BitConverter.ToInt16(data, pos); pos += 2;
            packet.VNorth = rawVNorth / 100.0;
            short rawVEast = BitConverter.ToInt16(data, pos); pos += 2;
            packet.VEast = rawVEast / 100.0;
            short rawVUp = BitConverter.ToInt16(data, pos); pos += 2;
            packet.VUp = rawVUp / 100.0;

            short rawYaw = BitConverter.ToInt16(data, pos); pos += 2;
            packet.Yaw = rawYaw / 100.0;

            ulong rawTime = BitConverter.ToUInt64(data, pos); pos += 8;
            packet.GpsTime = rawTime / 1000.0;

            int rawRcLat = BitConverter.ToInt32(data, pos); pos += 4;
            int rawRcLon = BitConverter.ToInt32(data, pos); pos += 4;
            int rawHomeLon = BitConverter.ToInt32(data, pos); pos += 4;
            int rawHomeLat = BitConverter.ToInt32(data, pos); pos += 4;

            packet.RcLatitude = rawRcLat / 1e7;
            packet.RcLongitude = rawRcLon / 1e7;
            packet.HomeLongitude = rawHomeLon / 1e7;
            packet.HomeLatitude = rawHomeLat / 1e7;

            packet.DeviceType = BitConverter.ToInt32(data, pos); pos += 4;
            packet.UuidLen = BitConverter.ToInt32(data, pos); pos += 4;
            packet.Uuid = new byte[20];
            Array.Copy(data, pos, packet.Uuid, 0, 20);
            pos += 20;
            packet.Crc = BitConverter.ToUInt16(data, pos); pos += 2;

            return packet;
        }

        private bool TryParsePacketFromBuffer()
        {
            if (_receiveBuffer.Count < 8) return false;

            int magicIndex = -1;
            for (int i = 0; i <= _receiveBuffer.Count - 4; i++)
            {
                if (_receiveBuffer[i] == 0x00 && _receiveBuffer[i + 1] == 0xB2 &&
                    _receiveBuffer[i + 2] == 0x0E && _receiveBuffer[i + 3] == 0x00)
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
            if (magicIndex > 0) _receiveBuffer.RemoveRange(0, magicIndex);
            if (_receiveBuffer.Count < 8) return false;

            uint dataSize = BitConverter.ToUInt32(_receiveBuffer.ToArray(), 4);
            if (dataSize < 80 || dataSize > 4096)
            {
                _receiveBuffer.RemoveRange(0, 1);
                return false;
            }

            int totalPacketLength = 8 + (int)dataSize;
            if (_receiveBuffer.Count < totalPacketLength) return false;

            var packet = ParsePacket(_receiveBuffer.ToArray(), 0, totalPacketLength);
            if (packet != null)
            {
                UpdateDataGrid(packet);
            }

            _receiveBuffer.RemoveRange(0, totalPacketLength);
            return true;
        }

        // 更新 DataGridView 显示
        private void UpdateDataGrid(PacketData packet)
        {
            if (dataGridView.InvokeRequired)
            {
                dataGridView.Invoke(new Action(() => UpdateDataGrid(packet)));
                return;
            }

            dataGridView.Rows.Clear();
            dataGridView.Rows.Add("MagicNum", $"0x{packet.MagicNum:X8}");
            dataGridView.Rows.Add("DataSize", packet.DataSize);
            dataGridView.Rows.Add("DataType", $"0x{packet.DataType:X8}");
            dataGridView.Rows.Add("LenPack", packet.LenPack);
            dataGridView.Rows.Add("ZeroByte", packet.ZeroByte);
            dataGridView.Rows.Add("Version", packet.Version);
            dataGridView.Rows.Add("SequenceNum", packet.SequenceNum);
            dataGridView.Rows.Add("StateInfo", $"0x{packet.StateInfo:X4}");
            dataGridView.Rows.Add("SerialNum", packet.SerialNum);
            dataGridView.Rows.Add("Longitude", packet.Longitude);
            dataGridView.Rows.Add("Latitude", packet.Latitude);
            dataGridView.Rows.Add("Altitude", packet.Altitude);
            dataGridView.Rows.Add("Height", packet.Height);
            dataGridView.Rows.Add("VNorth", packet.VNorth);
            dataGridView.Rows.Add("VEast", packet.VEast);
            dataGridView.Rows.Add("VUp", packet.VUp);
            dataGridView.Rows.Add("Yaw", packet.Yaw);
            dataGridView.Rows.Add("GpsTime", packet.GpsTime);
            dataGridView.Rows.Add("RcLatitude", packet.RcLatitude);
            dataGridView.Rows.Add("RcLongitude", packet.RcLongitude);
            dataGridView.Rows.Add("HomeLongitude", packet.HomeLongitude);
            dataGridView.Rows.Add("HomeLatitude", packet.HomeLatitude);
            dataGridView.Rows.Add("DeviceType", packet.DeviceType);
            dataGridView.Rows.Add("UuidLen", packet.UuidLen);
            if (packet.Uuid != null && packet.UuidLen > 0)
            {
                string uuidHex = string.Join(" ", packet.Uuid.Take(packet.UuidLen).Select(b => b.ToString("X2")));
                dataGridView.Rows.Add("Uuid", uuidHex);
            }
            else
            {
                dataGridView.Rows.Add("Uuid", "(null or zero length)");
            }
            dataGridView.Rows.Add("Crc", $"0x{packet.Crc:X4}");
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
