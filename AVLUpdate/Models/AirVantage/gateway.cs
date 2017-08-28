using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.AirVantage
{
  public class gateway
  {
    public string uid { get; set; }
    public long? imei { get; set; }
    public string macAddress { get; set; }
    public string serialNumber { get; set; }
    public string type { get; set; }
    public gateway()
    {

    }
  }
}
