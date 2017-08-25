using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AVLUpdate.Models.FleetComplete
{
  public class AccessToken
  {
    public string Domain { get; set; }
    public bool IsMustChangePassword { get; set; }
    public int LicenseExpiryDays { get; set; }
    public string Name { get; set; }
    public string SessionID { get; set; }
    public string UserID { get; set; }
    public string Version { get; set; } = "";
    public string Token { get; set; } = "";
    public string APIVersion
    {
      get
      {
        if (Version.Length == 0)
        {
          return "";
        }
        else
        {
          return Version.Substring(0, 5).Replace(".", "_");
        }
        
      }
    }
    public System.Net.WebHeaderCollection Headers
    {
      get
      {
        var whc = new System.Net.WebHeaderCollection();
        if (Token.Length != 0)
        {
          whc.Add("ClientID", Program.GetCS(Program.CS_Type.FC_Client_Id));
          whc.Add("UserID", UserID);
          whc.Add("Token", Token);
        }
        return whc;
      }
    }
    //figure out a way to add the header from here

    public AccessToken()
    {
      

    }

  public static AccessToken Authenticate()
    {
      string clientId = Program.GetCS(Program.CS_Type.FC_Client_Id);
      string username = Program.GetCS(Program.CS_Type.FC_User);
      string password = Program.GetCS(Program.CS_Type.FC_Password);
      string url = $"https://hosted.fleetcomplete.com/Authentication/Authentication.svc/authenticate/user?clientid={clientId}&userlogin={username}&userpassword={password}";
      string json = Program.GetJSON(url);
      if (json != null)
      {
        return JsonConvert.DeserializeObject<Models.FleetComplete.AccessToken>(json);
      }
      else
      {
        return new AccessToken();
      }
    }
  }

}
