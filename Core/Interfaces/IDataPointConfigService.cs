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
}
