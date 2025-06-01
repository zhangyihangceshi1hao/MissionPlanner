using MissionPlanner.Utilities;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using System.Text.RegularExpressions;
using static IronPython.Modules.PythonRegex;
using Match = System.Text.RegularExpressions.Match;
using System.Linq;
using DotSpatial.Data;
using Accord.Imaging.Filters;

namespace MissionPlanner.Swarm
{
    
  
    [PreventTheming]
    public partial class FormationControls : Form
    {

        public Dictionary<int,Swarms> swarmsDictionary = new Dictionary<int,Swarms>();

        Formation SwarmInterface = null;
        bool threadrun = false;
        public bool Vertical { get; set; }
        public bool Vertical2 { get; set; }
        public FormationControls()
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
            comboxitem_Load();
            CMB_mavs.DataSource = bindingSource1;
            CMB_mavs.ValueMember = "Value";
            CMB_mavs.DisplayMember = "Key";
            //初始化Swarms类
            swarmsDictionary.Add(1, new Swarms(1, grid1, new Formation(), new List<MAVState>()));
            updateicons();
            

            this.MouseWheel += new MouseEventHandler(FollowLeaderControl_MouseWheel);

            MessageBox.Show("this is beta, use at own risk");

            MissionPlanner.Utilities.Tracking.AddPage(this.GetType().ToString(), this.Text);

            AddCheckboxesToFlowLayoutPanel();
        }
      
        // 添加新数据的方法
        private void AddNewItemToComboBox(int newItem)
        {
            // 确保操作在UI线程上执行
            if (comboBox1.InvokeRequired)
            {
                comboBox1.Invoke(new Action<int>(AddNewItemToComboBox), newItem);
                return;
            }

            // 获取当前数据源
            var dataSource = bindingSource2.DataSource as List<int>;

            // 如果数据源不存在则创建
            if (dataSource == null)
            {
                dataSource = new List<int>();
            }

            // 添加新项（如果不存在）
            if (!dataSource.Contains(newItem))
            {
                // 挂起UI更新
                comboBox1.BeginUpdate();
                bindingSource2.SuspendBinding();

                try
                {
                    // 添加新项
                    dataSource.Add(newItem);

                    // 重新排序（可选）
                    dataSource = dataSource.OrderBy(x => x).ToList();

                    // 重新绑定数据源
                    bindingSource2.DataSource = null;
                    bindingSource2.DataSource = dataSource;

                    // 设置选中新添加的项
                    comboBox1.SelectedItem = newItem;
                }
                finally
                {
                    // 恢复UI更新
                    bindingSource2.ResumeBinding();
                    comboBox1.EndUpdate();
                }
            }
            else
            {
                // 如果已存在，直接选中该项
                comboBox1.SelectedItem = newItem;
            }
        }
        private void RemoveItemFromComboBox(int itemToRemove)
        {
            if (comboBox1.InvokeRequired)
            {
                comboBox1.Invoke(new Action<int>(RemoveItemFromComboBox), itemToRemove);
                return;
            }

            var dataSource = bindingSource2.DataSource as List<int>;

            if (dataSource != null)
            {
                // 挂起UI更新
                comboBox1.BeginUpdate();
                bindingSource2.SuspendBinding();

                try
                {
                    // 移除指定项
                    if (dataSource.Contains(itemToRemove))
                    {
                        dataSource.Remove(itemToRemove);
                    }

                    // 重新绑定数据源
                    bindingSource2.DataSource = null;
                    bindingSource2.DataSource = dataSource;

                    // 设置默认选中项（如第一个）
                    if (comboBox1.Items.Count > 0)
                    {
                        comboBox1.SelectedIndex = 0;
                    }
                }
                finally
                {
                    // 恢复UI更新
                    bindingSource2.ResumeBinding();
                    comboBox1.EndUpdate();
                }
            }
        }
        // 修改后的加载方法（添加默认项）
        private void comboxitem_Load()
        {
            // 获取当前数据源
            var dataSource = bindingSource2.DataSource as List<int>;

            // 初始化或添加默认值
            if (dataSource == null)
            {
                dataSource = new List<int>();
            }

            // 添加默认值1（如果不存在）
            if (!dataSource.Contains(1))
            {
                dataSource.Add(1);
            }

            // 重新绑定
            bindingSource2.DataSource = null;
            bindingSource2.DataSource = dataSource;

            // 设置选中项
            comboBox1.SelectedItem = 1;
        }

