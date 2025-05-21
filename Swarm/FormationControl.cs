using MissionPlanner.Utilities;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using static IronPython.Modules._ast;
using static MissionPlanner.GCSViews.FlightPlanner;
using System.Globalization;
using System.IO;
using static MissionPlanner.Swarm.Grid;
using static MissionPlanner.Utilities.MissionFile;
using System.Linq;
using Newtonsoft.Json;
using Accord.Imaging.Filters;
using static Microsoft.Scripting.Hosting.Shell.ConsoleHostOptions;
using System.Text.RegularExpressions;
using netDxf.Entities;
using static MissionPlanner.Utilities.LTM;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading;
using static MAVLink;
using static MissionPlanner.GCSViews.FlightData;
using System.Threading.Tasks;
using MissionPlanner.Utilities.nfz;
using Org.BouncyCastle.Asn1.Pkcs;
using netDxf.Collections;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using Org.BouncyCastle.Ocsp;
using static IronPython.Modules.PythonWeakRef;
using NetTopologySuite.Utilities;
using System.Timers;
using static MissionPlanner.Swarm.FormationControl;


namespace MissionPlanner.Swarm
{
    public partial class FormationControl : Form
    {
     
        Formation SwarmInterface = null;
        bool threadrun = false;
        public bool Vertical { get; set; }
        public FormationControl()
        {

            InitializeComponent();

            SwarmInterface = new Formation();

            TopMost = true;
        
            Dictionary<String, MAVState> mavStates = new Dictionary<string, MAVState>();

            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    mavStates.Add(port.BaseStream.PortName + " " + mav.sysid + " " + mav.compid, mav);
                }
            }

            if (mavStates.Count == 0)
                return;

            bindingSource1.DataSource = mavStates;

            CMB_mavs.DataSource = bindingSource1;
            CMB_mavs.ValueMember = "Value";
            CMB_mavs.DisplayMember = "Key";

            bindingSource2.DataSource = mavStates;

            comboBox1.DataSource = bindingSource2;
            comboBox1.ValueMember = "Value";
            comboBox1.DisplayMember = "Key";
            textBox4.Text = "0";

            updateicons();

            this.MouseWheel += new MouseEventHandler(FollowLeaderControl_MouseWheel);

            MessageBox.Show("this is beta, use at own risk");

            MissionPlanner.Utilities.Tracking.AddPage(this.GetType().ToString(), this.Text);

            countdown.Elapsed += sendheartbeat;
            
