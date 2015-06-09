using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
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
using SQLite;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace STREAMED
{

  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class LoginPage : Page
  {
    public LoginPage()
    {
      this.InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
      this.initTable();

      // メールアドレスが保存されている場合は前回のログイン情報でログイン
      String mailAddress = SettingUtil.getString(SettingUtil.MAILADDRESS_KEY);
      if (!StringUtil.isEmpty(mailAddress))
      {
        Task<bool> loginTask = Task.Run(async () =>
        {
          StreamedRequest request = new StreamedRequest();
          String response = await request.login();
          Dictionary<string, object> responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

          bool success = false;
          int resultCode = Int16.Parse(responseObject["result_code"] as String);
          switch (resultCode)
          {
            case 0:
              // 処理中枚数を最新にするためにクライアントだけ更新
              bool ret = await DatabaseManager.GetInstance().syncClient();
              if (ret)
              {
                // ログインに成功したら値を保存しておく
                saveLoginInfo(responseObject);
                success = true;
              }
              break;
          }
          return success;
        });
        Task.WaitAny(new Task[] { loginTask });
        if (loginTask.Result)
        {
          // メインメニューへ
          this.Frame.Navigate(typeof(MainMenuPage));
        }
      }
      loginButton.Focus(FocusState.Keyboard);
    }

    private void loginButton_Tapped(object sender, TappedRoutedEventArgs e)
    {
      if (isValid())
      {
        login();
      }
    }

    private bool isValid()
    {
      string mailAddres = mailAddressText.Text;
      string password = passwordText.Password;

      if (StringUtil.isEmpty(mailAddres))
      {
        errorText.Text = "メールアドレスを入力してください";
        return false;
      }
      if (StringUtil.isEmpty(password))
      {
        errorText.Text = "パスワードを入力してください";
        return false;
      }
      errorText.Text = "";
      return true;
    }

    private async void login()
    {
      this.progressRing.IsActive = true;
      this.loginButton.IsEnabled = false;

      try
      {
        String result = loginProcess();
        // Success
        if (result == null)
        {
          // 必要なマスタデータを同期
          var manager = DatabaseManager.GetInstance();
          await manager.syncAll((ret) =>
          {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
              // メインメニューへ
              this.Frame.Navigate(typeof(MainMenuPage));
            });
          });
        }
        // Fail
        else
        {
          errorText.Text = result;
        }
      }
      finally
      {
        this.progressRing.IsActive = false;
        this.loginButton.IsEnabled = true;
      }
    }

    public String loginProcess()
    {
      String testPrefix = "testserver@";
      String sgPrefix = "sg@";
      String stagingPrefix = "staging@";

      int serverID = 0;

      String email = mailAddressText.Text;
      String password = passwordText.Password;
      if (email.StartsWith(stagingPrefix))
      {
        email = email.Replace(stagingPrefix, "");
        serverID = StreamedRequest.STG_SERVER;
      }
      else if (email.StartsWith(testPrefix))
      {
        email = email.Replace(testPrefix, "");
        serverID = StreamedRequest.TESTSERVER;
      }
      else if (email.StartsWith(sgPrefix))
      {
        email = email.Replace(sgPrefix, "");
        serverID = StreamedRequest.SG_SERVER;
      }
      else
      {
        serverID = StreamedRequest.PRO_SERVER;
      }

      // サーバの情報を保存
      Task<String> loginTask = Task.Run(async () =>
      {
        StreamedRequest request = new StreamedRequest();
        String response = await request.joinUserProfile(email, email, password, serverID);
        Dictionary<string, object> responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

        int resultCode = -1;
        if (responseObject != null && responseObject.ContainsKey("result_code"))
        {
          resultCode = Int16.Parse(responseObject["result_code"] as String);
        }
        String errorMessage = null;
        switch (resultCode)
        {
          case 0:
            // ログインに成功したら値を保存しておく
            SettingUtil.putString(SettingUtil.LOGIN_ID_KEY, email);
            SettingUtil.putString(SettingUtil.MAILADDRESS_KEY, email);
            SettingUtil.putString(SettingUtil.PASSWORD_KEY, password);
            saveLoginInfo(responseObject);

            // プランのチェック
            errorMessage = await checkPlan();
            if (errorMessage != null)
            {
              // プランチェックに引っかかったらログイン情報を削除
              MainMenuPage.resetLoginInfo();
            }
            break;
          case 9003:
            errorMessage = "現在のプランではご利用になれません";
            break;
          case 2002:
            errorMessage = "有効なメールアドレスではありません";
            break;
          default:
            errorMessage = "メールとパスワードが一致しません";
            break;
        }

        return errorMessage;
      });

      Task.WaitAny(new Task[] { loginTask });
      String result = loginTask.Result;
      return result;
    }


    private void saveLoginInfo(Dictionary<string, object> responseObject)
    {
      if (responseObject.ContainsKey("duniq_id"))
      {
        String uniqueId = responseObject["duniq_id"] as String;
        SettingUtil.putString(SettingUtil.UNIQUE_ID_KEY, uniqueId);
      }

      if (responseObject.ContainsKey("latest_version"))
      {
        String latestVersion = responseObject["latest_version"] as String;
        SettingUtil.putString(SettingUtil.LATEST_VERSION_KEY, latestVersion);
      }
    }

    private async Task<String> checkPlan()
    {
      String errorMessage = null;
      StreamedRequest request = new StreamedRequest();
      String response = await request.getUserPlan();
      Dictionary<string, object> responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
      int resultCode = Int16.Parse(responseObject["result_code"] as String);
      switch (resultCode)
      {
        case 0:
          var data = responseObject["data"];
          Dictionary<string, object> dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
          if (!dataDict.ContainsKey(StreamedRequest.PLAN_ID_ACCOUNTANT.ToString()))
          {
            errorMessage = "現在のプランではご利用になれません";
          }
          break;
        default:
          errorMessage = "プランの取得に失敗しました";
          break;
      }
      return errorMessage;
    }

    private void initTable()
    {
      DatabaseManager dbManager = DatabaseManager.GetInstance();

      SQLiteAsyncConnection connection = dbManager.getConnection();
      var existingTables = connection.QueryAsync<SqliteMaster>("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;")
.GetAwaiter()
.GetResult();

      if (existingTables.Any(x => x.name == "Client") != true)
      {
        connection.CreateTableAsync<Client>().GetAwaiter().GetResult();
      }

      if (existingTables.Any(x => x.name == "Category") != true)
      {
        connection.CreateTableAsync<Category>().GetAwaiter().GetResult();
      }

      if (existingTables.Any(x => x.name == "Document") != true)
      {
        connection.CreateTableAsync<Document>().GetAwaiter().GetResult();
      }

      if (existingTables.Any(x => x.name == "DefaultCategory") != true)
      {
        connection.CreateTableAsync<DefaultCategory>().GetAwaiter().GetResult();
      }
    }
  }
}