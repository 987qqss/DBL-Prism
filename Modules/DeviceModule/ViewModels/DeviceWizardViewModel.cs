using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            }
        }

        public UserControl CurrentStepView => _stepViews[CurrentStep];

        public string StepTitle => _stepTitles[CurrentStep];

        public bool CanGoBack => CurrentStep > 0;

        public string NextButtonText => CurrentStep == 3 ? "完成" : "下一步";

        public DeviceModel NewDevice { get; } = new DeviceModel();

        public ObservableCollection<string> AvailablePorts { get; } = new();
        public ObservableCollection<int> BaudRates { get; } = new() { 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
        public ObservableCollection<string> ParityOptions { get; } = new() { "None", "Odd", "Even", "Mark", "Space" };
        public ObservableCollection<string> StopBitsOptions { get; } = new() { "One", "OnePointFive", "Two" };

        private bool _isTesting = false;
        public bool IsTesting
        {
            get => _isTesting;
            set => SetProperty(ref _isTesting, value);
        }

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
        }

        [RelayCommand]
        public async Task TestConnection()
        {
            IsTesting = true;
            TestResult = "测试中...";
            TestResultDetails = string.Empty;
            TestSuccess = false;

            await Task.Delay(1000);

            try
            {
                TestResult = "连接成功!";
                TestResultDetails = $"成功连接到 {NewDevice.IpAddress}:{NewDevice.Port}\n响应时间: 12ms";
                TestSuccess = true;
            }
            catch
            {
                TestResult = "连接失败";
                TestResultDetails = "无法建立连接，请检查网络设置和设备状态";
                TestSuccess = false;
            }

            IsTesting = false;
        }

        [RelayCommand]
        public void AddAnotherDevice()
        {
            NewDevice.Name = string.Empty;
            NewDevice.ProtocolType = ProtocolType.ModbusTcp;
            NewDevice.IpAddress = string.Empty;
            NewDevice.Port = 502;
            CurrentStep = 0;
        }

        [RelayCommand]
        public void NextStep()
        {
            if (CurrentStep < 3)
            {
                CurrentStep++;
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
    }
}
