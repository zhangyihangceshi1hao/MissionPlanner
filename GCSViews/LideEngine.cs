using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MissionPlanner;

namespace MissionPlanner.GCSViews
{
    public partial class LideEngine : Form
    {
        // 不再需要本地模拟数据存储，直接从MainV2.comPort获取
        private DateTime lastUpdateTime = DateTime.MinValue;

        public LideEngine()
        {
            // 确保先初始化 components
            components = new System.ComponentModel.Container();
            InitializeComponent();
        }

        private void LideEngine_Load(object sender, EventArgs e)
        {
            // 启动定时器
            timerUpdate.Start();

            // 初始化控制命令下拉框
            cbControlCommand.SelectedIndex = 0;

            // 初始更新显示
            UpdateDisplay();
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            // 直接更新UI显示
            UpdateDisplay();

            // 更新状态栏
            toolStripStatusLabel2.Text = $"最后更新: {DateTime.Now:HH:mm:ss}";
        }

        private void UpdateDisplay()
        {
            try
            {
                // 检查comPort是否有效
                if (MainV2.comPort == null)
                {
                    Console.WriteLine("comPort未初始化");
                    return;
                }

                // 状态1 - 从MainV2.comPort直接获取
                byte engineSystemStatus = MainV2.comPort.MAV.cs.engine_system_status;
                byte engineRunning = MainV2.comPort.MAV.cs.engine_running;
                byte maintenanceStatus = MainV2.comPort.MAV.cs.maintenance_status;
                float engineRuntimeHours = MainV2.comPort.MAV.cs.engine_runtime_hours;
                ushort engineRuntimeMinutes = MainV2.comPort.MAV.cs.engine_runtime_minutes;
                ushort fuelConsumptionMl = MainV2.comPort.MAV.cs.fuel_consumption_ml;
                float fuelRateInstant = MainV2.comPort.MAV.cs.fuel_rate_instant;

                lblEngineSystemStatus.Text = GetStatusText(engineSystemStatus);
                lblEngineSystemStatus.ForeColor = GetStatusColor(engineSystemStatus);

                lblEngineRunning.Text = engineRunning == 1 ? "运行" : "停止";
                lblEngineRunning.ForeColor = engineRunning == 1 ? Color.Green : Color.Red;

                lblMaintenanceStatus.Text = GetMaintenanceStatusText(maintenanceStatus);
                lblEngineRuntimeHoursStat.Text = engineRuntimeHours.ToString("F1");
                lblEngineRuntimeMinutes.Text = engineRuntimeMinutes.ToString();
                lblFuelConsumptionMl.Text = fuelConsumptionMl.ToString();
                lblFuelRateInstant.Text = fuelRateInstant.ToString("F1");

                // 状态2
                byte throttleFeedback = MainV2.comPort.MAV.cs.throttle_feedback;
                ushort engineRpm = MainV2.comPort.MAV.cs.engine_rpm;
                float cylinderTemp1 = MainV2.comPort.MAV.cs.cylinder_temp_1;
                float cylinderTemp2 = MainV2.comPort.MAV.cs.cylinder_temp_2;
                float cylinderTemp3 = MainV2.comPort.MAV.cs.cylinder_temp_3;
                float cylinderTemp4 = MainV2.comPort.MAV.cs.cylinder_temp_4;

                lblThrottleFeedback.Text = throttleFeedback.ToString();
                lblEngineRpm.Text = engineRpm.ToString();
                lblCylinderTemp1.Text = cylinderTemp1.ToString("F1");
                lblCylinderTemp2.Text = cylinderTemp2.ToString("F1");
                lblCylinderTemp3.Text = cylinderTemp3.ToString("F1");
                lblCylinderTemp4.Text = cylinderTemp4.ToString("F1");

                // 状态3
                float exhaustTemp1 = MainV2.comPort.MAV.cs.exhaust_temp_1;
                float exhaustTemp2 = MainV2.comPort.MAV.cs.exhaust_temp_2;
                float exhaustTemp3 = MainV2.comPort.MAV.cs.exhaust_temp_3;
                float exhaustTemp4 = MainV2.comPort.MAV.cs.exhaust_temp_4;
                float coolingDoorDuty1 = MainV2.comPort.MAV.cs.cooling_door_duty_1;
                float coolingDoorDuty2 = MainV2.comPort.MAV.cs.cooling_door_duty_2;
                float coolingDoorDuty3 = MainV2.comPort.MAV.cs.cooling_door_duty_3;
                float coolingDoorDuty4 = MainV2.comPort.MAV.cs.cooling_door_duty_4;

                lblExhaustTemp1.Text = exhaustTemp1.ToString("F1");
                lblExhaustTemp2.Text = exhaustTemp2.ToString("F1");
                lblExhaustTemp3.Text = exhaustTemp3.ToString("F1");
                lblExhaustTemp4.Text = exhaustTemp4.ToString("F1");
                lblCoolingDoorDuty1.Text = coolingDoorDuty1.ToString("F1");
                lblCoolingDoorDuty2.Text = coolingDoorDuty2.ToString("F1");
                lblCoolingDoorDuty3.Text = coolingDoorDuty3.ToString("F1");
                lblCoolingDoorDuty4.Text = coolingDoorDuty4.ToString("F1");

                // 状态4
                float fuelPressureTarget = MainV2.comPort.MAV.cs.fuel_pressure_target;
                float fuelPressureActual = MainV2.comPort.MAV.cs.fuel_pressure_actual;
                ushort fuelPumpRpm = MainV2.comPort.MAV.cs.fuel_pump_rpm;
                float railPressureTarget = MainV2.comPort.MAV.cs.rail_pressure_target;
                float railPressureActual = MainV2.comPort.MAV.cs.rail_pressure_actual;
                float systemVoltage = MainV2.comPort.MAV.cs.system_voltage;
                ushort oilConsumption = MainV2.comPort.MAV.cs.oil_consumption;

                lblFuelPressureTarget.Text = fuelPressureTarget.ToString("F1");
                lblFuelPressureActual.Text = fuelPressureActual.ToString("F1");
                lblRailPressureTarget.Text = railPressureTarget.ToString("F1");
                lblRailPressureActual.Text = railPressureActual.ToString("F1");
                lblFuelPumpRpm.Text = fuelPumpRpm.ToString();
                lblSystemVoltage.Text = systemVoltage.ToString("F1");
                lblOilConsumption.Text = oilConsumption.ToString();

                // 状态5
                float throttle1Deviation = MainV2.comPort.MAV.cs.throttle1_deviation;
                float throttle1Position = MainV2.comPort.MAV.cs.throttle1_position;
                float throttle2Deviation = MainV2.comPort.MAV.cs.throttle2_deviation;
                float throttle2Position = MainV2.comPort.MAV.cs.throttle2_position;
                float intakeTemperature = MainV2.comPort.MAV.cs.intake_temperature;
                float environmentPressure = MainV2.comPort.MAV.cs.environment_pressure;
                float oilLevel = MainV2.comPort.MAV.cs.oil_level;

                lblThrottle1Deviation.Text = throttle1Deviation.ToString("F1");
                lblThrottle1Position.Text = throttle1Position.ToString("F1");
                lblThrottle2Deviation.Text = throttle2Deviation.ToString("F1");
                lblThrottle2Position.Text = throttle2Position.ToString("F1");
                lblIntakeTemperature.Text = intakeTemperature.ToString("F1");
                lblEnvironmentPressure.Text = environmentPressure.ToString("F1");
                lblOilLevel.Text = oilLevel.ToString("F1");

                // 汇总数据
                byte engineStatusSummary = MainV2.comPort.MAV.cs.engine_status_summary;
                ushort engineRpmSummary = MainV2.comPort.MAV.cs.engine_rpm_summary;
                byte throttleFeedbackSummary = MainV2.comPort.MAV.cs.throttle_feedback_summary;
                float cylinderTempMax = MainV2.comPort.MAV.cs.cylinder_temp_max;
                float exhaustTempMax = MainV2.comPort.MAV.cs.exhaust_temp_max;
                float fuelPressureTargetSummary = MainV2.comPort.MAV.cs.fuel_pressure_target_summary;
                float railPressureTargetSummary = MainV2.comPort.MAV.cs.rail_pressure_target_summary;
                float systemVoltageSummary = MainV2.comPort.MAV.cs.system_voltage_summary;
                float engineRuntimeHoursSummary = MainV2.comPort.MAV.cs.engine_runtime_hours_summary;
                ushort engineRuntimeMinutesSummary = MainV2.comPort.MAV.cs.engine_runtime_minutes_summary;
                byte faultCount = MainV2.comPort.MAV.cs.fault_count;
                byte maintenanceStatusSummary = MainV2.comPort.MAV.cs.maintenance_status_summary;
                byte engineHealthScore = MainV2.comPort.MAV.cs.engine_health_score;
                ushort maintenanceTimeRemaining = MainV2.comPort.MAV.cs.maintenance_time_remaining;

                lblEngineStatus.Text = GetStatusText(engineStatusSummary);
                lblEngineStatus.ForeColor = GetStatusColor(engineStatusSummary);
                lblEngineRpmSum.Text = engineRpmSummary.ToString();
                lblThrottleFeedbackSum.Text = throttleFeedbackSummary.ToString();
                lblCylinderTempMax.Text = cylinderTempMax.ToString("F1");
                lblExhaustTempMax.Text = exhaustTempMax.ToString("F1");
                lblFuelPressureTargetSum.Text = fuelPressureTargetSummary.ToString("F1");
                lblRailPressureTargetSum.Text = railPressureTargetSummary.ToString("F1");
                lblSystemVoltageSum.Text = systemVoltageSummary.ToString("F1");
                lblEngineRuntimeHours.Text = engineRuntimeHoursSummary.ToString("F1");
                lblEngineRuntimeMins.Text = engineRuntimeMinutesSummary.ToString();
                lblFaultCount.Text = faultCount.ToString();
                lblMaintenanceRemaining.Text = maintenanceTimeRemaining.ToString();
                lblEngineHealthScore.Text = engineHealthScore.ToString();

                // 根据数值变化颜色
                UpdateColorCoding(cylinderTemp1, cylinderTemp2, cylinderTemp3, cylinderTemp4,
                    exhaustTemp1, exhaustTemp2, exhaustTemp3, exhaustTemp4,
                    systemVoltage, systemVoltageSummary, oilLevel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新显示时出错: {ex.Message}");
            }
        }

        private void UpdateColorCoding(float cylTemp1, float cylTemp2, float cylTemp3, float cylTemp4,
                                     float exhTemp1, float exhTemp2, float exhTemp3, float exhTemp4,
                                     float voltage, float voltageSum, float oilLevel)
        {
            try
            {
                // 缸头温度颜色编码
                UpdateTemperatureColor(lblCylinderTemp1, cylTemp1, 150, 180);
                UpdateTemperatureColor(lblCylinderTemp2, cylTemp2, 150, 180);
                UpdateTemperatureColor(lblCylinderTemp3, cylTemp3, 150, 180);
                UpdateTemperatureColor(lblCylinderTemp4, cylTemp4, 150, 180);

                // 排气温度颜色编码
                UpdateTemperatureColor(lblExhaustTemp1, exhTemp1, 700, 800);
                UpdateTemperatureColor(lblExhaustTemp2, exhTemp2, 700, 800);
                UpdateTemperatureColor(lblExhaustTemp3, exhTemp3, 700, 800);
                UpdateTemperatureColor(lblExhaustTemp4, exhTemp4, 700, 800);

                // 电压颜色编码
                UpdateVoltageColor(lblSystemVoltage, voltage, 22, 28);
                UpdateVoltageColor(lblSystemVoltageSum, voltageSum, 22, 28);

                // 油位颜色编码
                UpdateOilLevelColor(lblOilLevel, oilLevel, 20, 10);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新颜色编码时出错: {ex.Message}");
            }
        }

        private void UpdateTemperatureColor(Label label, float value, float warning, float critical)
        {
            if (label == null) return;

            if (value >= critical)
                label.ForeColor = Color.Red;
            else if (value >= warning)
                label.ForeColor = Color.Orange;
            else
                label.ForeColor = Color.Green;
        }

        private void UpdateVoltageColor(Label label, float value, float min, float max)
        {
            if (label == null) return;

            if (value < min || value > max)
                label.ForeColor = Color.Red;
            else if (value < min + 1 || value > max - 1)
                label.ForeColor = Color.Orange;
            else
                label.ForeColor = Color.Green;
        }

        private void UpdateOilLevelColor(Label label, float value, float warning, float critical)
        {
            if (label == null) return;

            if (value <= critical)
                label.ForeColor = Color.Red;
            else if (value <= warning)
                label.ForeColor = Color.Orange;
            else
                label.ForeColor = Color.Green;
        }

        private string GetStatusText(int status)
        {
            switch (status)
            {
                case 1: return "正常";
                case 2: return "异常";
                case 3: return "警告";
                default: return "未知";
            }
        }

        private Color GetStatusColor(int status)
        {
            switch (status)
            {
                case 1: return Color.Green;
                case 2: return Color.Red;
                case 3: return Color.Orange;
                default: return Color.Gray;
            }
        }

        private string GetMaintenanceStatusText(int status)
        {
            switch (status)
            {
                case 0: return "正常";
                case 1: return "100h保养";
                case 2: return "200h保养";
                case 3: return "300h大修";
                default: return "未知";
            }
        }
        private void btnSendControl_Click(object sender, EventArgs e)
        {
            try
            {
                // 解析控制命令 - 使用decimal类型来保持精度
                decimal throttleRequestDecimal = decimal.Parse(tbThrottleRequest.Text);
                decimal altitudeDecimal = decimal.Parse(tbAltitude.Text);
                decimal airspeedDecimal = decimal.Parse(tbAirspeed.Text);
                byte controlCommandSelection = (byte)cbControlCommand.SelectedIndex;
                byte reserved1 = byte.Parse(tbReserved1.Text);
                byte reserved2 = byte.Parse(tbReserved2.Text);

                // 获取复选框状态
                bool isStartValid = checkBox1.Checked;    // 启动有效位
                bool isHeatingValid = checkBox2.Checked;  // 加热有效位
                bool isAltitudeValid = checkBox3.Checked; // 海拔有效位
                bool isAirspeedValid = checkBox4.Checked; // 空速有效位

                // ========== 验证部分 ==========

                // 验证油门请求范围(0-100)和精度(0.1)
                if (throttleRequestDecimal < 0 || throttleRequestDecimal > 100)
                {
                    MessageBox.Show("油门请求应在 0-100% 范围内", "输入错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 检查油门请求精度是否为0.1的倍数
                decimal throttleRemainder = throttleRequestDecimal * 10 % 1;
                if (throttleRemainder != 0)
                {
                    MessageBox.Show("油门请求精度应为0.1%，如：10.0%, 25.5%, 99.9%等",
                        "精度错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 验证海拔范围(0-10000)和精度(1) - 如果有效才验证
                if (isAltitudeValid)
                {
                    if (altitudeDecimal < 0 || altitudeDecimal > 10000)
                    {
                        MessageBox.Show("海拔应在 0-10000 米范围内", "输入错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (altitudeDecimal != Math.Floor(altitudeDecimal))
                    {
                        MessageBox.Show("海拔精度应为1米，请输入整数",
                            "精度错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // 验证空速范围(0-63)和精度(整数) - 如果有效才验证
                if (isAirspeedValid)
                {
                    // 验证范围 0-63
                    if (airspeedDecimal < 0 || airspeedDecimal > 63)
                    {
                        MessageBox.Show("空速应在 0-63 m/s 范围内", "输入错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 验证为整数
                    if (airspeedDecimal != Math.Floor(airspeedDecimal))
                    {
                        MessageBox.Show("空速应为整数，如：0, 25, 63等",
                            "精度错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // 验证预留字段范围(0-255)
                if (reserved1 < 0 || reserved1 > 255 || reserved2 < 0 || reserved2 > 255)
                {
                    MessageBox.Show("预留字段应在 0-255 范围内", "输入错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ========== 构建控制命令字节 ==========
                byte controlCommand = 0x00; // 初始化为0

                // 根据组合框选择的命令设置对应位
                switch (controlCommandSelection)
                {
                    case 0: // 无操作 - 所有指令位都置0
                        break;

                    case 1: // 停止发动机 - 停机指令 (Bit0 = 1)
                        controlCommand |= 0x01; // Bit0 = 1
                        break;

                    case 2: // 停止加热 - 加热指令 (Bit1 = 0)
                        break;

                    case 3: // 请求加热 - 加热指令 (Bit1 = 1)
                        controlCommand |= 0x02; // Bit1 = 1
                        break;

                    case 4: // 待机 - 启动指令 (Bit2 = 0)
                        break;

                    case 5: // 启动发动机 - 启动指令 (Bit2 = 1)
                        controlCommand |= 0x04; // Bit2 = 1
                        break;

                    case 6: // 摆桨指令 - 摆桨指令 (Bit3 = 1)
                        controlCommand |= 0x08; // Bit3 = 1
                        break;
                }

              

                // ========== MAVLink转换部分 ==========

                // 1. 油门请求：0-1000对应0-100%，放大10倍
                ushort mavlinkThrottle = (ushort)(throttleRequestDecimal * 10);

                // 2. 海拔：直接使用米数值（如果无效则发送0）
                ushort mavlinkAltitude =  (ushort)altitudeDecimal ;

                // 3. 空速：乘以4编码
                // 用户输入整数0-63，乘以4编码后范围0-252
                byte mavlinkAirspeed = (byte)(airspeedDecimal * 4);
                    if (mavlinkAirspeed > 252) // 安全检查
                    {
                        MessageBox.Show($"空速编码值超出范围: {mavlinkAirspeed} > 252",
                            "转换错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                
               

                // ========== 模拟CAN数据帧构建和打印 ==========
                Console.WriteLine("\n========== 模拟CAN数据帧构建 ==========");

                // 假设的CAN ID和长度
                uint canId = 0x00000600; // 您的CAN ID
                byte dlc = 8;

                // 模拟数据帧构建（与飞控端相同逻辑）
                byte[] canData = new byte[8];

                // 油门请求 (0-1000对应0-100%) - 大端序
                canData[0] = (byte)((mavlinkThrottle >> 8) & 0xFF); // 油门高字节
                canData[1] = (byte)(mavlinkThrottle & 0xFF);        // 油门低字节

                // 海拔高度 (米) - 大端序
                canData[2] = (byte)((mavlinkAltitude >> 8) & 0xFF); // 海拔高字节
                canData[3] = (byte)(mavlinkAltitude & 0xFF);        // 海拔低字节

                // 空速 (m/s) - 乘以4编码
                canData[4] = mavlinkAirspeed;

                // 控制命令字节
                canData[5] = controlCommand;

                // 预留字节
                canData[6] = reserved1;
                canData[7] = reserved2;

                // 打印CAN数据帧（与飞控端相同的格式）
                Console.WriteLine("CAN控制命令数据帧:");
                Console.WriteLine($"ID: 0x{canId:X8}, 长度: {dlc}字节");
                Console.WriteLine($"[0]: {canData[0]:X2} (油门高字节)");
                Console.WriteLine($"[1]: {canData[1]:X2} (油门低字节)");
                Console.WriteLine($"[2]: {canData[2]:X2} (海拔高字节)");
                Console.WriteLine($"[3]: {canData[3]:X2} (海拔低字节)");
                Console.WriteLine($"[4]: {canData[4]:X2} (空速编码值)");
                Console.WriteLine($"[5]: {canData[5]:X2} (控制命令)");
                Console.WriteLine($"[6]: {canData[6]:X2} (预留1)");
                Console.WriteLine($"[7]: {canData[7]:X2} (预留2)");

                // 解析字段值
                ushort throttleVal = (ushort)((canData[0] << 8) | canData[1]);
                ushort altitudeVal = (ushort)((canData[2] << 8) | canData[3]);
                float airspeedVal = canData[4] / 4.0f;

                Console.WriteLine("\n字段解析:");
                Console.WriteLine($"  油门: {throttleVal} -> {throttleVal / 10.0f:F1}%");
                Console.WriteLine($"  海拔: {altitudeVal}米");
                Console.WriteLine($"  空速编码: {canData[4]} -> {airspeedVal:F0} m/s");

                // 解析控制命令字节的各个位
                // 解析控制命令字节的各个位
                Console.WriteLine("\n控制命令字节位详情:");
                for (int bit = 7; bit >= 0; bit--)
                {
                    bool bitValue = (controlCommand & (1 << bit)) != 0;
                    string bitStatus = bitValue ? "1" : "0";

                    // 使用switch语句替代switch表达式
                    string bitName;
                    switch (bit)
                    {
                        case 0:
                            bitName = "停机指令";
                            break;
                        case 1:
                            bitName = "加热指令";
                            break;
                        case 2:
                            bitName = "启动指令";
                            break;
                        case 3:
                            bitName = "摆桨指令";
                            break;
                        case 4:
                            bitName = "启动有效";
                            break;
                        case 5:
                            bitName = "加热有效";
                            break;
                        case 6:
                            bitName = "海拔有效";
                            break;
                        case 7:
                            bitName = "空速有效";
                            break;
                        default:
                            bitName = "未知";
                            break;
                    }

                    Console.WriteLine($"  Bit{bit} ({bitName}): {bitStatus}");
                }

                Console.WriteLine($"\n最终控制命令字节:");
                Console.WriteLine($"  二进制: {Convert.ToString(controlCommand, 2).PadLeft(8, '0')}");
                Console.WriteLine($"  十六进制: 0x{controlCommand:X2}");
                Console.WriteLine($"  十进制: {controlCommand}");

                Console.WriteLine("\n参数转换详情:");
                Console.WriteLine($"  油门请求: {throttleRequestDecimal}% -> {mavlinkThrottle} (×10)");
                Console.WriteLine($"  海拔: {altitudeDecimal}米 -> {mavlinkAltitude}");
                Console.WriteLine($"  空速: {airspeedDecimal}m/s -> {mavlinkAirspeed} (×4)");
                Console.WriteLine($"  空速有效: {isAirspeedValid}, 海拔有效: {isAltitudeValid}");
                Console.WriteLine($"  启动有效: {isStartValid}, 加热有效: {isHeatingValid}");
                Console.WriteLine($"  预留1: {reserved1}, 预留2: {reserved2}");

                Console.WriteLine("==========================================\n");

                // 发送 MAVLink 消息
                SendMavlinkControlCommand(mavlinkThrottle, mavlinkAltitude, mavlinkAirspeed,
                    controlCommand, reserved1, reserved2);

                // 在UI界面上显示CAN数据帧信息
                string canFrameInfo = $"CAN数据帧信息:\n";
                canFrameInfo += $"ID: 0x{canId:X8}\n";
                canFrameInfo += $"长度: {dlc}字节\n";
                canFrameInfo += $"数据字节:\n";
                for (int i = 0; i < 8; i++)
                {
                    canFrameInfo += $"  [{i}]: 0x{canData[i]:X2}\n";
                }

                canFrameInfo += $"\n字段解析:\n";
                canFrameInfo += $"油门: {throttleVal / 10.0f:F1}% ({throttleVal})\n";
                canFrameInfo += $"海拔: {altitudeVal}米\n";
                canFrameInfo += $"空速: {airspeedVal:F0}m/s (编码: {canData[4]})\n";
                canFrameInfo += $"控制命令: 0x{controlCommand:X2}\n";

                // 可选：在界面上显示
                MessageBox.Show("控制命令发送成功!\n\n" + canFrameInfo, "发送成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (FormatException)
            {
                MessageBox.Show("请输入有效的数字", "格式错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OverflowException)
            {
                MessageBox.Show("输入数值超出允许范围", "范围错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发送失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //private void btnSendControl_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // 解析控制命令
        //        int throttleRequest = int.Parse(tbThrottleRequest.Text);
        //        int altitude = int.Parse(tbAltitude.Text);
        //        byte airspeed = byte.Parse(tbAirspeed.Text);
        //        byte controlCommand = (byte)cbControlCommand.SelectedIndex;
        //        byte reserved1 = byte.Parse(tbReserved1.Text);
        //        byte reserved2 = byte.Parse(tbReserved2.Text);

        //        // 验证数据范围
        //        if (throttleRequest < 0 || throttleRequest > 1000)
        //        {
        //            MessageBox.Show("油门请求应在0-1000范围内", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        if (altitude < 0 || altitude > 10000)
        //        {
        //            MessageBox.Show("海拔应在0-10000范围内", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        // 发送MAVLink消息
        //        SendMavlinkControlCommand(throttleRequest, altitude, airspeed, controlCommand, reserved1, reserved2);

        //        MessageBox.Show("控制命令发送成功!", "发送成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //    catch (FormatException)
        //    {
        //        MessageBox.Show("请输入有效的数字", "格式错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"发送失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        private void SendMavlinkControlCommand(int throttle, int altitude, int airspeed, byte controlCmd, byte reserved1, byte reserved2)
        {
            try
            {
                // 创建LIDE_CAN_CONTROL消息
                MAVLink.mavlink_lide_can_control_t msg = new MAVLink.mavlink_lide_can_control_t();
                msg.throttle_request = (ushort)throttle;
                msg.altitude = (ushort)altitude;
                msg.airspeed = (ushort)airspeed;
                msg.control_command = controlCmd;
                msg.reserved1 = reserved1;
                msg.reserved2 = reserved2;

                // 发送消息
                if (MainV2.comPort != null && MainV2.comPort.BaseStream.IsOpen)
                {
                    MainV2.comPort.sendPacket(msg, MainV2.comPort.MAV.sysid, MainV2.comPort.MAV.compid);
                    Console.WriteLine($"发送控制命令: 油门={throttle}, 海拔={altitude}, 空速={airspeed}, 命令={controlCmd}");
                }
                else
                {
                    MessageBox.Show("通信端口未连接", "发送失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送控制命令失败: {ex.Message}");
                MessageBox.Show($"发送失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 数据接收处理函数 - 现在数据已经自动更新到MainV2.comPort中
        public void ProcessMavlinkMessage(MAVLink.MAVLinkMessage msg)
        {
            // 数据已经在MAVLink解析时更新到MainV2.comPort的字段中
            // 这里可以记录接收时间或触发其他事件
            lastUpdateTime = DateTime.Now;

            // 如果需要，可以在这里触发UI更新
            if (this.Visible)
            {
                if (InvokeRequired)
                {
                    this.Invoke(new Action(() => UpdateDisplay()));
                }
                else
                {
                    UpdateDisplay();
                }
            }
        }
    }

    // 数据存储类 - 现在不再需要，直接从MainV2.comPort获取
    /*
    public class EngineData
    {
        // ... 原有的字段定义 ...
    }
    */
}