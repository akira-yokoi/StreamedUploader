using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace STREAMED
{
  /**
   * 勘定科目を保持するモデル
   */
  class Category
  {
    public String clientId { get; set; }
    public String name { get; set; }
    public String id { get; set; }
    public String uc { get; set; }

    public override string ToString()
    {
      return name;
    }
  }
}
