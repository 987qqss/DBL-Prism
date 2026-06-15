using Prism.Mvvm;
using Prism.Commands;

namespace SettingsModule.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private string _ipAddress = "192.168.1.100";
        private string _port = "502";
        private string _slaveAddress = "1";
        private bool _autoConnect = true;

        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        public string Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public string SlaveAddress
        {
            get => _slaveAddress;
            set => SetProperty(ref _slaveAddress, value);
        }

        public bool AutoConnect
        {
            get => _autoConnect;
            set => SetProperty(ref _autoConnect, value);
        }

        public DelegateCommand TestConnectionCommand { get; }
        public DelegateCommand SaveSettingsCommand { get; }
        public DelegateCommand RestoreDefaultsCommand { get; }

        public SettingsViewModel()
        {
            TestConnectionCommand = new DelegateCommand(TestConnection);
            SaveSettingsCommand = new DelegateCommand(SaveSettings);
            RestoreDefaultsCommand = new DelegateCommand(RestoreDefaults);
        }

        private void TestConnection()
        {
        }

        private void SaveSettings()
        {
        }

        private void RestoreDefaults()
        {
            IpAddress = "192.168.1.100";
            Port = "502";
            SlaveAddress = "1";
            AutoConnect = true;
        }
    }
}