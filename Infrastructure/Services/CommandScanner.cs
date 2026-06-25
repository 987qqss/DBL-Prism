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
        /// 匹配规则: [DeviceCommandClass].DeviceName == DeviceModel.Name
        /// </summary>
        //  将与产线集合里的设备名相同的预定义的设备命令添加到传入的产线集合中去
        public void ScanAndRegister(IEnumerable<ProductionLineModel> lines)
        {
            var deviceLookup = new Dictionary<string, List<DeviceModel>>();
            foreach (var line in lines)
                foreach (var device in line.Devices)
                {
                    if (!deviceLookup.ContainsKey(device.Name))
                        deviceLookup[device.Name] = new();
                    deviceLookup[device.Name].Add(device);
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

                if (!deviceLookup.TryGetValue(classAttr.DeviceName, out var devices))
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
