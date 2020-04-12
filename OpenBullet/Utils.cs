using System.Windows;
using System.Windows.Media;

namespace OpenBullet
{
    public static class Utils
    {
        public static Color GetColor(string propertyName)
        {
            try { return ((SolidColorBrush)Application.Current.Resources[propertyName]).Color; }
            catch { return ((SolidColorBrush)Application.Current.Resources["ForegroundMain"]).Color; }
        }

        public static SolidColorBrush GetBrush(string propertyName)
        {
            try { return (SolidColorBrush)Application.Current.Resources[propertyName]; }
            catch { return (SolidColorBrush)Application.Current.Resources["ForegroundMain"]; }
        }
    }
}
