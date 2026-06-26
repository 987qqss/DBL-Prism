using Core.Models;
using Prism.Events;

namespace Core.Events
{
    /// <summary>设备树中选中设备的通知事件</summary>
    public class DeviceSelectedEvent : PubSubEvent<DeviceModel> { }
}
