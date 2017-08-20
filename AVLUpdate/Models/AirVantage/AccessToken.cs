﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.AirVantange
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
      string user = Program.Get_ConnStr(Program.CS_Type.User);
      string password = Program.Get_ConnStr(Program.CS_Type.Password);
      string clientid = Program.Get_ConnStr(Program.CS_Type.client_id);
      string clientsecret = Program.Get_ConnStr(Program.CS_Type.client_secret);
      string url = $"https://na.airvantage.net/api/oauth/token?grant_type=password&username={user}&password={password}&client_id={clientid}&client_secret={clientsecret}";
      var wr = HttpWebRequest.Create(url);
      string json = "";
      try
      {
        var response = wr.GetResponse();
        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
        {
          json = sr.ReadToEnd();
          return JsonConvert.DeserializeObject<AccessToken>(json);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex);
        return new AccessToken();
      }

    }

  }
}
