using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Newtonsoft.Json;

namespace STREAMED
{
  public class StreamedRequest
  {
    public const string TEST_SERVER = "http://streamed.startup-plus.jp/api/";
    //const string TEST_SERVER = "https://ssl.streamedup.com/api/";
    public const string PRODUCTION_SERVER = "https://ssl.streamedup.com/api/";
    public const string STAGING_SERVER = "http://streamedup-stg-env-k2hsvn6uup.elasticbeanstalk.com/";
    public const string SINGAPORE_SERVER = "http://streamed-sg.startup-plus.jp/api/";

    public const int TESTSERVER = 1;
    public const int PRO_SERVER = 2;
    public const int SG_SERVER = 3;
    public const int STG_SERVER = 4;

    public const int PLAN_ID_ACCOUNTANT = 6;

    private String getUrl(String apiName)
    {
      String server = SettingUtil.getString(SettingUtil.SERVER_SETTING_KEY, PRODUCTION_SERVER);
      return server + apiName;
    }

    private String createParam(String key, String value)
    {
      StringBuilder jsonBuilder = new StringBuilder();
      jsonBuilder.Append( "\"" + key + "\":\"" + value + "\"");
      return jsonBuilder.ToString();
    }

    private String createCommonRequest()
    {
      return createCommonRequest(null);
    }

    private String createCommonRequest(String clientId)
    {
      String lastTicket = SettingUtil.getString(SettingUtil.LAST_TICKET_SETTING_KEY, null);
      String uniqueId = SettingUtil.getString(SettingUtil.UNIQUE_ID_KEY, null);

      StringBuilder jsonBuilder = new StringBuilder();
      jsonBuilder.Append("{");
      jsonBuilder.Append(createParam("app_version", getAppVersion()));
      jsonBuilder.Append("," + createParam("last_ticket", lastTicket));
      if (clientId != null) {
        jsonBuilder.Append("," + createParam("client_id", clientId));
      }
      jsonBuilder.Append("," + createParam("duniq_id", uniqueId));
      jsonBuilder.Append("}");
      return jsonBuilder.ToString();
    }

    public String getCategoryList( String clientId)
    {
      String url = getUrl( "get_all_category.json");
      String data = createCommonRequest( clientId);
      return requestServer(url, data);
    }

    public String getUserPlan()
    {
      String url = getUrl("get_user_plan.json");
      String data = createCommonRequest();
      return requestServer(url, data);
    }

    public String getClientList()
    {
      String url = getUrl("get_client.json");
      String data = createCommonRequest();
      return requestServer(url, data);
    }

    public String getIndustryList()
    {
      String url = getUrl("get_all_user_industries.json");
      String data = createCommonRequest();
      return requestServer(url, data);
    }

    public String getAppVersion()
    {
      Windows.ApplicationModel.PackageVersion version = Windows.ApplicationModel.Package.Current.Id.Version;
      return version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
    }

    public String getDeviceId(){
      return "ASAFDASFASDFASDASSS";
    }

    public String joinUserProfile(string loginId, string mail, String pass, int serverID)
    {
      /*prepare the URL*/

      String server = null;
      switch (serverID)
      {
        case TESTSERVER:
          server = TEST_SERVER;
          break;
        case PRO_SERVER:
          server = PRODUCTION_SERVER;
          break;
        case SG_SERVER:
          server = SINGAPORE_SERVER;
          break;
        case STG_SERVER:
          server = STAGING_SERVER;
          break;
      }
      SettingUtil.putString(SettingUtil.SERVER_SETTING_KEY, server);

      EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();

      String loginUrl = getUrl( "join_user_profile.json");

      StringBuilder dataBuilder = new StringBuilder();
      dataBuilder.Append("{");
      dataBuilder.Append(createParam("app_version", getAppVersion()));
      dataBuilder.Append( "," + createParam("login_id", loginId));
      dataBuilder.Append("," + createParam("mailadrs", mail));
      dataBuilder.Append("," + createParam("password", pass));
      dataBuilder.Append("," + createParam("device_id", getDeviceId()));
      dataBuilder.Append("," + createParam("device_model", deviceInfo.SystemProductName));
      dataBuilder.Append("," + createParam("user_type", "1"));
      dataBuilder.Append("," + createParam("device_os_version", deviceInfo.OperatingSystem));
      dataBuilder.Append("," + createParam("device_token", getDeviceId()));
      dataBuilder.Append("," + createParam("device_os_type", "Windows"));
      dataBuilder.Append("}");

      return requestServer(loginUrl, dataBuilder.ToString());
    }
    public String login()
    {
      String uniqueId = SettingUtil.getString(SettingUtil.UNIQUE_ID_KEY, null);
      String loginId = SettingUtil.getString(SettingUtil.LOGIN_ID_KEY, null);
      String mailAddress = SettingUtil.getString(SettingUtil.MAILADDRESS_KEY, null);
      String password = SettingUtil.getString(SettingUtil.PASSWORD_KEY, null);

      /*prepare the URL*/
      String loginUrl = getUrl("login.json");
      StringBuilder dataBuilder = new StringBuilder();
      dataBuilder.Append("{");
      dataBuilder.Append(createParam("app_version", getAppVersion()));
      dataBuilder.Append("," + createParam("login_id", loginId));
      dataBuilder.Append("," + createParam("mailadrs", mailAddress));
      dataBuilder.Append("," + createParam("password", password));
      dataBuilder.Append("," + createParam("duniq_id", uniqueId));
      dataBuilder.Append("}");
      return requestServer(loginUrl, dataBuilder.ToString());
    }

