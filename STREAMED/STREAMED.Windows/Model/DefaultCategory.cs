using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STREAMED
{
  /**
   * アップロード時に指定した勘定科目を保持しておくためのモデル
   */
  class DefaultCategory
  {
    [SQLite.AutoIncrement, SQLite.PrimaryKey, SQLite.Indexed]
    public int id { get; set; }

    public int DocumentType { get; set; }

    public String DebitCategoryName { get; set; }

    public String CreditCategoryName { get; set; }
  }
}
