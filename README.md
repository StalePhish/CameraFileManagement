# Camera File Management
Highly configurable windows application and CLI to manage importing photos and videos from any type of camera.

Load directly from an SD card or local directory to batch copy and rename thousands of files with a timestamped filename including camera model and original filename as suffixes such as `2024-11-15 14.34.12 CANON SX740 IMG_0001.jpg`.

For years, I was frustrated with managing photos from a fleet of Canon cameras, often ending up with dozens of copies of IMG_0001.jpg and no efficient way to archive and sort them. Searching for a specific photo was a hassle, as I had to sift through my personal archives organized by camera model and deal with unhelpful filenames and sort order. My fleet has since expanded to include Nikon, GoPro, Google Pixel cell phones, and more, so it was time for a change. I developed Camera File Management ("CFM," or you could even pronounce it "seafoam") as a hobby project for myself, but I imagine many others are in similar situations.

![Cameras](https://github.com/user-attachments/assets/047b1786-2ab8-4552-a07c-bcb0587ed539)

## Camera Copy
The meat-and-potatoes of Camera File Management is the `Camera Copy` tab. This is where a source directory is input resulting in a large grid being populated with pending renames calculated by the camera make/model of every indivudal file including individual destination directories and filenames calculated per application-level settings or camera-level settings defined on the `Settings` tab. You can then decide to make changes and then Run the batch copy and rename while observing progress, status, and applicable error messages.

The meat-and-potatoes functionality of Camera File Management lies in the `Camera Copy` tab. Here, you input a source directory, which populates a large grid with pending file copies calculated based on the camera make/model of each individual file. This includes individual destination directories and filenames, determined by the application-level or camera-level settings defined in the `Settings` tab to combine a date/time stamp, camera alias, and suffix. You can then review and make any necessary changes before running the batch copy and rename process, while observing progress, status, and any applicable error messages.
   
![Camera File](https://github.com/user-attachments/assets/4031ef78-0939-4033-8bd0-81a14eec9d21)

- Toolbar:
  - `Load Removable Drive` - Search for removable drives that may be connected and load the one that was found.
    - Note if a removable drive is already inserted when Camera File Management is launched, it will be selected automatically.    
  - `Source Directory` - The source directory to load files from, filtered by the application-level `File Extensions` setting.
    - When the application first launches, it attempts to load any removable drives that are connected, then tries to load from the application-level `Default Source Directory`.
    - Press the embedded ↺ button to reset to the application-level `Default Source Directory`.
    - Press the … button to browse.
    - Drag-and-drop a file or folder from Windows Explorer to load it without browsing.
  - `Camera` - Displays which camera is loaded if one could be detected.
  - `⮊` - Expander for more camera settings
    - `Apply to Unknown` - Sets the camera for any unknown files to the one selected in the `Camera` dropdown.
    - `Apply to All` - Sets the camera for all files to the one selected in the `Camera` dropdown, overwriting any previous cameras detected.
    - `Persist on Card` - Write a `CFM` file to the root of an external drive (SD card, etc) containing the alias of the selected `Camera` dropdown.
  - `Advanced ⮈` - Expander for advanced settings
    - `Append Suffix` - Input text and press the embedded ✎ button to append it as a suffix for every file in the grid.
    - `Include Subfolders` - Check the box to search subfolders under the `Source Directory`. Uncheck the box to only search the exact `Source Directory`.
    - `Open Folders After` - Check the box to open the destination directories in Windows Explorer of any files that were successfully copied with those files selected.
      - Note that if an excessive amount of files were copied into one directory, the files won't be selected upon Windows Explorer launching.
  - `Select All` - Selects all files from the grid.
  - `Deselect Unknown` - Deselect files from the grid that don't have a known camera.
- Grid:
  - `Selected` - Check the box for the file to be copied during Run. Defaulted to checked for known cameras.
  - `Status` - `Ready/Success/Processing/Failed/Canceled/Skipped`.
    - For `Failed`, mouse-over to see the failure reason for that file.
  - `Camera` - Camera Alias for the file. Select from the dropdown to change to a different camera setting, which will trigger the file's fields to be recalculated.
  - `Source` - Source file path relative to the `Source Directory`.
  - `Destination Directory` - Destination directory path. Defaulted to camera's `Destination Directory` setting including any application-specific `Date/Time Format` setting directory parent.
  - `Browse` - Browse to select a different `Destination Directory`.
  - `Destination File Name` - Destination file name calculated from `Source` file name, application-level `Date/Time Format` using `Date/Time Priority`, camera's `Camera Alias` setting, and suffixed with camera's `Format` setting and original file extension.
  - `Destination Exists` - Checked on if the concatenated `Destination Directory` and `Destination File Name` is already an existing file.
  - `Move (or Copy)` - Check the box if you want files from this camera to move out of the source directory and into the destination directory. Unchecked to copy to leave the source file untouched. Defaulted to the camera's `Move (or Copy)` setting.
- Footer:
  - Progress bar - Displays loading or file copy status including errors if applicable.
  - `Run` - Runs the copy or move for each file that has its `Selected` checkbox checked.
    - `Move All` (rightclick `Run`) - Overrides the move-or-copy setting per file to move everything.
    - `Copy All` (rightclick `Run`) - Overrides the move-or-copy setting per file to copy everything.
    - `Rename Only` (rightclick `Run`) - Overrides the move-or-copy setting per file to rename everything in place without copying.
    - `Demonstration` (rightclick `Run`) - Performs a simulated copy for demonstration purposes.
  - `Cancel` - Halts any current Run.
    - Note: only appears when a Run is in progress.

## File Attributes
Any image or video file will often contain not just one Date/Time attribute but between four to ten different attributes, several of which could vary by seconds to hours from the expected correct time. Even from one camera that produces both .jpg and .mp4 files, you may need to read different attributes for each filetype otherwise your files may be saved out of order. To help configure your camera-specific settings page for an accurate timestamp, I have provided this `File Attributes` tab. It assists in determining the attribute name you want to override for that camera if the application default is not suitable. Browse to a file or drag-and-drop a file from Windows Explorer to load it without browsing to see a list of many attributes to find the right one. For example, some cameras have a more accurate DateTime attribute, while others may have a more accurate DateAcquired or CreationTime and may not even have a DateTime.

![File Attributes](https://github.com/user-attachments/assets/73621fdf-507e-444d-96d5-6f5d1b678b17)

- Toolbar
  - `File` - Absolute file path of the file to inspect.
    - Press the … button to browse.
- Grid
  - `Name` - Attribute name
  - `Value` - Attribute value
  - `Source` - Source of attribute such as a third party library used to read attribute data
- Footer
  - `Select Camera` - Camera Alias for the file. Select from the dropdown to change to a different camera setting, which will trigger the file name to be recalculated.
  - File Name - Destination Directory and File Name calculated from the camera settings.
  - `Copy` - Run a copy on this one file.

## Settings
The core of configurability for Camera File Management lies in the `Settings` tab, where both application-level and camera-level settings are configured. Whenever a setting is modified, a green ⎙ icon will briefly flicker in the upper left corner of the tab, indicating that the changes have been auto-saved to disk at `%APPDATA%\CAndrews\CAndrews.CameraFileManagement\user.cfm`.

![CFM Settings](https://github.com/user-attachments/assets/e21e11e2-9232-4706-88f5-ab5ad3216623)

- Toolbar
  - `⎙ (Save icon)` - flickers green when the settings are being auto-saved.
  - `Import` - Open an existing Camera File Management Settings (.cfm) file. This is helpful if you want to share a .cfm file across multiple devices.
  - `Export` - Saves a copy of the settings to a new Camera File Management Settings (.cfm) file. You only need to use this if you are going to share your settings or make a backup, as the settings are auto-saved.
  - `Add Camera` - Browse to an image file to attempt to load camera attributes into a new camera row in the settings grid.
    - Note that if you press Cancel a blank row can be created for manual input. I recommend adding a blank camera row with valid Destination Directory to copy files such as screenshots that don't have a camera make and model specified.
  - `Date/Time Format` - Application-level Date/Time format for destination filenames.
    - Note that one or more directory parents can be part of this format and will be reflected in the destination file name.
    - Press the embedded ↺ button to reset to the default of `yyyy/yyyy-MM-dd HH.mm.ss`, which will resolve for example to `.\2024\2024-11-15 14.34.12.jpg`.
    - This format was chosen because it is the filename format of files uploaded from an Android phone using [Dropbox](https://play.google.com/store/apps/details?id=com.dropbox.android) to the local `Dropbox\Camera Uploads` directory which I use to back up cell phone photos.
  - `Date/Time Priority` - Application-level priority order to read Date/Time attributes to determine correct time stamp.
    - Press the embedded ↺ button to reset to the default of `DateTime DateAcquired CreationTime LastAccessTime LastWriteTime`.
  - `File Extensions` - Space-separated list of file extensions to search for.
    - Ensure to include common extensions like jpg, avi, and mp4. You may need to add specific extensions if your camera manufacturer uses a proprietary format for RAW files, and you might want to omit extensions like cfg to prevent SD card configuration files from being treated as image files.
    - Press the embedded ↺ button to reset to the default.
  - `Default Source Directory` - Directory to automatically search on application launch if a removable drive is not found. 
    - Press the embedded ↺ button to reset to blank.
    - Press the … button to browse.
    - An example is the location of your Camera Uploads from Dropbox.
- Grid
  - `Enabled` - Check the box if you want this camera to appear in camera selection dropdowns throughout the application. Uncheck to hide from those dropdowns. Note that regardless of the checkbox state, if a file is resolved to be a particular camera, it will use it.
  - `Type` - Type of camera from the list `None/Camera/Video/Phone/Drone/Dashcam/Scanner`. `Camera` and `Drone` will attempt to eject their removable drive after files were copied, if applicable. `Dashcam` will only attempt to copy read-only files.
  - `Camera Alias` - Shortname camera alias that gets included in the destination filename. Uppercase is suggested. For example of you alias a Nikon Coolpix P950 as `NIKON P950`, the destination filen ame may result in `2024-11-15 14.34.12 NIKON P950 DSCN0001.jpg`.
  - `Make` - Manufacturer's name of the camera.
    - This is used in conjunction with `Model` to allow for the determination of which camera a file was created with, and it must match the file attribute exactly.
  - `Model` - Model name of the camera.
    - This is used in conjunction with `Make` to allow for the determination of which camera a file was created with, and it must match the file attribute exactly.
  - `Destination Directory` - Absolute or relative directory path to copy files to for this camera.
    - For example `C:\Users\chris\Pictures`. Note that the output filename will be a concatenation of this directory with the result of the application-level `Date/Time Format` applied to each file. If directories were included as part of the format, they will be resolved here. For example a complete output path may be `C:\Users\chris\Pictures\2024\2024-11-15 14.34.12 CANON SX740 IMG_0001.jpg`.
  - `Browse` - Browse to the `Destination Directory`.
  - `Format` - [Regular Expression](https://en.wikiversity.org/wiki/Regular_expressions) to run on the source filename to preserve any existing filename information if desired.
    - For example `(IMG|M\w{2})+(_.+)` will extract the "IMG_0001" portion of the filename and append it to the end of the generated timestamp filename. For example `2024-11-15 14.34.12 CANON SX740 IMG_0001.jpg`. A helpful resource for developing regular expressions is [regex101](https://regex101.com/).
  - `Date/Time Priority` - Override the application-level `Date/Time Priority` for this specific camera.
  - `Move (or Copy)` - Check the box if you want files from this camera to move out of the source directory and into the destination directory. Unchecked to copy to leave the source file untouched.
  - `Delete` - Removes the camera from the settings grid.

## Command Line Interface
Camera File Management provides a CLI (command-line interface) accessed via `CAndrews.CameraFileManagement.CLI.exe` in a command prompt. This provides most of the same functionality as the full application, but its main purpose is to create shortcuts for frequently run commands. For example every day I open a shortcut to automatically run against my Dropbox\Camera Uploads directory to grab photos taken with my phone the day before.

All commands are arguments against `CAndrews.CameraFileManagement.CLI.exe`, for example `CAndrews.CameraFileManagement.CLI.exe about`
- `camera -?` - Camera copy command
  - `list <path>` - Display camera files to be copied
  - `run <path>` - Run file transfer using individual move or copy settings per file
  - `move <path>` - Run file transfer explicitly moving all files
  - `copy <path>` - Run file transfer explicitly copying all files
  - `rename <path>` - Run file transfer explicitly renaming all files in place
  - `demo <path>` - Run file transfer in demonstration mode that does not copy and files
- `file <path>` - Displays file attributes
- `settings -?` - Manages settings
  - `settings list` - Displays settings
  - `import <path>` - Import settings from .cfm file
  - `export <path>` - Export settings to .cfm file
  - `set-format <value>` - Sets the Date/Time format
  - `reset-format` - Resets the Date/Time format to factory default
  - `set-extensions <value>` - Sets the Date/Time format
  - `reset-extensions` - Resets the supported file extensions to factory default
  - `set-default-source-directory <value>` - Sets the default source directory
  - `reset-default-source-directory` - Resets the default source directory to factory default
  - `add <path>` - Adds a new camera from a sample file
  - `delete <camera|index>` - Deletes a camera
  - `set <camera|index> <property> <value>` - Sets a property's value
- `about` - Displays Help>About information

## IDE and Plugins
- [Visual Studio 2022 Community Edition](https://visualstudio.microsoft.com/vs/community/)
- [Increment Version on Build](https://marketplace.visualstudio.com/items?itemName=nti-de.IncrementVersionOnBuild) by NTI DE, used to set the build number of the application in `YYYY.MM.DD HHMM` format instead of a using a traditional `1.0.0.0001` pattern.
- Simply open the solution in Visual Studio and build. Run the `Application` to launch the graphical user interface or use Windows Explorer to browse to `%APPDATA%\CAndrews\CAndrews.CameraFileManagement\artifacts\bin\Debug` where you will find `CAndrews.CameraFileManagement.Application.exe` and `CAndrews.CameraFileManagement.CLI.exe`. I would recommend creating a shortcut to the executable.

## Dependencies from nuget.org
1. [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) by Drew Noakes
2. [Newtonsoft.Json](https://www.newtonsoft.com/json) by James Newton-King
3. [System.CommandLine](https://github.com/dotnet/command-line-api) by Microsoft
4. [taglib-sharp-netstandard2.0](https://github.com/mono/taglib-sharp) by taglib-sharp
5. [UsbEject.NetCore](https://github.com/jcapellman/usbeject) by Jarred Capellman, Simon Mourier, Dmitry Shechtman
6. [WindowsAPICodePack](https://github.com/PWagner1/Windows-API-CodePack-NET) by Microsoft Corporation, Jacob Slusser, Peter William Wagner
