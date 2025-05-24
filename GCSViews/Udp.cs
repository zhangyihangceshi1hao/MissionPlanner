using Microsoft.Scripting.Utils;
using MissionPlanner.ArduPilot;
using MissionPlanner.Swarm.Sequence;
using MissionPlanner.Utilities;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Interop;
using static Community.CsharpSqlite.Sqlite3;
using static IronPython.Modules._ast;
using static MAVLink;
using static MissionPlanner.GCSViews.Udp;
using DateTime = System.DateTime;

namespace MissionPlanner.GCSViews
{
    public partial class Udp : Form
    {
        public Udp()
        {
            InitializeComponent();
            countdown.Elapsed += sendMassage; 
            //senddata.Elapsed += senddt;
            //ten.Elapsed += sensorsend;
            instance = this;
        }

        public static Udp instance = null;

        IPEndPoint endPoint;

        IPEndPoint endPointudp;

        Thread receive;

        System.Timers.Timer countdown = new System.Timers.Timer { Interval = 10000, AutoReset = true };

        //System.Timers.Timer senddata = new System.Timers.Timer { Interval = 1000, AutoReset = true };

        //System.Timers.Timer ten = new System.Timers.Timer { Interval = 10000, AutoReset = true };
        //MAVLinkInterface MAV = new MAVLinkInterface();
        public static Locationwp gotohere = new Locationwp();

        private void myButton2_Click(object sender, EventArgs e)
        {
            if (myButton2.Text == "连接")
            {
                endPoint = new IPEndPoint(IPAddress.Parse(textBox1.Text), int.Parse(textBox3.Text));
                udpClient = new UdpClient(5565);

                receive = new Thread(receivemessage);
                receive.Start();
                //countdown.Start();
                myButton2.Text = "断开连接";
            }
            else
            {
                //countdown.Stop();
                udpClient.Close();
                myButton2.Text = "连接";
            }
        }


        private void udp_closing(object sender, FormClosingEventArgs e)
        {
            if (udpClient != null)
            {
                udpClient.Close();
            }
         
            countdown.Stop();
       
        }


      


        private async void receivemessage()
        {
            while (true)
            {
                try
                {



                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = new byte[4096];
                    data = udpClient.Receive(ref remoteEndPoint);//此方法把数据来源ip、port放到第二个参数中

                    if (data[0] == 0xFF && data[1] == 0xFA && data[2] == 0x01 && data[3] == 0x00)
                    {
                        control_command packet = ByteArrayToStructure<control_command>(data);
                        if (packet.control_type == 0x01)
                        {

                            //control_command packet = ByteArrayToStructure<control_command>(data);
                            MessageBox.Show("加载航点！");

                            await Task.Run(() =>
                            {
                                //this.Invoke((MethodInvoker)delegate
                                //{

                                //});
                                foreach (var port in MainV2.Comports)
                                {
                                    foreach (var mav in port.MAVlist)
                                    {
                                        if (mav.sysid == BitConverter.ToInt16(data, 12))
                                        {
                                            gotohere = new Locationwp();

                                            gotohere.id = (ushort)MAVLink.MAV_CMD.WAYPOINT;

                                            //gotohere.uav_id = BitConverter.ToInt16(data, 13);
                                            gotohere.lat = (double)packet.latitude/10000000;
                                            gotohere.lng = (double)packet.longitude / 10000000;
                                            gotohere.alt = (float)packet.relative_height/100;
                                            gotohere.frame = MainV2.comPort.MAV.GuidedMode.frame;

                                            //bool isarmed = mav.cs.armed;



                                            //try
                                            //{
                                            //    //MainV2.comPort.setGuidedModeWP(gotohere);
                                            //    //MAVLinkInterface.setGuidedModeWP(mav.sysid, mav.compid, gotohere, true);
                                            //    new MAVLinkInterface().setGuidedModeWP(mav.sysid, mav.compid, gotohere, true);
                                            //}
                                            //catch (Exception ex)
                                            //{
                                            //    CustomMessageBox.Show(Strings.CommandFailed + ex.Message, Strings.ERROR);
                                            //}
                                            // 修改 data[3]（例如改为 0x01）
                                            packet.msg_type = 0x01;
                                            packet.src_id = 0x01;
                                            packet.dst_id = 0x00;
                                            //// 原样发送回来源 IP 和 Port
                                            //udpClient.Send(data, data.Length, remoteEndPoint);

                                            byte[] send = StructToBytes(packet);
                                            udpClient.Send(send, send.Length, endPoint);
                                        }
                                    }
                                }


                            });

                           
                        }
                        if ((byte)packet.control_type == 0x02)
                        {
                            //MessageBox.Show("开始执行任务！");
                            //解锁,需要修改参数AUTO_OPTIONS==3
                            //while()
                            foreach (var port in MainV2.Comports)
                            {
                                foreach (var mav in port.MAVlist)
                                {
                                    if (mav.sysid == packet.uav_id) {


                                        List<Locationwp> commands = new List<Locationwp>();

                                        Locationwp temp1 = new Locationwp();



                                        temp1.id = (ushort)22;

                                        temp1.p1 = 0;

                                        temp1.alt = (float)50;

                                        temp1.lat = (double)0;

                                        temp1.lng = (double)0;



                                        temp1.p2 = 0;

                                        temp1.p3 = 0;

                                        temp1.p4 = 0;

                                        temp1.Tag = "0";



                                        temp1.frame = 3;

                                        commands.Add(temp1);

                                        Locationwp temp2 = new Locationwp();



                                        temp2.id = (ushort)16;

                                        temp2.p1 = 0;

                                        temp2.alt = (float)gotohere.alt;

                                        temp2.lat = (double)gotohere.lat;

                                        temp2.lng = (double)gotohere.lng;

                                       
                                        temp2.p2 = 0;

                                        temp2.p3 = 0;

                                        temp2.p4 = 0;

                                        temp2.Tag = "0";



                                        temp2.frame = 3;

                                        commands.Add(temp2);



                                        Locationwp home = new Locationwp();



                                        home.frame = (byte)MAVLink.MAV_FRAME.GLOBAL;

                                        home.id = (ushort)MAVLink.MAV_CMD.WAYPOINT;

                                        home.alt = (float)50;

                                        home.lat = (double)mav.cs.HomeLocation.Lat;

                                        home.lng = (double)mav.cs.HomeLocation.Lng;

                                        commands.Insert(0, home);



                                        mav_mission.upload(port, mav.sysid,
                                        mav.compid, 0,
                                         commands,
                                         (percent, status) =>
                                         {
                                         }).ConfigureAwait(false);
                                        await Task.Run(() =>
                                        {
                                            Thread.Sleep(1000);
                                            //doCommand(sysid, compid, MAV_CMD.DO_SET_MODE, mode.base_mode, mode.custom_mode, 0, 0, 0, 0, 0, false);
                                            while (mav.cs.mode != "Auto")
                                            {
                                                //foreach (var port in MainV2.Comports)
                                                //{
                                                //    foreach (var mav in port.MAVlist)
                                                //    {
                                                //        if (mav == Leader)
                                                //            continue;

                                                //        port.doARM(mav.sysid, mav.compid, true);
                                                //    }
                                                //}
                                                port.setMode("Auto");
                                                //MAV.setMode(mav.sysid, mav.compid, "Auto");
                                            }



                                            while (!mav.cs.armed)
                                            {
                                                port.doARM(true);
                                            }
                                        });


                                    }

                                }
                            }
                                    //起飞

                                    //指点飞行




                                }
                        if (packet.control_type == 0x03)
                        {
                            MessageBox.Show("向前！");
                        }
                        if (packet.control_type == 0x04)
                        {
                            MessageBox.Show("向后！");
                        }
                        if (packet.control_type == 0x05)
                        {
                            MessageBox.Show("向左！");
                        }
                        if (packet.control_type == 0x06)
                        {
                            MessageBox.Show("向右！");
                        }
                        if (packet.control_type == 0x07)
                        {
                            MessageBox.Show("返航！");
                        }

                        //control_command packet1 =new control_command();
                    }

                   
                }
                catch
                {

                }
            }
        }

