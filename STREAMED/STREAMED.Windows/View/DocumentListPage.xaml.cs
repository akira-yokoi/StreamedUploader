using STREAMED.Common;
using STREAMED.Model;
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

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace STREAMED
{
  /// <summary>
  /// A page that displays a collection of item previews.  In the Split Application this page
  /// is used to display and select one of the available groups.
  /// </summary>
  public sealed partial class DocumentListPage : Page
  {
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

    public DocumentListPage()
    {
      this.InitializeComponent();
      this.navigationHelper = new NavigationHelper(this);
      this.navigationHelper.LoadState += navigationHelper_LoadState;
    }

    /// <summary>
    /// Populates the page with content passed during navigation.  Any saved state is also
    /// provided when recreating a page from a prior session.
    /// </summary>
    /// <param name="sender">
    /// The source of the event; typically <see cref="NavigationHelper"/>
    /// </param>
    /// <param name="e">Event data that provides both the navigation parameter passed to
    /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
    /// a dictionary of state preserved by this page during an earlier
    /// session.  The state will be null the first time a page is visited.</param>
    private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
    {
      loadDocument();
    }

    private async void loadDocument()
    {
      List<Document> documentList = new List<Document>();

      var lfMan = LocalFileManager.SharedManager;
      var files = await lfMan.getImageFiles();
      var folder = await lfMan.getStreamedFolder();

      foreach(var item in files)
      {
        Document document = new Document();
        document.DocumentType = "領収書";
        document.DebitCategory = "未払金";
        document.CreditCategory = "現金";
        document.ImagePath = item.Name;

        var file = await folder.GetFileAsync(item.Name);
        var stream = await file.OpenReadAsync();
        document.BMP = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
        document.BMP.SetSource(stream);

        documentList.Add(document);
      }

      /*
      for (int cnt = 0; cnt < 50; cnt++)
      {
        Document document = new Document();
        document.DocumentType = "領収書";
        document.DebitCategory = "未払金";
        document.CreditCategory = "現金";
        document.ImagePath = "C:\\Users\\朗\\Pictures\\花・自然\\Nature1.jpg";
        document.ImagePath = "ms-appx:///Assets/images.png";
        documentList.Add(document);
      }*/

      this.defaultViewModel["Items"] = documentList;
    }

    #region NavigationHelper registration

    /// The methods provided in this section are simply used to allow
    /// NavigationHelper to respond to the page's navigation methods.
    /// 
    /// Page specific logic should be placed in event handlers for the  
    /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
    /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
    /// The navigation parameter is available in the LoadState method 
    /// in addition to page state preserved during an earlier session.

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      navigationHelper.OnNavigatedTo(e);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
      navigationHelper.OnNavigatedFrom(e);
    }

    #endregion
    private void backButton_Click(object sender, RoutedEventArgs e)
    {
      this.Frame.GoBack();
    }

    private void homeButton_Click(object sender, RoutedEventArgs e)
    {
      ViewUtil.goHome(this.Frame);
    }

    private void uploadButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private void itemGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
      Document document = (Document)e.ClickedItem;
      this.Frame.Navigate(typeof(DocumentDetailPage), document);
    }
  }
}