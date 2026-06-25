using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Interfaces;
using Core.Models;

namespace DeviceModule.ViewModels
{
    public partial class DeviceStateViewModel : ObservableObject
    {
        private readonly IDeviceExecutionService _deviceExec;
        private readonly IUserSessionService _userSession;
        private CancellationTokenSource? _cts;
        private DeviceModel? _dashboardDevice;

        [ObservableProperty] private bool isPolling;

        [ObservableProperty] private string userName = string.Empty;

        // ─── 数据点（XAML 绑定）───
        public ModbusDataPoint Temperature { get; } = new()
        {
            Name = "温度1", SlaveId = 1, RegisterAddress = 0x1000,
            ModbusType = ModbusFunctionCode.ReadInputRegisters, Unit = "°C",
            DataFormat = DataFormat.Int16, Scale = 0.1f
        };

        public ModbusDataPoint Humidity { get; } = new()
        {
            Name = "湿度1", SlaveId = 1, RegisterAddress = 0x1001,
            ModbusType = ModbusFunctionCode.ReadInputRegisters, Unit = "%RH",
            DataFormat = DataFormat.Int16, Scale = 0.1f
        };

        public ModbusDataPoint WISState { get; } = new()
        {
            Name = "水浸传感器1", SlaveId = 1, RegisterAddress = 0x3000,
            ModbusType = ModbusFunctionCode.ReadCoils, DataFormat = DataFormat.Bool
        };

        public ModbusDataPoint BatTemp { get; } = new()
        {
            Name = "液冷温度", SlaveId = 1, RegisterAddress = 0x1010,
            ModbusType = ModbusFunctionCode.ReadInputRegisters, Unit = "°C",
            DataFormat = DataFormat.Int16, Scale = 0.1f
        };

        public ModbusDataPoint CoolSet { get; } = new()
        {
            Name = "制冷设定", SlaveId = 1, RegisterAddress = 0x2000,
            ModbusType = ModbusFunctionCode.ReadHoldingRegisters, Unit = "°C",
            DataFormat = DataFormat.Int16, Scale = 0.1f
        };

        public ModbusDataPoint HeatSet { get; } = new()
        {
            Name = "制热设定", SlaveId = 1, RegisterAddress = 0x2002,
            ModbusType = ModbusFunctionCode.ReadHoldingRegisters, Unit = "°C",
            DataFormat = DataFormat.Int16, Scale = 0.1f
        };

        public ModbusDataPoint Fuse1_State { get; } = new()
        {
            Name = "熔断器1", SlaveId = 1, RegisterAddress = 0x3002,
            ModbusType = ModbusFunctionCode.ReadCoils, DataFormat = DataFormat.Bool
        };

        public ModbusDataPoint Fuse2_State { get; } = new()
        {
            Name = "熔断器2", SlaveId = 1, RegisterAddress = 0x3003,
            ModbusType = ModbusFunctionCode.ReadCoils, DataFormat = DataFormat.Bool
        };

        public ModbusDataPoint Fuse3_State { get; } = new()
        {
            Name = "熔断器3", SlaveId = 1, RegisterAddress = 0x3004,
            ModbusType = ModbusFunctionCode.ReadCoils, DataFormat = DataFormat.Bool
        };

        public ModbusDataPoint AirCondition { get; } = new()
        {
            Name = "空调", SlaveId = 1, RegisterAddress = 0x3005,
            ModbusType = ModbusFunctionCode.ReadCoils, DataFormat = DataFormat.Bool
        };

        public ModbusDataPoint FireAlarm { get; } = new()
        {
            Name = "消防系统", SlaveId = 1, RegisterAddress = 0x3006,
            ModbusType = ModbusFunctionCode.ReadCoils, DataFormat = DataFormat.Bool
        };

        public ModbusDataPoint SmokeAlarm { get; } = new()
        {
            Name = "烟雾报警", SlaveId = 1, RegisterAddress = 0x3007,
            ModbusType = ModbusFunctionCode.ReadCoils, DataFormat = DataFormat.Bool
        };

        public ModbusDataPoint Dehumidifier1_State { get; } = new()
        {
            Name = "除湿机1", SlaveId = 1, RegisterAddress = 0x2004,
            ModbusType = ModbusFunctionCode.ReadCoils, DataFormat = DataFormat.Bool
        };

        public ModbusDataPoint Dehumidifier2_State { get; } = new()
        {
            Name = "除湿机2", SlaveId = 1, RegisterAddress = 0x2007,
            ModbusType = ModbusFunctionCode.ReadCoils, DataFormat = DataFormat.Bool
        };

        public ModbusDataPoint Dehumidifier3_State { get; } = new()
        {
            Name = "除湿机3", SlaveId = 1, RegisterAddress = 0x2008,
            ModbusType = ModbusFunctionCode.ReadCoils, DataFormat = DataFormat.Bool
        };

