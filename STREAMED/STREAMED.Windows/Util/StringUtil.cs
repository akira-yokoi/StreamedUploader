using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STREAMED
{
  public class StringUtil
  {
    public static bool isEmpty(string text)
    {
      if (text == null || text.Length == 0)
      {
        return true;
      }
      return false;
    }
  }
}