        // 示例调用 - 在需要添加新数据的地方调用
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // 示例：添加新数字5
            AddNewItemToComboBox(5);
        
        }
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            RemoveItemFromComboBox(2);
        }
        private void AddCheckboxesToFlowLayoutPanel()
        {
            // 假设 flowLayoutPanel1 已经存在并初始化
            flowLayoutPanel1.Controls.Clear(); // 清空原有控件
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.AutoSize = false;  // 关键！
            flowLayoutPanel1.WrapContents = false; // 禁用换行
            // 收集所有 MAV 并按 sysid 排序
            var allMavs = MainV2.Comports
                .SelectMany(port => port.MAVlist)
                .OrderBy(mav => mav.sysid)
                .ToList();
            // 按排序后的顺序添加 CheckBox
            foreach (var mav in allMavs)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Text = "无人机" + mav.sysid + "号";
                checkBox.AutoSize = true;
                checkBox.Name = "checkBox_uav" + mav.sysid;
                checkBox.CheckedChanged += CheckBox_CheckedChanged;             

                flowLayoutPanel1.Controls.Add(checkBox);
                flowLayoutPanel1.Controls.Add(checkBox);
                flowLayoutPanel1.Controls.Add(checkBox);
            }
            
            flowLayoutPanel1.ResumeLayout(); // 恢复布局更新
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox cb)
            {
                Console.WriteLine($"CheckBox {cb.Name} 状态: {cb.Checked}");
            }
        }
        void FollowLeaderControl_MouseWheel(object sender, MouseEventArgs e)
        {
            int number1 = ExtractNumber(tabControl1.SelectedTab.Name);


            // 假设 grid1 是添加在某个容器中的控件（如 this 或 tabPage1）
            string controlName = "grid"+ number1;
            //int scaleValue = 100;

            // 查找控件
            Control[] foundControls = this.Controls.Find(controlName, true);

            if (foundControls.Length > 0 && foundControls[0] is Grid targetGrid)
            {
                if (e.Delta < 0)
                {
                    targetGrid.setScale(targetGrid.getScale() + 4);
                }
                else
                {
                    targetGrid.setScale(targetGrid.getScale() - 4);
                }
            }
            else
            {
                MessageBox.Show($"未找到名为 {controlName} 的 Grid 控件");
            }
        }
        public static int ExtractNumber(string input)
        {
            // 匹配字符串结尾的数字部分
            Match match = Regex.Match(input, @"\d+$");

            if (match.Success)
            {
                return int.Parse(match.Value);
            }
            else
            {
                throw new ArgumentException("字符串中未找到数字部分");
            }
        }
        void updateicons()
        {
            bindingSource1.ResetBindings(false);
            int swarm_id = int.Parse(comboBox1.Text==""?"1": comboBox1.Text);
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {

                    if (swarmsDictionary[swarm_id].SwarmList.Contains(mav))
                    {
                        //swarmsDictionary[swarm_id].SwarmsInterface.setLeader(mav);

                        if (mav == swarmsDictionary[swarm_id].SwarmsInterface.getLeader())
                        {
                            if (mav == CMB_mavs.SelectedValue)
                            {
                                ((Formation)swarmsDictionary[swarm_id].SwarmsInterface).setOffsets(mav, 0, 0, 0);
                                var vector = swarmsDictionary[swarm_id].SwarmsInterface.getOffsets(mav);
                                swarmsDictionary[swarm_id].Grid.UpdateIcon(mav, (float)vector.x, (float)vector.y, (float)vector.z, false);

                            }
                        }
                        else
                        {
                            var vector = swarmsDictionary[swarm_id].SwarmsInterface.getOffsets(mav);
                            swarmsDictionary[swarm_id].Grid.UpdateIcon(mav, (float)vector.x, (float)vector.y, (float)vector.z, true);
                        }
                    }
                }
            }
            swarmsDictionary[swarm_id].Grid.Invalidate();

            //bindingSource1.ResetBindings(false);

            //foreach (var port in MainV2.Comports)
            //{
            //    foreach (var mav in port.MAVlist)
            //    {
            //        if (mav == SwarmInterface.getLeader())
            //        {
            //            ((Formation)SwarmInterface).setOffsets(mav, 0, 0, 0);
            //            var vector = SwarmInterface.getOffsets(mav);
            //            grid1.UpdateIcon(mav, (float)vector.x, (float)vector.y, (float)vector.z, false);
            //        }
            //        else
            //        {
            //            var vector = SwarmInterface.getOffsets(mav);
            //            grid1.UpdateIcon(mav, (float)vector.x, (float)vector.y, (float)vector.z, true);
            //        }
            //    }
            //}
            //grid1.Invalidate();
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

            if (!Vertical2)
            {
                int swarm_id = int.Parse(comboBox1.Text == "" ? "1" : comboBox1.Text);
                //swarmsDictionary[swarm_id].SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
                //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateposition = 10;
                //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateattitude = 10;
             
                if (swarmsDictionary[swarm_id].SwarmsInterface != null)
                {
                 
                    new System.Threading.Thread(() => mainloop(swarm_id)) { IsBackground = true }.Start();
                    BUT_Start.Text = Strings.Stop;
                }
            }
            else {

                string param = "Hello from thread";
                new System.Threading.Thread(() => mainloop1(param)) { IsBackground = true }.Start();
                BUT_Start.Text = Strings.Stop;
            }
        }
        private void mainloop1(object obj)
        {
            threadrun = true;
            string message = (string)obj;
            Console.WriteLine("线程开始运行：" + message);

            foreach (var swarms in swarmsDictionary.Values)
            {
                // 捕获当前 swarm 对象，避免闭包问题
                var currentSwarms = swarms;

                // 启动线程并传参
                System.Threading.Thread thread = new System.Threading.Thread(() =>
                {
                    // make sure leader is high freq updates
                    int swarm_id = currentSwarms.SwarmId;
                    currentSwarms.SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
                    currentSwarms.SwarmsInterface.Leader.cs.rateposition = 10;
                    currentSwarms.SwarmsInterface.Leader.cs.rateattitude = 10;

                    while (threadrun && !this.IsDisposed)
                    {
                        // update leader pos
                        currentSwarms.SwarmsInterface.UpdateSwarms(currentSwarms.SwarmsInterface.Leader);

                        // update other mavs
                        currentSwarms.SwarmsInterface.SendCommandSwarms(currentSwarms.SwarmList);

                        // 10 hz
                        System.Threading.Thread.Sleep(100);
                    }
                });

                thread.IsBackground = true; // 设置为后台线程
                thread.Start();
            }
            
        }

        void mainloop(int swarm_id)
        {

                threadrun = true;
           
                // make sure leader is high freq updates
               
                swarmsDictionary[swarm_id].SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
                swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateposition = 10;
                swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateattitude = 10;

                while (threadrun && !this.IsDisposed)
                {
                    // update leader pos
                    swarmsDictionary[swarm_id].SwarmsInterface.UpdateSwarms(swarmsDictionary[swarm_id].SwarmsInterface.Leader);

                    // update other mavs
                    swarmsDictionary[swarm_id].SwarmsInterface.SendCommandSwarms(swarmsDictionary[swarm_id].SwarmList);

                    // 10 hz
                    System.Threading.Thread.Sleep(100);
                }
            
         
        }

        private void BUT_Arm_Click(object sender, EventArgs e)
        {
            int swarm_id = int.Parse(comboBox1.Text == "" ? "1" : comboBox1.Text);


            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateposition = 10;
            //swarmsDictionary[swarm_id].swarmList.Contains();
            //swarmsDictionary[swarm_id].SwarmList.Contains(mav)
            if (swarmsDictionary[swarm_id].SwarmsInterface != null)
            {
                swarmsDictionary[swarm_id].SwarmsInterface.ArmSwarms(swarmsDictionary[swarm_id].SwarmList, Vertical, Vertical2);
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Vertical = checkBox1.Checked;
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Vertical2 = checkBox2.Checked;
        }
        private void BUT_Disarm_Click(object sender, EventArgs e)
        {
            int swarm_id = int.Parse(comboBox1.Text == "" ? "1" : comboBox1.Text);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateposition = 10;
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateattitude = 10;
            if (swarmsDictionary[swarm_id].SwarmsInterface != null)
            {
                swarmsDictionary[swarm_id].SwarmsInterface.DisarmSwarms(swarmsDictionary[swarm_id].SwarmList, Vertical, Vertical2);
            }
        }

        private void BUT_Takeoff_Click(object sender, EventArgs e)
        {
            int swarm_id = int.Parse(comboBox1.Text == "" ? "1" : comboBox1.Text);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateposition = 10;
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateattitude = 10;
            if (swarmsDictionary[swarm_id].SwarmsInterface != null)
            {
                swarmsDictionary[swarm_id].SwarmsInterface.TakeoffSwarms(swarmsDictionary[swarm_id].SwarmList, Vertical, Vertical2, float.Parse(textBox1.Text==""?"5": textBox1.Text));
            }
        }

        private void BUT_Land_Click(object sender, EventArgs e)
        {
            int swarm_id = int.Parse(comboBox1.Text == "" ? "1" : comboBox1.Text);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateposition = 10;
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateattitude = 10;
            if (swarmsDictionary[swarm_id].SwarmsInterface != null)
            {
                swarmsDictionary[swarm_id].SwarmsInterface.LandSwarms(swarmsDictionary[swarm_id].SwarmList, Vertical, Vertical2);
            }
        }
        private void BUT_Rtl_Click(object sender, EventArgs e)
        {
            int swarm_id = int.Parse(comboBox1.Text == "" ? "1" : comboBox1.Text);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateposition = 10;
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateattitude = 10;
            if (swarmsDictionary[swarm_id].SwarmsInterface != null)
            {
                swarmsDictionary[swarm_id].SwarmsInterface.RTL_ALL_Swarms(swarmsDictionary[swarm_id].SwarmList, Vertical, Vertical2);
            }
        }

        private void BUT_Rtl_successively_Click(object sender, EventArgs e)
        {
            int swarm_id = int.Parse(comboBox1.Text == "" ? "1" : comboBox1.Text);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateposition = 10;
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateattitude = 10;
            if (swarmsDictionary[swarm_id].SwarmsInterface != null)
            {
                int a = int.Parse(textBox2.Text==""?"0": textBox2.Text);
                swarmsDictionary[swarm_id].SwarmsInterface.Rtl_successively_ALL(swarmsDictionary[swarm_id].SwarmList, Vertical, Vertical2, a);
            }
        }
        private void BUT_Brake_Click(object sender, EventArgs e)
        {
            int swarm_id = int.Parse(comboBox1.Text == "" ? "1" : comboBox1.Text);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateposition = 10;
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateattitude = 10;
            if (swarmsDictionary[swarm_id].SwarmsInterface != null)
            {              
                swarmsDictionary[swarm_id].SwarmsInterface.Brake_ALL(swarmsDictionary[swarm_id].SwarmList, Vertical, Vertical2);
            }
        }
        private void BUT_leader_Click(object sender, EventArgs e)
        {

            //if (SwarmInterface != null)
            //{
            //    var vectorlead = SwarmInterface.getOffsets(MainV2.comPort.MAV);

            //    foreach (var port in MainV2.Comports)
            //    {
            //        foreach (var mav in port.MAVlist)
            //        {
            //            var vector = SwarmInterface.getOffsets(mav);

            //            SwarmInterface.setOffsets(mav, (float)(vector.x - vectorlead.x),
            //                (float)(vector.y - vectorlead.y),
            //                (float)(vector.z - vectorlead.z));
            //        }
            //    }

            //    SwarmInterface.setLeader(MainV2.comPort.MAV);
            //    updateicons();
            //    BUT_Start.Enabled = true;
            //    BUT_Updatepos.Enabled = true;
            //}

            int swarm_id = int.Parse(comboBox1.Text);
            MAVState targetMav = MainV2.Comports
                 .SelectMany(port => port.MAVlist)  // 合并所有 MAVlist 列表
                 .FirstOrDefault(mav => mav == CMB_mavs.SelectedValue);  // 查找 sysid 匹配的 MAVState
          
            if (swarmsDictionary[swarm_id].SwarmsInterface != null)
            {
                var vectorlead = swarmsDictionary[swarm_id].SwarmsInterface.getOffsets(targetMav);

                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        var vector = swarmsDictionary[swarm_id].SwarmsInterface.getOffsets(mav);

                        swarmsDictionary[swarm_id].SwarmsInterface.setOffsets(mav, (float)(vector.x - vectorlead.x),
                            (float)(vector.y - vectorlead.y),
                            (float)(vector.z - vectorlead.z));

                    }
                }
                swarmsDictionary[swarm_id].SwarmsInterface.setLeader(targetMav);
                updateicons();
                BUT_Start.Enabled = true;
                BUT_Updatepos.Enabled = true;
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


           
            int swarm_id = int.Parse(comboBox1.Text == "" ? "1" : comboBox1.Text);
            if (mav == swarmsDictionary[swarm_id].SwarmsInterface.Leader)
            {
                CustomMessageBox.Show("Can not move Leader");
                ico.z = 0;
            }
            else {
                ((Formation)swarmsDictionary[swarm_id].SwarmsInterface).setOffsets(mav, x, y, z);
            }
        }

        private void Control_FormClosing(object sender, FormClosingEventArgs e)
        {
            threadrun = false;
        }

        private void BUT_Updatepos_Click(object sender, EventArgs e)
        {
            int swarm_id = int.Parse(comboBox1.Text == "" ? "1" : comboBox1.Text);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateposition = 10;
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateattitude = 10;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (swarmsDictionary[swarm_id].SwarmList.Contains(mav))
                    {
                        mav.cs.UpdateCurrentSettings(null, true, port, mav);

                        if (mav == swarmsDictionary[swarm_id].SwarmsInterface.Leader)
                            continue;

                        Vector3 offset = getOffsetFromLeader(((Formation)swarmsDictionary[swarm_id].SwarmsInterface).getLeader(), mav);

                        if (Math.Abs(offset.x) < 200 && Math.Abs(offset.y) < 200)
                        {
                            swarmsDictionary[swarm_id].Grid.UpdateIcon(mav, (float)offset.y, (float)offset.x, (float)offset.z, true);
                            ((Formation)swarmsDictionary[swarm_id].SwarmsInterface).setOffsets(mav, offset.y, offset.x, offset.z);
                        }
                    }
                }
            }
        }

        //private void timer_status_Tick(object sender, EventArgs e)
        //{
        //    // clean up old
        //    foreach (Control ctl in PNL_status.Controls)
        //    {
        //        bool match = false;
        //        foreach (var port in MainV2.Comports)
        //        {
        //            foreach (var mav in port.MAVlist)
        //            {
        //                if (mav == (MAVState)ctl.Tag)
        //                {
        //                    match = true;

        //                }
        //            }
        //        }

        //        if (match == false)
        //            ctl.Dispose();
        //    }

        //    // setup new
        //    foreach (var port in MainV2.Comports)
        //    {
        //        foreach (var mav in port.MAVlist)
        //        {
        //            bool exists = false;
        //            foreach (Control ctl in PNL_status.Controls)
        //            {
        //                if (ctl is Status && ctl.Tag == mav)
        //                {
        //                    exists = true;
        //                    ((Status)ctl).GPS.Text = mav.cs.gpsstatus >= 3 ? "OK" : "Bad";
        //                    ((Status)ctl).Armed.Text = mav.cs.armed.ToString();
        //                    ((Status)ctl).Mode.Text = mav.cs.mode;
        //                    ((Status)ctl).MAV.Text = mav.ToString();
        //                    ((Status)ctl).Guided.Text = mav.GuidedMode.x / 1e7 + "," + mav.GuidedMode.y / 1e7 + "," +
        //                                                 mav.GuidedMode.z;
        //                    ((Status)ctl).Location1.Text = mav.cs.lat + "," + mav.cs.lng + "," +
        //                                                    mav.cs.alt;

        //                    if (mav == SwarmInterface.Leader)
        //                    {
        //                        ((Status)ctl).ForeColor = Color.Red;
        //                    }
        //                    else
        //                    {
        //                        ((Status)ctl).ForeColor = Color.Black;
        //                    }
        //                }
        //            }

        //            if (!exists)
        //            {
        //                Status newstatus = new Status();
        //                newstatus.Tag = mav;
        //                PNL_status.Controls.Add(newstatus);
        //            }
        //        }
        //    }
        //}

        private void but_guided_Click(object sender, EventArgs e)
        {
            int swarm_id = int.Parse(comboBox1.Text == "" ? "1" : comboBox1.Text);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateposition = 10;
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateattitude = 10;
            if (swarmsDictionary[swarm_id].SwarmsInterface != null)
            {
                swarmsDictionary[swarm_id].SwarmsInterface.GuidedModeSwarms(swarmsDictionary[swarm_id].SwarmList,Vertical, Vertical2);
            }

           
        }

        private void but_auto_Click(object sender, EventArgs e)
        {
            int swarm_id = int.Parse(comboBox1.Text == "" ? "1" : comboBox1.Text);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, swarmsDictionary[swarm_id].SwarmsInterface.Leader.sysid, swarmsDictionary[swarm_id].SwarmsInterface.Leader.compid);
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateposition = 10;
            //swarmsDictionary[swarm_id].SwarmsInterface.Leader.cs.rateattitude = 10;
            if (swarmsDictionary[swarm_id].SwarmsInterface != null)
            {
                swarmsDictionary[swarm_id].SwarmsInterface.AutoModeSwarms(swarmsDictionary[swarm_id].SwarmList, Vertical, Vertical2);
            }
        }

        private void AddFormationButton_Click(object sender, EventArgs e)
        {

            //tabControl1.SelectedTab.Name
            int maxIndex = 0;

            // 查找当前最大的 "编队X" 编号
            foreach (TabPage page in tabControl1.TabPages)
            {
                if (page.Text.StartsWith("编队"))
                {
                    string numStr = page.Text.Substring(2);
                    if (int.TryParse(numStr, out int index))
                    {
                        if (index > maxIndex)
                            maxIndex = index;
                    }
                }
            }

            // 创建新标签页
            System.Windows.Forms.TabPage tabPage = new System.Windows.Forms.TabPage();
            tabPage.Text = "编队" + (maxIndex + 1);
            tabPage.Name = "tabPage" + (maxIndex + 1);

          


            

            // 插入到 "+" 号按钮之前（如果存在）
            int insertIndex = tabControl1.TabPages.Count;
            if (tabControl1.TabPages.Count > 0 && tabControl1.TabPages[tabControl1.TabPages.Count - 1].Text == "+")
                insertIndex -= 1;
            // 创建 Grid 并设置 ForeColor
            MissionPlanner.Swarm.Grid grid = new MissionPlanner.Swarm.Grid();
            // 
            // grid2
            // 
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Location = new System.Drawing.Point(3, 3);
            grid.Name = "grid" + (maxIndex + 1);
            grid.Size = new System.Drawing.Size(1224, 549);
            grid.TabIndex = 8;
            grid.Vertical = false;
            grid.UpdateOffsets += new MissionPlanner.Swarm.Grid.UpdateOffsetsEvent(this.grid1_UpdateOffsets);

            //tabPage.Controls.Add(grid);
            tabPage.Location = new System.Drawing.Point(4, 28);
            tabPage.Name = "formationTabPage" + (maxIndex + 1);
            tabPage.Padding = new System.Windows.Forms.Padding(3);
            tabPage.Size = new System.Drawing.Size(1230, 555);
            tabPage.TabIndex = 0;
            tabPage.Text = "编队" + (maxIndex + 1);
            tabPage.UseVisualStyleBackColor = true;

            tabPage.Controls.Add(grid);
            tabControl1.TabPages.Insert(insertIndex, tabPage);

            AddNewItemToComboBox(maxIndex + 1);
            List<MAVState> mavStateList = new List<MAVState>();
           
            //CollectiveFormationNumber();
            //添加到swarmsDictionary集合中
            //foreach (var port in MainV2.Comports)
            //{
            //    foreach (var mav in port.MAVlist)
            //    {
            //        if () {
                        //mavStateList.Add(mav);
                        //mavStates.Add(port.BaseStream.PortName + " " + mav.sysid + " " + mav.compid, mav);
                        swarmsDictionary.Add((maxIndex + 1), new Swarms((maxIndex + 1), grid,new Formation(), mavStateList));
            //}
            //}
            //}


            
            updateicons();
        }

        private void RemoveFormationButton_Click(object sender, EventArgs e)
        {
            // 确保至少保留一个 "编队" 标签页
            if (tabControl1.TabPages.Count <= 1)
            {
                MessageBox.Show("至少保留一个编队标签页！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 查找最后一个有效的 "编队X" 标签页
            TabPage lastPage = null;
            for (int i = tabControl1.TabPages.Count - 1; i >= 0; i--)
            {
                if (tabControl1.TabPages[i].Text.StartsWith("编队"))
                {
                    lastPage = tabControl1.TabPages[i];
                    swarmsDictionary.Remove(i+1);
                    RemoveItemFromComboBox(i+1);
                    RemovePanel(i+1);
                    break;
                }
            }

            if (lastPage != null)
            {
                tabControl1.TabPages.Remove(lastPage);
            }

           
            //CollectiveFormationNumber();

        }
        // 定义一个类级变量来保存所有 Panel 和其对应编号
        private List<(int tabId, Panel panel)> panelList = new List<(int, Panel)>();

        private void myButton3_Click(object sender, EventArgs e)
        {
            // 获取当前选中的编队编号
            if (!int.TryParse(comboBox1.Text, out int tabPageNumber))
                return;

            // 尝试从 panelList 中查找是否已存在该编队 Panel
            var existing = panelList.FirstOrDefault(x => x.tabId == tabPageNumber);

            // 创建新的分隔 Panel（无论是否存在都新建）
            Panel separator = new Panel();
            separator.BackColor = Color.LightGray;
            separator.AutoSize = false;
            separator.Name = "panel_uav" + tabPageNumber;
            separator.Width = flowLayoutPanel2.Width / 3;
            separator.Height = flowLayoutPanel2.Height;
            separator.Padding = new Padding(5);
            separator.BorderStyle = BorderStyle.FixedSingle;

            // 创建 FlowLayoutPanel 用于垂直排列内容
            FlowLayoutPanel contentPanel = new FlowLayoutPanel();
            contentPanel.FlowDirection = FlowDirection.TopDown; // 从上往下排
            contentPanel.WrapContents = false;                 // 不换行
            contentPanel.Dock = DockStyle.Fill;                // 填满容器
            contentPanel.AutoSize = true;
            contentPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // 创建 Label 显示编队编号
            Label label = new Label();
            label.Text = $"编队 {tabPageNumber}";
            label.AutoSize = true;
            label.Font = new Font("微软雅黑", 9, FontStyle.Bold);
            label.ForeColor = Color.DarkBlue;
            label.TextAlign = ContentAlignment.MiddleLeft;

            // 添加到 FlowLayoutPanel
            contentPanel.Controls.Add(label);

            // 获取选中的无人机ID列表
            List<int> selectedUAVIds = GetSelectedUAVs();

            if (selectedUAVIds == null || !selectedUAVIds.Any())
            {
                return; // 如果没有选中任何 UAV，直接返回
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (selectedUAVIds.Contains(mav.sysid))
                    {
                        swarmsDictionary[int.Parse(comboBox1.Text)].SwarmList.Add(mav);
                    }
                }
            }
            // 筛选符合要求的无人机，并排序
            var filteredMavs = MainV2.Comports
                .SelectMany(port => port.MAVlist)
                .Where(mav => selectedUAVIds.Contains(mav.sysid))
                .OrderBy(mav => mav.sysid)
                .ToList();

            // 开始添加新的 UAV 显示项
            foreach (var mav in filteredMavs)
            {
                int sysid = mav.sysid;

                // 创建 Label
                Label label_uav = new Label();
                label_uav.Text = $"无人机{sysid}号";
                label_uav.AutoSize = true;
                label_uav.Name = "select_checkBox_uav" + sysid;

                // 添加到 FlowLayoutPanel
                contentPanel.Controls.Add(label_uav);
            }

            // 将 FlowLayoutPanel 添加到 Panel 中
            separator.Controls.Add(contentPanel);

            // 判断是否已存在该编队 Panel
            if (existing.panel != null)
            {
                // 存在：移除旧的 Panel
                panelList.Remove(existing);

                // 可选：从 flowLayoutPanel2 中移除控件
                if (flowLayoutPanel2.Controls.Contains(existing.panel))
                {
                    flowLayoutPanel2.Controls.Remove(existing.panel);
                }
            }

            // 添加新的 Panel
            panelList.Add((tabPageNumber, separator));

            // 对 panelList 按 tabId 升序排序
            var sortedList = panelList.OrderBy(x => x.tabId).ToList();

            // 清空 flowLayoutPanel2
            flowLayoutPanel2.Controls.Clear();

            // 按照排序后的顺序重新添加 Panel
            foreach (var item in sortedList)
            {
                flowLayoutPanel2.Controls.Add(item.panel);
            }

            // 更新布局
            flowLayoutPanel2.ResumeLayout();
            flowLayoutPanel2.PerformLayout();

            // 绑定 Resize 事件（可选）
            foreach (var item in sortedList)
            {
                flowLayoutPanel2.Resize += (s, ev) =>
                {
                    item.panel.Width = flowLayoutPanel2.Width / 3;
                    item.panel.Height = flowLayoutPanel2.Height;
                };
            }


            //保存swarmsDictionary

            swarmsDictionary[tabPageNumber].SwarmList = filteredMavs;

         
            
        }
        //private void myButton3_Click(object sender, EventArgs e)
        //{
        //    //int tabPageNumber = tabControl1.TabPages.Count;

        //    int tabPageNumber  =  int.Parse(comboBox1.Text);
        //    //for(int i = 1; i <= tabPageNumber; i++) {
        //    Panel separator = new Panel();
        //    separator.BackColor = Color.LightGray;
        //    separator.AutoSize = false;
        //    separator.Name = "pannel_uav" + tabPageNumber;
        //    // 初始设置
        //    separator.Width = flowLayoutPanel2.Width / 4;
        //    separator.Height = flowLayoutPanel2.Height;
        //    // 创建 Label 显示编队编号
        //    Label label = new Label();
        //    label.Text = $"编队 { tabPageNumber }";
        //    label.AutoSize = true;
        //    label.Font = new Font("微软雅黑", 9, FontStyle.Bold);
        //    label.ForeColor = Color.DarkBlue;

        //    // 将 Label 添加到 Panel 中
        //    separator.Controls.Add(label);
        //    // 添加到 flowLayoutPanel2
        //    flowLayoutPanel2.Controls.Add(separator);

        //    // 监听 flowLayoutPanel2 的 Resize 事件
        //    flowLayoutPanel2.Resize += (s, ev) =>
        //    {
        //        separator.Width = flowLayoutPanel2.Width/4;
        //        separator.Height = flowLayoutPanel2.Height;
        //    };


        //    //}
        //    /***
        //    // 获取当前选中的编队编号
        //    int selectedComBox = int.Parse(comboBox1.Text);

        //    // 获取选中的无人机ID列表
        //    List<int> selectedUAVIds = GetSelectedUAVs();

        //    if (selectedUAVIds == null || !selectedUAVIds.Any())
        //    {
        //        return; // 如果没有选中任何 UAV，直接返回
        //    }

        //    // 筛选符合要求的无人机，并排序
        //    var filteredMavs = MainV2.Comports
        //        .SelectMany(port => port.MAVlist)
        //        .Where(mav => selectedUAVIds.Contains(mav.sysid))
        //        .OrderBy(mav => mav.sysid) // 按 sysid 排序
        //        .ToList();

        //    // ✅ 清空原有布局
        //    flowLayoutPanel2.Controls.Clear();
        //    //// 创建 Label
        //    //Label labelbase = new Label();
        //    //labelbase.Text = $"集群{selectedComBox}";
        //    //labelbase.AutoSize = true;
        //    //labelbase.Name = "select_checkBox_uav";
        //    //labelbase.BackColor = Color.Blue; // 偶数时的背景色
        //    //flowLayoutPanel2.Controls.Add(labelbase);
        //    // 开始添加新的 UAV 显示项
        //    foreach (var mav in filteredMavs)
        //    {
        //        int sysid = mav.sysid;

        //        // 创建 Label
        //        Label label = new Label();
        //        label.Text = $"无人机{sysid}号";
        //        label.AutoSize = true;
        //        label.Name = "select_checkBox_uav" + sysid;
        //        // 根据 selectedComBox 的奇偶性设置背景色
        //        SetLabelBackgroundColor(label, selectedComBox);
        //        // 添加到主容器
        //        flowLayoutPanel2.Controls.Add(label);       
        //    }
        //    // 添加分隔符（如果至少有一个无人机）
        //    if (flowLayoutPanel2.Controls.Count > 0)
        //    {
        //        Panel separator = new Panel();
        //        separator.BackColor = Color.LightGray;
        //        separator.Height = 2;
        //        separator.AutoSize = false;
        //        separator.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        //        separator.MinimumSize = new Size(flowLayoutPanel2.Width - 3, 2);
        //        separator.Margin = new Padding(0);

        //        flowLayoutPanel2.Controls.Add(separator);
        //    }
        //    // 更新布局
        //    flowLayoutPanel2.ResumeLayout();
        //    flowLayoutPanel2.PerformLayout();
        //    ***/
        //}
        // 辅助方法：根据 selectedComBox 的奇偶性设置 Label 的背景色
        private void SetLabelBackgroundColor(Label label, int selectedComBox)
        {
            if (selectedComBox % 2 == 0)
            {
                label.BackColor = Color.LightBlue; // 偶数时的背景色
            }
            else
            {
                label.BackColor = Color.LightYellow; // 奇数时的背景色
            }
        }
        // 删除指定sysid的无人机
        private void RemoveUAVFromFlowLayoutPanel(int sysid)
        {
            List<int> selectedUAVIds = GetSelectedUAVs();

            // 筛选符合要求的无人机
            var filteredMavs = MainV2.Comports
                .SelectMany(port => port.MAVlist)
                .Where(mav => selectedUAVIds.Contains(mav.sysid))
                .OrderBy(mav => mav.sysid)
                .ToList();

            foreach (Control ctl in flowLayoutPanel2.Controls)
            {
               
                    {
                        if (ctl is Label cb && cb.Name == $"select_checkBox_uav{sysid}")
                        {
                            flowLayoutPanel2.Controls.Remove(ctl);
                            return;
                        }
                    }
                
            }
        }
        private void RemoveUAVFromFlowLayoutPanelList(object sender, EventArgs e)
        {
            RemovePanel(int.Parse(comboBox2.Text));
            //swarmsDictionary.Remove(int.Parse(comboBox2.Text));
            swarmsDictionary[int.Parse(comboBox2.Text)].SwarmList = new List<MAVState>();

        }
        /// <summary>
        /// 删除指定编队编号的 Panel
        /// </summary>
        /// <param name="tabId">要删除的编队编号</param>
        private void RemovePanel(int tabId)
        {
            // 查找是否存在该 Panel
            var itemToRemove = panelList.FirstOrDefault(x => x.tabId == tabId);

            if (itemToRemove.panel != null)
            {
                // 从 flowLayoutPanel2 控件集合中移除
                if (flowLayoutPanel2.Controls.Contains(itemToRemove.panel))
                {
                    flowLayoutPanel2.Controls.Remove(itemToRemove.panel);
                }

                // 从 panelList 中移除
                panelList.Remove(itemToRemove);

                // 刷新布局
                RefreshFlowLayoutPanel();
            }
            else
            {
                // 可选：提示用户未找到该 Panel
                // MessageBox.Show($"未找到编队 {tabId} 的面板");
            }
        }

        /// <summary>
        /// 刷新 flowLayoutPanel2，按 tabId 排序显示 panelList 中的内容
        /// </summary>
        private void RefreshFlowLayoutPanel()
        {
            // 清空当前所有控件
            flowLayoutPanel2.Controls.Clear();

            // 按 tabId 升序排序
            var sortedList = panelList.OrderBy(x => x.tabId).ToList();

            // 重新添加 Panel
            foreach (var item in sortedList)
            {
                flowLayoutPanel2.Controls.Add(item.panel);
            }

            // 更新布局
            flowLayoutPanel2.ResumeLayout();
            flowLayoutPanel2.PerformLayout();
        }
        // 获取选中的无人机ID列表
        private List<int> GetSelectedUAVs()
        {
            List<int> selectedSysIds = new List<int>();

            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is CheckBox checkBox && checkBox.Checked)
                {
                    string name = checkBox.Name;
                    if (name.StartsWith("checkBox_uav") && int.TryParse(name.Substring(12), out int sysid))
                    {
                        selectedSysIds.Add(sysid);
                    }
                }
            }

            return selectedSysIds;
        }

        public class Swarms
        {
        
            private int swarm_id;
            private Grid grid;
            Formation swarmsInterface;
            private List<MAVState> swarmList;
            // 构造函数
            public Swarms(int swarmId, Grid grid, Formation swarmsInterface, List<MAVState> swarmList)
            {
                SwarmId = swarmId;
                Grid = grid;
                SwarmsInterface = swarmsInterface;
                SwarmList = swarmList ?? new List<MAVState>(); // 防止 null
            }
            // 属性：SwarmId
            public int SwarmId
            {
                get { return swarm_id; }
                set { swarm_id = value; }
            }

            // 属性：Grid
            public Grid Grid
            {
                get { return grid; }
                set { grid = value; }
            }
             public Formation SwarmsInterface
            {
                get { return swarmsInterface; }
                set { swarmsInterface = value; }
            }

            // 属性：SwarmList
            public List<MAVState> SwarmList
            {
                get { return swarmList; }
                set { swarmList = value; }
            }
        }

       
    }
}