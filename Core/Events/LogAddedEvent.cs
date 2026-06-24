using Prism.Events;
using Core.Models.LogModel;

namespace Core.Events
{
    //定义一个发送日志的事件，
    public class LogAddedEvent : PubSubEvent<LogEntry> { }
}
