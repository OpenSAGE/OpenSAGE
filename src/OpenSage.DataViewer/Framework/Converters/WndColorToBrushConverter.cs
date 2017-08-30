using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using OpenSage.Data.Wnd;

namespace OpenSage.DataViewer.Framework.Converters
{
    public sealed class WndColorToBrushConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new WndColorToBrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = (WndColor)value;
            return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
