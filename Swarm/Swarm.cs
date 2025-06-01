using log4net;
using Org.BouncyCastle.Asn1.Esf;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace MissionPlanner.Swarm
{
    public abstract class Swarm
    {
        internal static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal MAVState Leader = null;

        public void setLeader(MAVState lead)
        {
            Leader = lead;
        }

        public MAVState getLeader()
        {
            return Leader;
        }

        public void Arm()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == Leader)
                        continue;

                    port.doARM(mav.sysid, mav.compid, true);
                }
            }
        }
        public void ArmSwarms(List<MAVState> mavSwarmsList, bool vertical, bool vertica2)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (vertica2)
                    {
                        if (!vertical)
                        {
                            if (mav == Leader)
                                continue;
                        }
                        port.doARM(mav.sysid, mav.compid, true);
                    }
                    else
                    {
                        if (mavSwarmsList.Contains(mav))
                        {
                            if (!vertical)
                            {
                                if (mav == Leader)
                                    continue;
                            }
                            port.doARM(mav.sysid, mav.compid, true);
                        }
                    }
                }
            }
        }

        public void Disarm()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == Leader)
                        continue;

                    port.doARM(mav.sysid, mav.compid, false);
                }
            }
        }    
        public void DisarmSwarms(List<MAVState> mavSwarmsList, bool vertical, bool vertica2)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (vertica2) {
                        if (!vertical)
                        {
                            if (mav == Leader)
                                continue;
                        }
                        port.doARM(mav.sysid, mav.compid, false);
                    }
                    else {
                        if (mavSwarmsList.Contains(mav))
                        {
                            if (!vertical)
                            {
                                if (mav == Leader)
                                    continue;
                            }
                            port.doARM(mav.sysid, mav.compid, false);
                        }
                    }
                }
            }
        }

        public void Takeoff()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == Leader)
                        continue;

                    port.setMode(mav.sysid, mav.compid, "GUIDED");

                    port.doCommand(mav.sysid, mav.compid, MAVLink.MAV_CMD.TAKEOFF, 0, 0, 0, 0, 0, 0, 5);
                }
            }
        }   
        public void TakeoffSwarms(List<MAVState> mavSwarmsList, bool vertical, bool vertica2,float latparam)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (vertica2) {
                        if (!vertical)
                        {
                            if (mav == Leader)
                                continue;
                        }
                        port.setMode(mav.sysid, mav.compid, "GUIDED");

                        port.doCommand(mav.sysid, mav.compid, MAVLink.MAV_CMD.TAKEOFF, 0, 0, 0, 0, 0, 0, latparam);
                    }
                    else {
                        if (mavSwarmsList.Contains(mav))
                        {
                            if (!vertical)
                            {
                                if (mav == Leader)
                                    continue;
                            }
                            port.setMode(mav.sysid, mav.compid, "GUIDED");

                            port.doCommand(mav.sysid, mav.compid, MAVLink.MAV_CMD.TAKEOFF, 0, 0, 0, 0, 0, 0, latparam);
                        }
                    }
                }
            }
        }

        public void Land()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    port.setMode(mav.sysid, mav.compid, "Land");
                }
            }
        } 
        public void LandSwarms(List<MAVState> mavSwarmsList, bool vertical, bool vertica2)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (vertica2) {
                        if (!vertical)
                        {
                            if (mav == Leader)
                                continue;
                        }
                        port.setMode(mav.sysid, mav.compid, "Land");
                    }
                    else {
                        if (mavSwarmsList.Contains(mav))
                        {
                            if (!vertical)
                            {
                                if (mav == Leader)
                                    continue;
                            }
                            port.setMode(mav.sysid, mav.compid, "Land");
                        }
                    }
                }
            }
        }
        public void RTL_ALL_Swarms(List<MAVState> mavSwarmsList, bool vertical, bool vertica2)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (vertica2)
                    {
                        if (!vertical)
                        {
                            if (mav == Leader)
                                continue;
                        }
                        port.setMode(mav.sysid, mav.compid, "RTL");
                    }
                    else
                    {
                        if (mavSwarmsList.Contains(mav))
                        {
                            if (!vertical)
                            {
                                if (mav == Leader)
                                    continue;
                            }
                            port.setMode(mav.sysid, mav.compid, "RTL");
                        }
                    }
                }
            }
        }
        public async void Brake_ALL(List<MAVState> mavSwarmsList, bool vertical, bool vertica2)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (vertica2)
                    {
                        if (!vertical)
                        {
                            if (mav == Leader)
                                continue;
                        }
                        port.setMode(mav.sysid, mav.compid, "Brake");
                    
                    }
                    else
                    {
                        if (mavSwarmsList.Contains(mav))
                        {
                            if (!vertical)
                            {
                                if (mav == Leader)
                                    continue;
                            }
                            port.setMode(mav.sysid, mav.compid, "Brake");
                           
                        }
                    }
                }
            }
        }

        public async void Rtl_successively_ALL(List<MAVState> mavSwarmsList, bool vertical, bool vertica2, int sleep_time)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (vertica2)
                    {
                        if (!vertical)
                        {
                            if (mav == Leader)
                                continue;
                        }
                        port.setMode(mav.sysid, mav.compid, "RTL");
                        await Task.Delay(sleep_time * 1000);  // 延迟3000毫秒（即3秒）
                    }
                    else
                    {
                        if (mavSwarmsList.Contains(mav))
                        {
                            if (!vertical)
                            {
                                if (mav == Leader)
                                    continue;
                            }
                            port.setMode(mav.sysid, mav.compid, "RTL");
                            await Task.Delay(sleep_time * 1000);  // 延迟3000毫秒（即3秒）
                        }
                    }
                }
            }
        }
        public void Stop()
        {
        }

        public void GuidedMode()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == Leader)
                        continue;

                    port.setMode(mav.sysid, mav.compid, "GUIDED");
                }
            }
        }
        public void GuidedModeSwarms(List<MAVState> mavSwarmsList, bool vertical, bool vertica2)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (vertica2)
                    {
                        if (!vertical)
                        {
                            if (mav == Leader)
                                continue;
                        }

                        port.setMode(mav.sysid, mav.compid, "GUIDED");
                    }
                    else
                    {
                        if (mavSwarmsList.Contains(mav))
                        {
                            if (!vertical)
                            {
                                if (mav == Leader)
                                    continue;
                            }

                            port.setMode(mav.sysid, mav.compid, "GUIDED");
                        }
                    }
                }
            }
        }

        public void AutoMode()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == Leader)
                        continue;

                    port.setMode(mav.sysid, mav.compid, "AUTO");
                }
            }
        }
        public void AutoModeSwarms(List<MAVState> mavSwarmsList, bool vertical, bool vertica2)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (vertica2)
                    {
                        if (!vertical)
                        {
                            if (mav == Leader)
                                continue;
                        }

                        port.setMode(mav.sysid, mav.compid, "AUTO");
                    }
                    else
                    {
                        if (mavSwarmsList.Contains(mav))
                        {
                            if (!vertical)
                            {
                                if (mav == Leader)
                                    continue;
                            }

                            port.setMode(mav.sysid, mav.compid, "AUTO");
                        }
                    }
                }
            }
        }

        public abstract void Update();

        public abstract void SendCommand();
    }
}