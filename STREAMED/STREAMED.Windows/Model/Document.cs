using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace STREAMED
{
  /**
   * スキャンしたドキュメント情報を保持するクラス
   */
  class Document
  {
    public static int STATUS_WAIT_UPLOAD = 1;
    public static int STATUS_UPLOADED = 3;
    public static int STATUS_UPLOAD_ERROR = 2;

    [SQLite.AutoIncrement, SQLite.PrimaryKey, SQLite.Indexed]
    public int id { get; set; }
    public int DocumentType { get; set; }
    public String ClientId { get; set; }
    public String DebitCategoryId { get; set; }
    public String DebitCategoryName { get; set; }
    public String DebitUserCategory { get; set; }
    public String CreditCategoryId { get; set; }
    public String CreditCategoryName { get; set; }
    public String CreditUserCategory { get; set; }
    public String ImagePath { get; set; }

    public int Status { get; set; }


    [SQLite.Ignore]
    public BitmapImage BMP { get; set; }
  }
}
