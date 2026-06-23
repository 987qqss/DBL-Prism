using Core.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace DeviceModule.ViewModels
{
    /// <summary>
    /// 设备对话框视图模型 - 用于新增/编辑设备信息
    /// </summary>
    public class DeviceDialogViewModel : BindableBase
    {
        private string _title = string.Empty;
        private bool _isEditMode;
        private string _name = string.Empty;
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
                _name = device.Name ?? string.Empty;
            }
            else
            {
                _name = string.Empty;
            }

            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(Title));
        }

        public DeviceModel GetResult()
        {
            if (_originalModel != null)
            {
                _originalModel.Name = Name;
                return _originalModel;
            }

            return new DeviceModel
            {
                Id = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                Name = Name
            };
        }

        private bool CanExecuteConfirm() => !string.IsNullOrWhiteSpace(Name);
        private void ExecuteConfirm() => CloseAction?.Invoke(true);
        private void ExecuteCancel() => CloseAction?.Invoke(false);
    }
}
