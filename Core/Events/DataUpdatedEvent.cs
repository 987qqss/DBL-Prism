using Prism.Events;
using Core.Models;

namespace Core.Events
{
    public class DataUpdatedEvent : PubSubEvent<ModbusReadResult> { }
}