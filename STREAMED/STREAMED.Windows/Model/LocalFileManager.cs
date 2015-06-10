using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace STREAMED.Model
{
  public class LocalFileManager
  {
    public const string RelPathStreamed = "Streamed";

    private static LocalFileManager _manager;

    public static LocalFileManager SharedManager
    {
      get
      {
        if(_manager == null)
        {
          _manager = new LocalFileManager();
        }
        return _manager;
      }
    }

    public async Task<StorageFolder> getStreamedFolder()
    {
      var appFolder = await Windows.Storage.KnownFolders.PicturesLibrary.TryGetItemAsync(RelPathStreamed) as StorageFolder;

      if (appFolder == null)
      {
        //Create folder
        appFolder = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFolderAsync(RelPathStreamed);
      }

      return appFolder;
    }

    public async Task<List<StorageFile>> getImageFiles()
    {
      var folder = await getStreamedFolder();

      var lst = await folder.GetFilesAsync();

      List<StorageFile> flist = new List<StorageFile>(lst);

      return flist;
    }

    public async Task<StorageFile> getStorageFileByFilename(string filename)
    {
      var folder = await getStreamedFolder();
      var file = await folder.GetFileAsync(filename);

      return file;
    }

    public async Task<bool> deleteImageFile(string filename)
    {
      bool ret = false;
      var file = await getStorageFileByFilename(filename);
      if(file != null && file.IsAvailable)
      {
        await file.DeleteAsync(StorageDeleteOption.Default);
        ret = true;
      }
      return ret;
    }

    public async Task<BitmapImage> getBitmapImage(string filename)
    {
      var file = await getStorageFileByFilename(filename);
      var stream = await file.OpenReadAsync();
      var bmp = new BitmapImage();
      bmp.SetSource(stream);
      return bmp;
    }

    public async Task<string> getImageBase64(string filename)
    {
      var file = await getStorageFileByFilename(filename);
      var stream = await file.OpenReadAsync();
      var decoder = await BitmapDecoder.CreateAsync(stream);
      var pixels = await decoder.GetPixelDataAsync();
      var bytes = pixels.DetachPixelData();
      var str = await ToBase64(bytes, (uint)decoder.PixelWidth, decoder.PixelHeight, decoder.DpiX, decoder.DpiY);
      return str;
    }

    private async Task<string> ToBase64(byte[] image,uint height,uint width,double dpiX = 96,double dpiY = 96)
    {
      var encoded = new InMemoryRandomAccessStream();
      var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, encoded);
      encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, height, width, dpiX, dpiY, image);
      await encoder.FlushAsync();
      encoded.Seek(0);

      var bytes = new byte[encoded.Size];
      await encoded.AsStream().ReadAsync(bytes, 0, bytes.Length);

      var str = Convert.ToBase64String(bytes);
      return str;
    }

    public async Task createDummyFiles()
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
