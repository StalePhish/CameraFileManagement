using CAndrews.CameraFileManagement.Application.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CAndrews.CameraFileManagement.Application.View;

/// <summary>
/// Interaction logic for SettingsControl.xaml
/// </summary>
public partial class SettingsControl : UserControl
{
    private readonly SettingsViewModel _viewModel = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public SettingsControl()
    {
        InitializeComponent();

        _viewModel = DataContext as SettingsViewModel;
    }

    /// <summary>
    /// When a file is drag-and-dropped onto the window
    /// </summary>
    /// <param name="sender">Object that raised the event</param>
    /// <param name="e">Event arguments</param>
    private void DropFiles(object sender, DragEventArgs e)
    {
        ((string[])e.Data.GetData(DataFormats.FileDrop)).ToList().ForEach(
            Settings.Instance.CameraSettings.TryAddCamera);
        _viewModel.SaveSettings();
    }

    /// <summary>
    /// Browse to select a Destination folder for the selected row
    /// </summary>
    /// <param name="sender">Data row that raised the event</param>
    /// <param name="e">Event arguments</param>
    private void BrowseDestination(object sender, RoutedEventArgs e)
    {
        for (var vis = sender as Visual; vis is not null; vis = VisualTreeHelper.GetParent(vis) as Visual)
        {
            if (vis is DataGridRow row && row.Item is CameraSetting camera)
            {
                if (Browse.BrowseForDirectory($"Selected a destination folder for {camera.Model}.", out string path, camera.Destination))
                {
                    camera.Destination = path;
                    _viewModel.SaveSettings();
                }
                break;
            }
        }
    }

    /// <summary>
    /// Delete the selected row
    /// </summary>
    /// <param name="sender">Data row that raised the event</param>
    /// <param name="e">Event arguments</param>
    private void Delete(object sender, RoutedEventArgs e)
    {
        for (var vis = sender as Visual; vis is not null; vis = VisualTreeHelper.GetParent(vis) as Visual)
        {
            if (vis is DataGridRow row && row.Item is CameraSetting camera)
            {
                Settings.Instance.CameraSettings.Remove(camera);
                _viewModel.SaveSettings();
                break;
            }
        }
    }

    /// <summary>
    /// Triggers upon a cell of the data grid having been edited
    /// </summary>
    /// <param name="sender">Data row that raised the event</param>
    /// <param name="e">Event arguments</param>
    private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit && e.Column as DataGridBoundColumn is not null)
        {
            _viewModel.SaveSettings();
        }
    }

    /// <summary>
    /// Triggers upon a checkbox state being toggled
    /// </summary>
    /// <param name="sender">CheckBox that raised the event</param>
    /// <param name="e">Event arguments</param>
    private void CheckBox_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveSettings();
    }

    /// <summary>
    /// Triggers upon a combobox selection being completed
    /// </summary>
    /// <param name="sender">ComboBox that raised the event</param>
    /// <param name="e">Event arguments</param>
    private void ComboBox_DropDownClosed(object sender, EventArgs e)
    {
        _viewModel.SaveSettings();
    }
}
