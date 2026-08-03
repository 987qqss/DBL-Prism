using System.Reflection;
using Core.Models;

namespace Infrastructure.Services
{
    /// <summary>
    /// 命令扫描器 —— 启动时反射扫描所有带 [DeviceCommandClass] 的类，
    /// 将其 [DeviceCommand] 方法生成为 DeviceCommand 实例注入到对应设备下。
    /// 命令类的构造依赖通过 IServiceProvider 解析。
    /// </summary>
    public class CommandScanner
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandScanner(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 扫描所有程序集，将预定义命令注入到匹配的设备中。
        /// 匹配规则:
        ///   优先 [DeviceCommandClass].DeviceType == DeviceModel.DeviceType
        ///   回退 [DeviceCommandClass].DeviceName == DeviceModel.Name（兼容旧写法）
        /// </summary>
        public void ScanAndRegister(IEnumerable<ProductionLineModel> lines)
        {
            // 按类型标识索引（优先）
            var typeLookup = new Dictionary<string, List<DeviceModel>>(StringComparer.OrdinalIgnoreCase);
            // 按设备名索引（回退）
            var nameLookup = new Dictionary<string, List<DeviceModel>>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in lines)
            {
                foreach (var device in line.Devices)
                {
                    if (!string.IsNullOrWhiteSpace(device.DeviceType))
                    {
                        if (!typeLookup.ContainsKey(device.DeviceType))
                            typeLookup[device.DeviceType] = new();
                        typeLookup[device.DeviceType].Add(device);
                    }

                    if (!nameLookup.ContainsKey(device.Name))
                        nameLookup[device.Name] = new();
                    nameLookup[device.Name].Add(device);
                }
            }

            var scannedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !a.FullName!.StartsWith("System") && !a.FullName.StartsWith("Microsoft"))
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException) { return Type.EmptyTypes; }
                })
                .Where(t => t.GetCustomAttribute<DeviceCommandClassAttribute>() != null);

            foreach (var type in scannedTypes)
            {
                var classAttr = type.GetCustomAttribute<DeviceCommandClassAttribute>()!;

                // 优先按 DeviceType 匹配，找不到再按 DeviceName 匹配
                List<DeviceModel>? devices = null;
                if (!string.IsNullOrWhiteSpace(classAttr.DeviceType))
                    typeLookup.TryGetValue(classAttr.DeviceType, out devices);

                if (devices == null && !string.IsNullOrWhiteSpace(classAttr.DeviceName))
                    nameLookup.TryGetValue(classAttr.DeviceName, out devices);

                if (devices == null || devices.Count == 0)
                    continue;

                // 通过 DI 或 Activator 创建命令类实例
                var instance = CreateInstance(type);

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    var cmdAttr = method.GetCustomAttribute<DeviceCommandAttribute>();
                    if (cmdAttr == null) continue;

                    var cmd = new DeviceCommand
                    {
                        Id = $"SCAN_{Guid.NewGuid():N}"[..12],
                        Name = cmdAttr.Name,
                        ProtocolAddress = cmdAttr.ProtocolAddress,
                        CommandType = cmdAttr.CommandType,
                        DataFormat = cmdAttr.DataFormat,
                        Scale = cmdAttr.Scale,
                        Offset = cmdAttr.Offset,
                        Unit = cmdAttr.Unit,
                        ExecuteAction = async (driver) =>
                        {
                            var result = method.Invoke(instance, new object?[] { driver });
                            if (result is Task task) await task;
                        }
                    };

                    foreach (var device in devices)
                    {
                        cmd.DeviceId = device.Id;
                        device.Commands.Add(cmd);
                    }
                }
            }
        }

        private object CreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type)
                    ?? throw new InvalidOperationException($"无法创建 {type.Name} 的实例");
            }
            catch (MissingMethodException)
            {
                // 有构造函数依赖，尝试用 IServiceProvider 解析
                try
                {
                    var ctors = type.GetConstructors();
                    var ctor = ctors[0];
                    var args = ctor.GetParameters()
                        .Select(p => _serviceProvider.GetService(p.ParameterType))
                        .ToArray();
                    return ctor.Invoke(args);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"无法解析命令类 {type.Name} 的构造依赖: {ex.Message}", ex);
                }
            }
        }
    }
}
