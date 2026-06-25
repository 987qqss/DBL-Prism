using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Core.Models.LogModel;

namespace LogModule.Converters
{
    public class LogLevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = (LogLevel)value;
            return level switch
            {
                LogLevel.Error => new SolidColorBrush(Color.FromRgb(255, 100, 100)), // 亮红
                LogLevel.Warn  => new SolidColorBrush(Color.FromRgb(255, 200, 60)),  // 琥珀
                LogLevel.Info  => new SolidColorBrush(Color.FromRgb(180, 230, 255)), // 青白 (工业SCADA风格)
                LogLevel.Debug => new SolidColorBrush(Color.FromRgb(130, 140, 155)), // 灰蓝
                _              => new SolidColorBrush(Color.FromRgb(200, 210, 220))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
