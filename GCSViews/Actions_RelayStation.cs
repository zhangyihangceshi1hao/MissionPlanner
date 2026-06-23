// ============================================================
// 无人机数据中继站协议 - Mission Planner 集成界面
// 文件: Actions_RelayStation.cs
// 放置于: MissionPlanner/GCSViews/Actions_RelayStation.cs
//
// 集成方式:
//   1. 将 RelayStation/ 目录整体复制到 MissionPlanner/ 项目根
//   2. 在 VS 解决方案中"添加现有项"引入这 4 个 .cs 文件
//   3. 在 GCSViews/FlightData.cs 的 Action 选项卡区域(tabPageActions)
//      或 Actions.cs 的 tabControl 末尾追加本控件:
//
//      var relayTab = new TabPage("中继站");
//      relayTab.Controls.Add(new Actions_RelayStation { Dock = DockStyle.Fill });
//      tabControl1.TabPages.Add(relayTab);
//
// ============================================================

using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using MissionPlanner.Controls;
using MissionPlanner.RelayStation;

namespace MissionPlanner.GCSViews
{
    public partial class Actions_RelayStation : MyUserControl
    {
        // ── 通信层实例 ──────────────────────────────────────
        private RelayUdpClient _relay = new RelayUdpClient();
        private uint           _wpSeq = 0;

        // ── 控件引用（由 InitializeComponent 创建）──────────
        private GroupBox  grpConn, grpControl, grpWaypoint, grpStatus;
        private TextBox   txtIp, txtPort, txtUavId, txtLog;
        private Button    btnConnect, btnDisconnect;
        private ComboBox  cmbCtrlType, cmbMode;
        private NumericUpDown numTakeoffAlt, numLon, numLat, numRelAlt;
        private Button    btnSendCmd;
        private DataGridView dgvWaypoints;
        private Button    btnAddWp, btnDelWp, btnSendWps, btnClearWps;
        private Label     lblConnStatus, lblArmStatus, lblMode, lblBatt,
                          lblLat, lblLon, lblAlt, lblSpeed, lblSat;
        private System.Windows.Forms.Timer _uiRefreshTimer;

        // ── 最新回传数据缓存 ─────────────────────────────────
        private BasicAttitude _lastAttitude;
        private NavData       _lastNav;
        private StatusData    _lastStatus;
        private BatteryData   _lastBattery;
        private bool          _hasAttitude, _hasNav, _hasStatus, _hasBattery;

        public Actions_RelayStation()
        {
            InitializeRelayComponent();
            WireRelayEvents();
        }

