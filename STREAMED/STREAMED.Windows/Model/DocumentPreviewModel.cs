using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STREAMED.Model
{
  public class DocumentPreviewModel
  {
    public Document document { get; set; }
    public List<Document> listItems { get; set; }
    public int currentIndex { get; set; }
  }
}
