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

    public void UpdateUnitsUsing()
    {
      if (UnitUsingDataIsExpired)
      {
        UnitTracking.UpdateUnitsUsing();
        UnitUsingDataTimeOut = DateTime.Now.AddSeconds(UnitUsingSecondsToWait);        
      }
    }

    public void UpdateGISUnitLocations(List<GIS.UnitLocation> ull)
    {
      var ull = 

    }

    public void UpdateAirVantage(List<AirVantage.AirVantageData> avd)
    {
      if (avd == null || avd.Count() == 0) return;
      // otherwise we're going to join our two lists and update what's different.
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