        private T ByteArrayToStructure<T>(byte[] data) where T : struct
        {
            IntPtr ptr = Marshal.AllocHGlobal(data.Length);
            try
            {
                Marshal.Copy(data, 0, ptr, data.Length);
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

       


        private UdpClient udpClient;

        

                           
        //姿态信息
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct pose_information
        {
            public UInt16 magic;
            public byte version;
            public byte msg_type;
            public byte src_id;
            public byte dst_id;
            public UInt32 seq_num;
            public UInt16 payload_len;

            public byte arm_state;
            public byte mode_type;
            public UInt16 uav_id;
            public UInt32 latitude;
            public UInt32 longitude;
            public UInt16 altitude;
            public UInt16 relative_height;
            public UInt16 flyspeed;
            public UInt16 roll;
            public UInt16 pitch;
            public UInt16 yaw;
            public UInt16 voltage;
        }

      
        //控制指令
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct control_command
        {
            public UInt16 magic;
            public byte version;
            public byte msg_type;
            public byte src_id;
            public byte dst_id;
            public UInt32 seq_num;
            public UInt16 payload_len;

            public UInt16 uav_id;
            public byte control_type;
            public UInt32 latitude;
            public UInt32 longitude;
            public UInt16 relative_height;
        }

        

        public static long GetMillisecondsSinceEpoch()
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long millis = DateTime.UtcNow.Ticks / 10000; // Ticks are 100-nanosecond intervals
            return millis - epoch.Ticks / 10000;
        }



        //{"类型“MissionPlanner.Udp+Register”不能作为非托管结构进行封送处理；无法计算有意义的大小或偏移量。"}
        public static byte[] StructToBytes(object structObj)
        {
            int size = Marshal.SizeOf(structObj);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structObj, buffer, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        UInt32 sequence = 0;
        /// <summary>
        /// 发送无人机基本信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendMassage(object sender, ElapsedEventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    try
                    {
                        sequence++;
                        pose_information data = new pose_information
                        {
                            magic = 0xFAFF,
                            version = 0x01,
                            msg_type = 0x01,
                            src_id = 0x01,
                            dst_id = 0x00,
                            seq_num = (UInt32)sequence,
                            payload_len = (UInt16)28,
                           
                            
                            arm_state = 0x01,
                            mode_type = 0x01,
                            uav_id = (UInt16)mav.sysid,
                            latitude = (UInt32)(mav.cs.lat * 1e6),
                            longitude = (UInt32)(mav.cs.lng * 1e6),
                            altitude =1,
                            relative_height = (UInt16)(mav.cs.alt * 10),
                            flyspeed =1,
                            roll=1,
                            pitch=1,
                            yaw=1,
                            voltage=1
                        };
                        byte[] send = StructToBytes(data);
                        udpClient.Send(send, send.Length, endPoint);                      
                    }
                    catch
                    {

                    }
                }
            }
        }

       
       
    }
}
