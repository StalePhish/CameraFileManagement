using CAndrews.CameraFileManagement.Application.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CAndrews.CameraFileManagement.Application.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindow()
    {
        Settings.Instance.Load();

        InitializeComponent();        

        _viewModel = DataContext as MainWindowViewModel;

        // Build a dictionary of tab indexes to their respective view models
        _viewModel.TabsIndex = [];
        for (int i = 0; i < _tabs.Items.Count; ++i)
        {
            _viewModel.TabsIndex.Add((_tabs.Items[i] as TabItem).Header.ToString(), i);
        }
    }

    /// <summary>
    /// Intercepts keyboard presses
    /// </summary>
    /// <param name="sender">Object that raised the event</param>
    /// <param name="e">Event arguments</param>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && 
            e.Key >= Key.D0 && e.Key <= Key.D9)
        {
            _tabs.SelectedIndex = e.Key - Key.D1;
            e.Handled = true;
        }
    }
}