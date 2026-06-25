namespace Core.Models
{
    /// <summary>
    /// 标记一个类为设备命令集。启动时由 CommandScanner 反射扫描，
    /// 类中的 [DeviceCommand] 方法将被自动加入对应设备的命令集合。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DeviceCommandClassAttribute : Attribute
    {
        /// <summary>目标设备名称（与 DeviceModel.Name 精确匹配）</summary>
        public string DeviceName { get; }

        public DeviceCommandClassAttribute(string deviceName)
        {
            DeviceName = deviceName;
        }
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
