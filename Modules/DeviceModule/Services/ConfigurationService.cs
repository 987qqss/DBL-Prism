using Core.Interfaces;
using Core.Models;
using Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeviceModule.Services
{
    /// <summary>
    /// 设备树配置服务 —— 管理产线/设备/命令的持久化，作为全局唯一数据源
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILogService _logService;
        private readonly CommandScanner? _scanner;

        //定义json序列化过滤条件
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
            Converters = { new JsonStringEnumConverter() },
        };

        /// <summary>JSON 根对象结构</summary>
        internal class ConfigRoot
        {
            public List<ProductionLineModel> ProductionLines { get; set; } = new();
        }

        //产线集合数据源，ViewModel绑定到这里
        public ObservableCollection<ProductionLineModel> ProductionLines { get; private set; } = new();

        public ConfigurationService(ILogService logService, CommandScanner scanner)
        {
            _logService = logService;
            _scanner = scanner;
            LoadDefaultData();//加载数据到集合

            // 启动时扫描并注入预定义命令
            _scanner.ScanAndRegister(ProductionLines);
            _logService.Info("预定义命令扫描完成", "Config");
        }

        /// <summary>导出：将 ProductionLines 包装为 {"ProductionLines":[...]} 写入文件</summary>
        public void ExportConfig(string filePath)
        {
            var root = new ConfigRoot { ProductionLines = ProductionLines.ToList() };
            var json = JsonSerializer.Serialize(root, JsonOptions);
            System.IO.File.WriteAllText(filePath, json);

            var totalDevices = root.ProductionLines.Sum(l => l.Devices.Count);
            var totalCmds = root.ProductionLines.Sum(l => l.Devices.Sum(d => d.Commands.Count));
            _logService.Info($"配置导出成功 → {System.IO.Path.GetFileName(filePath)} (产线:{root.ProductionLines.Count}, 设备:{totalDevices}, 命令:{totalCmds})", "Config");
        }

        /// <summary>导入：从 JSON 文件反序列化，替换当前 ProductionLines</summary>
        public void ImportConfig(string filePath)
        {
            var json = System.IO.File.ReadAllText(filePath);
            var root = JsonSerializer.Deserialize<ConfigRoot>(json, JsonOptions);

            if (root?.ProductionLines == null || root.ProductionLines.Count == 0)
            {
                _logService.Warn("导入配置失败：文件中无有效数据", "Config");
                return;
            }

            // 重新建立父子关系并同步协议类型
            foreach (var line in root.ProductionLines)
            {
                foreach (var device in line.Devices)
                {
                    device.ProductionLineId = line.Id;
                    device.Status = DeviceStatus.NotConfigured;
                    device.IsConnected = false;

                    // 从反序列化的 Config 中同步 ProtocolType
                    if (device.Config != null)
                        device.ProtocolType = device.Config.ProtocolType;

                    foreach (var cmd in device.Commands)
                    {
                        cmd.DeviceId = device.Id;
                    }
                }
            }

            ProductionLines.Clear();
            foreach (var line in root.ProductionLines)
                ProductionLines.Add(line);

            var totalDevices = root.ProductionLines.Sum(l => l.Devices.Count);
            var totalCmds = root.ProductionLines.Sum(l => l.Devices.Sum(d => d.Commands.Count));
            _logService.Info($"配置导入成功 ← {System.IO.Path.GetFileName(filePath)} (产线:{root.ProductionLines.Count}, 设备:{totalDevices}, 命令:{totalCmds})", "Config");
        }

        /// <summary>加载默认示例数据（首次启动或无配置文件时）</summary>
        public void LoadDefaultData()
        {
            ProductionLines.Clear();

            var pl1 = new ProductionLineModel { Id = "PL001", Name = "电池仓产线 A" };
            var dev1 = new DeviceModel { Id = "DEV001", Name = "BMS-主控制器", Status = DeviceStatus.Online };
            dev1.Commands.Add(new DeviceCommand { Id = "CMD001", Name = "读取总电压", CommandType = CommandType.Read, ProtocolAddress = "03:1000:2" });
            dev1.Commands.Add(new DeviceCommand { Id = "CMD002", Name = "读取电芯温度", CommandType = CommandType.Read, ProtocolAddress = "04:1010:4", Unit = "℃" });
            dev1.Commands.Add(new DeviceCommand { Id = "CMD003", Name = "设置充电阈值", CommandType = CommandType.Write, ProtocolAddress = "06:2000:1" });
            pl1.Devices.Add(dev1);
            var dev2 = new DeviceModel { Id = "DEV002", Name = "PCS-储能变流器", Status = DeviceStatus.Online };
            dev2.Commands.Add(new DeviceCommand { Id = "CMD004", Name = "读取有功功率", CommandType = CommandType.Read, ProtocolAddress = "03:1100:2", Unit = "kW" });
            dev2.Commands.Add(new DeviceCommand { Id = "CMD005", Name = "设置功率上限", CommandType = CommandType.Write, ProtocolAddress = "06:2100:1" });
            dev2.Commands.Add(new DeviceCommand { Id = "CMD006", Name = "读取运行状态", CommandType = CommandType.Read, ProtocolAddress = "03:1102:1" });
            pl1.Devices.Add(dev2);

            var pl2 = new ProductionLineModel { Id = "PL002", Name = "电池仓产线 B" };
            var dev3 = new DeviceModel { Id = "DEV003", Name = "EMS-能源管理系统", Status = DeviceStatus.Offline };
            dev3.Commands.Add(new DeviceCommand { Id = "CMD007", Name = "读取SOC", CommandType = CommandType.Read, ProtocolAddress = "03:1200:1", Unit = "%" });
            dev3.Commands.Add(new DeviceCommand { Id = "CMD008", Name = "读取SOH", CommandType = CommandType.Read, ProtocolAddress = "03:1201:1", Unit = "%" });
            pl2.Devices.Add(dev3);
            var dev4 = new DeviceModel { Id = "DEV004", Name = "温控系统", Status = DeviceStatus.Online };
            dev4.Commands.Add(new DeviceCommand { Id = "CMD009", Name = "读取环境温度", CommandType = CommandType.Read, ProtocolAddress = "04:1300:1", Unit = "℃" });
            dev4.Commands.Add(new DeviceCommand { Id = "CMD010", Name = "读取环境湿度", CommandType = CommandType.Read, ProtocolAddress = "04:1301:1", Unit = "%RH" });
            dev4.Commands.Add(new DeviceCommand { Id = "CMD011", Name = "设置目标温度", CommandType = CommandType.Write, ProtocolAddress = "06:2200:1" });
            pl2.Devices.Add(dev4);

            var pl3 = new ProductionLineModel { Id = "PL003", Name = "未分组设备" };
            var dev5 = new DeviceModel { Id = "DEV005", Name = "消防主机", Status = DeviceStatus.NotConfigured };
            dev5.Commands.Add(new DeviceCommand { Id = "CMD012", Name = "读取烟感状态", CommandType = CommandType.Read, ProtocolAddress = "02:1400:1" });
            pl3.Devices.Add(dev5);

            ProductionLines.Add(pl1);
            ProductionLines.Add(pl2);
            ProductionLines.Add(pl3);

            _logService?.Info("默认示例数据加载完成 (3条产线, 5台设备, 12条命令)", "Config");
        }
    }
}
