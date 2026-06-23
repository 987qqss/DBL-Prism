

using Core.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace DeviceModule.ViewModels
{
    /// <summary>
    /// 产线对话框视图模型 - 用于新增/编辑产线信息
    /// </summary>
    public class ProductionLineDialogViewModel : BindableBase
    {
        private string _title = string.Empty;
        private bool _isEditMode;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private ProductionLineModel? _originalModel;

        /// <summary>
        /// 对话框标题
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// 是否为编辑模式
        /// </summary>
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        /// <summary>
        /// 产线名称
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    // 当名称改变时，验证并更新确认命令状态
                    ((DelegateCommand)ConfirmCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 产线描述
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// 确认命令
        /// </summary>
        public DelegateCommand ConfirmCommand { get; }

        /// <summary>
        /// 取消命令
        /// </summary>
        public DelegateCommand CancelCommand { get; }

        /// <summary>
        /// 窗口关闭操作（由View在点击按钮后调用）
        /// </summary>
        public System.Action<bool>? CloseAction { get; set; }

        public ProductionLineDialogViewModel()
        {
            ConfirmCommand = new DelegateCommand(ExecuteConfirm, CanExecuteConfirm);
            CancelCommand = new DelegateCommand(ExecuteCancel);
        }

        /// <summary>
        /// 初始化对话框
        /// </summary>
        /// <param name="productionLine">产线对象</param>
        /// <param name="isEditMode">是否为编辑模式</param>
        public void Initialize(ProductionLineModel productionLine, bool isEditMode)
        {
            _originalModel = productionLine;
            IsEditMode = isEditMode;
            Title = isEditMode ? "编辑产线" : "新增产线";

            // 如果是编辑模式，填充现有数据
            if (isEditMode && productionLine != null)
            {
                _name = productionLine.Name ?? string.Empty;
                _description = productionLine.Description ?? string.Empty;
            }
            else
            {
                // 新增模式，使用默认值
                _name = string.Empty;
                _description = string.Empty;
            }

            // 通知属性变更
            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(Description));
            RaisePropertyChanged(nameof(Title));
        }

        /// <summary>
        /// 获取编辑后的产线对象
        /// </summary>
        public ProductionLineModel GetResult()
        {
            if (_originalModel != null)
            {
                // 更新原有对象
                _originalModel.Name = Name;
                _originalModel.Description = Description;
                return _originalModel;
            }

            // 创建新对象
            return new ProductionLineModel
            {
                Id = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                Name = Name,
                Description = Description
            };
        }

        private bool CanExecuteConfirm()
        {
            // 验证：名称不能为空
            return !string.IsNullOrWhiteSpace(Name);
        }

        private void ExecuteConfirm()
        {
            // 触发窗口关闭，返回true表示确认
            CloseAction?.Invoke(true);
        }

        private void ExecuteCancel()
        {
            // 触发窗口关闭，返回false表示取消
            CloseAction?.Invoke(false);
        }
    }
}
