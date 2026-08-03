namespace Core.Models
{
    /// <summary>
    /// 标记一个类为设备命令集。启动时由 CommandScanner 反射扫描，
    /// 类中的 [DeviceCommand] 方法将被自动加入对应设备的命令集合。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DeviceCommandClassAttribute : Attribute
    {
        /// <summary>
        /// 目标设备类型标识（与 DeviceModel.DeviceType 精确匹配）。
        /// 优先使用 DeviceType 匹配；为 null 时回退到 DeviceName 匹配（向后兼容）。
        /// 相比设备名，类型标识不随现场改名而变化，更稳定。
        /// </summary>
        public string? DeviceType { get; }

        /// <summary>目标设备名称（与 DeviceModel.Name 精确匹配，兼容旧写法）</summary>
        public string DeviceName { get; }

        /// <summary>指定设备类型标识（推荐）</summary>
        public DeviceCommandClassAttribute(string deviceType, DeviceCommandClassMatchMode mode = DeviceCommandClassMatchMode.Type)
        {
            if (mode == DeviceCommandClassMatchMode.Type)
            {
                DeviceType = deviceType;
                DeviceName = string.Empty;
            }
            else
            {
                DeviceType = null;
                DeviceName = deviceType;
            }
        }
    }

    /// <summary>设备命令类匹配模式</summary>
    public enum DeviceCommandClassMatchMode
    {
        /// <summary>按 DeviceModel.DeviceType 匹配（推荐，稳定）</summary>
        Type,
        /// <summary>按 DeviceModel.Name 匹配（兼容旧写法）</summary>
        Name
    }

    /// <summary>
    /// 标记一个方法为设备命令。该方法必须符合签名:
    /// Task MethodName(IDeviceDriver? driver)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DeviceCommandAttribute : Attribute
    {
        /// <summary>命令显示名称</summary>
        public string Name { get; }

        /// <summary>协议地址字符串（可为空，表示纯自定义逻辑）</summary>
        public string ProtocolAddress { get; set; } = string.Empty;

        /// <summary>命令类型</summary>
        public CommandType CommandType { get; set; } = CommandType.Read;

        /// <summary>数据格式</summary>
        public DataFormat DataFormat { get; set; } = DataFormat.Int16;

        /// <summary>转换系数</summary>
        public float Scale { get; set; } = 1.0f;

        /// <summary>偏移量</summary>
        public float Offset { get; set; } = 0.0f;

        /// <summary>单位</summary>
        public string Unit { get; set; } = string.Empty;

        public DeviceCommandAttribute(string name)//带一个参数的构造函数
        {
            Name = name;
        }

        //带两个参数的构造函数
        public DeviceCommandAttribute(string name, string protocolAddress)
        {
            Name = name;
            ProtocolAddress = protocolAddress;
        }
    }
}
