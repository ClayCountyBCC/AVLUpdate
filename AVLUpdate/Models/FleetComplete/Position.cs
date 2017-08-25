using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.FleetComplete
{
  public class Position
  {
    decimal Latitude { get; set; }
    decimal longitude { get; set; }
    string Address { get; set; }

    public Position()
    {

    }
  }
}