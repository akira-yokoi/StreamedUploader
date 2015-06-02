using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace STREAMED
{
  public class Log
  {
    public static void debug(String message)
    {
      Debug.WriteLine(message);
    }
  }
}
