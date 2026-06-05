using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AfvalKalender.DesktopUI.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isBezig && isBezig)
        {
            return Brushes.Orange;
        }
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
