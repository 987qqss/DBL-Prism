using Core.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace DeviceModule.ViewModels;

/// <summary>
/// 报警配置对话框 ViewModel —— 为命令设置报警上下限。
/// 确认后把阈值写回 DeviceCommand 的 AlarmUpperLimit/AlarmLowerLimit 并启用报警。
/// </summary>
public class AlarmConfigDialogViewModel : BindableBase
{
    private string _title = string.Empty;
    private string _pointName = string.Empty;
    private string _upperLimit = string.Empty;
    private string _lowerLimit = string.Empty;
    private DeviceCommand? _command;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>数据点名（显示用）</summary>
    public string PointName
    {
        get => _pointName;
        set => SetProperty(ref _pointName, value);
    }

    /// <summary>报警上限（字符串，可空）</summary>
    public string UpperLimit
    {
        get => _upperLimit;
        set
        {
            if (SetProperty(ref _upperLimit, value))
                ((DelegateCommand)ConfirmCommand).RaiseCanExecuteChanged();
        }
    }

    /// <summary>报警下限（字符串，可空）</summary>
    public string LowerLimit
    {
        get => _lowerLimit;
        set
        {
            if (SetProperty(ref _lowerLimit, value))
                ((DelegateCommand)ConfirmCommand).RaiseCanExecuteChanged();
        }
    }

    public DelegateCommand ConfirmCommand { get; }
    public DelegateCommand CancelCommand { get; }
    public Action<bool>? CloseAction { get; set; }

    public AlarmConfigDialogViewModel()
    {
        ConfirmCommand = new DelegateCommand(ExecuteConfirm, CanExecuteConfirm);
        CancelCommand = new DelegateCommand(() => CloseAction?.Invoke(false));
    }

    /// <summary>初始化：传入命令，预填已有阈值</summary>
    public void Initialize(DeviceCommand command)
    {
        _command = command;
        PointName = command.Name;
        Title = $"报警配置 - {command.Name}";
        UpperLimit = command.AlarmUpperLimit?.ToString("0.##") ?? string.Empty;
        LowerLimit = command.AlarmLowerLimit?.ToString("0.##") ?? string.Empty;
    }

    private bool CanExecuteConfirm()
    {
        // 上下限至少填一个
        return !string.IsNullOrWhiteSpace(UpperLimit) || !string.IsNullOrWhiteSpace(LowerLimit);
    }

    private void ExecuteConfirm()
    {
        if (_command == null) return;

        // 解析上下限（非法输入则提示）
        if (!string.IsNullOrWhiteSpace(UpperLimit) && !double.TryParse(UpperLimit, out _))
        {
            System.Windows.MessageBox.Show("报警上限必须是数字", "输入错误",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }
        if (!string.IsNullOrWhiteSpace(LowerLimit) && !double.TryParse(LowerLimit, out _))
        {
            System.Windows.MessageBox.Show("报警下限必须是数字", "输入错误",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        _command.IsAlarmEnabled = true;
        _command.AlarmUpperLimit = string.IsNullOrWhiteSpace(UpperLimit) ? null : double.Parse(UpperLimit);
        _command.AlarmLowerLimit = string.IsNullOrWhiteSpace(LowerLimit) ? null : double.Parse(LowerLimit);

        CloseAction?.Invoke(true);
    }
}
