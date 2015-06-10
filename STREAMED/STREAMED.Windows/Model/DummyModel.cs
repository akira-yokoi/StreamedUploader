using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace STREAMED.Model
{
  class DummyModel
  {
    public static async Task TruncateDocumentTable()
    {
      DatabaseManager dman = DatabaseManager.GetInstance();
      var con = dman.getConnection();
      await con.ExecuteAsync("delete from Document");
    }

    public static async Task DeleteAllPictures()
    {
      LocalFileManager lfman = LocalFileManager.SharedManager;
      var folder = await lfman.getStreamedFolder();

      var lst = await folder.GetFilesAsync();
      foreach(var file in lst)
      {
        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
      }
    }

    public static async Task CreateDocumentData()
    {
      DatabaseManager dman = DatabaseManager.GetInstance();
      LocalFileManager lfman = LocalFileManager.SharedManager;
      var con = dman.getConnection();
      List<Client> clientList = await con.Table<Client>().ToListAsync();

      int cidx = 0;
      foreach(var client in clientList)
      {
        AsyncTableQuery<Category> query = con.Table<Category>();
        List<Category> categoryList = await query.Where(x => x.clientId == client.id).ToListAsync();

        var folder = await lfman.getStreamedFolder();

        for (var idx = 1; idx <= ((cidx+1)*5); idx++)
        {
          string fn = string.Format("img_{0}_{1:00}.png", client.id, idx);
          var file = await folder.TryGetItemAsync(fn) as IStorageFile;
          if (file == null)
          {
            StorageFile wfile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/images.png"));

            await wfile.CopyAsync(folder, fn);
          }

          Document doc = new Document();
          doc.ClientId = client.id;
          doc.CreditCategoryId = categoryList[cidx+1].id;
          doc.CreditCategoryName = categoryList[cidx+1].name;
          doc.DebitCategoryId = categoryList[cidx+1].id;
          doc.DebitCategoryName = categoryList[cidx+1].name;
          doc.DocumentType = ScanSetting.DOC_TYPE_RECEIPT;
          doc.ImagePath = fn;
          await con.InsertAsync(doc);
        }
        cidx++;
      }

      var lst = await con.Table<Document>().ToListAsync();
      int cnt = lst.Count;
    }
  }
}
