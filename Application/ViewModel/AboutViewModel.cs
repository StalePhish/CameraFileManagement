using System.Collections.ObjectModel;

namespace CAndrews.CameraFileManagement.Application.ViewModel;

/// <summary>
/// View Model for About Control
/// </summary>
internal class AboutViewModel
{
    /// <summary>
    /// Constructor
    /// </summary>
    public AboutViewModel()
    {
        AboutInformation = new(About.GetAboutInfo());
    }

    /// <summary>
    /// The collection of file information bound to the data grid
    /// </summary>
    public ObservableCollection<Tuple<string, string>> AboutInformation { get; set; }
}
