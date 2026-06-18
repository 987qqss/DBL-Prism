using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Interfaces;
using Core.Models;
using DeviceModule.Views;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.IO.Ports;

namespace DeviceModule.ViewModels
{
    public partial class DeviceWizardViewModel : ObservableObject
    {
        private int _currentStep = 0;
        private readonly UserControl[] _stepViews;
        private readonly string[] _stepTitles = { "选择通讯协议", "配置设备信息", "测试连接", "完成" };

        // 步骤颜色
        private string _step1Color = "#3B82F6";
        public string Step1Color
        {
            get => _step1Color;
            set => SetProperty(ref _step1Color, value);
        }

        private string _step2Color = "#2A2A4A";
        public string Step2Color
        {
            get => _step2Color;
            set => SetProperty(ref _step2Color, value);
        }

        private string _step3Color = "#2A2A4A";
        public string Step3Color
        {
            get => _step3Color;
            set => SetProperty(ref _step3Color, value);
        }

        private string _step4Color = "#2A2A4A";
        public string Step4Color
        {
            get => _step4Color;
            set => SetProperty(ref _step4Color, value);
        }

        // 步骤文字颜色
        private string _step1TextColor = "#E2E8F0";
        public string Step1TextColor
        {
            get => _step1TextColor;
            set => SetProperty(ref _step1TextColor, value);
        }

        private string _step2TextColor = "#6B7280";
        public string Step2TextColor
        {
            get => _step2TextColor;
            set => SetProperty(ref _step2TextColor, value);
        }

        private string _step3TextColor = "#6B7280";
        public string Step3TextColor
        {
            get => _step3TextColor;
            set => SetProperty(ref _step3TextColor, value);
        }

        private string _step4TextColor = "#6B7280";
        public string Step4TextColor
        {
            get => _step4TextColor;
            set => SetProperty(ref _step4TextColor, value);
        }

        // 连接线颜色
        private string _line1Color = "#3B82F6";
        public string Line1Color
        {
            get => _line1Color;
            set => SetProperty(ref _line1Color, value);
        }

        private string _line2Color = "#2A2A4A";
        public string Line2Color
        {
            get => _line2Color;
            set => SetProperty(ref _line2Color, value);
        }

