using System;
using System.Windows.Data;
using System.Windows.Media;

namespace OpenBullet.Converters
{
    public class BooleanBrushConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new SolidColorBrush((bool)value?.Equals(true) ? Colors.Tomato : Colors.Yellow);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
        #endregion
    }
}
