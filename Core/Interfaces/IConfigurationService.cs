using Core.Models;
using System.Collections.ObjectModel;

namespace Core.Interfaces
{
    /// <summary>
    /// 设备树配置服务 —— 提供产线/设备/命令的导入导出，以及全局唯一的配置数据源
    /// DeviceCommandTreeViewModel 和 MenuBarViewModel 通过此接口共享同一份数据
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>全局唯一的产线集合（ViewModel 绑定此属性）</summary>
        ObservableCollection<ProductionLineModel> ProductionLines { get; }

        /// <summary>将当前配置序列化为 JSON 并写入文件</summary>
        void ExportConfig(string filePath);

        /// <summary>从 JSON 文件反序列化配置，替换当前 ProductionLines</summary>
        void ImportConfig(string filePath);

        /// <summary>加载默认示例数据</summary>
        void LoadDefaultData();
    }
}
