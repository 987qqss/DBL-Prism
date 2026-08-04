namespace Core.Models;

/// <summary>
/// 数据点展示类型 —— 决定监控界面用哪种模板展示。
/// 由 DataFormat 自动推导，用户无需手动指定。
/// </summary>
public enum DataPointDisplayType
{
    /// <summary>数值型 → 实时曲线（温度/电压/电流等）</summary>
    TrendChart,

    /// <summary>布尔型 → 状态指示灯（IO/开关/报警位等）</summary>
    Indicator,

    /// <summary>字符串/其它 → 文本显示</summary>
    Text
}

/// <summary>从 DataFormat 推导展示类型</summary>
public static class DataPointDisplayResolver
{
    public static DataPointDisplayType Resolve(DataFormat format)
    {
        return format switch
        {
            DataFormat.Bool => DataPointDisplayType.Indicator,
            DataFormat.String => DataPointDisplayType.Text,
            DataFormat.ByteArray => DataPointDisplayType.Text,
            _ => DataPointDisplayType.TrendChart // 数值型（Int16/Int32/Float/Double/UInt16 等）
        };
    }
}
