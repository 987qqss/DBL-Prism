using Core.Models;

namespace Core.Interfaces;

/// <summary>
/// 数据点存储（数据中心 / Broker）——
/// 生产者（采集服务）写入数据，订阅者（报警/监控/报表）消费数据。
/// 发布-订阅模型：Publish 广播给所有订阅者，订阅者自行过滤。
/// </summary>
public interface IDataPointStore
{
    /// <summary>数据点定义列表（来自独立配置）</summary>
    IReadOnlyList<DataPointConfig> Definitions { get; }

    /// <summary>数据点定义集合变化时触发</summary>
    event Action? DefinitionsChanged;

    /// <summary>注册/更新一个数据点定义</summary>
    void RegisterDefinition(DataPointConfig config);

    /// <summary>移除一个数据点定义</summary>
    void RemoveDefinition(string configId);

    /// <summary>生产者写入一个数据点值并广播给所有订阅者</summary>
    void Publish(DataPointValue value);

    /// <summary>订阅数据点更新（广播式，订阅者自行过滤）</summary>
    void Subscribe(IDataPointSubscriber subscriber);

    /// <summary>取消订阅</summary>
    void Unsubscribe(IDataPointSubscriber subscriber);

    /// <summary>获取某数据点的最新值（按 Key "设备Id.点名"）</summary>
    DataPointValue? GetLatest(string pointKey);

    /// <summary>获取某设备的全部最新值快照</summary>
    IReadOnlyDictionary<string, DataPointValue> GetDeviceSnapshot(string deviceId);

    /// <summary>获取所有最新值快照</summary>
    IReadOnlyDictionary<string, DataPointValue> GetAllLatest();
}
