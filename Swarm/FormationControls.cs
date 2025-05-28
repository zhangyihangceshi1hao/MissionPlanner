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

namespace MissionPlanner.Swarm
{
    [PreventTheming]
    public partial class FormationControls : Form
    {
        Formation SwarmInterface = null;
        bool threadrun = false;
        
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
            //flowLayoutPanel1.AutoSize = true;
            //flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.AutoSize = false;  // 关键！
            //flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            //flowLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            //flowLayoutPanel1.Name = "flowLayoutPanel1";
            //flowLayoutPanel1.Size = new System.Drawing.Size(250, 400); // 固定尺寸
            //flowLayoutPanel1.TabIndex = 0;
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
            if (SwarmInterface != null)
            {
                SwarmInterface.Arm();
            }
        }

        private void BUT_Disarm_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Disarm();
            }
        }

        private void BUT_Takeoff_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Takeoff();
            }
        }

        private void BUT_Land_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Land();
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

                    if (Math.Abs(offset.x) < 200 && Math.Abs(offset.y) < 200)
                    {
                        grid1.UpdateIcon(mav, (float)offset.y, (float)offset.x, (float)offset.z, true);
                        ((Formation)SwarmInterface).setOffsets(mav, offset.y, offset.x, offset.z);
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
                            ((Status)ctl).Location1.Text = mav.cs.lat + "," + mav.cs.lng + "," +
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
                SwarmInterface.GuidedMode();
            }
        }

        private void but_auto_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.AutoMode();
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
            //CollectiveFormationNumber();

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
                    RemoveItemFromComboBox(i+1);
                    break;
                }
            }

            if (lastPage != null)
            {
                tabControl1.TabPages.Remove(lastPage);
            }

            
            //CollectiveFormationNumber();

        }

        private void myButton3_Click(object sender, EventArgs e)
        {
            // 获取当前选中的编队编号
            int selectedComBox = int.Parse(comboBox1.Text);

            // 获取选中的无人机ID列表
            List<int> selectedUAVIds = GetSelectedUAVs();

            if (selectedUAVIds == null || !selectedUAVIds.Any())
            {
                return; // 如果没有选中任何 UAV，直接返回
            }

            // 筛选符合要求的无人机，并排序
            var filteredMavs = MainV2.Comports
                .SelectMany(port => port.MAVlist)
                .Where(mav => selectedUAVIds.Contains(mav.sysid))
                .OrderBy(mav => mav.sysid) // 按 sysid 排序
                .ToList();

            // ✅ 清空原有布局
            flowLayoutPanel2.Controls.Clear();

            // 开始添加新的 UAV 显示项
            foreach (var mav in filteredMavs)
            {
                int sysid = mav.sysid;

                // 创建 Label
                Label label = new Label();
                label.Text = $"无人机{sysid}号";
                label.AutoSize = true;
                label.Name = "select_checkBox_uav" + sysid;

                // 创建删除按钮（可选）
                Button deleteBtn = new Button();
                deleteBtn.Text = "X";
                deleteBtn.Width = 30;
                deleteBtn.Height = 20;
                deleteBtn.Click += (s, ev) =>
                {
                    RemoveUAVFromFlowLayoutPanel(sysid);
                };

                // 组合布局
                FlowLayoutPanel clusterPanel = new FlowLayoutPanel();
                clusterPanel.FlowDirection = FlowDirection.LeftToRight;
                clusterPanel.AutoSize = true;
                clusterPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                clusterPanel.Padding = new Padding(5);
                clusterPanel.Margin = new Padding(0);
                clusterPanel.Controls.Add(label);
                clusterPanel.Controls.Add(deleteBtn);

                // 分隔线
                Label separator = new Label();
                separator.BackColor = Color.LightGray;
                separator.Height = 2;
                separator.Dock = DockStyle.Fill;

                // 添加到主容器
                flowLayoutPanel2.Controls.Add(clusterPanel);
                flowLayoutPanel2.Controls.Add(separator);
            }

            // 更新布局
            flowLayoutPanel2.ResumeLayout();
            flowLayoutPanel2.PerformLayout();
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
                if (ctl is FlowLayoutPanel clusterPanel)
                {
                    foreach (Control subCtl in clusterPanel.Controls)
                    {
                        if (subCtl is Label cb && cb.Name == $"select_checkBox_uav{sysid}")
                        {
                            flowLayoutPanel2.Controls.Remove(clusterPanel);
                            return;
                        }
                    }
                }
            }
        }
        private void RemoveUAVFromFlowLayoutPanelList(object sender, EventArgs e)
        {
            List<int> selectedUAVIds = GetSelectedUAVs();
            foreach (int sysid in selectedUAVIds)
            {
                RemoveUAVFromFlowLayoutPanel(sysid);
            }


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
    }
}