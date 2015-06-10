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
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace STREAMED
{
  /// <summary>
  /// A basic page that provides characteristics common to most applications.
  /// </summary>
  public sealed partial class DocumentDetailPage : Page
  {
    enum RotateType
    {
      RotateLeft,RotateRight
    };

    private Document document;
    private List<Document> documentList;
    private int pageIndex;

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
      var model = (DocumentPreviewModel)e.Parameter;

      document = model.document;
      pageIndex = model.currentIndex;
      documentList = model.listItems;

      await loadDocument();
    }

    private async Task loadDocument()
    {
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
      float rate1 = ((float)scrView.ActualWidth) / ((float)bmp.PixelWidth);
      float rate2 = ((float)scrView.ActualHeight) / ((float)bmp.PixelHeight);
      float? rate = Math.Min(rate1, rate2);
      scrView.ChangeView(null, null, rate);

      prevImageButton.IsEnabled = pageIndex > 0;
      nextImageButton.IsEnabled = pageIndex < (documentList.Count-1);
    }


    private void backButton_Click(object sender, RoutedEventArgs e)
    {
      this.Frame.GoBack();
    }

    private async void deleteButton_Click(object sender, RoutedEventArgs e)
    {
      LocalFileManager lfMan = LocalFileManager.SharedManager;
      bool ret = await lfMan.deleteImageFile(document.ImagePath);
      if(ret)
      {
        this.Frame.GoBack();
      }
    }

    private async void rotateRight_Click(object sender, RoutedEventArgs e)
    {
      await rotateImage(RotateType.RotateRight);
    }

    private async void rotateLeft_Click(object sender, RoutedEventArgs e)
    {
      await rotateImage(RotateType.RotateLeft);
    }

    #region private async Task rotateImage() : 画像回転
    /// <summary>
    /// 画像回転
    /// </summary>
    /// <param name="rtype"></param>
    /// <returns></returns>
    private async Task rotateImage(RotateType rtype)
    {
      var filename = document.ImagePath;
      LocalFileManager lfMan = LocalFileManager.SharedManager;
      var file = await lfMan.getStorageFileByFilename(filename);
      if (file == null)
        return;

      var data = await FileIO.ReadBufferAsync(file);

      var ms = new InMemoryRandomAccessStream();
      var dw = new DataWriter(ms);
      dw.WriteBuffer(data);
      await dw.StoreAsync();
      ms.Seek(0);

      var bm = new BitmapImage();
      await bm.SetSourceAsync(ms);

      var wb = new WriteableBitmap(bm.PixelWidth, bm.PixelHeight);
      ms.Seek(0);

      await wb.SetSourceAsync(ms);
      var wb2 = wb.Rotate(rtype == RotateType.RotateLeft ? 270 : 90);

      using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
      {
        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(
            BitmapEncoder.PngEncoderId, stream);
        Stream pixelStream = wb2.PixelBuffer.AsStream();
        byte[] pixels = new byte[pixelStream.Length];
        await pixelStream.ReadAsync(pixels, 0, pixels.Length);

        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
            (uint)wb2.PixelWidth, (uint)wb2.PixelHeight, 96.0, 96.0, pixels);
        await encoder.FlushAsync();
      }

      await loadDocument();
    }
    #endregion

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

    private async void prevImageButton_Click(object sender, RoutedEventArgs e)
    {
      if(pageIndex > 0)
      {
        pageIndex--;
        document = documentList[pageIndex];
        await loadDocument();
      }
    }

    private async void nextImageButton_Click(object sender, RoutedEventArgs e)
    {
      if(pageIndex < (documentList.Count - 1))
      {
        pageIndex++;
        document = documentList[pageIndex];
        await loadDocument();
      }
    }
  }
}
