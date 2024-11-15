namespace CAndrews.CameraFileManagement;

/// <summary>
/// Enumerators
/// </summary>
public static class Enumerations
{
    /// <summary>
    /// Types of cameras
    /// </summary>
    public enum CameraType
    {
        None,
        Camera,
        Video,
        Phone,
        Drone,
        Dashcam,
        Scanner
    }

    /// <summary>
    /// Move or copy each file, based on the file's camera settings or overridden
    /// </summary>
    public enum CopyType
    {
        Run,
        Move,
        Copy,
        Rename,
        Demonstration
    }

    /// <summary>
    /// Status enumeration
    /// </summary>
    public enum StatusType
    {
        Ready,
        Success,
        Processing,
        Failed,
        Canceled,
        Skipped
    }

    /// <summary>
    /// Progress type enumeration
    /// </summary>
    public enum ProgressType
    {
        Info,
        Initialization,
        CopyStart,
        CopyFinish,
        Completed
    }
}
