using Core.Interfaces;
using Core.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace DeviceModule.ViewModels
{
    public class DeviceDialogViewModel : BindableBase
    {
        private string _title = string.Empty;
        private bool _isEditMode;
        private string _id = string.Empty;
        private string _name = string.Empty;
        private ProtocolType _protocolType = ProtocolType.ModbusTcp;
        private DeviceModel? _originalModel;

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

        public string Id
        {
            get => _id;
            set
            {
                if (SetProperty(ref _id, value))
                {
                    ((DelegateCommand)ConfirmCommand).RaiseCanExecuteChanged();
                }
            }
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

        public ProtocolType ProtocolType
        {
            get => _protocolType;
            set => SetProperty(ref _protocolType, value);
        }

        public Array ProtocolTypes => Enum.GetValues(typeof(ProtocolType));

        public DelegateCommand ConfirmCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public System.Action<bool>? CloseAction { get; set; }

        public DeviceDialogViewModel()
        {
            ConfirmCommand = new DelegateCommand(ExecuteConfirm, CanExecuteConfirm);
            CancelCommand = new DelegateCommand(ExecuteCancel);
        }

        public void Initialize(DeviceModel device, bool isEditMode)
        {
            _originalModel = device;
            IsEditMode = isEditMode;
            Title = isEditMode ? "编辑设备" : "新增设备";

            if (isEditMode && device != null)
            {
                _id = device.Id;
                _name = device.Name ?? string.Empty;
                _protocolType = device.ProtocolType;
            }
            else
            {
                _id = Guid.NewGuid().ToString("N")[..8].ToUpper();
                _name = string.Empty;
                _protocolType = ProtocolType.ModbusTcp;
            }

            RaisePropertyChanged(nameof(Id));
            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(ProtocolType));
            RaisePropertyChanged(nameof(Title));
            ((DelegateCommand)ConfirmCommand).RaiseCanExecuteChanged();
        }

        public DeviceModel GetResult()
        {
            if (_originalModel != null)
            {
                _originalModel.Id = Id;
                _originalModel.Name = Name;
                _originalModel.ProtocolType = ProtocolType;
                return _originalModel;
            }

            return new DeviceModel
            {
                Id = Id,
                Name = Name,
                ProtocolType = ProtocolType
            };
        }

        private bool CanExecuteConfirm() => !string.IsNullOrWhiteSpace(Id) && !string.IsNullOrWhiteSpace(Name);
        private void ExecuteConfirm() => CloseAction?.Invoke(true);
        private void ExecuteCancel() => CloseAction?.Invoke(false);
    }
}
