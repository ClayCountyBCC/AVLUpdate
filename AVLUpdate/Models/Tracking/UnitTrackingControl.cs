using System;
using System.Collections.Generic;
using System.Linq;

namespace AVLUpdate.Models.Tracking
{
  public class UnitTrackingControl
  {
    private const int UnitUsingSecondsToWait = 30;
    private DateTime UnitUsingDataTimeOut { get; set; } = DateTime.MinValue;
    private bool UnitUsingDataIsExpired
    {
      get
      {
        return UnitUsingDataTimeOut < DateTime.Now;
      }
    }
    private List<UnitTracking> utl { get; set; } = new List<UnitTracking>();
    private DateTime DateLastUpdated { get; set; } = DateTime.MinValue;

    public UnitTrackingControl()
    {      
    }

    public void UpdateTrackingData()
    {
      utl = UnitTracking.Get();
    }
    
    public void UpdateGISUnitLocations(List<GIS.UnitLocation> ull)
    {
       foreach(GIS.UnitLocation ul in ull)
      {
        var found = (from ut in utl
                     where ut.imei == ul.imei || ut.phoneNumber == ul.phoneNumber
                     select ut);
        int count = found.Count();
        if (count == 0)
        {
          new ErrorLog("Unknown Unit Found in AVL Data", ul.deviceId.ToString(), "", "", "");
        }
        else
        {
          if(count == 1)
          {
            UnitTracking u = found.First();
            if(u.dateLastCommunicated < ul.timestampLocal) // don't update it if it's older than our current data.
            {
              u.isChanged = true;
              u.dateLastCommunicated = ul.timestampLocal;
              u.dateUpdated = ul.timestampLocal;
              u.latitude = ul.Location.Latitude;
              u.longitude = ul.Location.Longitude;
              u.ipAddress = ul.ipAddress;
              u.gpsSatelliteCount = ul.satelliteCount;
              u.direction = ul.direction;
              u.dataSource = "AVL";
              u.velocityMPH = ul.velocityMPH;
            }
          }
          else // more than one row was found.
          {
            // if we hit this, we've found more than one unit with this unit's phone number/imei
            // we're going to ignore this data and throw an error that we can follow up on.
            new ErrorLog("Too many Unit matches Found in AVL Data", ul.deviceId.ToString(), "", "", "");
          }
        }
      }
    }

    

    public void UpdateAirVantage(List<AirVantage.AirVantageData> avd)
    {
      if (avd == null || avd.Count() == 0) return;
      // otherwise we're going to join our two lists and update what's different.
      foreach (AirVantage.AirVantageData a in avd)
      {
        var ul = (from ut in utl
                 where ut.unitcode == a.unitcode
                 select ut).ToList();
        if(ul.Count() > 0)
        {
          var u = ul.First();
          if (a.imei > 0 && a.imei != u.imei)
          {
            u.imei = a.imei;
            u.isChanged = true;
          }
          if(a.phone_number > 0 && a.phone_number != u.phoneNumber)
          {
            u.phoneNumber = a.phone_number;
            u.isChanged = true;
          }
        }
        else
        { // we didn't find the unit in our data, we should add it.
          utl.Add(new UnitTracking(a.unitcode, a.imei, a.phone_number, "AV"));
        }
      }
    }

    public void UpdateFleetComplete(FleetComplete.FleetCompleteData fcd)
    {
      if (fcd == null) return;
    }

    public void Save()
    {
      // this function will assume that the utl variable has been as updated as it's going to get
      // and now we'll save it to the unit_tracking_table.
    }

  }
}
