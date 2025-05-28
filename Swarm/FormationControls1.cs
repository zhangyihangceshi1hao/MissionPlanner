using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MissionPlanner.Utilities;
//using Xamarin.Forms;


namespace MissionPlanner.Swarm
{
    //[PreventTheming]
    public partial class FormationControls1: Form
    {
       
        public FormationControls1()
        {
            InitializeComponent();
            // 设置窗体背景为科技蓝
            //this.BackColor = Color.FromArgb(12, 25, 49);
            //this.ForeColor = Color.White;
            //button1.FlatAppearance.BorderSize = 0; // 移除边框
            //// 设置按钮样式
            //button1.BackColor = Color.FromArgb(0, 191, 255);
            //button1.ForeColor = Color.Black;
            //button1.FlatStyle = FlatStyle.Flat;
            //button1.FlatAppearance.BorderSize = 0;

            //// 绑定悬停效果
            //button1.MouseEnter += (s, e) => button1.BackColor = Color.FromArgb(0, 255, 255);
            //button1.MouseLeave += (s, e) => button1.BackColor = Color.FromArgb(0, 191, 255);


            //// 强制刷新
            //this.Load += (s, e) => this.Invalidate();
            //button1.FlatStyle = FlatStyle.Flat;
            //button1.FlatAppearance.BorderSize = 0; // 移除默认边框
            ////button1.FlatAppearance.BorderColor = Color.Transparent; // 边框透明
            //button1.FlatAppearance.MouseOverBackColor = Color.FromArgb(73, 43, 58, 3); // 鼠标悬停颜色（可选）
            //button1.BackColor = Color.FromArgb(148, 193, 31); // 填充颜色
            //button1.ForeColor = Color.White; // 文字颜色

        }
      
        private void AddFormationButton_Click(object sender, EventArgs e)
        {
            int maxIndex = 0;

            // 查找当前最大的 "编队X" 编号
            foreach (TabPage page in tabControl.TabPages)
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
            TabPage newPage = new TabPage();
            newPage.Text = "编队" + (maxIndex + 1);
            newPage.Name = "formationTabPage" + (maxIndex + 1);

            // 设置样式（可选）
            newPage.Font = new System.Drawing.Font("Segoe UI", 9F);
            newPage.Padding = new Padding(3);

            // 插入到 "+" 号按钮之前（如果存在）
            int insertIndex = tabControl.TabPages.Count;
            if (tabControl.TabPages.Count > 0 && tabControl.TabPages[tabControl.TabPages.Count - 1].Text == "+")
                insertIndex -= 1;
            // 创建 Grid 并设置 ForeColor
            Grid grid = new MissionPlanner.Swarm.Grid();
            grid.Location = new System.Drawing.Point(6, 6);
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.ForeColor = System.Drawing.SystemColors.ControlText;
            grid.Name = "grid" + (maxIndex + 1);
            grid.Size = new System.Drawing.Size(1132, 545);
            grid.TabIndex = 8;
            grid.Vertical = false;
           
            newPage.Controls.Add(grid);
            tabControl.TabPages.Insert(insertIndex, newPage);
            
        }

        private void RemoveFormationButton_Click(object sender, EventArgs e)
        {
            // 确保至少保留一个 "编队" 标签页
            if (tabControl.TabPages.Count <= 1)
            {
                MessageBox.Show("至少保留一个编队标签页！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 查找最后一个有效的 "编队X" 标签页
            TabPage lastPage = null;
            for (int i = tabControl.TabPages.Count - 1; i >= 0; i--)
            {
                if (tabControl.TabPages[i].Text.StartsWith("编队"))
                {
                    lastPage = tabControl.TabPages[i];
                    break;
                }
            }

            if (lastPage != null)
            {
                tabControl.TabPages.Remove(lastPage);
            }
        }
    }
}
