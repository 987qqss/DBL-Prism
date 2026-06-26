using Core.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace DeviceModule.ViewModels
{
    public class CommandDialogViewModel : BindableBase
    {
        private string _title = string.Empty;
        private bool _isEditMode;
        private string _name = string.Empty;
        private CommandType _commandType = CommandType.Read;
        private string _protocolAddress = string.Empty;
        private string _writeValue = string.Empty;
        private DataFormat _dataFormat = DataFormat.Int16;
        private float _scale = 1.0f;
        private float _offset;
        private string _unit = string.Empty;
        private DeviceCommand? _originalModel;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    ((DelegateCommand)ConfirmCommand).RaiseCanExecuteChanged();
            }
        }

        public CommandType CommandType
        {
            get => _commandType;
            set
            {
                if (SetProperty(ref _commandType, value))
                {
                    RaisePropertyChanged(nameof(IsReadVisible));
                    RaisePropertyChanged(nameof(IsWriteVisible));
                }
            }
        }

        public string ProtocolAddress
        {
            get => _protocolAddress;
            set => SetProperty(ref _protocolAddress, value);
        }

        /// <summary>写入值（Write / ReadWrite 时显示）</summary>
        public string WriteValue
        {
            get => _writeValue;
            set => SetProperty(ref _writeValue, value);
        }

        public DataFormat DataFormat
        {
            get => _dataFormat;
            set => SetProperty(ref _dataFormat, value);
        }

        public float Scale
        {
            get => _scale;
            set => SetProperty(ref _scale, value);
        }

        public float Offset
        {
            get => _offset;
            set => SetProperty(ref _offset, value);
        }

        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        /// <summary>读取相关字段可见（Read / ReadWrite）</summary>
        public bool IsReadVisible => CommandType == CommandType.Read
            || CommandType == CommandType.ReadWrite;

        /// <summary>写入字段可见（Write / ReadWrite）</summary>
        public bool IsWriteVisible => CommandType == CommandType.Write
            || CommandType == CommandType.ReadWrite;

        public Array CommandTypes => Enum.GetValues(typeof(CommandType));
        public Array DataFormats => Enum.GetValues(typeof(DataFormat));

        public DelegateCommand ConfirmCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public Action<bool>? CloseAction { get; set; }

        public CommandDialogViewModel()
        {
            ConfirmCommand = new DelegateCommand(ExecuteConfirm, CanExecuteConfirm);
            CancelCommand = new DelegateCommand(ExecuteCancel);
        }

        public void Initialize(DeviceCommand command, bool isEditMode)
        {
            _originalModel = command;
            IsEditMode = isEditMode;
            Title = isEditMode ? "编辑命令" : "新增命令";

            if (isEditMode && command != null)
            {
                _name = command.Name;
                _commandType = command.CommandType;
                _protocolAddress = command.ProtocolAddress;
                _writeValue = command.WriteValue?.ToString() ?? string.Empty;
                _dataFormat = command.DataFormat;
                _scale = command.Scale;
                _offset = command.Offset;
                _unit = command.Unit;
            }
            else
            {
                _name = string.Empty;
                _commandType = CommandType.Read;
                _protocolAddress = string.Empty;
                _writeValue = string.Empty;
                _dataFormat = DataFormat.Int16;
                _scale = 1.0f;
                _offset = 0f;
                _unit = string.Empty;
            }

            RaiseAll();
        }

        private void RaiseAll()
        {
            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(CommandType));
            RaisePropertyChanged(nameof(ProtocolAddress));
            RaisePropertyChanged(nameof(WriteValue));
            RaisePropertyChanged(nameof(DataFormat));
            RaisePropertyChanged(nameof(Scale));
            RaisePropertyChanged(nameof(Offset));
            RaisePropertyChanged(nameof(Unit));
            RaisePropertyChanged(nameof(Title));
            RaisePropertyChanged(nameof(IsReadVisible));
            RaisePropertyChanged(nameof(IsWriteVisible));
        }

        public DeviceCommand GetResult()
        {
            var cmd = _originalModel ?? new DeviceCommand
            {
                Id = Guid.NewGuid().ToString("N")[..8].ToUpper(),
            };

            cmd.Name = Name;
            cmd.CommandType = CommandType;
            cmd.ProtocolAddress = ProtocolAddress;
            cmd.DataFormat = DataFormat;
            cmd.Scale = Scale;
            cmd.Offset = Offset;
            cmd.Unit = Unit;

            // 根据 DataFormat 解析写入值
            if (IsWriteVisible && !string.IsNullOrWhiteSpace(WriteValue))
            {
                cmd.WriteValue = ParseWriteValue(WriteValue, DataFormat);
            }
            else
            {
                cmd.WriteValue = null;
            }

            return cmd;
        }

        private static object? ParseWriteValue(string text, DataFormat format)
        {
            try
            {
                return format switch
                {
                    DataFormat.UInt16 => ushort.Parse(text),
                    DataFormat.Int16 => short.Parse(text),
                    DataFormat.UInt32 => uint.Parse(text),
                    DataFormat.Int32 => int.Parse(text),
                    DataFormat.Float => float.Parse(text),
                    DataFormat.Double => double.Parse(text),
                    DataFormat.Bool => bool.Parse(text),
                    DataFormat.String => text,
                    _ => text
                };
            }
            catch
            {
                return text; // 解析失败时保留原字符串
            }
        }

        private bool CanExecuteConfirm() => !string.IsNullOrWhiteSpace(Name);
        private void ExecuteConfirm() => CloseAction?.Invoke(true);
        private void ExecuteCancel() => CloseAction?.Invoke(false);
    }
}
