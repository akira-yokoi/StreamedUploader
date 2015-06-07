using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STREAMED
{
  /**
   * 未使用：将来的に顧客情報をグループ化したくなったら使用する
   */
  public class ClientGroup
  {
    public String GroupName { get; set; }

    public ObservableCollection<Client> clientList = new ObservableCollection<Client>();

    public void addClient(Client client)
    {
      this.clientList.Add(client);
    }

    public ObservableCollection<Client> ClientList
    {
      get { return this.clientList; }
    }
  }
}
