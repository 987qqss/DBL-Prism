using Core.Interfaces;
using Core.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace DeviceModule.ViewModels.ProtocolConfig
{
    public class ModbusTCPConfigViewModel : BindableBase, IProtocolConfigDialogViewModel
    {
        private string _title = "Modbus TCP 协议配置";
        private string _ipAddress = string.Empty;
        private int _port = 502;
        private byte _slaveId = 1;
        private int _timeout = 3000;
        private int _retryCount = 3;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                if (SetProperty(ref _ipAddress, value))
                {
                    ((DelegateCommand)ConfirmCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public byte SlaveId
        {
            get => _slaveId;
            set => SetProperty(ref _slaveId, value);
        }

        public int Timeout
        {
            get => _timeout;
            set => SetProperty(ref _timeout, value);
        }

        public int RetryCount
        {
            get => _retryCount;
            set => SetProperty(ref _retryCount, value);
        }

        public DelegateCommand ConfirmCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public System.Action<bool>? CloseAction { get; set; }

        public ModbusTCPConfigViewModel()
        {
            ConfirmCommand = new DelegateCommand(ExecuteConfirm, CanConfirm);
            CancelCommand = new DelegateCommand(ExecuteCancel);
        }

        public void Initialize(IProtocolConfig? existingConfig)
        {
            if (existingConfig is ModbusTCPModel config)
            {
                _ipAddress = config.IpAddress;
                _port = config.Port;
                _slaveId = config.SlaveId;
                _timeout = config.Timeout;
                _retryCount = config.RetryCount;
            }
            else
            {
                _ipAddress = string.Empty;
                _port = 502;
                _slaveId = 1;
                _timeout = 3000;
                _retryCount = 3;
            }

            RaisePropertyChanged(nameof(IpAddress));
            RaisePropertyChanged(nameof(Port));
            RaisePropertyChanged(nameof(SlaveId));
            RaisePropertyChanged(nameof(Timeout));
            RaisePropertyChanged(nameof(RetryCount));
        }

        public IProtocolConfig GetConfig()
        {
            return new ModbusTCPModel
            {
                IpAddress = IpAddress,
                Port = Port,
                SlaveId = SlaveId,
                Timeout = Timeout,
                RetryCount = RetryCount
            };
        }

        public bool CanConfirm() => !string.IsNullOrWhiteSpace(IpAddress);

        private void ExecuteConfirm() => CloseAction?.Invoke(true);
        private void ExecuteCancel() => CloseAction?.Invoke(false);
    }
}