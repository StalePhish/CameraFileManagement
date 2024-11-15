using static CAndrews.CameraFileManagement.Enumerations;

namespace CAndrews.CameraFileManagement;

/// <summary>
/// Structure to store progress
/// </summary>
public class CameraProgress
{
    /// <summary>
    /// Constructor
    /// </summary>
    public CameraProgress()
    {
        Value = 0;
        Maximum = 1;
        Indeterminate = false;
        Text = null;
        Result = StatusType.Success;
    }

    /// <summary>
    /// Progress value for the progress bar
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Progress maximum for the progress bar
    /// </summary>
    public double Maximum { get; set; }

    /// <summary>
    /// Progress bar indeterminate state
    /// </summary>
    public bool Indeterminate { get; set; }

    /// <summary>
    /// Progress bar display text
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Overall progress result
    /// </summary>
    public StatusType Result { get; set; }

    /// <summary>
    /// Display text in the format of 12.34 MB / 56.78 MB
    /// </summary>
    public string MegabyteText => string.Format("{0:N2} MB / {1:N2} MB", Value, Maximum);
}
