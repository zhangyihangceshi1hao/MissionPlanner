// ============================================================
// 无人机数据中继站协议 - 协议定义层
// 文件: RelayProtocol.cs
// 放置于: MissionPlanner/RelayStation/RelayProtocol.cs
// ============================================================

using System;
using System.Runtime.InteropServices;

namespace MissionPlanner.RelayStation
{
    // ────────────────────────────────────────────────────────
    // 帧头常量
    // ────────────────────────────────────────────────────────
    public static class RelayConst
    {
        public const ushort FRAME_HEADER   = 0xFAFF;
        public const byte   PROTOCOL_VER   = 0x01;
        public const int    HEADER_SIZE    = 11;  // header(2)+ver(1)+type(1)+uavid(1)+seq(4)+paylen(2)
        public const int    CRC_SIZE       = 2;
        public const int    MIN_FRAME_SIZE = HEADER_SIZE + CRC_SIZE;
        public const int    UDP_PORT       = 14501;

        // msg_type
        public const byte MSG_CONTROL      = 0x00;
        public const byte MSG_WAYPOINTS    = 0x01;
        public const byte MSG_BASIC_ATTITUDE = 0x02;
        public const byte MSG_SENSOR       = 0x03;
        public const byte MSG_BATTERY      = 0x04;
        public const byte MSG_PROPULSION   = 0x05;
        public const byte MSG_NAV          = 0x06;
        public const byte MSG_STATUS       = 0x07;
        public const byte MSG_DEVICE       = 0x08;
        public const byte MSG_RC           = 0x09;
    }

    // ────────────────────────────────────────────────────────
    // 通用帧头结构（小端字节序）
    // ────────────────────────────────────────────────────────
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RelayFrameHeader
    {
        public ushort FrameHeader;   // 0xFAFF
        public byte   Version;       // 0x01
        public byte   MsgType;
        public byte   UavId;
        public uint   SeqNum;
        public ushort PayloadLen;
    }

    // ────────────────────────────────────────────────────────
    // 2.2.1 控制指令 DATA 段
    // ────────────────────────────────────────────────────────
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ControlCmd
    {
        public byte   ControlType;   // 附录C: 0=解锁/闭锁 01=解锁 02=起飞 03=模式切换 04=指点飞行
        public byte   ModeType;      // 附录D: 00=引导 01=自动 02=定点 03=返航
        public short  TakeoffAlt;    // 高度*100，单位:m
        public int    Longitude;     // 经度*1e7，单位:°  int32
        public int    Latitude;      // 纬度*1e7，单位:°  int32
        public short  RelAltitude;   // 高度*100，单位:m
    }

    // ────────────────────────────────────────────────────────
    // 2.2.2 上传航线 — 单个航点
    // ────────────────────────────────────────────────────────
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaypointItem
    {
        public int    Longitude;     // 经度*1e7，单位:°
        public int    Latitude;      // 纬度*1e7，单位:°
        public byte   AltType;       // 0=相对高度 1=绝对高度
        public int    Altitude;      // 高度*100，单位:cm
    }

    // ────────────────────────────────────────────────────────
    // 2.2.3 基础姿态信息 DATA 段
    // ────────────────────────────────────────────────────────
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BasicAttitude
    {
        public byte   ArmStatus;     // 0=未解锁 1=已解锁
        public byte   ModeType;      // 附录C
        public ushort UavId;
        public int    Longitude;     // 经度*1e7，单位:°
        public int    Latitude;      // 纬度*1e7，单位:°
        public int    Altitude;      // 海拔*100，单位:cm
        public int    RelAltitude;   // 相对高度*100，单位:cm
        public short  HorizSpeed;    // 速度*100，单位:m/s
        public short  VertSpeed;     // 速度*100，单位:m/s
        public short  Roll;          // 横滚*100，单位:°  -90~90
        public short  Pitch;         // 俯仰*100，单位:°  -90~90
        public ushort Yaw;           // 偏航*100，单位:°  0~360
        public ushort BattVoltage;   // 电压*100，单位:V
        public int    HomeLon;       // 返航点经度*1e7
        public int    HomeLat;       // 返航点纬度*1e7
    }

