using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CAndrews.CameraFileManagement.Application.Converters;

/// <summary>
/// Converter for boolean to visibility
/// </summary>
internal class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Returns visible if the boolean is true, otherwise collapsed
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue && boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
