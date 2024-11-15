using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CAndrews.CameraFileManagement;

/// <summary>
/// Notifies clients that a property value has changed.
/// </summary>
public class NotifyPropertyChanged : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raise property changed event
    /// </summary>
    /// <param name="propertyName">Name of property</param>
    public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
