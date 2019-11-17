using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.FleetComplete
{
  public class FleetCompleteData
  {
    public int StatusCode { get; set; } = -1;
    public List<Asset> Data { get; set; } = new List<Asset>();
    public List<Error> Errors { get; set; } = new List<Error>();

    public FleetCompleteData()
    {

    }

    //public static FleetCompleteData Get(AccessToken token, ref string LastTimestamp)
    //{
    //  if (token == null) return null;
      
    //  string url = $"https://{token.Domain}/v{token.APIVersion}/Integration/WebAPI/GPS/Asset?top=500";
    //  if (LastTimestamp.Length > 0)
    //  {
    //    url += "&filter=LastUpdatedTimeStamp gt " + LastTimestamp;
    //  }
    //    string json = Program.GetJSON(url, ref LastTimestamp, token.Headers);
    //  if (json != null)
    //  {
    //    try
    //    {
    //      return JsonConvert.DeserializeObject<FleetCompleteData>(json);
    //    }
    //    catch(Exception ex)
    //    {
    //      new ErrorLog(ex, json);
    //      return null;
    //    }
    //  }
    //  else
    //  {
    //    return null;
    //  }
    //}

  }
}