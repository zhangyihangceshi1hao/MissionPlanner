using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MissionPlanner.GCSViews.FlightData;

namespace MissionPlanner.GCSViews
{
    public partial class UavStatus : Form
    {
        private FlowLayoutPanel flowPanel;
        private Timer refreshTimer;
        private Dictionary<string, Panel> dronePanels = new Dictionary<string, Panel>();
        //private FlowLayoutPanel flowPanel;
        public UavStatus()
        {
            InitializeComponent();
            InitializeCustomComponents();
            InitializeTimer();
            LoadInitialData();
        }
        private void InitializeTimer()
        {
            refreshTimer = new Timer { Interval = 1000 }; // 每秒刷新一次
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }

        private void LoadInitialData()
        {
            UpdateDroneList();
        }
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateDroneList();
        }
        System.Timers.Timer countdown = new System.Timers.Timer { Interval = 10000, AutoReset = true };
        private void InitializeCustomComponents()
        {
            // 创建 FlowLayoutPanel
            flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.TopDown
            };

            this.Controls.Add(flowPanel);

            //// 添加示例数据
            //var drones = new List<DroneData>();


            //foreach (var port in MainV2.Comports)
            //{
            //    foreach (var mav in port.MAVlist.OrderBy(m => m.sysid))
            //    {
            //        drones.Add(new DroneData
            //        {
            //            Id = mav.sysid + "",
            //            Arm = mav.cs.armed,
            //            Latitude = mav.cs.lat,
            //            Longitude = mav.cs.lng,
            //            Altitude = mav.cs.alt,
            //            AbsoluteAltitude = mav.cs.altasl,
            //            FlightMode = mav.cs.mode,
            //            GpsStatus = mav.cs.gpsstatus + ""
            //        });
            //    }
            //}
            //foreach (var drone in drones)
            //{
            //    var panel = CreateDroneInfoPanel(drone);
            //    flowPanel.Controls.Add(panel);
            //}

        }
        private Panel CreateDroneInfoPanel(DroneData drone)
        {
            var panel = new Panel
            {
                Width = 110,
                MinimumSize = new Size(90, 130),
                Padding = new Padding(10),
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = SystemColors.ControlLight
            };

            var label = new Label
            {
                AutoSize = true,
                Text = $"ID: {drone.Id}\n解锁: {drone.Arm}\n纬度: {drone.Latitude:F4}\n经度: {drone.Longitude:F4}\n海拔: {drone.Altitude:F2} m\n绝对高度: {drone.AbsoluteAltitude:F2} m\n模式: {drone.FlightMode}\nGPS数量: {drone.GpsStatus}",
                Font = new Font("微软雅黑", 9F)
            };

            panel.Controls.Add(label);
            return panel;
        }

        // 数据模型类
        public class DroneData
        {
            public string Id { get; set; }
            public bool Arm { get; set; }
            public double Latitude { get; set; }      // 纬度
            public double Longitude { get; set; }     // 经度
            public double Altitude { get; set; }      // 海拔高度（AMSL）
            public double AbsoluteAltitude { get; set; }  // 绝对高度（可能是相对于 home 点的高度）
            public string FlightMode { get; set; }    // 当前飞行模式（如 RTL、GUIDED、AUTO 等）
            public string GpsStatus { get; set; }     // GPS 状态（如 "3D Fix", "No Fix"）
        }

        public void UpdateDroneList()
        {
            var currentDrones = new Dictionary<string, DroneData>();

            // 收集当前所有无人机数据，并按 sysid 排序
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist.OrderBy(m => m.sysid))
                {
                    string key = mav.sysid.ToString();
                    currentDrones[key] = new DroneData
                    {
                        Id = key,
                        Arm = mav.cs.armed,
                        Latitude = mav.cs.lat,
                        Longitude = mav.cs.lng,
                        Altitude = mav.cs.alt,
                        AbsoluteAltitude = mav.cs.altasl,
                        FlightMode = mav.cs.mode,
                        GpsStatus = mav.cs.gpsstatus.ToString()
                    };
                }
            }
            // 更新或添加新的无人机信息
            foreach (var kvp in currentDrones.OrderBy(k => k.Key))
            {
                string key = kvp.Key;
                var drone = kvp.Value;

                if (dronePanels.TryGetValue(key, out Panel panel))
                {
                    if (panel.Controls.Count > 0)
                    {
                        // 更新已有 Panel 中的 Label 内容
                        var label = (Label)panel.Controls[0];
                        label.Text = $"ID: {drone.Id}\n解锁: {drone.Arm}\n纬度: {drone.Latitude:F6}\n经度: {drone.Longitude:F6}\n海拔: {drone.Altitude:F2} m\n绝对高度: {drone.AbsoluteAltitude:F2} m\n模式: {drone.FlightMode}\nGPS数量: {drone.GpsStatus}";
                    }
                    }
                else
                {
                    // 创建新 Panel 并加入字典和界面中
                    panel = CreateDroneInfoPanel(drone);
                    dronePanels[key] = panel;
                    flowPanel.Controls.Add(panel);
                }
            }
        }


    }
}
