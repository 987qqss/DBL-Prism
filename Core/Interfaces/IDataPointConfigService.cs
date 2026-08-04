using Core.Models;

namespace Core.Interfaces;

/// <summary>
/// 数据点配置服务 —— 独立数据点配置的持久化。
/// 用户添加/编辑数据点，系统围绕这份配置采集和报警。
/// </summary>
public interface IDataPointConfigService
{
    /// <summary>所有数据点配置</summary>
    IReadOnlyList<DataPointConfig> Configs { get; }

    /// <summary>加载配置（不存在则创建默认示例）</summary>
    void Load();

    /// <summary>保存配置到文件</summary>
    void Save();

    /// <summary>添加或更新一个数据点配置</summary>
    void AddOrUpdate(DataPointConfig config);

    /// <summary>删除一个数据点配置</summary>
    void Remove(string configId);

    /// <summary>按设备获取数据点配置</summary>
    IEnumerable<DataPointConfig> GetByDevice(string deviceId);

    /// <summary>
    /// 根据设备命令的监测标记增量同步配置。
    /// 用户勾选/取消"是否监控""是否报警"时调用，无需全量扫描。
    /// 已标记 → 创建或更新对应 DataPointConfig；全部取消 → 删除。
    /// </summary>
    void SyncFromCommand(DeviceModel device, DeviceCommand command);
}
