using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STREAMED
{
  /**
   * スキャンを行うまでに指定された情報を画面間で受け渡すためのモデル
   */
  class ScanSetting
  {
    public static int DOC_TYPE_RECEIPT = 1;
    public static int DOC_TYPE_INVOICE = 2;
    public static int DOC_TYPE_BANKBOOK = 3;
    public static int DOC_TYPE_CREDITCARD = 4;

    public String clientId { get; set; }
    public String clientName { get; set; }
    public int documentType { get; set; }
    public String debitCategoryCode { get; set; }
    public String debitCategoryName { get; set; }
    public String debitSubCategoryCode { get; set; }
    public String debitSubCategoryName { get; set; }
    public String creditCategoryCode { get; set; }
    public String creditCategoryName { get; set; }
    public String creditSubCategoryCode { get; set; }
    public String creditSubCategoryName { get; set; }

    public String getDocumentTypeName()
    {
      if (documentType == DOC_TYPE_RECEIPT)
      {
        return "領収書";
      }
      else if (documentType == DOC_TYPE_INVOICE)
      {
        return "請求書(受領)";
      }
      else if (documentType == DOC_TYPE_BANKBOOK)
      {
        return "通帳";
      }
      else if (documentType == DOC_TYPE_CREDITCARD)
      {
        return "クレジット明細";
      }
      return "未指定";
    }
  }
}
