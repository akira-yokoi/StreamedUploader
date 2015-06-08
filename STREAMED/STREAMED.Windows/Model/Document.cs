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
    [SQLite.AutoIncrement, SQLite.PrimaryKey, SQLite.Indexed]
    public int id { get; set; }
    public String DocumentType { get; set; }
    public String DebitCategory { get; set; }
    public String CreditCategory { get; set; }
    public String ImagePath { get; set; }
    public BitmapImage BMP { get; set; }
  }
}
