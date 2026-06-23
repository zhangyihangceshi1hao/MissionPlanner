// ============================================================
// 无人机数据中继站协议 - UDP 通信层
// 文件: RelayUdpClient.cs
// 放置于: MissionPlanner/RelayStation/RelayUdpClient.cs
// ============================================================

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using System.Reflection;

namespace MissionPlanner.RelayStation
{
    /// <summary>
    /// 封装 UDP 发送（控制指令/航线）和接收（无人机回传数据）
    /// 接收端口固定 14501，发送目标地址可配置
    /// </summary>
    public class RelayUdpClient : IDisposable
    {
        private static readonly ILog log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private UdpClient   _udpRecv;
        private UdpClient   _udpSend;
        private Thread      _recvThread;
        private bool        _running;
        private uint        _seqNum;

        public string  RemoteIp   { get; set; } = "192.168.1.1";
        public int     RemotePort { get; set; } = 14501;
        public byte    UavId      { get; set; } = 0x01;

        // ── 事件：收到各类数据包时触发 ──
        public event Action<BasicAttitude>  OnBasicAttitude;
        public event Action<SensorData>     OnSensorData;
        public event Action<BatteryData>    OnBatteryData;
        public event Action<NavData>        OnNavData;
        public event Action<StatusData>     OnStatusData;
        public event Action<string>         OnLog;

        public bool IsRunning => _running;

        // ────────────────────────────────────────
        // 启动 UDP 接收（绑定 14501）
        // ────────────────────────────────────────
        public void Start()
        {
            if (_running) return;
            try
            {
                _udpRecv = new UdpClient(RelayConst.UDP_PORT);
                _udpSend = new UdpClient();
                _running = true;
                _recvThread = new Thread(ReceiveLoop)
                {
                    IsBackground = true,
                    Name = "RelayUdpRecv"
                };
                _recvThread.Start();
                RaiseLog($"[Relay] 监听 UDP :{RelayConst.UDP_PORT}，目标 {RemoteIp}:{RemotePort}");
            }
            catch (Exception ex)
            {
                log.Error("RelayUdpClient.Start", ex);
                RaiseLog($"[Relay] 启动失败: {ex.Message}");
            }
        }

        public void Stop()
        {
            _running = false;
            try { _udpRecv?.Close(); } catch { }
            try { _udpSend?.Close(); } catch { }
            RaiseLog("[Relay] 已停止");
        }

        // ────────────────────────────────────────
        // 发送控制指令
        // ────────────────────────────────────────
        public void SendControlCmd(ControlCmd cmd)
        {
            byte[] frame = RelayPacketBuilder.BuildControlCmd(UavId, NextSeq(), cmd);
            SendRaw(frame);
            RaiseLog($"[Relay->] 控制指令 type={cmd.ControlType} mode={cmd.ModeType}");
        }

        // ────────────────────────────────────────
        // 发送航线
        // ────────────────────────────────────────
        public void SendWaypoints(WaypointItem[] wps)
        {
            byte[] frame = RelayPacketBuilder.BuildWaypoints(UavId, NextSeq(), wps);
            SendRaw(frame);
            RaiseLog($"[Relay->] 上传航线 共{wps.Length}个航点");
        }

        // ────────────────────────────────────────
        // 内部
        // ────────────────────────────────────────
        private uint NextSeq() => _seqNum++;

        private void SendRaw(byte[] data)
        {
            try
            {
                _udpSend?.Send(data, data.Length,
                    new IPEndPoint(IPAddress.Parse(RemoteIp), RemotePort));
            }
            catch (Exception ex) { RaiseLog($"[Relay] 发送失败: {ex.Message}"); }
        }

        private void ReceiveLoop()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            while (_running)
            {
                try
                {
                    byte[] buf = _udpRecv.Receive(ref ep);
                    ProcessFrame(buf);
                }
                catch (SocketException) { /* 关闭时正常退出 */ break; }
                catch (Exception ex)
                {
                    if (_running) log.Warn("RelayUdpClient.ReceiveLoop", ex);
                }
            }
        }

        private void ProcessFrame(byte[] buf)
        {
            byte msgType, uavId;
            uint seq;
            byte[] payload = RelayPacketBuilder.ParseFrame(buf, out msgType, out uavId, out seq);
            if (payload == null)
            {
                RaiseLog("[Relay<-] 收到无效帧（CRC错误或帧头不匹配）");
                return;
            }

            try
            {
                switch (msgType)
                {
                    case RelayConst.MSG_BASIC_ATTITUDE:
                        OnBasicAttitude?.Invoke(
                            RelayPacketBuilder.BytesToStruct<BasicAttitude>(payload));
                        break;
                    case RelayConst.MSG_SENSOR:
                        OnSensorData?.Invoke(
                            RelayPacketBuilder.BytesToStruct<SensorData>(payload));
                        break;
                    case RelayConst.MSG_BATTERY:
                        OnBatteryData?.Invoke(
                            DecodeBattery(payload));
                        break;
                    case RelayConst.MSG_NAV:
                        OnNavData?.Invoke(
                            RelayPacketBuilder.BytesToStruct<NavData>(payload));
                        break;
                    case RelayConst.MSG_STATUS:
                        OnStatusData?.Invoke(
                            RelayPacketBuilder.BytesToStruct<StatusData>(payload));
                        break;
                    default:
                        RaiseLog($"[Relay<-] 未处理 MsgType=0x{msgType:X2} seq={seq}");
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("RelayUdpClient.ProcessFrame", ex);
            }
        }

        /// <summary>
        /// 电池数据含变长电芯数组，需手动解析
        /// </summary>
        private BatteryData DecodeBattery(byte[] payload)
        {
            var bd = new BatteryData();
            int pos = 0;
            bd.MainVoltage = BitConverter.ToUInt16(payload, pos); pos += 2;
            bd.Remaining   = payload[pos++];
            bd.Current     = BitConverter.ToInt16(payload, pos);  pos += 2;
            bd.ConsumedMah = BitConverter.ToInt32(payload, pos);  pos += 4;
            bd.RangeLeft   = BitConverter.ToInt16(payload, pos);  pos += 2;
            bd.FcVoltage   = BitConverter.ToUInt16(payload, pos); pos += 2;
            bd.CellVoltage = new ushort[12];
            int cellCount  = Math.Min(12, (payload.Length - pos) / 2);
            for (int i = 0; i < cellCount; i++)
            {
                bd.CellVoltage[i] = BitConverter.ToUInt16(payload, pos);
                pos += 2;
            }
            return bd;
        }

        private void RaiseLog(string msg) => OnLog?.Invoke(msg);

        public void Dispose() => Stop();
    }
}
