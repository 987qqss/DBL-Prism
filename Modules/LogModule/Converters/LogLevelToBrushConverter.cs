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
                LogLevel.Error => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                LogLevel.Warn => new SolidColorBrush(Color.FromRgb(251, 191, 36)),
                LogLevel.Info => new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                LogLevel.Debug => new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                _ => new SolidColorBrush(Color.FromRgb(148, 163, 184))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
