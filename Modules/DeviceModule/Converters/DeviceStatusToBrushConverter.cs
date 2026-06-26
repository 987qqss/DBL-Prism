using Core.Interfaces;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DeviceModule.Converters
{
    /// <summary>设备状态 → 圆点颜色转换器</summary>
    public class DeviceStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (DeviceStatus)value;
            return status switch
            {
                DeviceStatus.Online        => new SolidColorBrush(Color.FromRgb(34, 197, 94)),   // 绿 — 已连接
                DeviceStatus.Offline       => new SolidColorBrush(Color.FromRgb(239, 68, 68)),   // 红 — 已配置未连接
                DeviceStatus.NotConfigured => new SolidColorBrush(Color.FromRgb(245, 158, 11)),  // 黄 — 未配置协议
                DeviceStatus.Error         => new SolidColorBrush(Color.FromRgb(239, 68, 68)),   // 红 — 异常
                _                          => new SolidColorBrush(Color.FromRgb(107, 114, 128))  // 灰 — 未知
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