        public DeviceStateViewModel(
            IDeviceExecutionService deviceExec,
            IUserSessionService userSession)
        {
            _deviceExec = deviceExec;
            _userSession = userSession;

            if (_userSession.IsLogin)
                UserName = _userSession.CurrentUser.UserName;
        }

        [RelayCommand]
        public async Task ReadPoll()
        {
            if (IsPolling) return;
            _cts = new CancellationTokenSource();
            IsPolling = true;

            try
            {
                _dashboardDevice = BuildDashboardDevice();
                if (!await _deviceExec.ConnectAsync(_dashboardDevice))
                    return;

                var pointMap = BuildPointMap();

                while (!_cts.Token.IsCancellationRequested)
                {
                    foreach (var cmd in _dashboardDevice.Commands)
                    {
                        if (_cts.Token.IsCancellationRequested) break;

                        var result = await _deviceExec.ReadAsync(_dashboardDevice, cmd);
                        if (result.Success && pointMap.TryGetValue(cmd.Id, out var point))
                        {
                            point.UpdateValue(result.RawValue);
                        }
                    }
                    await Task.Delay(1000, _cts.Token);
                }
            }
            finally
            {
                IsPolling = false;
                if (_dashboardDevice != null)
                    await _deviceExec.DisconnectAsync(_dashboardDevice);
            }
        }

        [RelayCommand]
        public void StopPoll()
        {
            _cts?.Cancel();
            IsPolling = false;
        }

        /// <summary>构建仪表盘设备及所有传感器读取命令</summary>
        private DeviceModel BuildDashboardDevice()
        {
            var device = new DeviceModel
            {
                Id = "dashboard-modbus-tcp",
                Name = "储能系统主控",
                Config = new ModbusTCPModel
                {
                    IpAddress = "127.0.0.1",
                    Port = 502,
                    SlaveId = 1,
                    Timeout = 3000
                }
            };

            void Add(string id, string name, string address, DataFormat fmt, float scale = 1f, float offset = 0f, string? unit = null)
            {
                device.Commands.Add(new DeviceCommand
                {
                    Id = id,
                    Name = name,
                    DeviceId = device.Id,
                    CommandType = CommandType.Read,
                    ProtocolAddress = address,
                    DataFormat = fmt,
                    Scale = scale,
                    Offset = offset,
                    Unit = unit ?? ""
                });
            }

            Add("temp1", "温度1", "04:1000:2", DataFormat.Int16, 0.1f, 0, "°C");
            Add("hum1", "湿度1", "04:1001:1", DataFormat.Int16, 0.1f, 0, "%RH");
            Add("ws1", "水浸传感器1", "01:3000:1", DataFormat.Bool);
            Add("liq_temp", "液冷温度", "04:1010:1", DataFormat.Int16, 0.1f, 0, "°C");
            Add("liq_cool", "制冷设定", "03:2000:2", DataFormat.Int16, 0.1f, 0, "°C");
            Add("liq_heat", "制热设定", "03:2002:2", DataFormat.Int16, 0.1f, 0, "°C");
            Add("fuse1", "熔断器1", "01:3002:1", DataFormat.Bool);
            Add("fuse2", "熔断器2", "01:3003:1", DataFormat.Bool);
            Add("fuse3", "熔断器3", "01:3004:1", DataFormat.Bool);
            Add("ac", "空调", "01:3005:1", DataFormat.Bool);
            Add("fire", "消防系统", "01:3006:1", DataFormat.Bool);
            Add("smoke", "烟雾报警", "01:3007:1", DataFormat.Bool);
            Add("dehum1", "除湿机1", "01:2004:3", DataFormat.Bool);
            Add("dehum2", "除湿机2", "01:2007:1", DataFormat.Bool);
            Add("dehum3", "除湿机3", "01:2008:1", DataFormat.Bool);

            return device;
        }

        private Dictionary<string, ModbusDataPoint> BuildPointMap() => new()
        {
            ["temp1"] = Temperature,
            ["hum1"] = Humidity,
            ["ws1"] = WISState,
            ["liq_temp"] = BatTemp,
            ["liq_cool"] = CoolSet,
            ["liq_heat"] = HeatSet,
            ["fuse1"] = Fuse1_State,
            ["fuse2"] = Fuse2_State,
            ["fuse3"] = Fuse3_State,
            ["ac"] = AirCondition,
            ["fire"] = FireAlarm,
            ["smoke"] = SmokeAlarm,
            ["dehum1"] = Dehumidifier1_State,
            ["dehum2"] = Dehumidifier2_State,
            ["dehum3"] = Dehumidifier3_State,
        };
    }
}
