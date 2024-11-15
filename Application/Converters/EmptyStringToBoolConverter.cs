using System.Globalization;
using System.Windows.Data;

namespace CAndrews.CameraFileManagement.Application.Converters;

/// <summary>
/// Converter for empty string validity to boolean
/// </summary>
internal class EmptyStringToBoolConverter : IValueConverter
{
    /// <summary>
    /// Returns true if the string is not empty, false if it is empty
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string stringValue && !string.IsNullOrWhiteSpace(stringValue);
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException($"{nameof(EmptyStringToBoolConverter)}.{nameof(ConvertBack)}");
    }
}