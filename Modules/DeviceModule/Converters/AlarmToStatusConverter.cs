using System;
using System.Globalization;
using System.Windows.Data;

namespace DeviceModule.Converters
{
    public class AlarmToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool alarm)
            {
                return alarm ? "报警" : string.Empty;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}