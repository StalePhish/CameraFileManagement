using CAndrews.CameraFileManagement.Application.ViewModel;
using System.Windows.Controls;

namespace CAndrews.CameraFileManagement.Application.View
{
    /// <summary>
    /// Interaction logic for ProgressControl.xaml
    /// </summary>
    public partial class ProgressControl : UserControl
    {
        /// <summary>
        /// View Model
        /// </summary>
        internal readonly ProgressViewModel ViewModel = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProgressControl()
        {
            InitializeComponent();

            ViewModel = DataContext as ProgressViewModel;
        }
    }
}
