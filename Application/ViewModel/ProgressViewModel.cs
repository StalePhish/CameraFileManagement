namespace CAndrews.CameraFileManagement.Application.ViewModel;

/// <summary>
/// View Model for ProgressVM Window
/// </summary>
internal class ProgressViewModel : NotifyPropertyChanged
{
    /*
     * put this in CameraControl and use _progressControl.ViewModel to attach the ProgressUpdate
     * 
        <Grid>
            <local:ProgressControl x:Name="_progressControl" />
        </Grid>
    */

    /// <summary>
    /// Constructor
    /// </summary>
    public ProgressViewModel()
    {
        //_cameraLoad.ProgressUpdate += UpdateProgress();
        //_cameraCopy.ProgressUpdate += UpdateProgress();
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~ProgressViewModel()
    {
        //_cameraLoad.ProgressUpdate -= UpdateProgress();
        //_cameraCopy.ProgressUpdate -= UpdateProgress();
    }

    #region Progress Updates

    /// <summary>
    /// <see cref="CameraProgress.Value"/>
    /// </summary>
    public double ProgressValue
    {
        get => _progressValue;
        set
        {
            _progressValue = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// <see cref="CameraProgress.Maximum"/>
    /// </summary>
    public double ProgressMaximum
    {
        get => _progressMaximum;
        set
        {
            _progressMaximum = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// <see cref="CameraProgress.Indeterminate"/>
    /// </summary>
    public bool ProgressIndeterminate
    {
        get => _progressIndeterminate;
        set
        {
            _progressIndeterminate = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// <see cref="CameraProgress.Text"/>
    /// </summary>
    public string ProgressText
    {
        get => _progressText;
        set
        {
            _progressText = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// <see cref="CameraProgress.Result"/>
    /// </summary>
    public Enumerations.StatusType ProgressResult
    {
        get => _progressResult;
        set
        {
            _progressResult = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Update the progress notification for the user
    /// </summary>
    /// <returns></returns>
    public Action<CameraProgress> UpdateProgress()
    {
        return progress =>
        {
            ProgressValue = progress.Value;
            ProgressMaximum = progress.Maximum;
            ProgressIndeterminate = progress.Indeterminate;
            ProgressText = progress.Text;
            ProgressResult = progress.Result;
        };
    }

    #endregion Progress Updates

    #region Private Members

    private double _progressValue = 0;
    private double _progressMaximum = 1;
    private bool _progressIndeterminate = false;
    private string _progressText;
    private Enumerations.StatusType _progressResult;

    #endregion Private Members
}
