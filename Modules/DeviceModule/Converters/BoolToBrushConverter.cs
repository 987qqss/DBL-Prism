using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DeviceModule.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool state)
            {
                return state ? Brushes.Red : Brushes.LimeGreen;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}