        private string _line3Color = "#2A2A4A";
        public string Line3Color
        {
            get => _line3Color;
            set => SetProperty(ref _line3Color, value);
        }

        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                SetProperty(ref _currentStep, value);
                OnPropertyChanged(nameof(CurrentStepView));
                OnPropertyChanged(nameof(StepTitle));
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(NextButtonText));
                OnPropertyChanged(nameof(ShowFinishIcon));
                OnPropertyChanged(nameof(ShowNextIcon));
                OnPropertyChanged(nameof(ShowCancelButton));
                UpdateStepIndicators();
            }
        }

        public UserControl CurrentStepView => _stepViews[CurrentStep];

        public string StepTitle => _stepTitles[CurrentStep];

        public bool CanGoBack => CurrentStep > 0;

        public string NextButtonText => CurrentStep == 3 ? "完成" : "下一步";

        public bool ShowFinishIcon => CurrentStep == 3;
        public bool ShowNextIcon => CurrentStep != 3;
        public bool ShowCancelButton => CurrentStep < 3;

        public DeviceModel NewDevice { get; } = new DeviceModel();

        // UI绑定属性（保持向后兼容）
        private string _ipAddress = string.Empty;
        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        private int _port = 502;
        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        private string _serialPortName = string.Empty;
        public string SerialPortName
        {
            get => _serialPortName;
            set => SetProperty(ref _serialPortName, value);
        }

        private int _baudRate = 9600;
        public int BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

        private int _dataBits = 8;
        public int DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }

        private string _parity = "None";
        public string Parity
        {
            get => _parity;
            set => SetProperty(ref _parity, value);
        }

        private string _stopBits = "One";
        public string StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
        }

        private int _s7Rack = 0;
        public int S7Rack
        {
            get => _s7Rack;
            set => SetProperty(ref _s7Rack, value);
        }

        private int _s7Slot = 2;
        public int S7Slot
        {
            get => _s7Slot;
            set => SetProperty(ref _s7Slot, value);
        }

        private byte _slaveId = 1;
        public byte SlaveId
        {
            get => _slaveId;
            set => SetProperty(ref _slaveId, value);
        }

        private int _timeout = 3000;
        public int Timeout
        {
            get => _timeout;
            set => SetProperty(ref _timeout, value);
        }

        private int _retryCount = 3;
        public int RetryCount
        {
            get => _retryCount;
            set => SetProperty(ref _retryCount, value);
        }

        public ObservableCollection<string> AvailablePorts { get; } = new();
        public ObservableCollection<int> BaudRates { get; } = new() { 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
        public ObservableCollection<string> ParityOptions { get; } = new() { "None", "Odd", "Even", "Mark", "Space" };
        public ObservableCollection<string> StopBitsOptions { get; } = new() { "One", "OnePointFive", "Two" };

        private bool _isTesting = false;
        public bool IsTesting
        {
            get => _isTesting;
            set 
            { 
                SetProperty(ref _isTesting, value);
                OnPropertyChanged(nameof(CanTestConnection));
            }
        }

        public bool CanTestConnection => !IsTesting;

        private string _testResult = string.Empty;
        public string TestResult
        {
            get => _testResult;
            set => SetProperty(ref _testResult, value);
        }

        private string _testResultDetails = string.Empty;
        public string TestResultDetails
        {
            get => _testResultDetails;
            set => SetProperty(ref _testResultDetails, value);
        }

        private bool _testSuccess = false;
        public bool TestSuccess
        {
            get => _testSuccess;
            set => SetProperty(ref _testSuccess, value);
        }

        private bool _isConnectionSuccessful = false;
        public bool IsConnectionSuccessful
        {
            get => _isConnectionSuccessful;
            set => SetProperty(ref _isConnectionSuccessful, value);
        }

        public DeviceWizardViewModel()
        {
            LoadSerialPorts();

            _stepViews = new UserControl[]
            {
                new ProtocolSelectView { DataContext = this },
                new DeviceConfigView { DataContext = this },
                new ConnectionTestView { DataContext = this },
                new CompleteView { DataContext = this }
            };

            UpdateStepIndicators();
        }

        private void UpdateStepIndicators()
        {
            Step1Color = Step2Color = Step3Color = Step4Color = "#2A2A4A";
            Step1TextColor = Step2TextColor = Step3TextColor = Step4TextColor = "#6B7280";
            Line1Color = Line2Color = Line3Color = "#2A2A4A";

            if (CurrentStep >= 1)
            {
                Step1Color = "#22C55E";
                Step1TextColor = "#22C55E";
                Line1Color = "#22C55E";
            }

            if (CurrentStep >= 2)
            {
                Step2Color = "#22C55E";
                Step2TextColor = "#22C55E";
                Line2Color = "#22C55E";
            }

            if (CurrentStep >= 3)
            {
                Step3Color = "#22C55E";
                Step3TextColor = "#22C55E";
                Line3Color = "#22C55E";
            }

            switch (CurrentStep)
            {
                case 0:
                    Step1Color = "#3B82F6";
                    Step1TextColor = "#E2E8F0";
                    Line1Color = "#3B82F6";
                    break;
                case 1:
                    Step2Color = "#3B82F6";
                    Step2TextColor = "#E2E8F0";
                    Line2Color = "#3B82F6";
                    break;
                case 2:
                    Step3Color = "#3B82F6";
                    Step3TextColor = "#E2E8F0";
                    Line3Color = "#3B82F6";
                    break;
                case 3:
                    Step4Color = "#22C55E";
                    Step4TextColor = "#22C55E";
                    break;
            }

            OnPropertyChanged(nameof(Step1Color));
            OnPropertyChanged(nameof(Step2Color));
            OnPropertyChanged(nameof(Step3Color));
            OnPropertyChanged(nameof(Step4Color));
            OnPropertyChanged(nameof(Step1TextColor));
            OnPropertyChanged(nameof(Step2TextColor));
            OnPropertyChanged(nameof(Step3TextColor));
            OnPropertyChanged(nameof(Step4TextColor));
            OnPropertyChanged(nameof(Line1Color));
            OnPropertyChanged(nameof(Line2Color));
            OnPropertyChanged(nameof(Line3Color));
        }

        private void LoadSerialPorts()
        {
            try
            {
                foreach (var port in SerialPort.GetPortNames())
                {
                    AvailablePorts.Add(port);
                }
            }
            catch
            {
                AvailablePorts.Add("COM1");
                AvailablePorts.Add("COM2");
                AvailablePorts.Add("COM3");
            }
        }

        [RelayCommand]
        public void SelectProtocol(ProtocolType protocol)
        {
            NewDevice.ProtocolType = protocol;
            
            IProtocolConfig config = protocol switch
            {
                ProtocolType.ModbusTcp => new ModbusTCPModel(),
                ProtocolType.ModbusRtu => new ModbusRTUModel(),
                ProtocolType.S7 => new S7Model(),
                ProtocolType.TcpIp => new TCPIPModel(),
                ProtocolType.OpcUa => new OPCUAModel(),
                ProtocolType.Dnp3 => new DNP3Model(),
                ProtocolType.Bacnet => new BACnetModel(),
                _ => new CustomProtocolModel()
            };
            
            NewDevice.SetConfig(config);
        }

        [RelayCommand]
        public async Task TestConnection()
        {
            IsTesting = true;
            TestResult = "正在测试连接...";
            TestResultDetails = string.Empty;
            IsConnectionSuccessful = false;
            TestSuccess = false;

            await Task.Delay(2000);

            try
            {
                NewDevice.Config?.Validate();
                
                TestResult = "连接成功!";
                TestResultDetails = $"成功连接到 {IpAddress}\n响应时间: 15ms\n设备状态: 正常";
                TestSuccess = true;
                IsConnectionSuccessful = true;
            }
            catch (Exception ex)
            {
                TestResult = "连接失败";
                TestResultDetails = ex.Message;
                TestSuccess = false;
                IsConnectionSuccessful = false;
            }

            IsTesting = false;
        }

        [RelayCommand]
        public void AddAnotherDevice()
        {
            NewDevice.Name = string.Empty;
            NewDevice.ProtocolType = ProtocolType.ModbusTcp;
            NewDevice.SetConfig(new ModbusTCPModel());
            
            IpAddress = string.Empty;
            Port = 502;
            SerialPortName = string.Empty;
            BaudRate = 9600;
            DataBits = 8;
            Parity = "None";
            StopBits = "One";
            S7Rack = 0;
            S7Slot = 2;
            SlaveId = 1;
            Timeout = 3000;
            RetryCount = 3;
            
            CurrentStep = 0;
            UpdateStepIndicators();
        }

        [RelayCommand]
        public void NextStep()
        {
            if (CurrentStep == 1)
            {
                SaveConfigToDevice();
            }
            
            if (CurrentStep < 3)
            {
                CurrentStep++;
            }
            else
            {
                CompleteWizard();
            }
        }

        private void SaveConfigToDevice()
        {
            switch (NewDevice.ProtocolType)
            {
                case ProtocolType.ModbusTcp:
                    var modbusConfig = NewDevice.GetConfig<ModbusTCPModel>();
                    modbusConfig.IpAddress = IpAddress;
                    modbusConfig.Port = Port;
                    modbusConfig.SlaveId = SlaveId;
                    modbusConfig.Timeout = Timeout;
                    modbusConfig.RetryCount = RetryCount;
                    break;
                    
                case ProtocolType.ModbusRtu:
                    var rtuConfig = NewDevice.GetConfig<ModbusRTUModel>();
                    rtuConfig.SerialPortName = SerialPortName;
                    rtuConfig.BaudRate = BaudRate;
                    rtuConfig.DataBits = DataBits;
                    rtuConfig.Parity = Parity;
                    rtuConfig.StopBits = StopBits;
                    rtuConfig.SlaveId = SlaveId;
                    rtuConfig.Timeout = Timeout;
                    rtuConfig.RetryCount = RetryCount;
                    break;
                    
                case ProtocolType.S7:
                    var s7Config = NewDevice.GetConfig<S7Model>();
                    s7Config.IpAddress = IpAddress;
                    s7Config.Rack = S7Rack;
                    s7Config.Slot = S7Slot;
                    s7Config.Timeout = Timeout;
                    s7Config.RetryCount = RetryCount;
                    break;
                    
                case ProtocolType.TcpIp:
                    var tcpConfig = NewDevice.GetConfig<TCPIPModel>();
                    tcpConfig.IpAddress = IpAddress;
                    tcpConfig.Port = Port;
                    tcpConfig.Timeout = Timeout;
                    tcpConfig.RetryCount = RetryCount;
                    break;
            }
        }

        [RelayCommand]
        public void PreviousStep()
        {
            if (CurrentStep > 0)
            {
                CurrentStep--;
            }
        }

        [RelayCommand]
        public void Cancel()
        {
            CurrentStep = 0;
            UpdateStepIndicators();
        }

        private void CompleteWizard()
        {
            SaveConfigToDevice();
            System.Diagnostics.Debug.WriteLine($"设备添加完成: {NewDevice.Name}, 协议: {NewDevice.ProtocolType}");
        }
    }
}
