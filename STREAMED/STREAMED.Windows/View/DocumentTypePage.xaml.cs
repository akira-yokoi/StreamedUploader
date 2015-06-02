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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace STREAMED
{
  /// <summary>
  /// A basic page that provides characteristics common to most applications.
  /// </summary>
  public sealed partial class DocumentTypePage : Page
  {
    private ScanSetting scanSetting;

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


    public DocumentTypePage()
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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      scanSetting = (ScanSetting)e.Parameter;
      this.clientNameText.Text = scanSetting.clientName;
    }

    /*
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
      navigationHelper.OnNavigatedFrom(e);
    }
     */

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

    private void backButton_Click(object sender, RoutedEventArgs e)
    {
      this.Frame.GoBack();
    }

    private void homeButton_Click(object sender, RoutedEventArgs e)
    {
      ViewUtil.goHome(this.Frame);
    }

    private void receiptButton_Click(object sender, RoutedEventArgs e)
    {
      scanSetting.documentType = ScanSetting.DOC_TYPE_RECEIPT;
      goNextPage();
    }

    private void invoiceButton_Click(object sender, RoutedEventArgs e)
    {
      scanSetting.documentType = ScanSetting.DOC_TYPE_INVOICE;
      goNextPage();
    }

    private void bankBookButton_Click(object sender, RoutedEventArgs e)
    {
      scanSetting.documentType = ScanSetting.DOC_TYPE_BANKBOOK;
      goNextPage();
    }

    private void creditCardButton_Click(object sender, RoutedEventArgs e)
    {
      scanSetting.documentType = ScanSetting.DOC_TYPE_CREDITCARD;
      goNextPage();
    }

    private void goNextPage()
    {
      this.Frame.Navigate(typeof(DocumentCategoryPage), scanSetting);
    }
  }
}
