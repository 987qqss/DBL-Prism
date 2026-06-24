using Core.Interfaces;
using Core.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace DeviceModule.ViewModels.ProtocolConfig
{
    public class ModbusRTUConfigViewModel : BindableBase, IProtocolConfigDialogViewModel
    {
        private string _title = "Modbus RTU 协议配置";
        private string _serialPortName = string.Empty;
        private int _baudRate = 9600;
        private int _dataBits = 8;
        private SerialParity _parity = SerialParity.None;
        private SerialStopBits _stopBits = SerialStopBits.One;
        private byte _slaveId = 1;
        private int _timeout = 3000;
        private int _retryCount = 3;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string SerialPortName
        {
            get => _serialPortName;
            set
            {
                if (SetProperty(ref _serialPortName, value))
                {
                    ((DelegateCommand)ConfirmCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public int BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

        public int DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }

        public SerialParity Parity
        {
            get => _parity;
            set => SetProperty(ref _parity, value);
        }

        public SerialStopBits StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
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

        public Array BaudRates => new[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };
        public Array DataBitsOptions => new[] { 5, 6, 7, 8 };
        public Array ParityOptions => Enum.GetValues(typeof(SerialParity));
        public Array StopBitsOptions => Enum.GetValues(typeof(SerialStopBits));

        public DelegateCommand ConfirmCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public System.Action<bool>? CloseAction { get; set; }

        public ModbusRTUConfigViewModel()
        {
            ConfirmCommand = new DelegateCommand(ExecuteConfirm, CanConfirm);
            CancelCommand = new DelegateCommand(ExecuteCancel);
        }

        public void Initialize(IProtocolConfig? existingConfig)
        {
            if (existingConfig is ModbusRTUModel config)
            {
                _serialPortName = config.SerialPortName;
                _baudRate = config.BaudRate;
                _dataBits = config.DataBits;
                _parity = config.Parity;
                _stopBits = config.StopBits;
                _slaveId = config.SlaveId;
                _timeout = config.Timeout;
                _retryCount = config.RetryCount;
            }
            else
            {
                _serialPortName = string.Empty;
                _baudRate = 9600;
                _dataBits = 8;
                _parity = SerialParity.None;
                _stopBits = SerialStopBits.One;
                _slaveId = 1;
                _timeout = 3000;
                _retryCount = 3;
            }

            RaisePropertyChanged(nameof(SerialPortName));
            RaisePropertyChanged(nameof(BaudRate));
            RaisePropertyChanged(nameof(DataBits));
            RaisePropertyChanged(nameof(Parity));
            RaisePropertyChanged(nameof(StopBits));
            RaisePropertyChanged(nameof(SlaveId));
            RaisePropertyChanged(nameof(Timeout));
            RaisePropertyChanged(nameof(RetryCount));
        }

        public IProtocolConfig GetConfig()
        {
            return new ModbusRTUModel
            {
                SerialPortName = SerialPortName,
                BaudRate = BaudRate,
                DataBits = DataBits,
                Parity = Parity,
                StopBits = StopBits,
                SlaveId = SlaveId,
                Timeout = Timeout,
                RetryCount = RetryCount
            };
        }

        public bool CanConfirm() => !string.IsNullOrWhiteSpace(SerialPortName);

        private void ExecuteConfirm() => CloseAction?.Invoke(true);
        private void ExecuteCancel() => CloseAction?.Invoke(false);
    }
}