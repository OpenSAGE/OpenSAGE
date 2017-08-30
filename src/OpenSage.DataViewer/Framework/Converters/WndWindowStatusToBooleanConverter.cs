using System;
using System.Globalization;
using System.Windows.Data;
using OpenSage.Data.Wnd;

namespace OpenSage.DataViewer.Framework.Converters
{
    public sealed class WndWindowStatusToBooleanConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new WndWindowStatusToBooleanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (WndWindowStatusFlags) value;
            var p = (WndWindowStatusFlags) parameter;
            return v.HasFlag(p);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