    public String logout()
    {
      String logoutUrl = getUrl("logout.json");
      return requestServer(logoutUrl, createCommonRequest());

    }
    public String uploadFile(string pathOfFile, string comment, string categoryId,
        string creditCategoryId, string creditUserCategory, string userCategory, string clientId, int receiptType)
    {
      String uploadUrl = getUrl("logout.json");

      /*request ID*/
      string requestID = SettingUtil.getString( SettingUtil.REQUEST_ID_KEY, null);
      if ( StringUtil.isEmpty( requestID))
      {
        requestID = "request_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        SettingUtil.putString(SettingUtil.REQUEST_ID_KEY, requestID);
      }

      string fileName = Path.GetFileName(pathOfFile);
      Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

      /*
      Image img = Image.FromFile(pathOfFile);
      string base64Img = Utility.ImageToBase64(img, ImageFormat.Jpeg);
      string subData = "[{" +
          "\"request_id\": " + "\"" + requestID + "\"," +
          "\"category_id\": " + "\"" + categoryId + "\"," +
          "\"credit_category_id\": " + "\"" + creditCategoryId + "\"," +
          "\"memo\": " + "\"" + comment + "\"," +
          "\"uc\": " + "\"" + userCategory + "\"," +
          "\"ucc\": " + "\"" + creditUserCategory + "\"," +
          "\"receipt_ticket_status\": 0," +
          "\"entry_time\": " + unixTimestamp + "," +
          "\"update_flg\": 0," +
          "\"receipt_file_name\": " + "\"" + fileName + "\"," +
          "\"receipt_type\": " + "\"" + receiptType + "\"," +
          "\"image_data\": " + "\"" + base64Img + "\"" + "}]";

      bool isAccountant = Properties.Settings.Default.isAccountant;

      String jsonData = "";

      Console.WriteLine("ClientId:" + clientId + "\n");
      if (isAccountant && clientId.Length > 1)
      {
        jsonData = "{\"app_version\":\"1.0\"," +
           "\"last_ticket\":\"" + Properties.Settings.Default.lastTicket + "\"," +
           "\"client_id\":\"" + clientId + "\"," +
           "\"data\":" + subData + "," +
         "\"duniq_id\":\"" + Properties.Settings.Default.uniqueId + "\"}";
        //Console.WriteLine(jsonData+"\n");
        return requestServer(uploadUrl, jsonData);
      }
      else if (!isAccountant)
      {
        jsonData = "{\"app_version\":\"1.0\"," +
               "\"last_ticket\":\"" + Properties.Settings.Default.lastTicket + "\"," +
               "\"data\":" + subData + "," +
             "\"duniq_id\":\"" + Properties.Settings.Default.uniqueId + "\"}";
        return requestServer(uploadUrl, jsonData);
      }
      else
      {
        return requestServer(null, null);
      }
       */
      return null;
    }


    public Stream getStream(HttpWebRequest wr)
    {

      return null;
    }

    private String requestServer(string url, string jsonData)
    {
      Debug.WriteLine(url);
      //Console.WriteLine(jsonData.ToString());

      //Debugger.Log(1,"Streamed",string.Format("Uploading {0} to {1}", file, url));
      string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
      Encoding ascii = System.Text.Encoding.GetEncoding("ASCII");
      byte[] boundarybytes = ascii.GetBytes("\r\n--" + boundary + "\r\n");
      String twoHyphens = "--";
      String lineEnd = "\r\n";

      WebRequest wr = (HttpWebRequest)WebRequest.Create(url);
      // タイムアウトプロパティが無い
      //      wr.Timeout = 10000;//10 seconds
      wr.ContentType = "multipart/form-data; boundary=" + boundary;
      wr.Method = "POST";
      // KeepAliveが無い
      //      wr.KeepAlive = true;
      wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

      Task<Stream> streamTask = wr.GetRequestStreamAsync();
        Task.WaitAny(new Task[] { streamTask });
        using (Stream rs = streamTask.Result)
        {
          byte[] hyphenBoundaryLineEndBytes = ascii.GetBytes(twoHyphens + boundary + lineEnd);
          rs.Write(hyphenBoundaryLineEndBytes, 0, hyphenBoundaryLineEndBytes.Length);
          string formData = "Content-Disposition: form-data; name=data" + lineEnd + lineEnd;
          byte[] formDataBytes = ascii.GetBytes(formData);
          rs.Write(formDataBytes, 0, formDataBytes.Length);

          byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
          rs.Write(jsonBytes, 0, jsonBytes.Length);

          byte[] lineEndBytes = System.Text.Encoding.UTF8.GetBytes(lineEnd);
          rs.Write(lineEndBytes, 0, lineEndBytes.Length);
          rs.Write(hyphenBoundaryLineEndBytes, 0, hyphenBoundaryLineEndBytes.Length);
        }

        Task<WebResponse> responseTask = wr.GetResponseAsync();
        Task.WaitAny(new Task[] { responseTask });
        WebResponse response = responseTask.Result;
        using (Stream responseStream = response.GetResponseStream())
        {
          StreamReader reader2 = new StreamReader(responseStream);
          String responseStr = reader2.ReadToEnd();

          Dictionary<string, object> responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseStr);

          if (responseObject.ContainsKey("last_ticket"))
          {
            String lastTicket = responseObject["last_ticket"] as String;
            SettingUtil.putString(SettingUtil.LAST_TICKET_SETTING_KEY, lastTicket);
          }

          int resultCode = Int16.Parse(responseObject["result_code"] as String);

          Debug.WriteLine(responseStr);
          return responseStr;
        }
        wr = null;
        response = null;
      }
  }
}
