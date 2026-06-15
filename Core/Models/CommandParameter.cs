namespace Core.Models
{
    //设备命令参数
    public class CommandParameter
    {
        //设备命令名称，例如读取温度
        public string Name { get; set; } = string.Empty;
        //设备命令值，
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ParameterType Type { get; set; } = ParameterType.String;
        public string[]? Options { get; set; }
        public bool Required { get; set; } = false;
    }

    public enum ParameterType
    {
        String,
        Integer,
        Float,
        Boolean,
        Enum,
        DateTime
    }
}
