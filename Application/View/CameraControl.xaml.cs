using CAndrews.CameraFileManagement.Application.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CAndrews.CameraFileManagement.Application.View;

/// <summary>
/// Interaction logic for CameraControl.xaml
/// </summary>
public partial class CameraControl : UserControl
{
    private readonly CameraViewModel _viewModel = null;

    public CameraControl()
    {
        InitializeComponent();

        _viewModel = DataContext as CameraViewModel;
        _viewModel.Initialize(_progressControl.ViewModel);
    }

    /// <summary>
    /// When a file is drag-and-dropped onto the window
    /// </summary>
    /// <param name="sender">Object that raised the event</param>
    /// <param name="e">Event arguments</param>
    private void DropFiles(object sender, DragEventArgs e)
    {
        var path = ((string[])e.Data.GetData(DataFormats.FileDrop)).ToList().FirstOrDefault();

        // Go up to the directory if a file path was drag-and-dropped
        if (!Directory.Exists(path))
        {
            path = Path.GetDirectoryName(path);
        }

        _viewModel.SourceDirectory = path;
    }

    /// <summary>
    /// Browse to select a Destination directory for the selected row
    /// </summary>
    /// <param name="sender">Data row that raised the event</param>
    /// <param name="e">Event arguments</param>
    private void BrowseDestination(object sender, RoutedEventArgs e)
    {
        for (var vis = sender as Visual; vis is not null; vis = VisualTreeHelper.GetParent(vis) as Visual)
        {
            if (vis is DataGridRow row && row.Item is CameraFileInfo file)
            {
                if (Browse.BrowseForDirectory($"Selected a destination folder for {file.SourceFileName}.", out string path, file.DestinationDirectory))
                {
                    file.DestinationDirectory = path;
                }
                break;
            }
        }
    }

    /// <summary>
    /// Expands the camera options when the selected camera is modified
    /// </summary>
    /// <param name="sender">ComboBox that raised the event</param>
    /// <param name="e">Event arguments</param>
    private void Camera_DropDownClosed(object sender, EventArgs e)
    {
        _cameraExpander.IsExpanded = true;
    }
}
