using System.Collections.ObjectModel;

namespace Core.Models
{
    //产线模型
    public class ProductionLineModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ObservableCollection<DeviceModel> Devices { get; } = new();
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
    }
}
