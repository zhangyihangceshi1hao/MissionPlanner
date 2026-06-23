// ============================================================
// 无人机数据中继站协议 - 封包 / 解包工具
// 文件: RelayPacketBuilder.cs
// 放置于: MissionPlanner/RelayStation/RelayPacketBuilder.cs
// ============================================================

using System;
using System.Net;
using System.Runtime.InteropServices;

namespace MissionPlanner.RelayStation
{
    /// <summary>
    /// 帧封包与解包，CRC = msg_type 到 DATA 最后一字节的逐字节异或和
    /// </summary>
    public static class RelayPacketBuilder
    {
        // ── CRC：从 msg_type 字节开始到 DATA 末尾的异或 ──
        public static ushort CalcCrc(byte[] buf, int start, int length)
        {
            byte lo = 0, hi = 0;
            for (int i = start; i < start + length; i++)
            {
                lo ^= buf[i];
                hi ^= buf[i];
            }
            return (ushort)((hi << 8) | lo);
        }

        // ── 将任意结构体序列化为字节数组（小端，Pack=1）──
        public static byte[] StructToBytes<T>(T obj) where T : struct
        {
            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(obj, ptr, false);
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally { Marshal.FreeHGlobal(ptr); }
            return arr;
        }

        // ── 从字节数组反序列化结构体 ──
        public static T BytesToStruct<T>(byte[] buf, int offset = 0) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(buf, offset, ptr, size);
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        // ── 封包：拼接帧头 + payload + CRC ──
        public static byte[] BuildFrame(byte msgType, byte uavId, uint seq, byte[] payload)
        {
            ushort payLen = (ushort)(payload?.Length ?? 0);
            int totalSize = RelayConst.HEADER_SIZE + payLen + RelayConst.CRC_SIZE;
            byte[] frame = new byte[totalSize];
            int pos = 0;

            // 帧头 (little-endian)
            frame[pos++] = 0xFF; frame[pos++] = 0xFA; // 0xFAFF 低字节先
            frame[pos++] = RelayConst.PROTOCOL_VER;
            frame[pos++] = msgType;
            frame[pos++] = uavId;
            // seq (4 bytes, little-endian)
            frame[pos++] = (byte)(seq);
            frame[pos++] = (byte)(seq >> 8);
            frame[pos++] = (byte)(seq >> 16);
            frame[pos++] = (byte)(seq >> 24);
            // payload_len (2 bytes, little-endian)
            frame[pos++] = (byte)(payLen);
            frame[pos++] = (byte)(payLen >> 8);

            // DATA
            if (payload != null && payLen > 0)
            {
                Buffer.BlockCopy(payload, 0, frame, pos, payLen);
                pos += payLen;
            }

            // CRC — 从 msg_type 字节（index=3）开始到 DATA 末尾
            int crcStart = 3;
            int crcLen   = 1 + 1 + 4 + 2 + payLen; // type+uavid+seq+paylen+data
            ushort crc = CalcCrc(frame, crcStart, crcLen);
            frame[pos++] = (byte)(crc);
            frame[pos++] = (byte)(crc >> 8);

            return frame;
        }

        // ── 解包：校验并返回 payload，失败返回 null ──
        public static byte[] ParseFrame(byte[] buf, out byte msgType, out byte uavId, out uint seq)
        {
            msgType = 0; uavId = 0; seq = 0;
            if (buf == null || buf.Length < RelayConst.MIN_FRAME_SIZE) return null;

            // 校验帧头
            if (buf[0] != 0xFF || buf[1] != 0xFA) return null;
            if (buf[2] != RelayConst.PROTOCOL_VER) return null;

            msgType = buf[3];
            uavId   = buf[4];
            seq     = (uint)(buf[5] | (buf[6] << 8) | (buf[7] << 16) | (buf[8] << 24));
            ushort payLen = (ushort)(buf[9] | (buf[10] << 8));

            if (buf.Length < RelayConst.HEADER_SIZE + payLen + RelayConst.CRC_SIZE) return null;

            // 校验 CRC
            int crcStart = 3;
            int crcLen   = 1 + 1 + 4 + 2 + payLen;
            ushort calcCrc = CalcCrc(buf, crcStart, crcLen);
            ushort rxCrc   = (ushort)(buf[RelayConst.HEADER_SIZE + payLen] |
                                      (buf[RelayConst.HEADER_SIZE + payLen + 1] << 8));
            if (calcCrc != rxCrc) return null;

            byte[] payload = new byte[payLen];
            Buffer.BlockCopy(buf, RelayConst.HEADER_SIZE, payload, 0, payLen);
            return payload;
        }

        // ── 便捷方法：打包控制指令 ──
        public static byte[] BuildControlCmd(byte uavId, uint seq, ControlCmd cmd)
        {
            byte[] payload = StructToBytes(cmd);
            return BuildFrame(RelayConst.MSG_CONTROL, uavId, seq, payload);
        }

        // ── 便捷方法：打包航线（含航点总数前缀）──
        public static byte[] BuildWaypoints(byte uavId, uint seq, WaypointItem[] wps)
        {
            byte count = (byte)Math.Min(wps.Length, 255);
            int wpSize = Marshal.SizeOf(typeof(WaypointItem));
            byte[] payload = new byte[1 + count * wpSize];
            payload[0] = count;
            for (int i = 0; i < count; i++)
            {
                byte[] wpBytes = StructToBytes(wps[i]);
                Buffer.BlockCopy(wpBytes, 0, payload, 1 + i * wpSize, wpSize);
            }
            return BuildFrame(RelayConst.MSG_WAYPOINTS, uavId, seq, payload);
        }
    }
}
