using Core.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace DeviceModule.ViewModels
{
    public class ProductionLineDialogViewModel : BindableBase
    {
        private string _title = string.Empty;
        private bool _isEditMode;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private ProductionLineModel? _originalModel;

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

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public DelegateCommand ConfirmCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public System.Action<bool>? CloseAction { get; set; }

        public ProductionLineDialogViewModel()
        {
            ConfirmCommand = new DelegateCommand(ExecuteConfirm, CanExecuteConfirm);
            CancelCommand = new DelegateCommand(ExecuteCancel);
        }

        public void Initialize(ProductionLineModel model, bool isEditMode)
        {
            _originalModel = model;
            IsEditMode = isEditMode;
            Title = isEditMode ? "编辑产线" : "新增产线";

            if (isEditMode && model != null)
            {
                _name = model.Name;
                _description = model.Description;
            }
            else
            {
                _name = string.Empty;
                _description = string.Empty;
            }

            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(Description));
            RaisePropertyChanged(nameof(Title));
        }

        public ProductionLineModel GetResult()
        {
            if (_originalModel != null)
            {
                _originalModel.Name = Name;
                _originalModel.Description = Description;
                _originalModel.UpdatedTime = DateTime.Now;
                return _originalModel;
            }

            return new ProductionLineModel
            {
                Id = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                Name = Name,
                Description = Description
            };
        }

        private bool CanExecuteConfirm() => !string.IsNullOrWhiteSpace(Name);
        private void ExecuteConfirm() => CloseAction?.Invoke(true);
        private void ExecuteCancel() => CloseAction?.Invoke(false);
    }
}