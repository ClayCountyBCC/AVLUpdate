using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AVLUpdate.Models.Tracking;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.AirVantage
{
  public class AirVantageData
  {
    public string uid { get; set; }
    public string name { get; set; }
    public gateway gateway { get; set; }
    public List<string> labels { get; set; }
    public List<subscription> subscriptions { get; set; } = new List<subscription>();
    public string unitcode
    {
      get
      {
        return (labels.Count() == 1) ? labels.First() : "";
      }
    }
    public long imei
    {
      get
      {
        return gateway.imei.HasValue ? gateway.imei.Value : 0;
      }
    }
    public long phone_number
    {
      get
      {
        if (subscriptions.Count() > 0)
        {
          var s = subscriptions.First();
          return s.mobileNumber.HasValue ? s.mobileNumber.Value : 0;
        }
        else
        {
          return 0;
        }

      }
    }

    public AirVantageData()
    {

    }

    public static List<AirVantageData>Get(AccessToken token)
    {
      // let's catch an error if we fail at getting a token.
      if (token == null) return null; 

      string url = $"https://na.airvantage.net/api/v1/systems?fields=name,labels,uid,gateway,subscriptions&access_token={token.access_token}";
      string json = Program.GetJSON(url);
      if (json != null)
      {
        return JObject.Parse(json).SelectToken("items").ToObject<List<AirVantageData>>();
      }
      else
      {
        return null;
      }
    }

    public void UpdateUnitTracking()
    {
      // This function will take whatever is returned for this object and 
      // update our UnitTracking table in the Tracking database.
      UnitTracking.UpdateAirVantageData(this);
    }

  }
}
