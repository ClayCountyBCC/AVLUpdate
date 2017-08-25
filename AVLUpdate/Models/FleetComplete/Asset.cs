using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.FleetComplete
{
  public class Asset
  {
    public string ID { get; set; }
    public string Description { get; set; }
    public string DeviceID { get; set; }
    public string LicensePlate { get; set; }
    public string VIN { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Manufacturer { get; set; }
    public bool HasMDT { get; set; }
    public bool AlternateDDSConfigured { get; set; }
    public bool IsDeleted { get; set; }
    public Position Position { get; set; }
    public Field Branch { get; set; }
    public Field HomeBase { get; set; }
    public Field AssetType { get; set; }
    public Field WorkSchedule { get; set; }
    public Field Resource { get; set; }
    public Satellite Satellite { get; set; }

    public Asset()
    {

    }
  }
}


  