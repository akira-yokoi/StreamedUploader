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
using System.Collections.ObjectModel;
using SQLite;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace STREAMED
{
  /// <summary>
  /// A basic page that provides characteristics common to most applications.
  /// </summary>
  public sealed partial class DocumentCategoryPage : Page
  {
    private ScanSetting scanSetting;

    private ObservableCollection<Category> categoryCollection = new ObservableCollection<Category>();

    private NavigationHelper navigationHelper;
    private ObservableDictionary defaultViewModel = new ObservableDictionary();

    private DefaultCategory defaultCategory = null;

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


    public DocumentCategoryPage()
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
      scanSetting = (ScanSetting)e.Parameter;
      this.clientNameText.Text = scanSetting.clientName;
      this.documentTypeText.Text = scanSetting.getDocumentTypeName();

      initCategory();
    }

    public async void initCategory()
    {
      DatabaseManager dbManager = DatabaseManager.GetInstance();
      AsyncTableQuery<Category> query = dbManager.getConnection().Table<Category>();
      List<Category> categoryList = await query.Where(x => x.clientId == scanSetting.clientId).ToListAsync();

      String defaultDebitCategoryName = null;
      String defaultCreditCategoryName = null;
      if (scanSetting.documentType == ScanSetting.DOC_TYPE_INVOICE)
      {
        defaultCreditCategoryName = "未払金";
      }
      else if( scanSetting.documentType == ScanSetting.DOC_TYPE_RECEIPT)
      {
        defaultCreditCategoryName = "現金";
      }

      // デフォルト値をDBから取得
      AsyncTableQuery<DefaultCategory> defaultCategoryQuery = dbManager.getConnection().Table<DefaultCategory>();
      List<DefaultCategory> defaultCategoryList = await defaultCategoryQuery.Where(x => x.DocumentType == scanSetting.documentType).ToListAsync();
      if (defaultCategoryList.Count != 0)
      {
        defaultCategory = defaultCategoryList.ElementAt(0);
        defaultDebitCategoryName = defaultCategory.DebitCategoryName;
        defaultCreditCategoryName = defaultCategory.CreditCategoryName;
      }

      Category selectDebitCategory = null;
      Category selectCreditCategory = null;
      foreach (Category category in categoryList)
      {
        categoryCollection.Add(category);
        if (category.name.Equals(defaultDebitCategoryName))
        {
          selectDebitCategory = category;
        }

        if (category.name.Equals(defaultCreditCategoryName))
        {
          selectCreditCategory = category;
        }
      }
      this.debitCategoryCombo.DataContext = categoryCollection;
      this.creditCategoryCombo.DataContext = categoryCollection;

      if (selectDebitCategory != null)
      {
        this.debitCategoryCombo.SelectedItem = selectDebitCategory;
      }
      if (selectCreditCategory != null)
      {
        this.creditCategoryCombo.SelectedItem = selectCreditCategory;
      }
    }

    /*
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
      navigationHelper.OnNavigatedFrom(e);
    }
     */ 

    #endregion

    private void backButton_Click(object sender, RoutedEventArgs e)
    {
      this.Frame.GoBack();
    }

    private void homeButton_Click(object sender, RoutedEventArgs e)
    {
      ViewUtil.goHome(this.Frame);
    }

    private async void scanButton_Click(object sender, RoutedEventArgs e)
    {
      Category debitCategory = (Category)this.debitCategoryCombo.SelectedItem;
      Category creditCategory = (Category)this.creditCategoryCombo.SelectedItem;

      if (debitCategory != null)
      {
        scanSetting.debitCategoryId = debitCategory.id;
        scanSetting.debitCategoryName = debitCategory.name;
      }
      if (creditCategory != null) {
        scanSetting.creditCategoryId = creditCategory.id;
        scanSetting.creditCategoryName = creditCategory.name;
      }

      bool isInsert=  false;
      if (defaultCategory == null)
      {
        defaultCategory = new DefaultCategory();
        isInsert = true;
      }
      defaultCategory.DocumentType = scanSetting.documentType;
      defaultCategory.DebitCategoryName = scanSetting.debitCategoryName;
      defaultCategory.CreditCategoryName = scanSetting.creditCategoryName;


      DatabaseManager dbManager = DatabaseManager.GetInstance();
      if( isInsert){
        await dbManager.getConnection().InsertAsync(defaultCategory);
      }
      else
      {
        await dbManager.getConnection().UpdateAsync(defaultCategory);
      }

      this.Frame.Navigate(typeof(DocumentListPage), scanSetting);
    }

  }
}
