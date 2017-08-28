using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AVLUpdate.Models.AirVantage
{
  public class subscription
  {
    public string state { get; set; }
    public string uid { get; set; }
    public string identifier { get; set; }
    public long? mobileNumber { get; set; }
    public string networkIdentifier { get; set; }
    public string eid { get; set; }
    [JsonProperty("operator")]
    public string subOperator { get; set; }
    public string appletGeneration { get; set; }
    public string formFactor { get; set; }
    public string confType { get; set; }
    public string technology { get; set; }

    public subscription()
    {

    }
  }
}
