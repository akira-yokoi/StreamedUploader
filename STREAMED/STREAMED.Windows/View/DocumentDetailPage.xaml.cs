using STREAMED.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using STREAMED.Model;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace STREAMED
{
  /// <summary>
  /// A basic page that provides characteristics common to most applications.
  /// </summary>
  public sealed partial class DocumentDetailPage : Page
  {
    private Document document;

    private NavigationHelper navigationHelper;
    private ObservableDictionary defaultViewModel = new ObservableDictionary();

    /// <summary>
    /// This can be changed to a strongly typed view model.
    /// </summary>
    public ObservableDictionary DefaultViewModel
    {
      get { return this.defaultViewModel; }
    }

    /// <summary>
    /// NavigationHelper is used on each page to aid in navigation and 
    /// process lifetime management
    /// </summary>
    public NavigationHelper NavigationHelper
    {
      get { return this.navigationHelper; }
    }


    public DocumentDetailPage()
    {
      this.InitializeComponent();
      this.navigationHelper = new NavigationHelper(this);
      this.navigationHelper.LoadState += navigationHelper_LoadState;
      this.navigationHelper.SaveState += navigationHelper_SaveState;
    }

    /// <summary>
    /// Populates the page with content passed during navigation. Any saved state is also
    /// provided when recreating a page from a prior session.
    /// </summary>
    /// <param name="sender">
    /// The source of the event; typically <see cref="NavigationHelper"/>
    /// </param>
    /// <param name="e">Event data that provides both the navigation parameter passed to
    /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
    /// a dictionary of state preserved by this page during an earlier
    /// session. The state will be null the first time a page is visited.</param>
    private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
    {
    }

    /// <summary>
    /// Preserves state associated with this page in case the application is suspended or the
    /// page is discarded from the navigation cache.  Values must conform to the serialization
    /// requirements of <see cref="SuspensionManager.SessionState"/>.
    /// </summary>
    /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
    /// <param name="e">Event data that provides an empty dictionary to be populated with
    /// serializable state.</param>
    private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
    {
    }

    #region NavigationHelper registration


    #endregion

    double minX = 0;
    double minY = 0;
    double maxX = 0;
    double maxY = 0;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
      document = (Document)e.Parameter;
      var lfMan = LocalFileManager.SharedManager;
      var bmp = await lfMan.getBitmapImage(document.ImagePath);

      bmp.ImageOpened += bmp_ImageOpened;
      this.image.Source = bmp;
    }

    /// <summary>
    /// 画像読み込み完了
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void bmp_ImageOpened(object sender, RoutedEventArgs e)
    {
      BitmapImage bmp = sender as BitmapImage;

      // 画像倍率調整
      float? rate = ((float)scrView.ActualWidth) / ((float)bmp.PixelWidth);
      scrView.ChangeView(null, null, rate);
    }


    private void backButton_Click(object sender, RoutedEventArgs e)
    {
      this.Frame.GoBack();
    }

    private void deleteButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private async void rotateRight_Click(object sender, RoutedEventArgs e)
    {
      var picker = new Windows.Storage.Pickers.FileOpenPicker();
      picker.FileTypeFilter.Add(".jpg");
      picker.FileTypeFilter.Add(".jpeg");
      picker.FileTypeFilter.Add(".png");
      StorageFile file = await picker.PickSingleFileAsync();

      //      StorageFile file = await StorageFile.GetFileFromPathAsync(document.ImagePath);
      BitmapImage bitmap = new BitmapImage(new Uri(document.ImagePath));

      var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
      var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);

      var memStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
      var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateForTranscodingAsync(memStream, decoder);

      StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
      StorageFile localFile = await localFolder.CreateFileAsync(file.Name);

      encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise90Degrees;
      encoder.BitmapTransform.InterpolationMode = Windows.Graphics.Imaging.BitmapInterpolationMode.Fant;

      try
      {
        await encoder.FlushAsync();
      }
      catch (Exception err)
      {
        switch (err.HResult)
        {
          case unchecked((int)0x88982F81): //WINCODEC_ERR_UNSUPPORTEDOPERATION
            // If the encoder does not support writing a thumbnail, then try again
            // but disable thumbnail generation.
            encoder.IsThumbnailGenerated = false;
            break;
          default:
            throw err;
        }
      }

      // Overwrite the contents of the file with the updated image stream.
      memStream.Seek(0);
      stream.Seek(0);
      stream.Size = 0;
      await RandomAccessStream.CopyAsync(memStream, stream);

      stream.Dispose();
      memStream.Dispose();
    }

    private void rotateLeft_Click(object sender, RoutedEventArgs e)
    {

    }

    private void image1_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
      // 最小値、最大値の計算
      minX = (image.RenderSize.Width * -1) + 100;
      maxX = image.RenderSize.Width - 100;
      minY = (image.RenderSize.Height * -1) + 100;
      maxY = image.RenderSize.Height - 200;

      var tr = (CompositeTransform)image.RenderTransform;
      // まずは平行移動から
      var x = e.Delta.Translation.X;
      var y = e.Delta.Translation.Y;
      // 拡大縮小
      var scale = e.Delta.Scale;

      // 操作点Oから画像基準点Aの相対位置OA
      var dx = tr.TranslateX - e.Position.X;
      var dy = tr.TranslateY - e.Position.Y;
      // 移動距離 = OA * (X - 1)
      x += dx * (scale - 1);
      y += dy * (scale - 1);
      // 回転
      var rotation = e.Delta.Rotation;
      // OAをrotation回転させただけ移動する
      var sin = Math.Sin(rotation * Math.PI / 180);
      var cos = Math.Cos(rotation * Math.PI / 180);
      // 移動距離 = ROT(OA) - OA
      x += (cos * dx - sin * dy) - dx;
      y += (sin * dx + cos * dy) - dy;
      tr.TranslateX += x;
      tr.TranslateY += y;

      double newX = tr.TranslateX;
      if( newX < minX){
        tr.TranslateX = minX;
      }
      else if (newX > maxX)
      {
        tr.TranslateX = maxX;
      }

      double newY = tr.TranslateY;
      if (newY < minY)
      {
        tr.TranslateY = minY;
      }
      else if (newY > maxY)
      {
        tr.TranslateY = maxY;
      }

      tr.ScaleX = tr.ScaleY = tr.ScaleX * scale;
    }

    private void prevImageButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private void nextImageButton_Click(object sender, RoutedEventArgs e)
    {

    }
  }
}
