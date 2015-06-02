using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace STREAMED
{
  class WinUtil
  {
    private StorageFolder getInstallLocation()
    {
      StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
      return storageFolder;
    }
  }
}
