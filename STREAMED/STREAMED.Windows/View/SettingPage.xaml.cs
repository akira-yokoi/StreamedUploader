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
  public sealed partial class SettingPage : Page
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


    public SettingPage()
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
      String scannerType = SettingUtil.getString(SettingUtil.SCANNER_KEY, SettingUtil.SCANNER_TYPE_IX100);
      if (StringUtil.Equals(scannerType, SettingUtil.SCANNER_TYPE_IX100))
      {
        ix100Button.IsChecked = true;
      }
      else if (StringUtil.Equals(scannerType, SettingUtil.SCANNER_TYPE_IX500))
      {
        ix500Button.IsChecked = true;
      }
      else if (StringUtil.Equals(scannerType, SettingUtil.SCANNER_TYPE_DS510))
      {
        ds510Button.IsChecked = true;
      }
      else if (StringUtil.Equals(scannerType, SettingUtil.SCANNER_TYPE_DS560))
      {
        ds560Button.IsChecked = true;
      }
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

    private void scanerButton_Clicked(object sender, RoutedEventArgs e)
    {
      if (sender == this.ix100Button)
      {
        SettingUtil.putString( SettingUtil.SCANNER_KEY, SettingUtil.SCANNER_TYPE_IX100);
        ix500Button.IsChecked = false;
        ds510Button.IsChecked = false;
        ds560Button.IsChecked = false;
      }
      else if (sender == this.ix500Button)
      {
        SettingUtil.putString(SettingUtil.SCANNER_KEY, SettingUtil.SCANNER_TYPE_IX500); 
        ix100Button.IsChecked = false;
        ds510Button.IsChecked = false;
        ds560Button.IsChecked = false;
      }
      else if (sender == this.ds510Button)
      {
        SettingUtil.putString(SettingUtil.SCANNER_KEY, SettingUtil.SCANNER_TYPE_DS510);
        ix100Button.IsChecked = false;
        ix500Button.IsChecked = false;
        ds560Button.IsChecked = false;
      }
      else if (sender == this.ds560Button)
      {
        SettingUtil.putString(SettingUtil.SCANNER_KEY, SettingUtil.SCANNER_TYPE_DS560);
        ix100Button.IsChecked = false;
        ix500Button.IsChecked = false;
        ds510Button.IsChecked = false;
      }
    }
  }
}