        // ════════════════════════════════════════════════════
        //  界面初始化
        // ════════════════════════════════════════════════════
        private void InitializeRelayComponent()
        {
            this.SuspendLayout();
            this.AutoScroll = true;

            // ── 连接配置组 ──────────────────────────────────
            grpConn = MakeGroup("连接配置", 4, 4, 620, 80);
            txtIp   = MakeText(grpConn, "目标IP", 70, "192.168.1.100", 20, 20, 140);
            txtPort = MakeText(grpConn, "端口",   230, "14501",         20, 20, 60);
            txtUavId = MakeText(grpConn, "无人机编号", 310, "1",          20, 20, 40);
            btnConnect    = MakeBtn(grpConn, "连接",    Color.FromArgb(46,125,50),  370, 18, 80, 28);
            btnDisconnect = MakeBtn(grpConn, "断开",    Color.FromArgb(183,28,28),  460, 18, 80, 28);
            btnDisconnect.Enabled = false;
            lblConnStatus = MakeLabel(grpConn, "● 未连接", 550, 24, Color.Gray);

            btnConnect.Click    += BtnConnect_Click;
            btnDisconnect.Click += BtnDisconnect_Click;

            // ── 控制指令组 ──────────────────────────────────
            grpControl = MakeGroup("控制指令", 4, 92, 620, 130);

            MakeLabel(grpControl, "控制类型:", 8, 24);
            cmbCtrlType = new ComboBox { Left=80, Top=20, Width=120,
                DropDownStyle=ComboBoxStyle.DropDownList };
            cmbCtrlType.Items.AddRange(new[]
            {
                "00 - 闭锁", "01 - 解锁", "02 - 起飞", "03 - 模式切换", "04 - 指点飞行"
            });
            cmbCtrlType.SelectedIndex = 0;
            grpControl.Controls.Add(cmbCtrlType);

            MakeLabel(grpControl, "飞行模式:", 210, 24);
            cmbMode = new ComboBox { Left=280, Top=20, Width=110,
                DropDownStyle=ComboBoxStyle.DropDownList };
            cmbMode.Items.AddRange(new[]
            {
                "00 - 引导", "01 - 自动", "02 - 定点", "03 - 返航"
            });
            cmbMode.SelectedIndex = 0;
            grpControl.Controls.Add(cmbMode);

            numTakeoffAlt = MakeNum(grpControl, "起飞高度(m):", 8,  60, 0,  500, 10);
            numLon        = MakeNum(grpControl, "经度(×1e7):",  210, 60, -1800000000, 1800000000, 0);
            numLat        = MakeNum(grpControl, "纬度(×1e7):",  8,  90, -900000000, 900000000, 0);
            numRelAlt     = MakeNum(grpControl, "相对高度(m):", 210, 90, -500, 10000, 0);

            btnSendCmd = MakeBtn(grpControl, "发送指令",
                Color.FromArgb(21, 101, 192), 500, 55, 110, 60);
            btnSendCmd.Font    = new Font("Microsoft YaHei", 10f, FontStyle.Bold);
            btnSendCmd.Enabled = false;
            btnSendCmd.Click  += BtnSendCmd_Click;

            // ── 航线上传组 ──────────────────────────────────
            grpWaypoint = MakeGroup("航线上传", 4, 230, 620, 190);

            dgvWaypoints = new DataGridView
            {
                Left = 4, Top = 20, Width = 510, Height = 155,
                AllowUserToAddRows = false,
                RowHeadersVisible = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.FromArgb(30, 30, 30),
                GridColor = Color.FromArgb(70, 70, 70),
                DefaultCellStyle = { BackColor = Color.FromArgb(40,40,40),
                                     ForeColor = Color.White },
                ColumnHeadersDefaultCellStyle = {
                    BackColor = Color.FromArgb(60,60,60), ForeColor = Color.LightGray }
            };
            dgvWaypoints.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "经度(°)", Name = "Lon", Width = 100 });
            dgvWaypoints.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "纬度(°)", Name = "Lat", Width = 100 });
            dgvWaypoints.Columns.Add(new DataGridViewComboBoxColumn
            {
                HeaderText = "高度类型", Name = "AltType", Width = 80,
                Items = { "相对高度", "绝对高度" }, DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            });
            dgvWaypoints.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "高度(m)", Name = "Alt", Width = 80 });
            grpWaypoint.Controls.Add(dgvWaypoints);

            btnAddWp   = MakeBtn(grpWaypoint, "添加",  Color.FromArgb(56,142,60), 520, 20, 90, 28);
            btnDelWp   = MakeBtn(grpWaypoint, "删除",  Color.FromArgb(183,28,28), 520, 55, 90, 28);
            btnSendWps = MakeBtn(grpWaypoint, "上传航线", Color.FromArgb(21,101,192), 520, 100, 90, 40);
            btnClearWps = MakeBtn(grpWaypoint, "清空",  Color.FromArgb(80,80,80), 520, 147, 90, 28);

            btnAddWp.Click   += (s, e) => AddWaypointRow();
            btnDelWp.Click   += (s, e) => DeleteSelectedRows();
            btnSendWps.Click += BtnSendWps_Click;
            btnClearWps.Click += (s, e) => dgvWaypoints.Rows.Clear();
            foreach (Button b in new[] { btnSendWps })
                b.Enabled = false;

            // ── 状态显示组 ──────────────────────────────────
            grpStatus = MakeGroup("无人机状态（实时回传）", 4, 428, 620, 165);

            int sx = 8, sy = 22, sw = 190;
            lblArmStatus = MakeStatus(grpStatus, "解锁状态: --",     sx,      sy);
            lblMode      = MakeStatus(grpStatus, "飞行模式: --",     sx+sw,   sy);
            lblBatt      = MakeStatus(grpStatus, "电池电压: -- V",   sx+sw*2, sy);
            lblLat       = MakeStatus(grpStatus, "纬度: --",         sx,      sy+30);
            lblLon       = MakeStatus(grpStatus, "经度: --",         sx+sw,   sy+30);
            lblAlt       = MakeStatus(grpStatus, "相对高度: -- m",   sx+sw*2, sy+30);
            lblSpeed     = MakeStatus(grpStatus, "水平速度: -- m/s", sx,      sy+60);
            lblSat       = MakeStatus(grpStatus, "卫星数: --",       sx+sw,   sy+60);

            // 日志框
            txtLog = new TextBox
            {
                Left = 4, Top = sy + 90, Width = 610, Height = 60,
                Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(20, 20, 20), ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 8f)
            };
            grpStatus.Controls.Add(txtLog);

            // ── UI 刷新定时器 ──────────────────────────────
            _uiRefreshTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _uiRefreshTimer.Tick += RefreshStatusDisplay;
            _uiRefreshTimer.Start();

            this.Controls.AddRange(new Control[] { grpConn, grpControl, grpWaypoint, grpStatus });
            this.ResumeLayout();
        }

        // ════════════════════════════════════════════════════
        //  事件绑定
        // ════════════════════════════════════════════════════
        private void WireRelayEvents()
        {
            _relay.OnBasicAttitude += att =>
            {
                _lastAttitude = att; _hasAttitude = true;
            };
            _relay.OnNavData += nav =>
            {
                _lastNav = nav; _hasNav = true;
            };
            _relay.OnStatusData += st =>
            {
                _lastStatus = st; _hasStatus = true;
            };
            _relay.OnBatteryData += bat =>
            {
                _lastBattery = bat; _hasBattery = true;
            };
            _relay.OnLog += msg => AppendLog(msg);
        }

        // ════════════════════════════════════════════════════
        //  按钮事件
        // ════════════════════════════════════════════════════
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (!IPAddress.TryParse(txtIp.Text.Trim(), out _))
            {
                AppendLog("[错误] 无效 IP 地址");
                return;
            }
            _relay.RemoteIp   = txtIp.Text.Trim();
            _relay.RemotePort = int.TryParse(txtPort.Text.Trim(), out int p) ? p : 14501;
            _relay.UavId      = byte.TryParse(txtUavId.Text.Trim(), out byte uid) ? uid : (byte)1;
            _relay.Start();

            btnConnect.Enabled    = false;
            btnDisconnect.Enabled = true;
            btnSendCmd.Enabled    = true;
            btnSendWps.Enabled    = true;
            lblConnStatus.Text    = "● 已连接";
            lblConnStatus.ForeColor = Color.LimeGreen;
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            _relay.Stop();
            btnConnect.Enabled    = true;
            btnDisconnect.Enabled = false;
            btnSendCmd.Enabled    = false;
            btnSendWps.Enabled    = false;
            lblConnStatus.Text    = "● 未连接";
            lblConnStatus.ForeColor = Color.Gray;
        }

        private void BtnSendCmd_Click(object sender, EventArgs e)
        {
            var cmd = new ControlCmd
            {
                ControlType = (byte)cmbCtrlType.SelectedIndex,
                ModeType    = (byte)cmbMode.SelectedIndex,
                TakeoffAlt  = (short)((double)numTakeoffAlt.Value * 100),
                Longitude   = (int)numLon.Value,
                Latitude    = (int)numLat.Value,
                RelAltitude = (short)((double)numRelAlt.Value * 100)
            };
            _relay.SendControlCmd(cmd);
        }

        private void BtnSendWps_Click(object sender, EventArgs e)
        {
            var wps = ParseWaypointGrid();
            if (wps == null || wps.Length == 0)
            {
                AppendLog("[警告] 航线为空，请先添加航点");
                return;
            }
            _relay.SendWaypoints(wps);
        }

        // ════════════════════════════════════════════════════
        //  航线表格操作
        // ════════════════════════════════════════════════════
        private void AddWaypointRow()
        {
            int idx = dgvWaypoints.Rows.Add();
            dgvWaypoints.Rows[idx].Cells["Lon"].Value     = "0.0000000";
            dgvWaypoints.Rows[idx].Cells["Lat"].Value     = "0.0000000";
            dgvWaypoints.Rows[idx].Cells["AltType"].Value = "相对高度";
            dgvWaypoints.Rows[idx].Cells["Alt"].Value     = "50";
        }

        private void DeleteSelectedRows()
        {
            foreach (DataGridViewRow row in dgvWaypoints.SelectedRows)
                if (!row.IsNewRow) dgvWaypoints.Rows.Remove(row);
        }

        private WaypointItem[] ParseWaypointGrid()
        {
            var list = new System.Collections.Generic.List<WaypointItem>();
            foreach (DataGridViewRow row in dgvWaypoints.Rows)
            {
                if (row.IsNewRow) continue;
                try
                {
                    double lon = double.Parse(row.Cells["Lon"].Value?.ToString() ?? "0");
                    double lat = double.Parse(row.Cells["Lat"].Value?.ToString() ?? "0");
                    double alt = double.Parse(row.Cells["Alt"].Value?.ToString() ?? "0");
                    byte atype = (row.Cells["AltType"].Value?.ToString() == "绝对高度") ? (byte)1 : (byte)0;
                    list.Add(new WaypointItem
                    {
                        Longitude = (int)(lon * 1e7),
                        Latitude  = (int)(lat * 1e7),
                        AltType   = atype,
                        Altitude  = (int)(alt * 100)  // *100 单位cm
                    });
                }
                catch { AppendLog($"[警告] 第{row.Index + 1}行数据格式错误，已跳过"); }
            }
            return list.ToArray();
        }

        // ════════════════════════════════════════════════════
        //  状态刷新（500ms 定时器）
        // ════════════════════════════════════════════════════
        private void RefreshStatusDisplay(object sender, EventArgs e)
        {
            if (!_relay.IsRunning) return;

            if (_hasAttitude)
            {
                lblArmStatus.Text    = $"解锁状态: {(_lastAttitude.ArmStatus == 1 ? "已解锁" : "未解锁")}";
                lblArmStatus.ForeColor = _lastAttitude.ArmStatus == 1 ? Color.LimeGreen : Color.OrangeRed;

                string[] modeNames = { "引导", "自动", "定点", "返航", "其它" };
                int modeIdx = Math.Min(_lastAttitude.ModeType, (byte)(modeNames.Length - 1));
                lblMode.Text = $"飞行模式: {modeNames[modeIdx]}";

                double voltage = _lastAttitude.BattVoltage / 100.0;
                lblBatt.Text = $"电池电压: {voltage:F2} V";
                lblBatt.ForeColor = voltage < 14.0 ? Color.OrangeRed : Color.White;

                double lat = _lastAttitude.Latitude  / 1e7;
                double lon = _lastAttitude.Longitude / 1e7;
                lblLat.Text = $"纬度: {lat:F7}°";
                lblLon.Text = $"经度: {lon:F7}°";

                double relAlt = _lastAttitude.RelAltitude / 100.0;
                lblAlt.Text = $"相对高度: {relAlt:F1} m";

                double hSpd = _lastAttitude.HorizSpeed / 100.0;
                lblSpeed.Text = $"水平速度: {hSpd:F2} m/s";
            }

            if (_hasNav)
                lblSat.Text = $"卫星数: {_lastNav.SatCount} (GPS:{GpsStatusStr(_lastNav.GpsStatus)})";
        }

        private static string GpsStatusStr(byte s) =>
            s == 0 ? "无信号" : s == 1 ? "2D定位" : s == 2 ? "3D定位" : "RTK";

        // ════════════════════════════════════════════════════
        //  日志追加（线程安全）
        // ════════════════════════════════════════════════════
        private void AppendLog(string msg)
        {
            if (txtLog.IsDisposed) return;
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke((Action)(() => AppendLog(msg)));
                return;
            }
            string line = $"[{DateTime.Now:HH:mm:ss}] {msg}\r\n";
            txtLog.AppendText(line);
            if (txtLog.Lines.Length > 200)
                txtLog.Clear();
        }

        // ════════════════════════════════════════════════════
        //  UI 辅助工厂方法
        // ════════════════════════════════════════════════════
        private GroupBox MakeGroup(string title, int x, int y, int w, int h)
        {
            var g = new GroupBox
            {
                Text = title, Left = x, Top = y, Width = w, Height = h,
                ForeColor = Color.LightGray,
                Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold)
            };
            this.Controls.Add(g);
            return g;
        }

        private TextBox MakeText(Control parent, string label, int labelX, string defVal,
                                 int y, int height, int width)
        {
            parent.Controls.Add(new Label
            {
                Text = label + ":", Left = labelX - 60, Top = y + 3,
                Width = 58, ForeColor = Color.LightGray, TextAlign = ContentAlignment.MiddleRight
            });
            var t = new TextBox { Left = labelX, Top = y, Width = width, Height = height, Text = defVal };
            parent.Controls.Add(t);
            return t;
        }

        private Button MakeBtn(Control parent, string text, Color backColor,
                               int x, int y, int w, int h)
        {
            var b = new Button
            {
                Text = text, Left = x, Top = y, Width = w, Height = h,
                BackColor = backColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei", 9f)
            };
            b.FlatAppearance.BorderSize = 0;
            parent.Controls.Add(b);
            return b;
        }

        private NumericUpDown MakeNum(Control parent, string label, int labelX, int y,
                                      decimal min, decimal max, decimal val)
        {
            parent.Controls.Add(new Label
            {
                Text = label, Left = labelX, Top = y + 3, Width = 100, ForeColor = Color.LightGray
            });
            var n = new NumericUpDown
            {
                Left = labelX + 102, Top = y, Width = 90,
                Minimum = min, Maximum = max, Value = val, DecimalPlaces = 0
            };
            parent.Controls.Add(n);
            return n;
        }

        private Label MakeLabel(Control parent, string text, int x, int y,
                                Color? color = null)
        {
            var l = new Label
            {
                Text = text, Left = x, Top = y, AutoSize = true,
                ForeColor = color ?? Color.LightGray
            };
            parent.Controls.Add(l);
            return l;
        }

        private Label MakeStatus(Control parent, string text, int x, int y)
        {
            var l = new Label
            {
                Text = text, Left = x, Top = y, Width = 185, AutoSize = false,
                ForeColor = Color.White,
                Font = new Font("Consolas", 9f)
            };
            parent.Controls.Add(l);
            return l;
        }

        // ════════════════════════════════════════════════════
        //  清理
        // ════════════════════════════════════════════════════
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _uiRefreshTimer?.Stop();
                _relay?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