    // ────────────────────────────────────────────────────────
    // 2.2.4 传感器数据信息 DATA 段
    // ────────────────────────────────────────────────────────
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SensorData
    {
        public short  AccX;          // 加速度*100，单位:m/s²
        public short  AccY;
        public short  AccZ;
        public short  GyroX;         // 角速度*100，单位:rad/s
        public short  GyroY;
        public short  GyroZ;
        public short  Pitch;         // 俯仰*100，单位:°
        public short  Roll;          // 横滚*100，单位:°
        public ushort Yaw;           // 偏航*100，单位:°
        public ushort Airspeed;      // 速度*100，单位:m/s
        public short  RelAlt;        // 高度*100，单位:m
        public short  ClimbRate;     // 爬升率*100，单位:m/s
        public short  ImuTemp;       // 温度*10，单位:°C
        public short  MagX;          // 磁场*100，单位:uT
        public short  MagY;
        public short  MagZ;
        public uint   Pressure;      // 气压*100，单位:Pa
        public ushort Humidity;      // 湿度*10，单位:%
        public ushort VibX;          // 振动*1000，单位:m/s²
        public ushort VibY;
        public ushort VibZ;
        public short  AoA;           // 迎角*100，单位:°
        public ushort Rangefinder;   // 距离*100，单位:m
    }

    // ────────────────────────────────────────────────────────
    // 2.2.5 电源/电池数据 DATA 段
    // ────────────────────────────────────────────────────────
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BatteryData
    {
        public ushort MainVoltage;   // 电压*100，单位:V   uint16(不会为负)
        public byte   Remaining;     // 剩余电量 %
        public short  Current;       // 电流*100，单位:A   int16(可为负=充电)
        public int    ConsumedMah;   // 已耗容量*10，单位:mAh
        public short  RangeLeft;     // 剩余续航*100，单位:km
        public ushort FcVoltage;     // 飞控板电压*1000，单位:V
        // 最多12节电芯
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public ushort[] CellVoltage; // 各电芯电压*1000，单位:V
    }

    // ────────────────────────────────────────────────────────
    // 2.2.7 导航/定位数据 DATA 段
    // ────────────────────────────────────────────────────────
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NavData
    {
        public int    Latitude;      // 纬度*1e7，单位:°
        public int    Longitude;     // 经度*1e7，单位:°
        public int    Altitude;      // 高度*1000，单位:mm
        public ushort GroundSpeed;   // 速度*100，单位:m/s
        public ushort Heading;       // 角度*10，单位:°
        public ushort HDOP;          // 精度*100，单位:m
        public uint   DistHome;      // 到起飞点距离*100，单位:cm
        public uint   TotalDist;     // 累计航程*100，单位:cm
        public uint   DistWp;        // 到航点距离*100，单位:cm
        public byte   SatCount;      // 卫星数量
        public byte   GpsStatus;     // 0=无信号 1=2D 2=3D 3=RTK
        public ushort CurrentWp;     // 当前航点编号
    }

    // ────────────────────────────────────────────────────────
    // 2.2.8 状态/错误信息 DATA 段
    // ────────────────────────────────────────────────────────
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StatusData
    {
        public byte   ArmStatus;     // 0=未解锁 1=已解锁
        public byte   LandStatus;    // 0=飞行中 1=已着陆
        public byte   ConnStatus;    // 0=未连接 1=已连接
        public byte   FlightMode;    // 0=引导/悬停 1=自动 2=返航 3=其它
        public ushort ErrorCount;    // 系统错误累计次数
        public byte   SafetyStatus;  // 0=正常 1=安全模式激活
        public short  AltError;      // 高度误差*100，单位:m
        public short  AirspeedError; // 空速误差*100，单位:m/s
    }
}
