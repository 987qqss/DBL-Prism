using CommunityToolkit.Mvvm.ComponentModel;
using Core.Models;

namespace DeviceModule.ViewModels
{
    public partial class DeviceConfigViewModel : ObservableObject
    {
        [ObservableProperty]
        private DeviceModel _newDevice = new DeviceModel();

        [ObservableProperty]
        private string _ipAddress = string.Empty;

        [ObservableProperty]
        private string _port = "502";

        [ObservableProperty]
        private string _slaveId = "1";

        [ObservableProperty]
        private string _serialPortName = string.Empty;

        [ObservableProperty]
        private int _baudRate = 9600;

        [ObservableProperty]
        private int _dataBits = 8;

        [ObservableProperty]
        private string _parity = "None";

        [ObservableProperty]
        private string _stopBits = "1";

        [ObservableProperty]
        private string _s7Rack = "0";

        [ObservableProperty]
        private string _s7Slot = "2";

        [ObservableProperty]
        private string _timeout = "5000";

        [ObservableProperty]
        private string _retryCount = "3";

        public string[] AvailablePorts { get; } = new[] { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6" };
        public int[] BaudRates { get; } = new[] { 9600, 19200, 38400, 57600, 115200 };
        public string[] ParityOptions { get; } = new[] { "None", "Odd", "Even", "Mark", "Space" };
        public string[] StopBitsOptions { get; } = new[] { "1", "1.5", "2" };
    }
}
