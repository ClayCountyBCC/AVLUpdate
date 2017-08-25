using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.FleetComplete
{
  public class Satellite
  {
    public bool SFOEnabled { get; set; }
    public int TransmitTime { get; set; }
    public int TransmitTimeUnit { get; set; }
    public bool IsTransmitIgnitionOnOffEnabled { get; set; }
    public bool IsTransmitInputStateChangesEnabled { get; set; }
    public bool IsTransmitAlarmsEnabled { get; set; }

    public Satellite()
    {

    }
  }
}