            textBox_uav1.Text = "0,0,0";
            textBox_uav2.Text = "0,0,0";
            textBox_uav3.Text = "0,0,0";
            textBox_uav4.Text = "0,0,0";
            textBox_uav5.Text = "0,0,0";
            textBox_uav6.Text = "0,0,0";
            textBox_uav7.Text = "0,0,0";
            textBox_uav8.Text = "0,0,0";
            textBox_uav9.Text = "0,0,0";
            textBox_uav10.Text = "0,0,0";
            textBox_uav11.Text = "0,0,0";
            textBox_uav12.Text = "0,0,0";
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    drone2SecondsAgos.Add(new Drone2SecondsAgo(mav.sysid,mav.cs.alt,mav.cs.roll,mav.cs.pitch));
                }
            }
           
           countdown2.Elapsed += updatedrone2SecondsAgos;
            countdown2.Start();
        }
        System.Timers.Timer countdown = new System.Timers.Timer { Interval = 2000, AutoReset = true };
        void FollowLeaderControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                grid1.setScale(grid1.getScale() + 4);
            }
            else
            {
                grid1.setScale(grid1.getScale() - 4);
            }
        }
        List<Drone2SecondsAgo> drone2SecondsAgos = new List<Drone2SecondsAgo>();
        System.Timers.Timer countdown2 = new System.Timers.Timer { Interval = 10000, AutoReset = true };
        void updateicons()
        {
            bindingSource1.ResetBindings(false);
            bindingSource2.ResetBindings(false);
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.getLeader())
                    {
                        ((Formation)SwarmInterface).setOffsets(mav, 0, 0, 0);
                        var vector = SwarmInterface.getOffsets(mav);
                        grid1.UpdateIcon(mav, (float)vector.x, (float)vector.y, (float)vector.z, false);
                    }
                    else
                    {
                        var vector = SwarmInterface.getOffsets(mav);
                        grid1.UpdateIcon(mav, (float)vector.x, (float)vector.y, (float)vector.z, true);
                    }
                }
            }
            grid1.Invalidate();
        }

        private void CMB_mavs_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_mavs.SelectedValue)
                    {
                        MainV2.comPort = port;
                        port.sysidcurrent = mav.sysid;
                        port.compidcurrent = mav.compid;
                    }
                }
            }
        }

        private void BUT_Start_Click(object sender, EventArgs e)
        {
            if (threadrun == true)
            {
                threadrun = false;
                BUT_Start.Text = Strings.Start;
                return;
            }

            if (SwarmInterface != null)
            {
                new System.Threading.Thread(mainloop) { IsBackground = true }.Start();


                BUT_Start.Text = Strings.Stop;
            }
        }

        void mainloop()
        {
            threadrun = true;

            // make sure leader is high freq updates
            SwarmInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, SwarmInterface.Leader.sysid, SwarmInterface.Leader.compid);
            SwarmInterface.Leader.cs.rateposition = 10;
            SwarmInterface.Leader.cs.rateattitude = 10;

            while (threadrun && !this.IsDisposed)
            {
                // update leader pos
                SwarmInterface.Update();

                // update other mavs
                SwarmInterface.SendCommand();

                // 10 hz
                System.Threading.Thread.Sleep(100);
            }


        }

        private void BUT_Arm_Click(object sender, EventArgs e)
        {


            if (CustomMessageBox.Show("Are you sure you want to Arm", "提示",
                    CustomMessageBox.MessageBoxButtons.YesNo) !=
                CustomMessageBox.DialogResult.Yes)
                return;

            if (SwarmInterface != null)
            {
                SwarmInterface.Arm_ALL(Vertical);
            }
        }

        private void BUT_Disarm_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Disarm_ALL(Vertical);
            }
        }

        private void BUT_Takeoff_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Takeoff_ALL(Vertical);
            }
        }

        private void BUT_Land_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Land_ALL(Vertical);
            }
        }

        private void BUT_leader_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                var vectorlead = SwarmInterface.getOffsets(MainV2.comPort.MAV);

                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        var vector = SwarmInterface.getOffsets(mav);

                        SwarmInterface.setOffsets(mav, (float)(vector.x - vectorlead.x),
                            (float)(vector.y - vectorlead.y),
                            (float)(vector.z - vectorlead.z));
                    }
                }

                SwarmInterface.setLeader(MainV2.comPort.MAV);
                updateicons();
                BUT_Start.Enabled = true;
                BUT_Updatepos.Enabled = true;
                savePoint.Enabled = true;
                loadPoint.Enabled = true;



            }
        }

        private void BUT_connect_Click(object sender, EventArgs e)
        {
            Comms.CommsSerialScan.Scan(true);

            DateTime deadline = DateTime.Now.AddSeconds(50);

            while (Comms.CommsSerialScan.foundport == false)
            {
                System.Threading.Thread.Sleep(100);

                if (DateTime.Now > deadline)
                {
                    CustomMessageBox.Show("Timeout waiting for autoscan/no mavlink device connected");
                    return;
                }
            }

            bindingSource1.ResetBindings(false);
            bindingSource2.ResetBindings(false);
        }

        public Vector3 getOffsetFromLeader(MAVState leader, MAVState mav)
        {
            //convert Wgs84ConversionInfo to utm
            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();

            IGeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;

            int utmzone = (int)((leader.cs.lng - -186.0) / 6.0);

            IProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(utmzone,
                leader.cs.lat < 0 ? false : true);

            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84, utm);

            double[] masterpll = { leader.cs.lng, leader.cs.lat };

            // get leader utm coords
            double[] masterutm = trans.MathTransform.Transform(masterpll);

            double[] mavpll = { mav.cs.lng, mav.cs.lat };

            //getLeader follower utm coords
            double[] mavutm = trans.MathTransform.Transform(mavpll);

            var heading = -leader.cs.yaw;

            var norotation = new Vector3(masterutm[1] - mavutm[1], masterutm[0] - mavutm[0], 0);

            norotation.x *= -1;
            norotation.y *= -1;

            return new Vector3(norotation.x * Math.Cos(heading * MathHelper.deg2rad) - norotation.y * Math.Sin(heading * MathHelper.deg2rad), norotation.x * Math.Sin(heading * MathHelper.deg2rad) + norotation.y * Math.Cos(heading * MathHelper.deg2rad), 0);
        }

        private void grid1_UpdateOffsets(MAVState mav, float x, float y, float z, Grid.icon ico)
        {
            if (mav == SwarmInterface.Leader)
            {
                CustomMessageBox.Show("Can not move Leader");
                ico.z = 0;
            }
            else
            {
                ((Formation)SwarmInterface).setOffsets(mav, x, y, z);
            }
        }

        private void Control_FormClosing(object sender, FormClosingEventArgs e)
        {
            threadrun = false;
        }

        private void BUT_Updatepos_Click(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    mav.cs.UpdateCurrentSettings(null, true, port, mav);

                    if (mav == SwarmInterface.Leader)
                        continue;

                    Vector3 offset = getOffsetFromLeader(((Formation)SwarmInterface).getLeader(), mav);

                    if (Math.Abs(offset.x) < 1000 && Math.Abs(offset.y) < 1000)
                    {
                        //此处是控制页面的icon位置



                        //此处的y,x，z比较特殊，其余的是x,y,z
                        grid1.UpdateIcon(mav, (float)offset.y, (float)offset.x, (float)offset.z, true);
                        ((Formation)SwarmInterface).setOffsets(mav, offset.y, offset.x, offset.z);
                    }
                }
            }
        }
        static bool IsPositiveNumber(string input)
        {

            string pattern = @"^[+-]?\d+(\.\d+)?$"; // 允许正负号，并允许小数部分
            return Regex.IsMatch(input, pattern);
        }

        private void BUT_UpdateOption_pos_Click(object sender, EventArgs e)
        {



            bool isGreaterThanZero1 = IsPositiveNumber(textBox1.Text);
            bool isGreaterThanZero2 = IsPositiveNumber(textBox2.Text);
            bool isGreaterThanZero3 = IsPositiveNumber(textBox3.Text);
            if (!isGreaterThanZero1)
            {
                MessageBox.Show("请在x轴输入非空数字");
                return;
            }
            if (!isGreaterThanZero2)
            {
                MessageBox.Show("请在y轴输入非空数字");
                return;
            }
            if (!isGreaterThanZero3)
            {
                MessageBox.Show("请在z轴输入非空数字");
                return;
            }

            float selectedNumber2 = float.Parse(textBox1.Text);
            float selectedNumber3 = float.Parse(textBox2.Text);
            float selectedNumber4 = float.Parse(textBox3.Text);
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {

                    if (mav == comboBox1.SelectedValue)
                    {
                        if (mav == SwarmInterface.Leader)
                            continue;






                        grid1.UpdateIcon(mav, selectedNumber2, selectedNumber3, selectedNumber4, true);
                        ((Formation)SwarmInterface).setOffsets(mav, selectedNumber2, selectedNumber3, selectedNumber4);
                    }
                }
            }




        }

        private void timer_status_Tick(object sender, EventArgs e)
        {
            // clean up old
            foreach (Control ctl in PNL_status.Controls)
            {
                bool match = false;
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (mav == (MAVState)ctl.Tag)
                        {
                            match = true;

                        }
                    }
                }

                if (match == false)
                    ctl.Dispose();
            }

            // setup new
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    bool exists = false;
                    foreach (Control ctl in PNL_status.Controls)
                    {
                        if (ctl is Status && ctl.Tag == mav)
                        {
                            exists = true;
                            ((Status)ctl).GPS.Text = mav.cs.gpsstatus >= 3 ? "OK" : "Bad";
                            ((Status)ctl).Armed.Text = mav.cs.armed.ToString();
                            ((Status)ctl).Mode.Text = mav.cs.mode;
                            ((Status)ctl).MAV.Text = mav.ToString();
                            ((Status)ctl).Guided.Text = mav.GuidedMode.x / 1e7 + "," + mav.GuidedMode.y / 1e7 + "," +
                                                         mav.GuidedMode.z;
                            ((Status)ctl).Location1.Text = mav.cs.lat + ",\n" + mav.cs.lng + ",\n" +
                                                            mav.cs.alt;

                            if (mav == SwarmInterface.Leader)
                            {
                                ((Status)ctl).ForeColor = Color.Red;
                            }
                            else
                            {
                                ((Status)ctl).ForeColor = Color.Black;
                            }
                        }
                    }

                    if (!exists)
                    {
                        Status newstatus = new Status();
                        newstatus.Tag = mav;
                        PNL_status.Controls.Add(newstatus);
                    }
                }
            }
        }

        private void but_guided_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.GuidedMode_ALL(Vertical);
            }
        }

        private void but_auto_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.AutoMode_ALL(Vertical);
            }
        }
        internal string wpfilename;


        public void BUT_LoadPoint_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "All Supported Types|*.txt;*.waypoints;*.shp;*.plan;*.kml";
                if (Directory.Exists(Settings.Instance["WPFileDirectory"] ?? ""))
                    fd.InitialDirectory = Settings.Instance["WPFileDirectory"];
                DialogResult result = fd.ShowDialog();
                string file = fd.FileName;

                if (File.Exists(file))
                {
                    Settings.Instance["WPFileDirectory"] = Path.GetDirectoryName(file);


                    string line = "";
                    using (var fstream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var fs = new StreamReader(fstream))
                    {
                        line = fs.ReadLine();
                    }

                    if (line.StartsWith("{"))
                    {
                        var format = ReadFile(file);

                        var cmds = ConvertToPoint(format);

                        BUT_Updatepos_Click_New(cmds);
                    }
                }
            }
        }



        private void BUT_Updatepos_Click_New(List<new_icon> icons)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    mav.cs.UpdateCurrentSettings(null, true, port, mav);

                    //if (mav == SwarmInterface.Leader)
                    //    continue;

                    Vector3 offset = getOffsetFromLeader(((Formation)SwarmInterface).getLeader(), mav);

                    if (Math.Abs(offset.x) < 1000 && Math.Abs(offset.y) < 1000)
                    {
                        //此处是控制页面的icon位置
                        //grid1.UpdateIcon(mav, (float)offset.y, (float)offset.x, (float)offset.z, true);
                        //((Formation)SwarmInterface).setOffsets(mav, offset.x, offset.y, offset.z);

                        //offset.x = 1+ mav.sysid;
                        //offset.y = 1+ mav.sysid;
                        //offset.z = 1+ mav.sysid;
                        foreach (var icon in icons)
                        {
                            if (icon.interf == mav.sysid)
                            {
                                bool colorIsRed = false;
                                if (icon.Color.Name.Equals("Red"))
                                {
                                    colorIsRed = true;
                                }
                                grid1.UpdateIcon(mav, (float)icon.x, (float)icon.y, (float)icon.z, colorIsRed);
                                ((Formation)SwarmInterface).setOffsets(mav, icon.x, icon.y, icon.z);
                            }
                        }


                    }
                }
            }
        }

        private void BUT_SavePoint_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog fd = new SaveFileDialog())
            {
                fd.Filter = "Mission|*.waypoints;*.txt|Mission JSON|*.mission";
                fd.DefaultExt = ".waypoints";
                fd.InitialDirectory = Settings.Instance["WPFileDirectory"] ?? "";
                fd.FileName = wpfilename;
                DialogResult result = fd.ShowDialog();
                string file = fd.FileName;
                List<new_icon> icons = new List<new_icon>();

                if (file != "" && result == DialogResult.OK)
                {
                    Settings.Instance["WPFileDirectory"] = Path.GetDirectoryName(file);

                    try
                    {

                        if (SwarmInterface != null)
                        {
                            var vectorlead = SwarmInterface.getOffsets(MainV2.comPort.MAV);


                            foreach (var port in MainV2.Comports)
                            {
                                foreach (var mav in port.MAVlist)
                                {

                                    new_icon save_icon = new new_icon();
                                    var vector = SwarmInterface.getOffsets(mav);
                                    save_icon.x = (float)vector.x;
                                    save_icon.y = (float)vector.y;
                                    save_icon.z = (float)vector.z;
                                    if (mav == SwarmInterface.Leader)
                                    {
                                        save_icon.Color = Color.Blue;
                                    }
                                    save_icon.interf = mav.sysid;

                                    icons.Add(save_icon);
                                }
                            }
                        }

                        // Convert icons list to RootObject type
                        var format = ConvertFromIcon(icons);

                        // Now you can save this format as object
                        WriteFile_icon(file, format);
                        return;

                    }
                    catch (Exception)
                    {
                        CustomMessageBox.Show(Strings.ERROR);
                    }
                }
            }
        }
        public static void WriteFile_icon(string filename, RootObject_ICon format)
        {
            var fileout = JsonConvert.SerializeObject(format, Formatting.Indented);

            File.WriteAllText(filename, fileout);
        }
        public class new_icon
        {
            public float x = 0;
            public float y = 0;
            public float z = 10;
            public int icosize = 20;
            public RectangleF bounds = new RectangleF();
            public Color Color = Color.Red;
            public String Name = "";
            public int interf = 0;
            public bool Movable = true;
        }
        public class RootObject_ICon
        {
            public string fileType { get; set; }
            public GeoFence geoFence { get; set; }
            public string groundStation { get; set; }
            public List<new_icon> icon { get; set; }
            public RallyPoints rallyPoints { get; set; }
            public int version { get; set; }
        }
        // Modified ConvertFromIcon method
        public static RootObject_ICon ConvertFromIcon(List<new_icon> list, byte frame = (byte)MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT)
        {
            RootObject_ICon temp = new RootObject_ICon()
            {
                groundStation = "MissionPlanner_ICon",
                version = 1,
                icon = new List<new_icon>()
            };


            temp.icon = list;

            return temp;
        }

        public static List<new_icon> ConvertToPoint(RootObject_ICon format)
        {
            List<new_icon> cmds = new List<new_icon>();

            // 空值检查
            if (format == null || format.icon == null)
                return cmds;

            // 遍历 RootObject_ICon 中的 Grid.icon 列表
            foreach (var gridIcon in format.icon)
            {
                // 创建目标 icon 对象
                var targetIcon = new new_icon();

                // 映射必要属性 (根据实际字段调整)
                targetIcon.x = gridIcon.x;
                targetIcon.y = gridIcon.y;
                targetIcon.z = gridIcon.z;
                targetIcon.Color = gridIcon.Color;
                targetIcon.interf = gridIcon.interf;


                cmds.Add(targetIcon);
            }

            return cmds;
        }

        public static RootObject_ICon ReadFile(string filename)
        {
            using (var file =
                new StreamReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var output = JsonConvert.DeserializeObject<RootObject_ICon>(file.ReadToEnd());

                return output;
            }
        }

        private void BUT_Brake_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Brake_ALL(Vertical);
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Vertical = checkBox1.Checked;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == comboBox1.SelectedValue)
                    {
                        var vector = SwarmInterface.getOffsets(mav);
                        this.textBox1.Text = (float)vector.x + "";
                        this.textBox2.Text = (float)vector.y + "";
                        this.textBox3.Text = (float)vector.z + "";

                    }

                }
            }
        }

        private void BUT_Rtl_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Rtl_ALL(Vertical);
            }
        }

        private void BUT_Rtl_successively_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                int a = int.Parse(textBox4.Text);
                SwarmInterface.Rtl_successively_ALL(Vertical, a);
            }
        }

        private void BUT_DifferenceX_Click(object sender, EventArgs e)
        {
            int a = int.Parse(textBox5.Text);
            int b = 0;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.Leader)
                        continue;


                    b = b + a;
                    var vector = SwarmInterface.getOffsets(mav);
                    this.textBox1.Text = (float)b + "";
                    this.textBox2.Text = (float)vector.y + "";
                    this.textBox3.Text = (float)vector.z + "";

                    grid1.UpdateIcon(mav, (float)b, (float)vector.y, (float)vector.z, true);
                    ((Formation)SwarmInterface).setOffsets(mav, b, (float)vector.y, (float)vector.z);

                }
            }

        }

        private void BUT_DifferenceY_Click(object sender, EventArgs e)
        {
            int a = int.Parse(textBox6.Text);
            int b = 0;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.Leader)
                        continue;


                    b = b + a;
                    var vector = SwarmInterface.getOffsets(mav);
                    this.textBox1.Text = (float)vector.x + "";
                    this.textBox2.Text = (float)b + "";
                    this.textBox3.Text = (float)vector.z + "";

                    grid1.UpdateIcon(mav, (float)vector.x, (float)b, (float)vector.z, true);
                    ((Formation)SwarmInterface).setOffsets(mav, vector.x, (float)b, (float)vector.z);


                }
            }
        }

        private void BUT_DifferenceZ_Click(object sender, EventArgs e)
        {
            int a = int.Parse(textBox7.Text);
            int b = 0;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.Leader)
                        continue;

                    b = b + a;

                    var vector = SwarmInterface.getOffsets(mav);
                    this.textBox1.Text = (float)vector.x + "";
                    this.textBox2.Text = (float)vector.y + "";
                    this.textBox3.Text = (float)b + "";

                    grid1.UpdateIcon(mav, (float)vector.x, (float)vector.y, (float)b, true);
                    ((Formation)SwarmInterface).setOffsets(mav, vector.x, (float)vector.y, (float)b);

                }
            }

        }

        public class Drone2SecondsAgo
        {
            public int Id { get; }
            public double Z { get; set; }
         
            public double Roll { get; set; } // 俯仰
            public double Pitch { get; set; } // 翻滚
           
            public Drone2SecondsAgo(int id, double z, double roll, double pitch)
            {
                Id = id;
                Z = z;
                Roll = roll;
                Pitch = pitch;
                
            }
            public void printDrone()
            {
                Console.WriteLine("drone id=" + Id + "Z=" + Z + "Roll=" + Roll + "Pitch=" + Pitch);
            }
        }
        // 定义无人机类（包含坐标）
        public class Drone
        {
            public string Id { get; }
            public double X { get; }
            public double Y { get; }
            public double Yaw { get; set; } // 偏航角
            public bool IsAlive { get; set; } // 是否存活（用于判断是否遇击）
            public double ChangeYaw { get; set; } //改变的偏航角
            public Drone(string id, double x, double y, double yaw, bool isAlive,double changeYaw)
            {
                Id = id;
                X = x;
                Y = y;
                Yaw = yaw;
                IsAlive = isAlive;
                ChangeYaw = changeYaw;
            }
            public void printDrone() {
                Console.WriteLine("drone id="+Id + "X="+X+"Y="+Y+"YAW="+Yaw+ "IsAlive=" + IsAlive + "ChangeYaw" + ChangeYaw);
            }
        }

        /// <summary>
        /// 按X轴坐标将无人机从左到右分为3组
        /// </summary>
        public static class DroneGrouping
        {
            public static List<List<Drone>> 按X轴分组(List<Drone> drones, int minClusterSize = 3)
            {
                

                var groups = new List<List<Drone>>();
                if (drones == null || drones.Count < 3)
                    return new List<List<Drone>>();

                // 按 X 轴升序排序
                //var sortedDrones = drones.OrderBy(d => d.X).ToList();
                //int n = sortedDrones.Count;


                int aliveDroneCount = drones.Count(d => d.IsAlive == true);//统计存活的个数
                List<Drone> aliveSortedDrones = drones
                                                .OrderBy(d => d.X)                // 先按 X 轴升序排序
                                                .Where(d => d.IsAlive)            // 再筛选出存活的无人机
                                                .ToList();                        // 最后转成 List

                List<Drone> deadDrones = drones .Where(d => d.IsAlive == false)  // 筛选出死亡的无人机
                                        .ToList();                   // 转成 List
                // 确定分组数 k
                int k;
                if (aliveDroneCount >= 9)
                {
                    k = 3;

                    int rem = aliveDroneCount % k;
                    int baseSize = (aliveDroneCount - rem) / k;
                    if (rem == 1)
                    {
                        //将drones分组依次是：baseSize+1,baseSize,baseSize,如果依次添加各组IsAlive==false也存入groups，但是个数要从后面下一位补一个存入groups
                        List<Drone> firstThreeDrones = aliveSortedDrones.Take(baseSize + 1).ToList();
                        groups.Add(firstThreeDrones);
                        List<Drone> secondThreeDrones = aliveSortedDrones.Skip(baseSize + 1).Take(baseSize).ToList();
                        groups.Add(secondThreeDrones);
                        List<Drone> thirdThreeDrones = aliveSortedDrones.Skip(2*baseSize + 1).Take(baseSize).ToList();
                        thirdThreeDrones.AddRange(deadDrones);
                        groups.Add(thirdThreeDrones);

                    }
                    else if (rem == 2)
                    {
                        //将drones分组依次是：baseSize + 1,baseSize+1,baseSize,如果依次添加各组IsAlive==false也存入groups，但是个数要从后面下一位补一个存入groups
                        //将drones分组依次是：baseSize+1,baseSize,baseSize,如果依次添加各组IsAlive==false也存入groups，但是个数要从后面下一位补一个存入groups
                        List<Drone> firstThreeDrones = aliveSortedDrones.Take(baseSize + 1).ToList();
                        groups.Add(firstThreeDrones);
                        List<Drone> secondThreeDrones = aliveSortedDrones.Skip(baseSize + 1).Take(baseSize+1).ToList();
                        groups.Add(secondThreeDrones);
                        List<Drone> thirdThreeDrones = aliveSortedDrones.Skip(2*baseSize + 2).Take(baseSize).ToList();
                        thirdThreeDrones.AddRange(deadDrones);
                        groups.Add(thirdThreeDrones);
                    }
                    else
                    {
                        //将drones分组依次是：baseSize,baseSize,baseSize,如果依次添加各组IsAlive==false也存入groups，但是个数要从后面下一位补一个存入groups
                        List<Drone> firstThreeDrones = aliveSortedDrones.Take(baseSize).ToList();
                        groups.Add(firstThreeDrones);
                        List<Drone> secondThreeDrones = aliveSortedDrones.Skip(baseSize).Take(baseSize).ToList();
                        groups.Add(secondThreeDrones);
                        List<Drone> thirdThreeDrones = aliveSortedDrones.Skip(2 * baseSize).Take(baseSize).ToList();
                        thirdThreeDrones.AddRange(deadDrones);
                        groups.Add(thirdThreeDrones);
                    }
                }
                else if (aliveDroneCount >= 6)
                {
                    k = 2;
                    int rem = aliveDroneCount % k;
                    int baseSize = (aliveDroneCount - rem) / k;
                    if (rem == 1)
                    {
                        //将drones分组依次是：baseSize+1,baseSize
                        List<Drone> firstThreeDrones = aliveSortedDrones.Take(baseSize + 1).ToList();
                        groups.Add(firstThreeDrones);
                        List<Drone> secondThreeDrones = aliveSortedDrones.Skip(baseSize + 1).Take(baseSize).ToList();
                        secondThreeDrones.AddRange(deadDrones);
                        groups.Add(secondThreeDrones);
                    }
                    else {
                        //将drones分组依次是：baseSize,baseSize
                        List<Drone> firstThreeDrones = aliveSortedDrones.Take(baseSize ).ToList();
                        groups.Add(firstThreeDrones);
                        List<Drone> secondThreeDrones = aliveSortedDrones.Skip(baseSize ).Take(baseSize).ToList();
                        secondThreeDrones.AddRange(deadDrones);
                        groups.Add(secondThreeDrones);
                    }
                }
                else
                {
                    k = 1;
                    
                    groups.Add(drones);
                }
                return groups;


                //    var groups = new List<List<Drone>>();
                //    int start = 0;

                //    for (int i = 0; i < k; i++)
                //    {
                //        int currentSize = baseSize + (i < rem ? 1 : 0);
                //        var group = sortedDrones.Skip(start).Take(currentSize).ToList();
                //        groups.Add(group);
                //        start += currentSize;
                //    }

                //    return groups;
                //}
            }
        }
      

        public static List<List<Drone>> SplitByYaw(List<List<Drone>> groups,float f_yaw)
        {
            //取[f_yaw-60,f_yaw+60] 为偏航角范围
            // 计算偏航角的范围： [f_yaw - 60, f_yaw + 60]
            double lowerBound = f_yaw - 60;
            double upperBound = f_yaw + 60;

            int a = groups.Count();

            int b = 120 / a;

            double c = lowerBound;



            // 存储最终的分组结果
            var resultGroups = new List<List<Drone>>();

            // 遍历现有的每个分组
            foreach (var group in groups)
            {
                // 将当前分组中的无人机按偏航角进行分配
                var newGroup = new List<Drone>();

                
                foreach (var drone in group)
                {
                        drone.ChangeYaw = c;
                        // 如果无人机的偏航角在范围内，加入新的分组
                        newGroup.Add(drone);
                    
                }

                // 如果新的分组不为空，添加到最终结果
                if (newGroup.Count > 0)
                {

                    resultGroups.Add(newGroup);
                }
                c = c + b;
            }



            return resultGroups;
        }

        private static bool isConnection = true;
        private byte[] uav_unconnection = new byte[20];
        private int unconn_count = 0;
        private int un_index_id = 0;
        private Vector3 un_vector =new Vector3();
        private float f_yaw = 0;
        private static CancellationTokenSource cancellationTokenSource;
        private Task currentTask;  // 跟踪当前任务
        
        private async void AUTO_Early_Warning_Click(object sender, EventArgs e)
        {
            //foreach (var port in MainV2.Comports)
            //{
            //    foreach (var mav in port.MAVlist)
            //    {
            //        // 1. 构造 SET_POSITION_TARGET_GLOBAL_INT 消息
            //        var msg = new mavlink_set_position_target_global_int_t
            //        {
            //            time_boot_ms = (uint)Environment.TickCount, // 当前时间戳
            //            coordinate_frame = (byte)MAV_FRAME.GLOBAL_RELATIVE_ALT_INT, // 使用相对海拔的全局坐标系
            //            lat_int = (int)(mav.cs.lat * 1e7), // 纬度（例如 30.0 度，转换为 int）
            //            lon_int = (int)(mav.cs.lng * 1e7), // 经度（例如 120.0 度，转换为 int）
            //            alt = 100, // 相对高度（米）

            //            vx = 0, // 忽略 x 方向速度
            //            vy = 0, // 忽略 y 方向速度
            //            vz = 0, // 忽略 z 方向速度

            //            afx = 0, // 忽略 x 方向加速度
            //            afy = 0, // 忽略 y 方向加速度
            //            afz = 0, // 忽略 z 方向加速度

            //            yaw = (float)(Math.PI / 2), // 偏航角（弧度，这里设置为 0，表示朝北）
            //            yaw_rate = 0, // 忽略偏航角速率

            //            type_mask = (ushort)(
            //                POSITION_TARGET_TYPEMASK.X_IGNORE | // 忽略位置
            //                POSITION_TARGET_TYPEMASK.Y_IGNORE |      // 忽略速度
            //                POSITION_TARGET_TYPEMASK.Z_IGNORE |   // 忽略加速度
            //                POSITION_TARGET_TYPEMASK.AX_IGNORE |  // 忽略偏航角速率
            //                POSITION_TARGET_TYPEMASK.AY_IGNORE |
            //                POSITION_TARGET_TYPEMASK.AZ_IGNORE |
            //                POSITION_TARGET_TYPEMASK.YAW_RATE_IGNORE |
            //                POSITION_TARGET_TYPEMASK.FORCE_SET
            //            ),

            //            target_system = mav.sysid,
            //            target_component = mav.compid
            //        };
            //        await Task.Run(() =>
            //        {
            //            this.Invoke((MethodInvoker)delegate {
            //                MainV2.comPort.generatePacket(
            //                    (byte)MAVLINK_MSG_ID.SET_POSITION_TARGET_GLOBAL_INT,
            //                    msg, mav.sysid, mav.compid);
            //            });
            //        });
            //    }
            //}
            if (myButton7.Text == "自动预警")
            {
                countdown.Start();
                countdown2.Start();
                myButton7.Text = "关闭预警";

                // 清理旧任务
                if (currentTask != null)
                {
                    cancellationTokenSource.Cancel();
                    await currentTask;  // 等待旧任务完成
                }

                // 创建新令牌源
                cancellationTokenSource = new CancellationTokenSource();


                //    // 启动新任务并保存引用
                currentTask = updateUAVVXYZAsync(cancellationTokenSource.Token);

            }
            else
            {
                countdown.Stop();
                countdown2.Stop();
                myButton7.Text = "自动预警";
                if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();  // 主动取消任务
                    //await currentTask;  // 等待任务完成
                }
            }


        }
        Dictionary<int, updatehome_lat> home_point_list = new Dictionary<int, updatehome_lat>();
        private void Checkpoint_Click(object sender, EventArgs e)
        {
            try
            {
                string[] parts1 = textBox_uav1.Text.Split(',');
                string[] parts2 = textBox_uav2.Text.Split(',');
                string[] parts3 = textBox_uav3.Text.Split(',');
                string[] parts4 = textBox_uav4.Text.Split(',');
                string[] parts5 = textBox_uav5.Text.Split(',');
                string[] parts6 = textBox_uav6.Text.Split(',');
                string[] parts7 = textBox_uav7.Text.Split(',');
                string[] parts8 = textBox_uav8.Text.Split(',');
                string[] parts9 = textBox_uav9.Text.Split(',');
                string[] parts10 = textBox_uav10.Text.Split(',');
                string[] parts11 = textBox_uav11.Text.Split(',');
                string[] parts12 = textBox_uav12.Text.Split(',');

                // 转换为 double 数组
                double[] numbers1 = parts1.Select(p => double.Parse(p)).ToArray();
                double[] numbers2 = parts2.Select(p => double.Parse(p)).ToArray();
                double[] numbers3 = parts3.Select(p => double.Parse(p)).ToArray();
                double[] numbers4 = parts4.Select(p => double.Parse(p)).ToArray();
                double[] numbers5 = parts5.Select(p => double.Parse(p)).ToArray();
                double[] numbers6 = parts6.Select(p => double.Parse(p)).ToArray();
                double[] numbers7 = parts7.Select(p => double.Parse(p)).ToArray();
                double[] numbers8 = parts8.Select(p => double.Parse(p)).ToArray();
                double[] numbers9 = parts9.Select(p => double.Parse(p)).ToArray();
                double[] numbers10 = parts10.Select(p => double.Parse(p)).ToArray();
                double[] numbers11 = parts11.Select(p => double.Parse(p)).ToArray();
                double[] numbers12 = parts12.Select(p => double.Parse(p)).ToArray();



                //List<updatehome_lat> home_point = new List<updatehome_lat>();
               
                home_point_list[1] = new updatehome_lat { x = numbers1[0], y = numbers1[1], z = numbers1[2] };
                home_point_list[2] = new updatehome_lat { x = numbers2[0], y = numbers2[1], z = numbers2[2] };
                home_point_list[3] = new updatehome_lat { x = numbers3[0], y = numbers3[1], z = numbers3[2] };
                home_point_list[4] = new updatehome_lat { x = numbers4[0], y = numbers4[1], z = numbers4[2] };
                home_point_list[5] = new updatehome_lat { x = numbers5[0], y = numbers5[1], z = numbers5[2] };
                home_point_list[6] = new updatehome_lat { x = numbers6[0], y = numbers6[1], z = numbers6[2] };
                home_point_list[7] = new updatehome_lat { x = numbers7[0], y = numbers7[1], z = numbers7[2] };
                home_point_list[8] = new updatehome_lat { x = numbers8[0], y = numbers8[1], z = numbers8[2] };
                home_point_list[9] = new updatehome_lat { x = numbers9[0], y = numbers9[1], z = numbers9[2] };
                home_point_list[10] = new updatehome_lat { x = numbers10[0], y = numbers10[1], z = numbers10[2] };
                home_point_list[11] = new updatehome_lat { x = numbers11[0], y = numbers11[1], z = numbers11[2] };
                home_point_list[12] = new updatehome_lat { x = numbers12[0], y = numbers12[1], z = numbers12[2] };
            }
            catch {

                MessageBox.Show("请认真检查坐标信息！！！" );

            }
            
        }
        public struct updatehome_lat
        { 
            public double x; 
            public double y; 
            public double z;
        
        }
            private async void setHomeHereToolStripMenuItem_Click(double lat, double lng,byte[] uav_unconnection)
        {

            //textBox_uav1.Text = "0,0,0";
            //textBox_uav2.Text = "0,0,0";
            //textBox_uav3.Text = "0,0,0";
            //textBox_uav4.Text = "0,0,0";
            //textBox_uav5.Text = "0,0,0";
            //textBox_uav6.Text = "0,0,0";
            //textBox_uav7.Text = "0,0,0";
            //textBox_uav8.Text = "0,0,0";
            //textBox_uav9.Text = "0,0,0";
            //textBox_uav10.Text = "0,0,0";
            //textBox_uav11.Text = "0,0,0";
            //textBox_uav12.Text = "0,0,0";
            //string[] parts1 = textBox_uav1.Text.Split(',');
            //string[] parts2 = textBox_uav2.Text.Split(',');
            //string[] parts3 = textBox_uav3.Text.Split(',');
            //string[] parts4 = textBox_uav4.Text.Split(',');
            //string[] parts5 = textBox_uav5.Text.Split(',');
            //string[] parts6 = textBox_uav6.Text.Split(',');
            //string[] parts7 = textBox_uav7.Text.Split(',');
            //string[] parts8 = textBox_uav8.Text.Split(',');
            //string[] parts9 = textBox_uav9.Text.Split(',');
            //string[] parts10 = textBox_uav10.Text.Split(',');
            //string[] parts11 = textBox_uav11.Text.Split(',');
            //string[] parts12 = textBox_uav12.Text.Split(',');

            //// 转换为 double 数组
            //double[] numbers1 = parts1.Select(p => double.Parse(p)).ToArray();
            //double[] numbers2 = parts2.Select(p => double.Parse(p)).ToArray();
            //double[] numbers3 = parts3.Select(p => double.Parse(p)).ToArray();
            //double[] numbers4 = parts4.Select(p => double.Parse(p)).ToArray();
            //double[] numbers5 = parts5.Select(p => double.Parse(p)).ToArray();
            //double[] numbers6 = parts6.Select(p => double.Parse(p)).ToArray();
            //double[] numbers7 = parts7.Select(p => double.Parse(p)).ToArray();
            //double[] numbers8 = parts8.Select(p => double.Parse(p)).ToArray();
            //double[] numbers9 = parts9.Select(p => double.Parse(p)).ToArray();
            //double[] numbers10 = parts10.Select(p => double.Parse(p)).ToArray();
            //double[] numbers11 = parts11.Select(p => double.Parse(p)).ToArray();
            //double[] numbers12 = parts12.Select(p => double.Parse(p)).ToArray();



            ////List<updatehome_lat> home_point = new List<updatehome_lat>();
            //Dictionary<int, updatehome_lat> home_point_list = new Dictionary<int, updatehome_lat>();
            //home_point_list[1] = new updatehome_lat { x = numbers1[0],y = numbers1[1],z = numbers1[2] };
            //home_point_list[2] = new updatehome_lat { x = numbers1[0],y = numbers1[1],z = numbers1[2] };
            //home_point_list[3] = new updatehome_lat { x = numbers1[0],y = numbers1[1],z = numbers1[2] };
            //home_point_list[4] = new updatehome_lat { x = numbers1[0],y = numbers1[1],z = numbers1[2] };
            //home_point_list[5] = new updatehome_lat { x = numbers1[0],y = numbers1[1],z = numbers1[2] };
            //home_point_list[6] = new updatehome_lat { x = numbers1[0],y = numbers1[1],z = numbers1[2] };
            //home_point_list[7] = new updatehome_lat { x = numbers1[0],y = numbers1[1],z = numbers1[2] };
            //home_point_list[8] = new updatehome_lat { x = numbers1[0],y = numbers1[1],z = numbers1[2] };
            //home_point_list[9] = new updatehome_lat { x = numbers1[0],y = numbers1[1],z = numbers1[2] };
            //home_point_list[10] = new updatehome_lat { x = numbers1[0],y = numbers1[1],z = numbers1[2] };
            //home_point_list[11] = new updatehome_lat { x = numbers1[0],y = numbers1[1],z = numbers1[2] };
            //home_point_list[12] = new updatehome_lat { x = numbers1[0],y = numbers1[1],z = numbers1[2] };

            //await Task.Run(() =>
            //{
            //    this.Invoke((MethodInvoker)delegate {
            //        foreach (var port in MainV2.Comports)
            //        {
            //            foreach (var mav in port.MAVlist)
            //            {
            //                yawDict[mav.sysid] = mav.cs.yaw;
            //            }
            //        }
            //    });
            //});
            //await Task.Run(() =>
            //{
            //    this.Invoke((MethodInvoker)delegate {
            //        foreach (var port in MainV2.Comports)
            //        {
            //            foreach (var mav in port.MAVlist)
            //            {
            //                yawDict[mav.sysid] = mav.cs.yaw;
            //            }
            //        }
            //    });
            //});
            //var alt = srtm.getAltitude(lat, lng);
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (uav_unconnection[mav.sysid-1]==1) {
                        continue;
                    }

                    //if (alt.currenttype != srtm.tiletype.valid && alt.currenttype != srtm.tiletype.ocean)
                    //{
                    //    CustomMessageBox.Show("No SRTM data for this area", Strings.ERROR);
                    //    return;
                    //}

                    //if (CustomMessageBox.Show(
                    //        "This will reset the onboard home position (effects RTL etc). Are you Sure?",
                    //        "Are you sure?", CustomMessageBox.MessageBoxButtons.OKCancel) ==
                    //    CustomMessageBox.DialogResult.OK)
                    //{

                    

                    MainV2.comPort.doCommandInt((byte)mav.sysid,
                        (byte)mav.compid,
                        MAVLink.MAV_CMD.DO_SET_HOME, 0, 0, 0, 0, (int)(home_point_list[mav.sysid].x * 1e7),
                        (int)(home_point_list[mav.sysid].y * 1e7), (float)(mav.cs.lat));


                    port.setMode(mav.sysid, mav.compid, "RTL");
                }
            }
            //try
            //{

            //}

            //await MainV2.comPort.getHomePositionAsync((byte)MainV2.comPort.sysidcurrent,
            //    (byte)MainV2.comPort.compidcurrent);
            //}
            //catch
            //{
            //    CustomMessageBox.Show(Strings.CommandFailed, Strings.ERROR);
            //}


            //18.4325505,109.8592043,10
        }
        private void updatedrone2SecondsAgos(object sender, ElapsedEventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    Drone2SecondsAgo result = drone2SecondsAgos.FirstOrDefault(d => d.Id == mav.sysid);

                    if (result != null) {

                        result.Z = mav.cs.alt;
                        result.Roll = mav.cs.roll;
                        result.Pitch = mav.cs.pitch;

                        result.printDrone();
                    }
                }
            }
        }
        private void sendheartbeat(object sender, ElapsedEventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (Math.Abs(mav.cs.lat - home_point_list[mav.sysid].x) < 0.0001&& Math.Abs(mav.cs.lng - home_point_list[mav.sysid].y) < 0.0001 && mav.cs.altasl <= (home_point_list[mav.sysid].z+20)) {
                        port.setMode(mav.sysid, mav.compid, "Land");

                    }
                
                }
            }
        }
         void GlobalToBody(double speedGlobal, double angleGlobal, float yawAngle, out float vx_body, out float vy_body)
        {

            // 计算全局坐标系中的速度分量
            double vGlobalX = speedGlobal * Math.Cos(DegreeToRadian(angleGlobal));  // 全局坐标系中的 X 速度分量
            double vGlobalY = speedGlobal * Math.Sin(DegreeToRadian(angleGlobal));  // 全局坐标系中的 Y 速度分量
            vx_body = (float)(vGlobalX * Math.Cos(DegreeToRadian(yawAngle)) + vGlobalY * Math.Sin(DegreeToRadian(yawAngle)));
            vy_body = (float)(-vGlobalX * Math.Sin(DegreeToRadian(yawAngle)) + vGlobalY * Math.Cos(DegreeToRadian(yawAngle)));
        }
        private float[] uav_yaw = new float[20];

        // 将角度转换为弧度
        static double DegreeToRadian(double degree)
        {
            return degree * Math.PI / 180.0;
        }

        private void isCancellationTokenSource()
        {
            cancellationTokenSource.Cancel(); // 主动取消任务
        }
        private static int time_cout;


        /********实时监听无人机的状态，如果有一个无人机的心跳包断开，根据时间与上一时间只差来判断是否断链**********/
        async Task updateUAVVXYZAsync(CancellationToken token)
        {
            
            unconn_count = 0;

            // 将监听循环放入后台线程
            await Task.Run(async () =>
            {
                while (isConnection && !token.IsCancellationRequested)
                {
                    foreach (var port in MainV2.Comports)
                    {
                        foreach (var mav in port.MAVlist)
                        {
                            var currentTime = DateTime.Now;
                            var interval = (currentTime - mav.cs.lastdata1).TotalSeconds;
                            Drone2SecondsAgo result = drone2SecondsAgos.FirstOrDefault(d => d.Id == mav.sysid);
                            if (interval > 10||mav.cs.gpsstatus < 3 || Math.Abs(mav.cs.alt - result.Z)>30|| Math.Abs(mav.cs.roll) > 45|| Math.Abs(mav.cs.pitch)>45)
                            {
                                uav_unconnection[mav.sysid - 1] = 1;
                                isConnection = false;
                                //Console.WriteLine("正在检查连接...数据丢失警告...");


                            }

                            //Console.WriteLine("线程正在执行..." + mav.cs.lastdata);

                            for (int i = 0; i < uav_unconnection.Length; i++)
                            {
                                int droneNumber = i + 1;
                                byte parameter = uav_unconnection[i];
                                Console.WriteLine($"uav {droneNumber} 号无人机 参数是 {parameter}");
                            }
                        }
                    }

                    // 添加延迟避免 CPU 过载
                    await Task.Delay(100, token);
                }
            }, token);

           

                    // 如果因连接中断退出循环，则执行后续逻辑
             if (!isConnection)
            {

                //foreach (var port in MainV2.Comports)
                //{
                //    foreach (var mav in port.MAVlist)
                //    { 
                        
                //    }
                //}


                threadrun = false;
                BUT_Start.Text = Strings.Start;
                //foreach (var port in MainV2.Comports)
                //{
                //    foreach (var mav in port.MAVlist)
                //    {
                //        port.setMode(mav.sysid, mav.compid, "Brake");
                //    }
                //}
                await HandleDroneGroupingAndFlightAsync(token);


            }
        }

        // 继续补全 HandleDroneGroupingAndFlightAsync 方法
        private async Task HandleDroneGroupingAndFlightAsync(CancellationToken token)
        {
            var drones = new List<List<Drone>>();
            var drones_1 = new List<Drone>();

            // 获取 Leader 的 yaw 角度（需安全访问 UI 资源）
            await Task.Run(() =>
            {
                this.Invoke((MethodInvoker)delegate {
                    foreach (var port in MainV2.Comports)
                    {
                        foreach (var mav in port.MAVlist)
                        {
                            if (mav == SwarmInterface.Leader)
                            {
                                f_yaw = mav.cs.yaw;
                            }
                        }
                    }
                });
            });

            // 收集无人机信息（需安全访问 UI 资源）
            await Task.Run(() =>
            {
                this.Invoke((MethodInvoker)delegate {
                    foreach (var port in MainV2.Comports)
                    {
                        foreach (var mav in port.MAVlist)
                        {
                            var vector = SwarmInterface.getOffsets(mav);
                            drones_1.Add(new Drone(mav.sysid + "", vector.x, vector.y, vector.z, uav_unconnection[mav.sysid-1]==0, 0));
                        }
                    }
                });
            });

            // 分组逻辑（纯计算无需 UI 交互）
            var groups = DroneGrouping.按X轴分组(drones_1);
            drones = SplitByYaw(groups, f_yaw);

            // 启动定时器（需回到 UI 线程）
            this.Invoke((MethodInvoker)delegate {
                timer1.Start();
            });
            // 给定条件
            double speedGlobal = 10.0; // 飞行器在全局坐标系下的速度，单位 m/s
            double angleGlobal = 0.0; // 飞行器在全局坐标系下的方向角度（度），即东北方向
            // 飞行控制主循环
            try
            {
                long startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                while (!token.IsCancellationRequested)
                {
                    //// 获取当前无人机位置信息（需安全访问 UI 资源）
                    //Dictionary<int, float> yawDict = new Dictionary<int, float>();
                    //await Task.Run(() =>
                    //{
                    //    this.Invoke((MethodInvoker)delegate {
                    //        foreach (var port in MainV2.Comports)
                    //        {
                    //            foreach (var mav in port.MAVlist)
                    //            {
                    //                yawDict[mav.sysid] = mav.cs.yaw;
                    //            }
                    //        }
                    //    });
                    //});

                    // 计算飞行指令
                    long endTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    double timeDifferenceSeconds = (endTimestamp - startTimestamp) / 1000.0;
                    float yawAngle = 0, yaw = 0;
                    // 计算速度矢量
                    float vx_body, vy_body, vz_body = 0;
                    foreach (var port in MainV2.Comports)
                    {
                        foreach (var mav in port.MAVlist)
                        {
                           

                            // 查找无人机分组信息
                            foreach (var group in drones)
                            {
                                foreach (var drone in group)
                                {
                                    if (drone.Id == mav.sysid.ToString())
                                    {
                                        yawAngle = (float)drone.ChangeYaw;
                                        yaw = (float)drone.Yaw;
                                        break;
                                    }
                                }
                            }

                            /*******************方式二----start*****************************/
                            double speed = 10.0;            // 期望速度 (m/s)

                            if (timeDifferenceSeconds < 10)
                            {
                                //Console.WriteLine("timeDifferenceSeconds < 10" );
                                GlobalToBody(speedGlobal, yawAngle, yaw, out vx_body, out vy_body);

                                // 1. 构造 SET_POSITION_TARGET_GLOBAL_INT 消息
                                var msg = new mavlink_set_position_target_global_int_t
                                {
                                    time_boot_ms = (uint)Environment.TickCount, // 当前时间戳
                                    coordinate_frame = (byte)MAV_FRAME.GLOBAL_RELATIVE_ALT_INT, // 使用相对海拔的全局坐标系
                                    lat_int = (int)(mav.cs.lat * 1e7), // 纬度（例如 30.0 度，转换为 int）
                                    lon_int = (int)(mav.cs.lng * 1e7), // 经度（例如 120.0 度，转换为 int）
                                    alt = mav.cs.alt, // 相对高度（米）
                                    //vx = (float)2, // 北向速度
                                    //vy = (float)2, // 东向速度
                                    vx = (float)vx_body, // 北向速度
                                    vy = (float)vy_body, // 东向速度
                                    vz = 0, // 忽略 z 方向速度

                                    afx = 0, // 忽略 x 方向加速度
                                    afy = 0, // 忽略 y 方向加速度
                                    afz = 0, // 忽略 z 方向加速度

                                    //yaw = (float)(Math.PI / 2), // 偏航角（弧度，这里设置为 0，表示朝北）
                                    yaw = (float)(yawAngle * Math.PI / 180.0), // 偏航角（弧度，这里设置为 0，表示朝北）
                                    yaw_rate = 0, // 忽略偏航角速率

                                    type_mask = (ushort)(
                                        POSITION_TARGET_TYPEMASK.X_IGNORE | // 忽略位置
                                        POSITION_TARGET_TYPEMASK.Y_IGNORE |      // 忽略速度
                                        POSITION_TARGET_TYPEMASK.Z_IGNORE |   // 忽略加速度
                                        POSITION_TARGET_TYPEMASK.AX_IGNORE |  // 忽略偏航角速率
                                        POSITION_TARGET_TYPEMASK.AY_IGNORE |
                                        POSITION_TARGET_TYPEMASK.AZ_IGNORE |
                                        POSITION_TARGET_TYPEMASK.YAW_RATE_IGNORE |
                                        POSITION_TARGET_TYPEMASK.FORCE_SET
                                    ),

                                    target_system = mav.sysid,
                                    target_component = mav.compid
                                };
                                await Task.Run(() =>
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        MainV2.comPort.generatePacket(
                                            (byte)MAVLINK_MSG_ID.SET_POSITION_TARGET_GLOBAL_INT,
                                            msg, mav.sysid, mav.compid);
                                    });
                                });

                            }
                            else if (timeDifferenceSeconds > 10 && timeDifferenceSeconds < 20)
                            {
                                Console.WriteLine("timeDifferenceSeconds > 20 && timeDifferenceSeconds < 30");
                                //GlobalToBody(speedGlobal, 0, mav.cs.yaw, out vx_body, out vy_body);

                                // 1. 构造 SET_POSITION_TARGET_GLOBAL_INT 消息
                                var msg = new mavlink_set_position_target_global_int_t
                                {
                                    time_boot_ms = (uint)Environment.TickCount, // 当前时间戳
                                    coordinate_frame = (byte)MAV_FRAME.GLOBAL_RELATIVE_ALT_INT, // 使用相对海拔的全局坐标系
                                    lat_int = (int)(mav.cs.lat * 1e7), // 纬度（例如 30.0 度，转换为 int）
                                    lon_int = (int)(mav.cs.lng * 1e7), // 经度（例如 120.0 度，转换为 int）
                                    alt = mav.cs.alt, // 相对高度（米）
                                    //vx = (float)2, // 北向速度
                                    //vy = (float)2, // 东向速度
                                    vx = (float)5, // 北向速度
                                    vy = (float)0, // 东向速度
                                    vz = 0, // 忽略 z 方向速度

                                    afx = 0, // 忽略 x 方向加速度
                                    afy = 0, // 忽略 y 方向加速度
                                    afz = 0, // 忽略 z 方向加速度

                                    //yaw = (float)(Math.PI / 2), // 偏航角（弧度，这里设置为 0，表示朝北）
                                    yaw = (float)0, // 偏航角（弧度，这里设置为 0，表示朝北）
                                    yaw_rate = 0, // 忽略偏航角速率

                                    type_mask = (ushort)(
                                        POSITION_TARGET_TYPEMASK.X_IGNORE | // 忽略位置
                                        POSITION_TARGET_TYPEMASK.Y_IGNORE |      // 忽略速度
                                        POSITION_TARGET_TYPEMASK.Z_IGNORE |   // 忽略加速度
                                        POSITION_TARGET_TYPEMASK.AX_IGNORE |  // 忽略偏航角速率
                                        POSITION_TARGET_TYPEMASK.AY_IGNORE |
                                        POSITION_TARGET_TYPEMASK.AZ_IGNORE |
                                        POSITION_TARGET_TYPEMASK.YAW_RATE_IGNORE |
                                        POSITION_TARGET_TYPEMASK.FORCE_SET
                                    ),

                                    target_system = mav.sysid,
                                    target_component = mav.compid
                                };
                                await Task.Run(() =>
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        MainV2.comPort.generatePacket(
                                            (byte)MAVLINK_MSG_ID.SET_POSITION_TARGET_GLOBAL_INT,
                                            msg, mav.sysid, mav.compid);
                                    });
                                });

                                //var msg1 = new mavlink_set_position_target_local_ned_t
                                //{
                                //    time_boot_ms = (uint)Environment.TickCount,
                                //    coordinate_frame = (byte)MAV_FRAME.BODY_NED, // 使用机体坐标系

                                //    // 位置（只设置）
                                //    x = 0,
                                //    y = 0,
                                //    z = 0,

                                //    // 速度（只设置 vx = 0.5 m/s）（被忽略）
                                //    vx = 5.0f,
                                //    vy = 0.0f,
                                //    vz = 0.0f,

                                //    // 加速度（被忽略）
                                //    afx = 0,
                                //    afy = 0,
                                //    afz = 0,

                                //    // 偏航角和偏航速率（被忽略）
                                //    yaw = 0,
                                //    yaw_rate = 0,

                                //    // 掩码：忽略位置、加速度、yaw、yaw_rate
                                //    type_mask = (ushort)(
                                //        POSITION_TARGET_TYPEMASK.X_IGNORE |
                                //        POSITION_TARGET_TYPEMASK.Y_IGNORE |
                                //        POSITION_TARGET_TYPEMASK.Z_IGNORE |
                                //        POSITION_TARGET_TYPEMASK.AX_IGNORE |
                                //        POSITION_TARGET_TYPEMASK.AY_IGNORE |
                                //        POSITION_TARGET_TYPEMASK.AZ_IGNORE |
                                //        POSITION_TARGET_TYPEMASK.FORCE_SET |
                                //        POSITION_TARGET_TYPEMASK.YAW_IGNORE |
                                //        POSITION_TARGET_TYPEMASK.YAW_RATE_IGNORE
                                //    ),

                                //    target_system = mav.sysid,
                                //    target_component = mav.compid
                                //};
                                //await Task.Run(() =>
                                //{
                                //    this.Invoke((MethodInvoker)delegate
                                //    {
                                //        MainV2.comPort.generatePacket(
                                //            (byte)MAVLINK_MSG_ID.SET_POSITION_TARGET_LOCAL_NED,
                                //            msg1, mav.sysid, mav.compid);
                                //    });
                                //});
                            }
                            else {
                                // 返回原点（需安全访问 UI 资源）
                                await Task.Run(() =>
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        setHomeHereToolStripMenuItem_Click(18.4291102, 109.8592257, uav_unconnection);
                                    });
                                });
                                return;

                            }
                            //else 
                            //{

                            //    // 返回原点（需安全访问 UI 资源）
                            //    await Task.Run(() =>
                            //    {
                            //        this.Invoke((MethodInvoker)delegate
                            //        {
                            //            setHomeHereToolStripMenuItem_Click(18.4291102, 109.8592257, uav_unconnection);
                            //        });
                            //    });
                            //    return;
                            //}
                            /*******************方式二----end*****************************/

                            /*******************方式一----start*****************************/
                            //if (timeDifferenceSeconds < 5)
                            //{

                            //    GlobalToBody(speedGlobal, yawAngle, yaw, out vx_body, out vy_body);
                            //    vz_body = -mav.sysid;
                            //}
                            //else if ( timeDifferenceSeconds >= 5 && timeDifferenceSeconds < 20)
                            //{
                            //    //GlobalToBody(speedGlobal, yawAngle+time_date, yaw, out vx_body, out vy_body);
                            //    GlobalToBody(speedGlobal, yawAngle + time_date, yaw, out vx_body, out vy_body);

                            //}
                            //else
                            //{
                            //    // 返回原点（需安全访问 UI 资源）
                            //    await Task.Run(() =>
                            //    {
                            //        this.Invoke((MethodInvoker)delegate {
                            //            setHomeHereToolStripMenuItem_Click(18.4291102, 109.8592257, uav_unconnection);
                            //        });
                            //    });
                            //    return;
                            //}
                            //// 构造控制指令
                            //mavlink_set_position_target_local_ned_t req = new mavlink_set_position_target_local_ned_t(
                            //    (uint)Environment.TickCount, 0, 1.52f, 1f,
                            //    vx_body, vy_body, vz_body, 0, 0, 0, 0, 0,
                            //    2503, mav.sysid, mav.compid, 1);
                            //// 发送指令（需安全访问 UI 资源）
                            //await Task.Run(() =>
                            //{
                            //    this.Invoke((MethodInvoker)delegate {
                            //        MainV2.comPort.generatePacket(
                            //            (byte)MAVLINK_MSG_ID.SET_POSITION_TARGET_LOCAL_NED,
                            //            req, mav.sysid, mav.compid);
                            //    });
                            //});
                            /*******************方式二----end*****************************/
                        }
                    }
                   

                    // 控制发送频率为 10Hz
                    await Task.Delay(100, token);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("飞行任务被取消");
            }
        }
        //async Task updateUAVVXYZAsync( CancellationToken token)
        //{
        //    threadrun = false;
        //    BUT_Start.Text = Strings.Start;
        //    // 启动异步任务发送指令，不会卡住主线程
        //    //await updateUAVVXYZAsync();

        //    unconn_count = 0;

        //    while (isConnection)
        //    {
        //        foreach (var port in MainV2.Comports)
        //        {
        //            foreach (var mav in port.MAVlist)

        //            {

        //                var currentTime = DateTime.Now;
        //                var interval = (currentTime - mav.cs.lastdata1).TotalSeconds;
        //                // 如果超过最大允许时间且未收到数据，则认为数据丢失
        //                if (interval > 5)
        //                {
        //                    uav_unconnection[mav.sysid - 1] = 1;
        //                    isConnection = false;
        //                    Console.WriteLine("正在检查连接...数据丢失警告...");

        //                }
        //                Console.WriteLine("线程正在执行..." + mav.cs.lastdata);

        //                for (int i = 0; i < uav_unconnection.Length; i++)
        //                {
        //                    int droneNumber = i + 1;
        //                    byte parameter = uav_unconnection[i];
        //                    Console.WriteLine($"uav {droneNumber} 号无人机 参数是 {parameter}");
        //                }

        //            }

        //        }

        //    }
        //    var drones = new List<List<Drone>>();
        //    //1.读取当前失恋无人机个数
        //    if (!isConnection)
        //    {

        //        int a = 5;
        //        int b = 0;
        //        var drones_1 = new List<Drone>();
        //        foreach (var port in MainV2.Comports)
        //        {
        //            foreach (var mav in port.MAVlist)
        //            {
        //                if (mav == SwarmInterface.Leader)
        //                {

        //                    f_yaw = mav.cs.yaw;

        //                }


        //            }
        //        }
        //        foreach (var port in MainV2.Comports)
        //        {
        //            foreach (var mav in port.MAVlist)
        //            {

        //                var vector = SwarmInterface.getOffsets(mav);
        //                drones_1.Add(new Drone(mav.sysid + "", vector.x, vector.y, vector.y, 0));

        //            }
        //        }
        //        var groups = DroneGrouping.按X轴分组(drones_1);

        //        Console.WriteLine("按 X 轴分组结果：");
        //        for (int i = 0; i < groups.Count; i++)
        //        {
        //            Console.WriteLine($"组 {i + 1}:");
        //            foreach (var drone in groups[i])
        //            {
        //                drone.printDrone();
        //            }
        //        }


        //        drones = SplitByYaw(groups, f_yaw);


        //    }
            
            

        //    timer1.Start();

        //    // 给定条件
        //    double speedGlobal = 10.0; // 飞行器在全局坐标系下的速度，单位 m/s
        //    double angleGlobal = 0.0; // 飞行器在全局坐标系下的方向角度（度），即东北方向



        //    // 获取起始时间戳（毫秒）
        //    long startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        //    foreach (var port in MainV2.Comports)
        //    {
        //        foreach (var mav in port.MAVlist)
        //        {
        //            uav_yaw[mav.sysid - 1] = mav.cs.yaw;
        //        }
        //    }

        //    try
        //    {
        //        while (!token.IsCancellationRequested)
        //        {
        //            Console.WriteLine("任务还在继续==============");
        //            // 添加取消检查点
        //            if (token.IsCancellationRequested)
        //            {
        //                Console.WriteLine("任务已取消");
        //                return;
        //            }
        //            //await Task.Delay(100, token).ConfigureAwait(false);
        //            //Console.WriteLine("取消任务的请求已收到，正在退出...");
        //            foreach (var port in MainV2.Comports)
        //            {
        //                foreach (var mav in port.MAVlist)
        //                {

        //                    float yawAngle = 0;  // 飞行器的偏航角度，单位为度
        //                    float yaw = 0;                              // 遍历现有的每个分组
        //                    foreach (var group in drones)
        //                    {



        //                        foreach (var drone in group)
        //                        {
        //                            if (drone.Id == mav.sysid + "")
        //                            {
        //                                yawAngle = (float)drone.ChangeYaw;
        //                                yaw = (float)drone.Yaw;
        //                            }
        //                        }


        //                    }


        //                    // 获取结束时间戳
        //                    long endTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //                    //Console.WriteLine($"结束时间戳（毫秒）: {endTimestamp}");


        //                    //转换为机体速度（假设当前偏航角为 0°）
        //                    float vx_body, vy_body, vz_body = 0;
                           
        //                    // 计算时间差（秒）
        //                    double timeDifferenceSeconds = (endTimestamp - startTimestamp) / 1000.0;
        //                    if (timeDifferenceSeconds < 5)
        //                    {
        //                        GlobalToBody(speedGlobal, yawAngle , yaw, out vx_body, out vy_body);
        //                        vz_body = -mav.sysid;

        //                        mavlink_set_position_target_local_ned_t req = new mavlink_set_position_target_local_ned_t((uint)Environment.TickCount, 0, 0, 0, vx_body, vy_body, vz_body, 0, 0, 0, 0, 0, 2503, mav.sysid, mav.compid, 1);//8


        //                        MainV2.comPort.generatePacket((byte)MAVLINK_MSG_ID.SET_POSITION_TARGET_LOCAL_NED, req, mav.sysid, mav.compid);
        //                    }
        //                    else if (timeDifferenceSeconds < 20&&timeDifferenceSeconds>=5) {
        //                        GlobalToBody(speedGlobal, yawAngle, yaw, out vx_body, out vy_body);
        //                        //GlobalToBody(speedGlobal, yawAngle + time_date, yaw, out vx_body, out vy_body);
        //                        vz_body = 0;

        //                        mavlink_set_position_target_local_ned_t req = new mavlink_set_position_target_local_ned_t((uint)Environment.TickCount, 0, 0, 0, vx_body, vy_body, vz_body, 0, 0, 0, 0, 0, 2503, mav.sysid, mav.compid, 1);//8


        //                        MainV2.comPort.generatePacket((byte)MAVLINK_MSG_ID.SET_POSITION_TARGET_LOCAL_NED, req, mav.sysid, mav.compid);
        //                    }
        //                    else {
        //                        //GlobalToBody(speedGlobal, yaw, yaw, out vx_body, out vy_body);
        //                        setHomeHereToolStripMenuItem_Click(18.4291102, 109.8592257);
                                
                                
        //                        return;
        //                    }


                            



        //                    //遇到危险，

        //                    //1.通过集群功能先差分高度为三组一边飞一边差分，更新界面

        //                }

        //            }
        //            // 控制发送频率为 10Hz（每秒发送 10 次）
        //            await Task.Delay(100); // 单位是毫秒
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        Console.WriteLine("任务被取消");
        //    }
            
        //}
private static Random rand = new Random();  // 随机数生成器
        private static int time_date = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            // 生成一个 -5 到 5 之间的随机数
            int randomValue = rand.Next(-30, 30);
            time_date = randomValue;
        }

        
    }
}

       