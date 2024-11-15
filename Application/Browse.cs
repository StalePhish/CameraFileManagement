using Microsoft.Win32;

namespace CAndrews.CameraFileManagement.Application;

internal static class Browse
{
    /// <summary>
    /// Browse for a directory and returns the path to it
    /// </summary>
    /// <param name="title">The title of the browse dialog</param>
    /// <param name="path">[out] The new value of the path, or original value on cancel</param>
    /// <param name="current">[optional] Current folder</param>
    /// <returns>True if new path selected; False if cancelled</returns>
    public static bool BrowseForDirectory(string title, out string path, string currentFolder = null)
    {
        path = currentFolder;
        var browse = new OpenFolderDialog
        {
            Title = title,
            InitialDirectory = currentFolder,
            Multiselect = false,
        };
        if (browse.ShowDialog() == true)
        {
            path = browse.FolderName;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Browse for a single file and returns the path
    /// </summary>
    /// <param name="title">The title of the browse dialog</param>
    /// <param name="path">[out] The new value of the path</param>
    /// <param name="current">[optional] Current file that was selected</param>
    /// <param name="filter">[optional] The filters to use for selecting the file</param>
    /// <param name="fileName">[optional] The default file name</param>
    /// <param name="mustExist">[optional] True if the file must exist, false if not</param>
    /// <returns>True if new path selected; False if cancelled</returns>
    public static bool BrowserForFilePath(string title, out string path, string current = null, 
        string filter = null, string fileName = null, bool mustExist = true)
    {
        path = null;
        var browse = new OpenFileDialog
        {
            Title = title,
            InitialDirectory = current,
            Filter = filter,
            FileName = fileName,
            CheckFileExists = mustExist,
            Multiselect = false,
        };
        if (browse.ShowDialog() == true)
        {
            path = browse.FileName;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Browse for multiple files and returns the paths
    /// </summary>
    /// <param name="title">The title of the browse dialog</param>
    /// <param name="paths">[out] The new value of the path</param>
    /// <param name="current">[optional] Current file that was selected</param>
    /// <param name="filter">[optional] The filters to use for selecting the files</param>
    /// <param name="fileName">[optional] The default file name</param>
    /// <param name="mustExist">[optional] True if the files must exist, false if not</param>
    /// <returns>True if new paths are selected; False if cancelled</returns>
    public static bool BrowseForFilePaths(string title, out string[] paths, string current = null, 
        string filter = null, string fileName = null, bool mustExist = true)
    {
        paths = null;
        var browse = new OpenFileDialog
        {
            Title = title,
            InitialDirectory = current,
            Filter = filter,
            FileName = fileName,
            CheckFileExists = mustExist,
            Multiselect = true,
        };
        if (browse.ShowDialog() == true)
        {
            paths = browse.FileNames;
            return true;
        }
        return false;
    }
}
