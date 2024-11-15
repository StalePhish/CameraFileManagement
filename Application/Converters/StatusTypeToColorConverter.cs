using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using static CAndrews.CameraFileManagement.Enumerations;

namespace CAndrews.CameraFileManagement.Application.Converters;

/// <summary>
/// Converter for <see cref="StatusType"/> to <see cref="Brushes"/> color
/// </summary>
internal class StatusTypeToColorConverter : IValueConverter
{
    /// <summary>
    /// Returns <see cref="Brushes"> based on the <see cref="StatusType"/>
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value as StatusType?) switch
        {
            StatusType.Failed => Brushes.Red,
            StatusType.Canceled => Brushes.Yellow,
            StatusType.Processing => Brushes.Blue,
            _ => Brushes.Green,
        };
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
