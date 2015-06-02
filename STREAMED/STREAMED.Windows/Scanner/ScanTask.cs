using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading;
using Windows.System;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace STREAMED
{
  class ScanTask
  {
    private static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
    public const Int32 WM_COPYDATA = 0x4A;

    [DllImport("User32.dll", EntryPoint = "SendMessage")]
    public static extern Int32 SendMessage(
                                        IntPtr hWnd,
                                        UInt32 Msg,
                                        UInt32 wParam,
                                        ref COPYDATASTRUCT lParam);

    [DllImport("USER32.dll", EntryPoint = "FindWindow")]
    public static extern IntPtr FindWindow(
                                        string lpClassName,
                                        string lpWindowName);

    [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
    public static extern Int32 GetPrivateProfileString(
                                        string lpAppName,
                                        string lpKeyName,
                                        string lpDefault,
                                        StringBuilder lpReturnedString,
                                        UInt32 nSize,
                                        string lpFileName);

    [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileInt")]
    public static extern UInt32 GetPrivateProfileInt(
                                        string lpAppName,
                                        string lpKeyName,
                                        Int32 nDefault,
                                        string lpFileName);

    [DllImport("advapi32.dll", EntryPoint = "RegOpenKeyEx")]
    public static extern Int32 RegOpenKeyEx(
                                        UIntPtr hKey,
                                        string lpSubKey,
                                        UInt32 ulOptions,
                                        UInt32 samDesired,
                                        out UIntPtr phkResult);

    [DllImport("advapi32.dll", EntryPoint = "RegQueryValueEx")]
    public static extern Int32 RegQueryValueEx(
                                        UIntPtr hKey,
                                        string lpValueName,
                                        IntPtr lpReserved,
                                        out UInt32 lpType,
                                        StringBuilder lpData,
                                        ref UInt32 lpcbData);

    [DllImport("advapi32.dll", EntryPoint = "RegCloseKey")]
    public static extern Int32 RegCloseKey(UIntPtr hKey);


    /// <summary>
    /// COPYDATASTRUCT
    /// </summary>
    public struct COPYDATASTRUCT
    {
      public Int32 dwData;
      public Int32 cbData;
      public IntPtr lpData;
    }

    /// <summary>
    /// SS_NOTIFY
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct SS_NOTIFY
    {
      public Int32 Mode;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
      public byte[] AppName;
    }

    /// <summary>
    /// SS_SCAN
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct SS_SCAN
    {
      public Int32 Mode;
      public bool ScanningSide;
    }

    /*
    protected override void WndProc(ref Message winMessage)
    {
      switch (winMessage.Msg)
      {
        // sendmessage
        case ImageConnectionsMain.WM_COPYDATA:
          string filePath;
          int result = 1;         // success
          try
          {
            ImageConnectionsMain.COPYDATASTRUCT cds = new ImageConnectionsMain.COPYDATASTRUCT();
            Type cdsType = cds.GetType();
            cds = (ImageConnectionsMain.COPYDATASTRUCT)winMessage.GetLParam(cdsType);
            filePath = cds.lpData;

            // copy file or move file
            CopyFile(filePath);

            // delete ini file
            File.Delete(filePath);
          }
          catch
          {
            // Please describe processing when the error occurs.
            result = 0;         // failed
          }
          winMessage.Result = (IntPtr)result;
          break;
        default:
          base.WndProc(ref winMessage);
          break;
      }
    }
    */

    public void scan()
    {
      Task scanTask = Task.Run(() => startScan());
      Task.WaitAny(new Task[] { scanTask });
      Log.debug("");
    }

    public async void startScan()
    {
      string ssmPath = GetSsManagerPath();
      launch(ssmPath);
      // scan
      Int32 result;

      IntPtr hWnd = IntPtr.Zero;
      hWnd = FindWindow("ScanSnap Manager MainWndClass", null);   // Win32API
      if (hWnd == IntPtr.Zero)
      {
        // Executing ScanSnap Manager
        // Get ScanSnap Manager path
        string cmdPath = GetSsManagerPath();
        if (String.IsNullOrEmpty(cmdPath) == false)
        {
          try
          {
            launch(cmdPath);
            for (int i = 0; i < 10; i++)
            {
              await Task.Delay(TimeSpan.FromSeconds(5));
              hWnd = FindWindow("ScanSnap Manager MainWndClass", null);
              if (hWnd != IntPtr.Zero)
              {
                break;
              }
            }
          }
          catch
          {
            // No relation of the extension. 
          }
        }
        if (hWnd == IntPtr.Zero)
        {
          ViewUtil.showMesssage("FindWindow failed");
          return;
        }
      }

      // Reserve command
      result = SendControlCommand(hWnd, 0);
      if (result != 0)
      {
        ViewUtil.showMesssage("ReserveCommand failed (" + result.ToString() + ")");
        return;
      }

      // ScanStart command
      result = SendStartScan(hWnd);
      if (result != 0)
      {
        ViewUtil.showMesssage("StartScanCommand failed (" + result.ToString() + ")");
      }

      // Release command
      result = SendControlCommand(hWnd, 1);
      if (result != 0)
      {
        ViewUtil.showMesssage("ReleaseCommand failed (" + result.ToString() + ")");
      }
      return;
    }

    private async void launch( String path)
    {
      LauncherOptions options = new LauncherOptions();
      options.DisplayApplicationPicker = false;
      options.TreatAsUntrusted = false;

      StorageFile storageFile = await StorageFile.GetFileFromPathAsync(path);

      bool success = await Launcher.LaunchFileAsync(storageFile, options);
      Log.debug("");
    }

    /// <summary>
    /// Send control comannd to the ScanSnap manager. 
    /// </summary>
    /// <param name="hWnd">Window handle</param>
    /// <param name="mode">0:RESERVE,1:RELEASE</param>
    /// <returns>Result of SendMessage</returns>
    private static Int32 SendControlCommand(IntPtr hWnd, int mode)
    {
      Int32 result = 0;

      COPYDATASTRUCT cds = new COPYDATASTRUCT();
      cds.dwData = 2;
      SS_NOTIFY notify = new SS_NOTIFY();
      notify.Mode = mode;
      notify.AppName = new byte[255];
      byte[] src = Encoding.GetEncoding("ASCII").GetBytes("ImageConnections");       // Please match as the name of the registry key.
      Array.Copy(src, notify.AppName, src.Length);
      cds.lpData = Marshal.AllocCoTaskMem(Marshal.SizeOf(notify));
      Marshal.StructureToPtr(notify, cds.lpData, true);
      cds.cbData = Marshal.SizeOf(notify);

      // SendMessage
      result = SendMessage(hWnd, WM_COPYDATA, (Int32)0, ref cds);    // Win32API
      Marshal.FreeCoTaskMem(cds.lpData);

      return result;
    }

    /// <summary>
    /// Send StartScan comannd to the ScanSnap manager.
    /// </summary>
    /// <param name="hWnd">Window handle</param>
    /// <returns>Result of SendMessage</returns>
    private static Int32 SendStartScan(IntPtr hWnd)
    {
      Int32 result = 0;

      COPYDATASTRUCT cds = new COPYDATASTRUCT();
      cds.dwData = 3;
      SS_SCAN scan = new SS_SCAN();
      scan.Mode = 0;
      scan.ScanningSide = true;                                       // One side
      cds.lpData = Marshal.AllocCoTaskMem(Marshal.SizeOf(scan));
      Marshal.StructureToPtr(scan, cds.lpData, true);
      cds.cbData = Marshal.SizeOf(scan);

      // SendMessage
      result = SendMessage(hWnd, WM_COPYDATA, (Int32)0, ref cds);    // Win32API
      Marshal.FreeCoTaskMem(cds.lpData);

      return result;
    }

    /// <summary>
    /// Get ScanSnap Manager path
    /// </summary>
    /// <remarks>
    /// It looks for "VirtualStore" when "Microsoft.Win32.Registry.LocalMachine.OpenSubKey" is used.
    /// </remarks>
    /// <returns>null:error, Not null:path string</returns>
    private string GetSsManagerPath()
    {
      string keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\PfuSsMon.exe";
      UIntPtr hKey;
      Int32 result;
      UInt32 KEY_READ = (UInt32)((0x00020000L |           // STANDARD_RIGHTS_READ
                                  0x0001 |                // KEY_QUERY_VALUE
                                  0x0008 |                // KEY_ENUMERATE_SUB_KEYS
                                  0x0010)                 // KEY_NOTIFY
                                  &
                                  (~0x00100000L));        // SYNCHRONIZE


      result = RegOpenKeyEx(HKEY_LOCAL_MACHINE, keyName, 0, KEY_READ, out hKey);  // Win32API
      if (result == 0)
      {
        UInt32 size = 1024;
        UInt32 type;
        string keyValue = null;
        StringBuilder keyBuffer = new StringBuilder((int)size);

        result = RegQueryValueEx(hKey, "", IntPtr.Zero, out type, keyBuffer, ref size); // Win32API
        if (result == 0)
        {
          keyValue = keyBuffer.ToString();
        }

        RegCloseKey(hKey);

        return (keyValue);
      }
      return (null);
    }
  }
}
