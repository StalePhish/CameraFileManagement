using System.Windows;
using System.Windows.Controls;
using CAndrews.CameraFileManagement.Application.ViewModel;

namespace CAndrews.CameraFileManagement.Application.View;

/// <summary>
/// Interaction logic for FileAttributesControl.xaml
/// </summary>
public partial class FileAttributesControl : UserControl
{
    private readonly FileAttributesViewModel _viewModel = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public FileAttributesControl()
    {
        InitializeComponent();

        _viewModel = DataContext as FileAttributesViewModel;
    }

    /// <summary>
    /// When a file is drag-and-dropped onto the window
    /// </summary>
    /// <param name="sender">Object that raised the event</param>
    /// <param name="e">Event arguments</param>
    private void DropFiles(object sender, DragEventArgs e)
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        _viewModel.Source = files[0];
    }
}
