using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.AVL
{
  class AccessToken
  {
    public DateTime create_date { get; } = DateTime.Now;
    public DateTime expiration_date
    {
      get
      {
        if (expires_in == -1)
        {
          return DateTime.MinValue;
        } else
        {
          return create_date.AddSeconds(expires_in);
        }
      }
    }
    public string access_token { get; set; }
    public string refresh_token { get; set; }
    public int expires_in { get; set; } = -1;
    public bool isExpired
    {
      get
      {
        return DateTime.Now > expiration_date;
      }
    }

    public AccessToken()
    {

    }

    public static AccessToken Authenticate()
    {

    }

  }
}
