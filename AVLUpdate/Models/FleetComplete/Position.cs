using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.FleetComplete
{
  public class Position
  {
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Address { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string Country { get; set; }
    public int? Speed { get; set; }
    public int? SpeedLimit { get; set; }
    public int SpeedLimitMI { get; set; }
    public decimal? Direction { get; set; }

    public Position()
    {

    }
  }
}