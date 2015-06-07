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
using Newtonsoft.Json;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace STREAMED
{
  /// <summary>
  /// A basic page that provides characteristics common to most applications.
  /// </summary>
  public sealed partial class MainMenuPage : Page
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


    public MainMenuPage()
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

    private async void logoutButton_Clicked(object sender, RoutedEventArgs e)
    {
      StreamedRequest request = new StreamedRequest();
      await request.logout();

      this.Frame.GoBack();

      // 結果に関わらず保存している状態を削除する
      resetLoginInfo();
      DatabaseManager.GetInstance().deleteAllTable();
    }

    public static void resetLoginInfo()
    {
      SettingUtil.removeValue(SettingUtil.LOGIN_ID_KEY);
      SettingUtil.removeValue(SettingUtil.MAILADDRESS_KEY);
      SettingUtil.removeValue(SettingUtil.PASSWORD_KEY);
    }

    private void scanButton_Click(object sender, RoutedEventArgs e)
    {
      // 顧客選択へ
      this.Frame.Navigate(typeof(ClientListPage));
    }

    private void uploadButton_Click(object sender, RoutedEventArgs e)
    {
      this.Frame.Navigate(typeof(DocumentListPage));
    }

    private void sycButton_Click(object sender, RoutedEventArgs e)
    {
      progressRing.IsActive = true;
      try
      {
        // 必要なマスタデータを同期
        DatabaseManager.GetInstance().syncAll();
        ViewUtil.showMesssage("同期しました", "顧客と勘定科目の情報が最新に更新されました");
      }
      finally
      {
        progressRing.IsActive = false;
      }
    }

    private void settingButton_Click(object sender, RoutedEventArgs e)
    {
      this.Frame.Navigate(typeof(SettingPage));
    }

    private void helpButton_Click(object sender, RoutedEventArgs e)
    {
      this.Frame.Navigate(typeof(WebViewPage), "ヘルプ,https://streamed.zendesk.com/hc/ja");
    }

    private void streamedWebButton_Click(object sender, RoutedEventArgs e)
    {
      this.Frame.Navigate(typeof(WebViewPage), "STREAMED Web,https://ssl.streamedup.com/webclient/join");
    }

    private void pageRoot_Loaded(object sender, RoutedEventArgs e)
    {
      uploadSummary();
    }

    private async void uploadSummary()
    {
      DatabaseManager manager = DatabaseManager.GetInstance();
      List<DocumentSummary> summaryList = await manager.getConnection().QueryAsync<DocumentSummary>("select sum(nReceiptsThisMonth) as numberOfThisMonth, sum(nReceiptsWait) as numberOfWait, sum(nReceiptsThisMonth) -sum(nReceiptsWait) as numberOfProcessed from Client");

      if (summaryList.Count != 0)
      {
        DocumentSummary summary = summaryList.ElementAt(0);
        numberOfProcessText.Text = String.Format( "{0:#,0}枚", summary.numberOfThisMonth);
        numberOfWaitText.Text = String.Format("{0:#,0}枚", summary.numberOfWait);
      }
    }

    private void testButton_Click(object sender, RoutedEventArgs e)
    {
      EpsonScanner epsonScan = new EpsonScanner();
      epsonScan.scan();
    }
  }
}
