using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissionPlanner.GCSViews
{
    public partial class controllerTCP: Form
    {
        public controllerTCP()
        {
            InitializeComponent();
        }

        private void myButton1_Click(object sender, EventArgs e)
        {
            MainV2.comPort.sendPacket(new MAVLink.mavlink_pps_tcp_t { frequency = int.Parse(textBox1.Text), enable = (byte)1 },
                       MainV2.comPort.sysidcurrent, MainV2.comPort.compidcurrent);
        }

        private void myButton2_Click(object sender, EventArgs e)
        {
            MainV2.comPort.sendPacket(new MAVLink.mavlink_pps_tcp_t { frequency = int.Parse(textBox1.Text), enable = (byte)0 },
                       MainV2.comPort.sysidcurrent, MainV2.comPort.compidcurrent);
        }
    }
}
