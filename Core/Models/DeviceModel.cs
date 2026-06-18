using Core.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Core.Models
{
    public class DeviceModel : IDevice
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string ProductionLineId { get; set; } = string.Empty;
        public ProtocolType ProtocolType { get; set; }//Э������
        
        public IProtocolConfig? Config { get; set; }//Э������
        
        public bool IsConnected { get; set; } = false;
        public DeviceStatus Status { get; set; } = DeviceStatus.NotConfigured;
        
        public ObservableCollection<DeviceCommand> Commands { get; } = new();
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
        
        public Task<bool> Connect()
        {
            Config?.Validate();
            IsConnected = true;
            Status = DeviceStatus.Online;
            return Task.FromResult(true);
        }
        
        public Task Disconnect()
        {
            IsConnected = false;
            Status = DeviceStatus.Offline;
            return Task.CompletedTask;
        }
        
        public Task<bool> TestConnection()
        {
            try
            {
                Config?.Validate();
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
        
        public T GetConfig<T>() where T : IProtocolConfig
        {
            return (T)Config!;
        }
        
        public void SetConfig(IProtocolConfig config)
        {
            Config = config;
            ProtocolType = config.ProtocolType;
        }
    }
}
