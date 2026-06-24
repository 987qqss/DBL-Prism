using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Core.Models
{
    /// <summary>产线模型 —— 设备分组的顶层容器</summary>
    public partial class ProductionLineModel : ObservableObject
    {
        private string _name = string.Empty;

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name//通知UI界面产线名称更改
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Description { get; set; } = string.Empty;
        public ObservableCollection<DeviceModel> Devices { get; } = new();
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
    }
}
