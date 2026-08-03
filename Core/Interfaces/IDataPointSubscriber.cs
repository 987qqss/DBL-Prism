using Core.Models;

namespace Core.Interfaces;

/// <summary>
/// 数据点订阅者 —— 实现此接口并订阅到 IDataPointStore，
/// 即可在数据点更新时收到通知。报警引擎、监控面板、报表服务都是订阅者。
/// </summary>
public interface IDataPointSubscriber
{
    /// <summary>
    /// 数据点更新回调。订阅者自行判断是否关心该数据点（按 PointKey/DeviceId 过滤）。
    /// 注意：可能在工作线程回调，涉及 UI 需 Dispatcher 调度。
    /// </summary>
    void OnDataPointUpdated(DataPointValue value);
}
