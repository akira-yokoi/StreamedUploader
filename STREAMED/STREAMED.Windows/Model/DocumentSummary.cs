using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STREAMED
{
  class DocumentSummary
  {
    // 今月処理した枚数
    public int numberOfThisMonth { get; set; }
    // 今月処理した枚数
    public int numberOfProcessed { get; set; }
    // 処理待ちの枚数
    public int numberOfWait{ get; set; }
  }
}
