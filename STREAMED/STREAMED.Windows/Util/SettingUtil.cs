using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows;
using Windows.Storage;

namespace STREAMED
{
  public class SettingUtil
  {
    public const string SERVER_SETTING_KEY = "SERVER";
    public const string LAST_TICKET_SETTING_KEY = "LAST_TICKET";
    public const string CURRENT_CLIENT_ID_SETTING_KEY = "CURRENT_CLIENT_ID";
    public const string UNIQUE_ID_KEY = "UNIQUE_ID";
    public const string LOGIN_ID_KEY = "LOGIN_ID";
    public const string MAILADDRESS_KEY = "MAILADDRESS";
    public const string PASSWORD_KEY = "PASSWORD";
    public const string REQUEST_ID_KEY = "REQUEST_ID";
    public const string LATEST_VERSION_KEY = "LATEST_VERSION";

    public static object get( string key)
    {
      ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
      return container.Values[key];
    }

    public static string getString(string key, string defaultValue)
    {
      ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
      object value = container.Values[key];
      if (value != null)
      {
        return value.ToString();
      }
      return defaultValue;
    }

    public static string getString(string key){
      return getString(key, null);
    }

    public static void putString(string key, string value)
    {
      ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
      container.Values[key] = value;
    }

    public static void removeValue(string key)
    {
      ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
      container.Values.Remove(key);
    }
  }
}
