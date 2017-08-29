using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.AirVantage
{
  public class AccessToken
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
    public string access_token { get; set; } = "";
    public string refresh_token { get; set; } = "";
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
      string user = Program.GetCS(Program.CS_Type.AV_User);
      string password = Program.GetCS(Program.CS_Type.AV_Password);
      string clientid = Program.GetCS(Program.CS_Type.AV_Client_Id);
      string clientsecret = Program.GetCS(Program.CS_Type.AV_Client_Secret);
      string url = $"https://na.airvantage.net/api/oauth/token?grant_type=password&username={user}&password={password}&client_id={clientid}&client_secret={clientsecret}";
      string json = Program.GetJSON(url);
      try
      {
        if (json != null)
        {
          return JsonConvert.DeserializeObject<AccessToken>(json);
        }
        else
        {
          return null;
        }

      }catch(Exception ex)
      {
        new ErrorLog(ex, url);
        return null;
      }
    }

  }
}
