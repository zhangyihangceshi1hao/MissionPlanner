namespace MissionPlanner.Swarm
{
    partial class FormationControls
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.CMB_mavs = new System.Windows.Forms.ComboBox();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.grid1 = new MissionPlanner.Swarm.Grid();
            this.BUT_Start = new MissionPlanner.Controls.MyButton();
            this.BUT_leader = new MissionPlanner.Controls.MyButton();
            this.BUT_Land = new MissionPlanner.Controls.MyButton();
            this.BUT_Takeoff = new MissionPlanner.Controls.MyButton();
            this.BUT_Disarm = new MissionPlanner.Controls.MyButton();
            this.BUT_Arm = new MissionPlanner.Controls.MyButton();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.BUT_Updatepos = new MissionPlanner.Controls.MyButton();
            this.timer_status = new System.Windows.Forms.Timer(this.components);
            this.but_guided = new MissionPlanner.Controls.MyButton();
            this.but_auto = new MissionPlanner.Controls.MyButton();
            this.myButton1 = new MissionPlanner.Controls.MyButton();
            this.myButton2 = new MissionPlanner.Controls.MyButton();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.myButton3 = new MissionPlanner.Controls.MyButton();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.bindingSource2 = new System.Windows.Forms.BindingSource(this.components);
            this.myButton4 = new MissionPlanner.Controls.MyButton();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.highlightTextRenderer1 = new BrightIdeasSoftware.HighlightTextRenderer();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.myButton5 = new MissionPlanner.Controls.MyButton();
            this.myButton6 = new MissionPlanner.Controls.MyButton();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.myButton7 = new MissionPlanner.Controls.MyButton();
            this.myButton8 = new MissionPlanner.Controls.MyButton();
            this.myButton9 = new MissionPlanner.Controls.MyButton();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.myButton10 = new MissionPlanner.Controls.MyButton();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.myButton11 = new MissionPlanner.Controls.MyButton();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.myButton12 = new MissionPlanner.Controls.MyButton();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.myButton13 = new MissionPlanner.Controls.MyButton();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.myButton14 = new MissionPlanner.Controls.MyButton();
            this.textBox10 = new System.Windows.Forms.TextBox();
            this.myButton15 = new MissionPlanner.Controls.MyButton();
            this.textBox11 = new System.Windows.Forms.TextBox();
            this.myButton16 = new MissionPlanner.Controls.MyButton();
            this.textBox12 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource2)).BeginInit();
            this.SuspendLayout();
            // 
            // CMB_mavs
            // 
            this.CMB_mavs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CMB_mavs.DataSource = this.bindingSource1;
            this.CMB_mavs.FormattingEnabled = true;
            this.CMB_mavs.Location = new System.Drawing.Point(291, 575);
            this.CMB_mavs.Name = "CMB_mavs";
            this.CMB_mavs.Size = new System.Drawing.Size(156, 26);
            this.CMB_mavs.TabIndex = 4;
            this.CMB_mavs.SelectedIndexChanged += new System.EventHandler(this.CMB_mavs_SelectedIndexChanged);
            // 
            // grid1
            // 
            this.grid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid1.Location = new System.Drawing.Point(3, 3);
            this.grid1.Name = "grid1";
            this.grid1.Size = new System.Drawing.Size(851, 470);
            this.grid1.TabIndex = 8;
            this.grid1.Vertical = false;
            this.grid1.UpdateOffsets += new MissionPlanner.Swarm.Grid.UpdateOffsetsEvent(this.grid1_UpdateOffsets);
            // 
            // BUT_Start
            // 
            this.BUT_Start.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BUT_Start.Enabled = false;
            this.BUT_Start.Location = new System.Drawing.Point(210, 638);
            this.BUT_Start.Name = "BUT_Start";
            this.BUT_Start.Size = new System.Drawing.Size(75, 23);
            this.BUT_Start.TabIndex = 6;
            this.BUT_Start.Text = "开始编队";
            this.BUT_Start.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_Start.UseVisualStyleBackColor = true;
            this.BUT_Start.Click += new System.EventHandler(this.BUT_Start_Click);
            // 
            // BUT_leader
            // 
            this.BUT_leader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BUT_leader.Location = new System.Drawing.Point(210, 578);
            this.BUT_leader.Name = "BUT_leader";
            this.BUT_leader.Size = new System.Drawing.Size(75, 23);
            this.BUT_leader.TabIndex = 5;
            this.BUT_leader.Text = "设置主机";
            this.BUT_leader.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_leader.UseVisualStyleBackColor = true;
            this.BUT_leader.Click += new System.EventHandler(this.BUT_leader_Click);
            // 
            // BUT_Land
            // 
            this.BUT_Land.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BUT_Land.Location = new System.Drawing.Point(12, 636);
            this.BUT_Land.Name = "BUT_Land";
            this.BUT_Land.Size = new System.Drawing.Size(75, 23);
            this.BUT_Land.TabIndex = 3;
            this.BUT_Land.Text = "降落";
            this.BUT_Land.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_Land.UseVisualStyleBackColor = true;
            this.BUT_Land.Click += new System.EventHandler(this.BUT_Land_Click);
            // 
            // BUT_Takeoff
            // 
            this.BUT_Takeoff.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BUT_Takeoff.Location = new System.Drawing.Point(12, 607);
            this.BUT_Takeoff.Name = "BUT_Takeoff";
            this.BUT_Takeoff.Size = new System.Drawing.Size(75, 23);
            this.BUT_Takeoff.TabIndex = 2;
            this.BUT_Takeoff.Text = "起飞";
            this.BUT_Takeoff.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_Takeoff.UseVisualStyleBackColor = true;
            this.BUT_Takeoff.Click += new System.EventHandler(this.BUT_Takeoff_Click);
            // 
            // BUT_Disarm
            // 
            this.BUT_Disarm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BUT_Disarm.Location = new System.Drawing.Point(93, 578);
            this.BUT_Disarm.Name = "BUT_Disarm";
            this.BUT_Disarm.Size = new System.Drawing.Size(75, 23);
            this.BUT_Disarm.TabIndex = 1;
            this.BUT_Disarm.Text = "上锁";
            this.BUT_Disarm.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_Disarm.UseVisualStyleBackColor = true;
            this.BUT_Disarm.Click += new System.EventHandler(this.BUT_Disarm_Click);
            // 
            // BUT_Arm
            // 
            this.BUT_Arm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BUT_Arm.Location = new System.Drawing.Point(12, 578);
            this.BUT_Arm.Name = "BUT_Arm";
            this.BUT_Arm.Size = new System.Drawing.Size(75, 23);
            this.BUT_Arm.TabIndex = 0;
            this.BUT_Arm.Text = "解锁";
            this.BUT_Arm.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_Arm.UseVisualStyleBackColor = true;
            this.BUT_Arm.Click += new System.EventHandler(this.BUT_Arm_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(505, 40);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(865, 508);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.grid1);
            this.tabPage1.Location = new System.Drawing.Point(4, 28);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(857, 476);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "编队 1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // BUT_Updatepos
            // 
            this.BUT_Updatepos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BUT_Updatepos.Enabled = false;
            this.BUT_Updatepos.Location = new System.Drawing.Point(372, 607);
            this.BUT_Updatepos.Name = "BUT_Updatepos";
            this.BUT_Updatepos.Size = new System.Drawing.Size(75, 23);
            this.BUT_Updatepos.TabIndex = 10;
            this.BUT_Updatepos.Text = "更新位置";
            this.BUT_Updatepos.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.BUT_Updatepos.UseVisualStyleBackColor = true;
            this.BUT_Updatepos.Click += new System.EventHandler(this.BUT_Updatepos_Click);
            // 
            // timer_status
            // 
            this.timer_status.Enabled = true;
            this.timer_status.Interval = 200;
            // 
            // but_guided
            // 
            this.but_guided.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.but_guided.Location = new System.Drawing.Point(210, 607);
            this.but_guided.Name = "but_guided";
            this.but_guided.Size = new System.Drawing.Size(75, 23);
            this.but_guided.TabIndex = 12;
            this.but_guided.Text = "引导模式";
            this.but_guided.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.but_guided.UseVisualStyleBackColor = true;
            this.but_guided.Click += new System.EventHandler(this.but_guided_Click);
            // 
            // but_auto
            // 
            this.but_auto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.but_auto.Location = new System.Drawing.Point(291, 607);
            this.but_auto.Name = "but_auto";
            this.but_auto.Size = new System.Drawing.Size(75, 23);
            this.but_auto.TabIndex = 13;
            this.but_auto.Text = "自动模式";
            this.but_auto.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.but_auto.UseVisualStyleBackColor = true;
            this.but_auto.Click += new System.EventHandler(this.but_auto_Click);
            // 
            // myButton1
            // 
            this.myButton1.Location = new System.Drawing.Point(93, 12);
            this.myButton1.Name = "myButton1";
            this.myButton1.Size = new System.Drawing.Size(75, 23);
            this.myButton1.TabIndex = 16;
            this.myButton1.Text = "删除编队";
            this.myButton1.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton1.UseVisualStyleBackColor = true;
            this.myButton1.Click += new System.EventHandler(this.RemoveFormationButton_Click);
            // 
            // myButton2
            // 
            this.myButton2.Location = new System.Drawing.Point(12, 12);
            this.myButton2.Name = "myButton2";
            this.myButton2.Size = new System.Drawing.Size(75, 23);
            this.myButton2.TabIndex = 15;
            this.myButton2.Text = "新增编队";
            this.myButton2.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton2.UseVisualStyleBackColor = true;
            this.myButton2.Click += new System.EventHandler(this.AddFormationButton_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(1376, 68);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(100, 476);
            this.flowLayoutPanel1.TabIndex = 17;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.flowLayoutPanel2.AutoScroll = true;
            this.flowLayoutPanel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(12, 68);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(494, 476);
            this.flowLayoutPanel2.TabIndex = 18;
            this.flowLayoutPanel2.WrapContents = false;
            // 
            // myButton3
            // 
            this.myButton3.Location = new System.Drawing.Point(174, 12);
            this.myButton3.Name = "myButton3";
            this.myButton3.Size = new System.Drawing.Size(75, 23);
            this.myButton3.TabIndex = 19;
            this.myButton3.Text = "加入编队";
            this.myButton3.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton3.UseVisualStyleBackColor = true;
            this.myButton3.Click += new System.EventHandler(this.myButton3_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.DataSource = this.bindingSource2;
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(255, 12);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(75, 26);
            this.comboBox1.TabIndex = 20;
            // 
            // myButton4
            // 
            this.myButton4.Location = new System.Drawing.Point(336, 12);
            this.myButton4.Name = "myButton4";
            this.myButton4.Size = new System.Drawing.Size(75, 23);
            this.myButton4.TabIndex = 19;
            this.myButton4.Text = "删除编队";
            this.myButton4.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton4.UseVisualStyleBackColor = true;
            this.myButton4.Click += new System.EventHandler(this.RemoveUAVFromFlowLayoutPanelList);
            // 
            // comboBox2
            // 
            this.comboBox2.DataSource = this.bindingSource2;
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(417, 12);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(75, 26);
            this.comboBox2.TabIndex = 20;
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(12, 550);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(106, 22);
            this.checkBox1.TabIndex = 21;
            this.checkBox1.Text = "包含主机";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(93, 550);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(106, 22);
            this.checkBox2.TabIndex = 22;
            this.checkBox2.Text = "全部编队";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Location = new System.Drawing.Point(93, 607);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(75, 28);
            this.textBox1.TabIndex = 23;
            // 
            // myButton5
            // 
            this.myButton5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.myButton5.Location = new System.Drawing.Point(93, 636);
            this.myButton5.Name = "myButton5";
            this.myButton5.Size = new System.Drawing.Size(75, 23);
            this.myButton5.TabIndex = 24;
            this.myButton5.Text = "返航";
            this.myButton5.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton5.UseVisualStyleBackColor = true;
            this.myButton5.Click += new System.EventHandler(this.BUT_Rtl_Click);
            // 
            // myButton6
            // 
            this.myButton6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.myButton6.Location = new System.Drawing.Point(12, 665);
            this.myButton6.Name = "myButton6";
            this.myButton6.Size = new System.Drawing.Size(75, 23);
            this.myButton6.TabIndex = 25;
            this.myButton6.Text = "依次返航";
            this.myButton6.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton6.UseVisualStyleBackColor = true;
            this.myButton6.Click += new System.EventHandler(this.BUT_Rtl_successively_Click);
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox2.Location = new System.Drawing.Point(93, 664);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(75, 28);
            this.textBox2.TabIndex = 26;
            // 
            // myButton7
            // 
            this.myButton7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.myButton7.Enabled = false;
            this.myButton7.Location = new System.Drawing.Point(291, 636);
            this.myButton7.Name = "myButton7";
            this.myButton7.Size = new System.Drawing.Size(75, 23);
            this.myButton7.TabIndex = 27;
            this.myButton7.Text = "加载编队";
            this.myButton7.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton7.UseVisualStyleBackColor = true;
            // 
            // myButton8
            // 
            this.myButton8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.myButton8.Enabled = false;
            this.myButton8.Location = new System.Drawing.Point(372, 638);
            this.myButton8.Name = "myButton8";
            this.myButton8.Size = new System.Drawing.Size(75, 23);
            this.myButton8.TabIndex = 28;
            this.myButton8.Text = "保存编队";
            this.myButton8.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton8.UseVisualStyleBackColor = true;
            // 
            // myButton9
            // 
            this.myButton9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.myButton9.Location = new System.Drawing.Point(12, 694);
            this.myButton9.Name = "myButton9";
            this.myButton9.Size = new System.Drawing.Size(75, 23);
            this.myButton9.TabIndex = 29;
            this.myButton9.Text = "刹车";
            this.myButton9.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton9.UseVisualStyleBackColor = true;
            this.myButton9.Click += new System.EventHandler(this.BUT_Brake_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(903, 575);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 18);
            this.label1.TabIndex = 30;
            this.label1.Text = "无人机编号：";
            // 
            // comboBox3
            // 
            this.comboBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBox3.DataSource = this.bindingSource2;
            this.comboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Location = new System.Drawing.Point(1007, 571);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(86, 26);
            this.comboBox3.TabIndex = 31;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(966, 603);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 18);
            this.label2.TabIndex = 32;
            this.label2.Text = "X轴：";
            // 
            // textBox3
            // 
            this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox3.Location = new System.Drawing.Point(1007, 600);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(86, 28);
            this.textBox3.TabIndex = 33;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(966, 634);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 18);
            this.label3.TabIndex = 32;
            this.label3.Text = "Y轴：";
            // 
            // textBox4
            // 
            this.textBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox4.Location = new System.Drawing.Point(1007, 631);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(86, 28);
            this.textBox4.TabIndex = 33;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(966, 664);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 18);
            this.label4.TabIndex = 32;
            this.label4.Text = "Z轴：";
            // 
            // textBox5
            // 
            this.textBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox5.Location = new System.Drawing.Point(1007, 661);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(86, 28);
            this.textBox5.TabIndex = 33;
            // 
            // myButton10
            // 
            this.myButton10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.myButton10.Enabled = false;
            this.myButton10.Location = new System.Drawing.Point(505, 578);
            this.myButton10.Name = "myButton10";
            this.myButton10.Size = new System.Drawing.Size(75, 23);
            this.myButton10.TabIndex = 34;
            this.myButton10.Text = "X轴移动";
            this.myButton10.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton10.UseVisualStyleBackColor = true;
            // 
            // textBox6
            // 
            this.textBox6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox6.Location = new System.Drawing.Point(586, 575);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(86, 28);
            this.textBox6.TabIndex = 35;
            // 
            // myButton11
            // 
            this.myButton11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.myButton11.Enabled = false;
            this.myButton11.Location = new System.Drawing.Point(505, 610);
            this.myButton11.Name = "myButton11";
            this.myButton11.Size = new System.Drawing.Size(75, 23);
            this.myButton11.TabIndex = 34;
            this.myButton11.Text = "Y轴移动";
            this.myButton11.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton11.UseVisualStyleBackColor = true;
            // 
            // textBox7
            // 
            this.textBox7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox7.Location = new System.Drawing.Point(586, 607);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(86, 28);
            this.textBox7.TabIndex = 35;
            // 
            // myButton12
            // 
            this.myButton12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.myButton12.Enabled = false;
            this.myButton12.Location = new System.Drawing.Point(505, 639);
            this.myButton12.Name = "myButton12";
            this.myButton12.Size = new System.Drawing.Size(75, 23);
            this.myButton12.TabIndex = 34;
            this.myButton12.Text = "Z轴移动";
            this.myButton12.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton12.UseVisualStyleBackColor = true;
            // 
            // textBox8
            // 
            this.textBox8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox8.Location = new System.Drawing.Point(586, 636);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(86, 28);
            this.textBox8.TabIndex = 35;
            // 
            // myButton13
            // 
            this.myButton13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.myButton13.Enabled = false;
            this.myButton13.Location = new System.Drawing.Point(505, 669);
            this.myButton13.Name = "myButton13";
            this.myButton13.Size = new System.Drawing.Size(75, 23);
            this.myButton13.TabIndex = 34;
            this.myButton13.Text = "偏航角";
            this.myButton13.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton13.UseVisualStyleBackColor = true;
            // 
            // textBox9
            // 
            this.textBox9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox9.Location = new System.Drawing.Point(586, 666);
            this.textBox9.Name = "textBox9";
            this.textBox9.Size = new System.Drawing.Size(86, 28);
            this.textBox9.TabIndex = 35;
            // 
            // myButton14
            // 
            this.myButton14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.myButton14.Enabled = false;
            this.myButton14.Location = new System.Drawing.Point(704, 578);
            this.myButton14.Name = "myButton14";
            this.myButton14.Size = new System.Drawing.Size(75, 23);
            this.myButton14.TabIndex = 34;
            this.myButton14.Text = "X轴差分";
            this.myButton14.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton14.UseVisualStyleBackColor = true;
            // 
            // textBox10
            // 
            this.textBox10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox10.Location = new System.Drawing.Point(785, 575);
            this.textBox10.Name = "textBox10";
            this.textBox10.Size = new System.Drawing.Size(86, 28);
            this.textBox10.TabIndex = 35;
            // 
            // myButton15
            // 
            this.myButton15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.myButton15.Enabled = false;
            this.myButton15.Location = new System.Drawing.Point(704, 610);
            this.myButton15.Name = "myButton15";
            this.myButton15.Size = new System.Drawing.Size(75, 23);
            this.myButton15.TabIndex = 34;
            this.myButton15.Text = "Y轴差分";
            this.myButton15.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton15.UseVisualStyleBackColor = true;
            // 
            // textBox11
            // 
            this.textBox11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox11.Location = new System.Drawing.Point(785, 607);
            this.textBox11.Name = "textBox11";
            this.textBox11.Size = new System.Drawing.Size(86, 28);
            this.textBox11.TabIndex = 35;
            // 
            // myButton16
            // 
            this.myButton16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.myButton16.Enabled = false;
            this.myButton16.Location = new System.Drawing.Point(704, 639);
            this.myButton16.Name = "myButton16";
            this.myButton16.Size = new System.Drawing.Size(75, 23);
            this.myButton16.TabIndex = 34;
            this.myButton16.Text = "Z轴差分";
            this.myButton16.TextColorNotEnabled = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(87)))), ((int)(((byte)(4)))));
            this.myButton16.UseVisualStyleBackColor = true;
            // 
            // textBox12
            // 
            this.textBox12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox12.Location = new System.Drawing.Point(785, 636);
            this.textBox12.Name = "textBox12";
            this.textBox12.Size = new System.Drawing.Size(86, 28);
            this.textBox12.TabIndex = 35;
            // 
            // FormationControls
            // 
            this.ClientSize = new System.Drawing.Size(1478, 744);
            this.Controls.Add(this.textBox9);
            this.Controls.Add(this.textBox12);
            this.Controls.Add(this.textBox8);
            this.Controls.Add(this.myButton13);
            this.Controls.Add(this.myButton16);
            this.Controls.Add(this.myButton12);
            this.Controls.Add(this.textBox11);
            this.Controls.Add(this.myButton15);
            this.Controls.Add(this.textBox7);
            this.Controls.Add(this.textBox10);
            this.Controls.Add(this.myButton11);
            this.Controls.Add(this.myButton14);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.myButton10);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.myButton9);
            this.Controls.Add(this.myButton8);
            this.Controls.Add(this.myButton7);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.myButton6);
            this.Controls.Add(this.myButton5);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.myButton4);
            this.Controls.Add(this.myButton3);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.myButton1);
            this.Controls.Add(this.myButton2);
            this.Controls.Add(this.but_auto);
            this.Controls.Add(this.but_guided);
            this.Controls.Add(this.BUT_Updatepos);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.BUT_Start);
            this.Controls.Add(this.BUT_leader);
            this.Controls.Add(this.CMB_mavs);
            this.Controls.Add(this.BUT_Land);
            this.Controls.Add(this.BUT_Takeoff);
            this.Controls.Add(this.BUT_Disarm);
            this.Controls.Add(this.BUT_Arm);
            this.MinimumSize = new System.Drawing.Size(1500, 800);
            this.Name = "FormationControls";
            this.Text = "Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Control_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.MyButton BUT_Arm;
        private Controls.MyButton BUT_Disarm;
        private Controls.MyButton BUT_Takeoff;
        private Controls.MyButton BUT_Land;
        private System.Windows.Forms.ComboBox CMB_mavs;
        private Controls.MyButton BUT_leader;
        private Controls.MyButton BUT_Start;
        private Grid grid1;
        //private Grid grid2;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        //private System.Windows.Forms.TabPage tabPage2;
        private Controls.MyButton BUT_Updatepos;
        private System.Windows.Forms.Timer timer_status;
        private Controls.MyButton but_guided;
        private Controls.MyButton but_auto;
        private Controls.MyButton myButton1;
        private Controls.MyButton myButton2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private Controls.MyButton myButton3;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.BindingSource bindingSource2;
        private Controls.MyButton myButton4;
        private System.Windows.Forms.ComboBox comboBox2;
        private BrightIdeasSoftware.HighlightTextRenderer highlightTextRenderer1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.TextBox textBox1;
        private Controls.MyButton myButton5;
        private Controls.MyButton myButton6;
        private System.Windows.Forms.TextBox textBox2;
        private Controls.MyButton myButton7;
        private Controls.MyButton myButton8;
        private Controls.MyButton myButton9;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox5;
        private Controls.MyButton myButton10;
        private System.Windows.Forms.TextBox textBox6;
        private Controls.MyButton myButton11;
        private System.Windows.Forms.TextBox textBox7;
        private Controls.MyButton myButton12;
        private System.Windows.Forms.TextBox textBox8;
        private Controls.MyButton myButton13;
        private System.Windows.Forms.TextBox textBox9;
        private Controls.MyButton myButton14;
        private System.Windows.Forms.TextBox textBox10;
        private Controls.MyButton myButton15;
        private System.Windows.Forms.TextBox textBox11;
        private Controls.MyButton myButton16;
        private System.Windows.Forms.TextBox textBox12;
    }
}