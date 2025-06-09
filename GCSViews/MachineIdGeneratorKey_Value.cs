using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DirectShowLib.DES;
using static MissionPlanner.GCSViews.FlightData;

namespace MissionPlanner.GCSViews
{
    public partial class MachineIdGeneratorKey_Value: Form
    {

        public string InputKey => textBox1.Text;
        public string InputValue => textBox2.Text;
        public MachineIdGeneratorKey_Value()
        {
            InitializeComponent();
            string currentMachineId = MachineIdGenerator.GetMachineId();
            textBox1.Text = currentMachineId;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // 可以加验证逻辑
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("请输入 Key");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
