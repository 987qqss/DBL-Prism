
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace StateMachineModule.ViewModels
{
    public class CommandRecord
    {
        public string Time { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
    }

    public class CommandRunViewModel : BindableBase
    {
        public ObservableCollection<CommandRecord> CommandHistory { get; } = new();

        public DelegateCommand ReadCommand { get; }
        public DelegateCommand WriteCommand { get; }
        public DelegateCommand StartStopCommand { get; }
        public DelegateCommand RefreshCommand { get; }

        public CommandRunViewModel()
        {
            ReadCommand = new DelegateCommand(() => AddRecord("读取数据", "成功", "已读取 16 个寄存器"));
            WriteCommand = new DelegateCommand(() => AddRecord("写入数据", "成功", "已写入 1 个寄存器"));
            StartStopCommand = new DelegateCommand(() => AddRecord("启停控制", "成功", "设备已启动"));
            RefreshCommand = new DelegateCommand(() => AddRecord("刷新状态", "成功", "状态已更新"));

            // 预填示例数据
            AddRecord("系统初始化", "成功", "已就绪");
        }

        private void AddRecord(string cmd, string status, string result)
        {
            CommandHistory.Insert(0, new CommandRecord
            {
                Time = DateTime.Now.ToString("HH:mm:ss"),
                Command = cmd,
                Status = status,
                Result = result
            });
        }
    }
}
