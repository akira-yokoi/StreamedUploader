using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;
using Windows.Storage.Pickers;

namespace STREAMED
{
  class EpsonScanner
  {
    const String EPSON_DRIVER_PATH = "C:\\Windows\\twain_32\\escndv\\";
    const String EPSON_SCAN_EXE = "escndv.exe";
    const String EPSON_SCAN_COMMAND = "escndv.exe -DS";
    const String EPSON_SCAN_SETTINGS_PARAM = " -J ";
    const String EPSON_SCAN_SETTINGS_FILE = "epsonScanSettings.ssf";
    const String EPSON_FOLDER_PATH = "FolderAbsolutePath";

    public async void scan()
    {
      openFile();

      String modelName = SettingUtil.getString(SettingUtil.SCANNER_KEY);
      String settingFilePath = FileUtil.getInstallLocation().Path + "\\Assets\\epsonScanSettings.ssf";
//      StorageFile storageFile = await StorageFile.GetFileFromPathAsync(settingFilePath);

      String command = EPSON_DRIVER_PATH + EPSON_SCAN_COMMAND + "\"" +
         modelName + "\"" + EPSON_SCAN_SETTINGS_PARAM +
         "\"" + settingFilePath + "\"";

      String cmd = "C:\\Program Files (x86)\\AirCampus2\\AirCampus.exe";

      StorageFile exeFile = await StorageFile.GetFileFromPathAsync(cmd);

      LauncherOptions options = new LauncherOptions();
      options.DisplayApplicationPicker = false;
      options.TreatAsUntrusted = false;


      bool success = await Launcher.LaunchFileAsync(exeFile, options);
      Log.debug("");
    }

    private async void openFile()
    {
      // using Windows.Storage.Pickers;
      FileOpenPicker openPicker = new FileOpenPicker();

      // 表示モードはリスト形式
      //openPicker.ViewMode = PickerViewMode.List;

      // 表示モードはサムネイル形式
      openPicker.ViewMode = PickerViewMode.Thumbnail;

      // ピクチャーライブラリーが起動時の位置
      // その他候補はPickerLocationIdを参照
      // http://msdn.microsoft.com/en-us/library/windows/apps/windows.storage.pickers.pickerlocationid
      openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

      // jpg, jpeg, pngのファイル形式から選択
      openPicker.FileTypeFilter.Add(".exe");

      // ファイルオープンピッカーを起動する
      StorageFile file = await openPicker.PickSingleFileAsync();
    }
  }
}
