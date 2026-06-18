using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Events;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Communication;
using Prism.Events;

namespace DeviceModule.ViewModels
{
    public partial class DeviceStateViewModel : ObservableObject
    {
        private readonly IModbusCommunicationService _communicationService;
        private readonly IPointTableService _pointTableService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IUserSessionService _userSessionService;
        private CancellationTokenSource _cts;

        [ObservableProperty]
        public bool isPolling;

        [ObservableProperty]
        private string userName = string.Empty;

        public DataPoint Temperature => _pointTableService.Temp1;
        public DataPoint Humidity => _pointTableService.Humidity1;
        public DataPoint WISState => _pointTableService.WaterSensor1;
        public DataPoint BatTemp => _pointTableService.LiquidTemperature;
        public DataPoint CoolSet => _pointTableService.LiquidSetCoolTemp;
        public DataPoint HeatSet => _pointTableService.LiquidSetHeatTemp;
        public DataPoint Fuse1_State => _pointTableService.Fuse1;
        public DataPoint Fuse2_State => _pointTableService.Fuse2;
        public DataPoint Fuse3_State => _pointTableService.Fuse3;
        public DataPoint AirCondition => _pointTableService.AirConditioner;
        public DataPoint FireAlram => _pointTableService.FireSystemState;
        public DataPoint SmokeAlarm => _pointTableService.SmokeAlarm;
        public DataPoint Dehumidifier1_State => _pointTableService.Dehumidifier1;
        public DataPoint Dehumidifier2_State => _pointTableService.Dehumidifier2;
        public DataPoint Dehumidifier3_State => _pointTableService.Dehumidifier3;

        public DeviceStateViewModel(IModbusCommunicationService communicationService,
            IPointTableService pointTableService,
            IEventAggregator eventAggregator,
            IUserSessionService userSessionService)
        {
            _communicationService = communicationService;
            _pointTableService = pointTableService;
            _eventAggregator = eventAggregator;
            _userSessionService = userSessionService;

            if (_userSessionService.IsLogin)
                UserName = _userSessionService.CurrentUser.UserName;

            _eventAggregator.GetEvent<DataUpdatedEvent>().Subscribe(OnDataReceived, Prism.Events.ThreadOption.UIThread);

            _cts = new CancellationTokenSource();
            Task.Run(() => _communicationService.ReadConsumer(_cts.Token));
        }

        private void OnDataReceived(ModbusReadResult result)
        {
            UpdatePointValue(result);
        }

        private void UpdatePointValue(ModbusReadResult result)
        {
            var pointMap = BuildPointLookup();

            if (result.Function is ModbusFunctionCode.ReadCoils or ModbusFunctionCode.ReadDiscreteInputs)
            {
                for (var i = 0; i < result.CoilValues.Length; i++)
                {
                    ushort address = (ushort)(result.StartAddress + i);
                    if (pointMap.TryGetValue((result.SlaveId, address, result.Function), out var point))
                    {
                        point.UpdateValue(result.CoilValues[i]);
                    }
                }
            }
            else
            {
                for (var i = 0; i < result.RegisterValues.Length; i++)
                {
                    ushort address = (ushort)(result.StartAddress + i);
                    if (pointMap.TryGetValue((result.SlaveId, address, result.Function), out var point))
                    {
                        point.UpdateValue(result.RegisterValues[i] / 10f);
                    }
                }
            }
        }

        [RelayCommand]
        public async Task ReadPoll()
        {
            if (IsPolling) return;
            _cts = new CancellationTokenSource();
            IsPolling = true;
            try
            {
                await _communicationService.ReadProducer(_cts.Token);
            }
            finally
            {
                IsPolling = false;
            }
        }

        [RelayCommand]
        public void StopPoll()
        {
            _cts?.Cancel();
            IsPolling = false;
        }

        private Dictionary<(byte, ushort, ModbusFunctionCode), DataPoint> BuildPointLookup()
        {
            var dict = new Dictionary<(byte, ushort, ModbusFunctionCode), DataPoint>();
            AddPoint(dict, _pointTableService.Temp1, ModbusFunctionCode.ReadInputRegisters);
            AddPoint(dict, _pointTableService.Humidity1, ModbusFunctionCode.ReadInputRegisters);
            AddPoint(dict, _pointTableService.WaterSensor1, ModbusFunctionCode.ReadCoils);
            AddPoint(dict, _pointTableService.WaterSensor2, ModbusFunctionCode.ReadCoils);
            AddPoint(dict, _pointTableService.LiquidTemperature, ModbusFunctionCode.ReadInputRegisters);
            AddPoint(dict, _pointTableService.LiquidSetCoolTemp, ModbusFunctionCode.ReadHoldingRegisters);
            AddPoint(dict, _pointTableService.LiquidSetHeatTemp, ModbusFunctionCode.ReadHoldingRegisters);
            AddPoint(dict, _pointTableService.Fuse1, ModbusFunctionCode.ReadCoils);
            AddPoint(dict, _pointTableService.Fuse2, ModbusFunctionCode.ReadCoils);
            AddPoint(dict, _pointTableService.Fuse3, ModbusFunctionCode.ReadCoils);
            AddPoint(dict, _pointTableService.AirConditioner, ModbusFunctionCode.ReadCoils);
            AddPoint(dict, _pointTableService.FireSystemState, ModbusFunctionCode.ReadCoils);
            AddPoint(dict, _pointTableService.SmokeAlarm, ModbusFunctionCode.ReadCoils);
            AddPoint(dict, _pointTableService.Dehumidifier1, ModbusFunctionCode.ReadCoils);
            AddPoint(dict, _pointTableService.Dehumidifier2, ModbusFunctionCode.ReadCoils);
            AddPoint(dict, _pointTableService.Dehumidifier3, ModbusFunctionCode.ReadCoils);
            return dict;
        }

        private void AddPoint(Dictionary<(byte, ushort, ModbusFunctionCode), DataPoint> dict, DataPoint point, ModbusFunctionCode func)
        {
            if (point is ModbusDataPoint modbusPoint)
            {
                dict[(modbusPoint.SlaveId, modbusPoint.RegisterAddress, func)] = point;
            }
        }
    }
}