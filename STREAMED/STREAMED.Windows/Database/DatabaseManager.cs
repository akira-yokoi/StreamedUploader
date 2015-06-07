using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using Newtonsoft.Json;

namespace STREAMED
{
  class DatabaseManager
  {
    private static DatabaseManager instance = new DatabaseManager();

    private SQLiteAsyncConnection connection;

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
      connection = new SQLiteAsyncConnection("streamed");
      await connection.CreateTableAsync<Category>();
      await connection.CreateTableAsync<Client>();
    }

    public SQLiteAsyncConnection getConnection()
    {
      return connection;
    }

    public async void deleteAllTable()
    {
      await connection.DropTableAsync<Category>();
      await connection.DropTableAsync<Client>();
    }

    public void syncAll()
    {
      Log.debug("syncAll");
      syncClient( true);
      Log.debug("syncAll");
    }

    public async void syncClient( bool withCategory)
    {
      StreamedRequest request = new StreamedRequest();
      String response = await request.getClientList();
      Dictionary<string, object> responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
      int resultCode = Int16.Parse(responseObject["result_code"] as String);
      switch (resultCode)
      {
        case 0:
          // 成功した場合はクライアントを取り出す
          var data = responseObject["data"];
          Dictionary<string, object> dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
          var clients = dataDict["clients"];

          Dictionary<string, object> clientDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(clients.ToString());
          List<object> valsList = new List<object>(clientDict.Values);

          List<Client> clientList = new List<Client>();
          foreach (object val in valsList)
          {
            Client client = JsonConvert.DeserializeObject<Client>(val.ToString());
            clientList.Add(client);
          }

          // クライアントテーブルへの書き込み
          try
          {
            await connection.DropTableAsync<Client>();
            await connection.DropTableAsync<DefaultCategory>();
          }
          catch (SQLite.SQLiteException ex)
          {
          }
          await connection.CreateTableAsync<Client>();
          if (clientList.Count != 0)
          {
            await connection.InsertAllAsync(clientList);
          }
          Log.debug("ClientSynced");

          // カテゴリテーブルへの書き込み
          if (withCategory)
          {
            try
            {
              await connection.DropTableAsync<Category>();
            }
            catch (SQLite.SQLiteException ex)
            {
            }
            await connection.CreateTableAsync<Category>();
            foreach (Client client in clientList)
            {
              syncCategory(client.id);
            }
          }
          break;
      }
    }

    public async void syncCategory( String clientId)
    {
      StreamedRequest request = new StreamedRequest();
      String response = await request.getCategoryList( clientId);
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
            await connection.InsertAllAsync(categoryList);
          }
          break;
      }
    }
  }
}
