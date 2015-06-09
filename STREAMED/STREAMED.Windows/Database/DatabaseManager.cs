using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using Newtonsoft.Json;
using System.Diagnostics;

namespace STREAMED
{
  class DatabaseManager
  {
    private static DatabaseManager instance = new DatabaseManager();

    private SQLiteAsyncConnection _connection;

    public static DatabaseManager GetInstance()
    {
      return instance;
    }

    private DatabaseManager()
    {
      initDatabase();
    }

    private async void initDatabase()
    {
      var con = getConnection();
      await con.CreateTableAsync<Category>();
      await con.CreateTableAsync<Client>();
    }

    public SQLiteAsyncConnection getConnection()
    {
      if (_connection == null)
      {
        _connection = new SQLiteAsyncConnection("streamed");
      }
      return _connection;
    }

    public async void deleteAllTable()
    {
      var con = getConnection();
      await con.DropTableAsync<Category>();
      await con.DropTableAsync<Client>();
      await con.DropTableAsync<DefaultCategory>();
      await con.DropTableAsync<Document>();
    }

    public async Task syncAll(Action<bool> completed)
    {
      bool ret = false;
      var con = getConnection();
      await con.RunInTransactionAsync(async (SQLiteAsyncConnection con2) =>
      {
        try
        {
          var clientList = await _syncClient(con2);

          // カテゴリテーブルへの書き込み
          await con2.DropTableAsync<Category>();
          await con2.CreateTableAsync<Category>();
          foreach (Client client in clientList)
          {
            await _syncCategory(client.id, con2);
          }
          ret = true;
        }
        catch
        {

        }

        completed(ret);
      });
    }

    public async Task<bool> syncClient()
    {
      bool ret = false;
      try
      {
        var con = getConnection();
        await _syncClient(con);
        ret = true;
      }
      catch (SQLiteException ex)
      {
        Debug.WriteLine("syncClient:Exception:" + ex.Message);
      }
      return ret;
    }

    public async Task<bool> syncCategory(String clientId)
    {
      bool ret = false;
      try
      {
        var con = getConnection();
        await _syncCategory(clientId, con);
        ret = true;
      }
      catch (SQLiteException ex)
      {
        Debug.WriteLine("syncCategory:Exception:" + ex.Message);
      }
      return ret;
    }


    private async Task<List<Client>> _syncClient(SQLiteAsyncConnection con)
    {
      List<Client> clientList = null;
      StreamedRequest request = new StreamedRequest();
      String response = await request.getClientList();
      Dictionary<string, object> responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
      int resultCode = Int16.Parse(responseObject["result_code"] as String);
      switch (resultCode)
      {
        case 0:
          try
          {
            // 成功した場合はクライアントを取り出す
            var data = responseObject["data"];
            Dictionary<string, object> dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
            var clients = dataDict["clients"];

            Dictionary<string, object> clientDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(clients.ToString());
            List<object> valsList = new List<object>(clientDict.Values);

            clientList = new List<Client>();
            foreach (object val in valsList)
            {
              Client client = JsonConvert.DeserializeObject<Client>(val.ToString());
              clientList.Add(client);
            }

            // クライアントテーブルへの書き込み
            await con.DropTableAsync<Client>();
            await con.CreateTableAsync<Client>();
            if (clientList.Count != 0)
            {
              await con.InsertAllAsync(clientList);
            }
            Log.debug("ClientSynced");
          }
          catch(SQLiteException ex)
          {
            throw ex;
          }
          break;
      }
      return clientList;
    }

    private async Task _syncCategory(String clientId, SQLiteAsyncConnection con)
    {
      StreamedRequest request = new StreamedRequest();
      String response = await request.getCategoryList(clientId);
      Dictionary<string, object> responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
      int resultCode = Int16.Parse(responseObject["result_code"] as String);
      switch (resultCode)
      {
        case 0:
          // 成功した場合は費目を取り出す
          var data = responseObject["data"];
          List<object> categoryValues = JsonConvert.DeserializeObject<List<object>>(data.ToString());

          List<Category> categoryList = new List<Category>();
          foreach (object val in categoryValues)
          {
            Category category = JsonConvert.DeserializeObject<Category>(val.ToString());
            category.clientId = clientId;
            categoryList.Add(category);
          }

          if (categoryList.Count != 0)
          {
            await con.InsertAllAsync(categoryList);
          }
          break;

        default:

          throw new Exception("カテゴリリスト取得失敗");
      }
    }
  }
}
