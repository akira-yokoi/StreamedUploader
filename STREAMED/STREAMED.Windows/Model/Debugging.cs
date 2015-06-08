using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace STREAMED.Model
{
  public class Debugging
  {
    public const string RelPathStreamed = "Streamed";


    public static async Task<StorageFolder> getStreamedFolder()
    {
      var appFolder = await Windows.Storage.KnownFolders.PicturesLibrary.TryGetItemAsync(RelPathStreamed) as StorageFolder;

      if (appFolder == null)
      {
        //Create folder
        appFolder = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFolderAsync(RelPathStreamed);
      }

      return appFolder;
    }

    public static async Task createDummyFiles()
    {
      var folder = await getStreamedFolder();

      for (var idx = 1; idx <= 10; idx++)
      {
        string fn = string.Format("img_{0:00}.png",idx);
        var file = await folder.TryGetItemAsync(fn) as IStorageFile;
        if (file == null)
        {
          StorageFile wfile = await StorageFile.GetFileFromApplicationUriAsync( new Uri("ms-appx:///Assets/images.png"));

          await wfile.CopyAsync(folder, fn);
        }
      }
    }
  }
}
