namespace CAndrews.CameraFileManagement.Application.ViewModel;

/// <summary>
/// View Model for Main Window
/// </summary>
internal class MainWindowViewModel : NotifyPropertyChanged
{
    /// <summary>
    /// Lookup for a tab index given a tab name
    /// </summary>
    public Dictionary<string, int> TabsIndex;

    /// <summary>
    /// Bound to the selected tab on the main window
    /// </summary>
    public int SelectedTab
    {
        get => _selectedTab;
        set
        {
            _selectedTab = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Command to open the Help>About dialog
    /// </summary>
    public RelayCommand AboutCommand => _helpCommand ??= new RelayCommand(
        param => SelectedTab = TabsIndex["About"]);

    #region Private Members

    private RelayCommand _helpCommand;
    private int _selectedTab;

    #endregion Private Members
}
