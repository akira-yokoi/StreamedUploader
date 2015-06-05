using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace STREAMED
{
  public class Client
  {
    public String id{ get; set; }
    public String login_id{ get; set; }
    public String company{ get; set; }
    public String industry{ get; set; }
    public String department{ get; set; }
    public String set_password { get; set; }
    public String receipt_point { get; set; }
    public String number_of_ticket { get; set; }
    public int nReceiptsThisMonth { get; set; }
    public int nReceiptsWait { get; set; }
    public String username { get; set; }
    public String email_activation { get; set; }
    public String accountant_email { get; set; }
    public DateTime last_upload { get; set; }
    public int this_month_count { get; set; }
    public int current_processing_count { get; set; }
    public String email { get; set; }
  }
}
