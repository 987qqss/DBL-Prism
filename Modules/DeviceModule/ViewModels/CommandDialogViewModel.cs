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
                {
                    ((DelegateCommand)ConfirmCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public CommandType CommandType
        {
            get => _commandType;
            set => SetProperty(ref _commandType, value);
        }

        public string ProtocolAddress
        {
            get => _protocolAddress;
            set => SetProperty(ref _protocolAddress, value);
        }

        public Array CommandTypes => Enum.GetValues(typeof(CommandType));

        public DelegateCommand ConfirmCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public System.Action<bool>? CloseAction { get; set; }

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
                _name = command.Name ?? string.Empty;
                _commandType = command.CommandType;
                _protocolAddress = command.ProtocolAddress ?? string.Empty;
            }
            else
            {
                _name = string.Empty;
                _commandType = CommandType.Read;
                _protocolAddress = string.Empty;
            }

            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(CommandType));
            RaisePropertyChanged(nameof(ProtocolAddress));
            RaisePropertyChanged(nameof(Title));
        }

        public DeviceCommand GetResult()
        {
            if (_originalModel != null)
            {
                _originalModel.Name = Name;
                _originalModel.CommandType = CommandType;
                _originalModel.ProtocolAddress = ProtocolAddress;
                return _originalModel;
            }

            return new DeviceCommand
            {
                Id = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                Name = Name,
                CommandType = CommandType,
                ProtocolAddress = ProtocolAddress
            };
        }

        private bool CanExecuteConfirm() => !string.IsNullOrWhiteSpace(Name);
        private void ExecuteConfirm() => CloseAction?.Invoke(true);
        private void ExecuteCancel() => CloseAction?.Invoke(false);
    }
}