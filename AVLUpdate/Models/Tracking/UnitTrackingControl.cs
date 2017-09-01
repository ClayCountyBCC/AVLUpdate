using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using System.Diagnostics;

namespace AVLUpdate.Models.Tracking
{
  public class UnitTrackingControl
  {
    private const int GISMaxAgeInSeconds = 60;
    private const int EmailFrequencyInSeconds = 60 * 5; // send an email every 5 minutes at most.
    private DateTime NextEmail { get; set; } = DateTime.MinValue;
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
    private DataTable dt { get; set; }

    public UnitTrackingControl()
    {
      dt = CreateDataTable();
    }

    private DataTable CreateDataTable()
    {
      var dt = new DataTable("UnitTrackingData");
      dt.Columns.Add("unitcode", typeof(string));
      dt.Columns.Add("using_unit", typeof(string));
      dt.Columns.Add("date_updated", typeof(DateTime));
      dt.Columns.Add("longitude", typeof(double));
      dt.Columns.Add("latitude", typeof(double));
      dt.Columns.Add("direction", typeof(int));
      dt.Columns.Add("velocity_mph", typeof(int));
      dt.Columns.Add("ip_address", typeof(string));
      dt.Columns.Add("gps_satellite_count", typeof(int));
      dt.Columns.Add("data_source", typeof(string));
      dt.Columns.Add("imei", typeof(long));
      dt.Columns.Add("phone_number", typeof(long));
      dt.Columns.Add("asset_tag", typeof(string));
      dt.Columns.Add("date_last_communicated", typeof(DateTime));
      return dt;
    }

    public void UpdateTrackingData()
    {
      utl = UnitTracking.Get();
    }
    
    private void CheckNewestGISData(DateTime d)
    {
      // this function is going to use the max age from the UnitLocation data
      // and check to see if it is within our expected window of time.  If it's
      // too old, we'll send an email.
      // We only want to send an error email if our max age difference is greater than GISMaxAgeInSeconds
      // AND if we haven't sent an email in the Seconds since EmailFrequencyInSeconds and NextEmail variable
      if(DateTime.Now.Subtract(d).Seconds > GISMaxAgeInSeconds)
      {
        if(NextEmail < DateTime.Now)
        {
          // we're going to send an email
          ErrorLog.SaveEmail(Program.GISDataErrorEmailAddresses, "GIS Server - AVL Data not updating", $"Data was last updated: {d.ToLongDateString()}");
          NextEmail = DateTime.Now.AddSeconds(EmailFrequencyInSeconds);
        }
      }
      else
      {
        NextEmail = DateTime.MinValue;
      }
    }

    public void UpdateGISUnitLocations(List<GIS.UnitLocation> ull)
    {
      if (ull == null || ull.Count() == 0) return;
      CheckNewestGISData((from u in ull
                          select u.timestampLocal).Max());

      foreach (GIS.UnitLocation ul in ull)
      {
        var found = (from ut in utl
                     where ut.imei == ul.deviceId || ut.phoneNumberNormalized == ul.deviceId
                     select ut);
        int count = found.Count();
        if (count == 0)
        {
          new ErrorLog("Unknown Unit Found in AVL Data", ul.deviceId.ToString(), "", "", "");
        }
        else
        {
          if (count == 1)
          {
            UnitTracking u = found.First();
            if (u.dateLastCommunicated < ul.timestampLocal) // don't update it if it's older than our current data.
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
            // we're going to ignore this data and throw an error that we can follow up on manually.
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
                 where ut.unitcode == a.unitcode.Trim()
                 select ut).ToList();
        if(ul.Count() > 0)
        {
          ul.First().UpdateAirVantageData(a);
        }
        else
        { // we didn't find the unit in our data, we should add it.
          utl.Add(new UnitTracking(a));
        }
      }
    }

    public void UpdateFleetComplete(FleetComplete.FleetCompleteData fcd)
    {
      if (fcd == null || fcd.Data == null || fcd.Data.Count() == 0) return;

      foreach (FleetComplete.Asset d in fcd.Data)
      {
        // we need to match this data based on our asset_tag field 
        // we need to treat the asset_tag as a primary key, even though it really isn't
        // in the unit_tracking_data table.
        if(d.AssetTag.Length > 0)
        {
          var ul = (from ut in utl
                    where ut.assetTag == d.AssetTag
                    select ut).ToList();
          if (ul.Count() == 1)
          {
            // let's update that unit, barring a few conditions.
            ul.First().UpdateFleetCompleteData(d);
          }
          else
          {
            if (ul.Count() == 0)
            {
              // let's add this unit to the utl
              utl.Add(new UnitTracking(d));
            }
            else
            {
              // if we hit this, we've found more than one unit with this unit's asset tag
              // we're going to ignore this data and throw an error that we can follow up on manually.
              new ErrorLog("Too many Asset Tag matches Found in Fleet Complete Data", d.AssetTag, "", "", "");
            }
          }
        }
      }
    }

    public void Save()
    {

      // this function will assume that the utl variable has been as updated as it's going to get
      // and now we'll save it to the unit_tracking_table.
      dt.Rows.Clear();
      var changed = (from ut in utl
                     where ut.isChanged || ut.usingUnit != null
                     select ut).ToList(); // we only want to save the changed records.

      foreach (UnitTracking u in changed)
      {
        dt.Rows.Add(u.unitcode, u.usingUnit, u.dateUpdated, u.longitude, u.latitude, u.direction, u.velocityMPH,
          u.ipAddress, u.gpsSatelliteCount, u.dataSource, u.imei, u.phoneNumber, u.assetTag, u.dateLastCommunicated);
      }

      string query = @"
        SET NOCOUNT, XACT_ABORT ON;

        MERGE unit_tracking_data WITH (HOLDLOCK) AS UTD

        USING @UnitTracking AS UT ON UTD.unitcode = UT.unitcode

        WHEN MATCHED THEN
          
          UPDATE 
          SET
            using_unit=UT.using_unit,
            date_updated=UT.date_updated,
            date_last_communicated=UT.date_last_communicated,
            longitude=UT.longitude,
            latitude=UT.latitude,
            direction=UT.direction,
            velocity_mph=UT.velocity_mph,
            gps_satellite_count=UT.gps_satellite_count,
            ip_address=UT.ip_address,
            data_source=UT.data_source,
            imei=UT.imei,
            phone_number=UT.phone_number,
            asset_tag=UT.asset_tag

        WHEN NOT MATCHED THEN

          INSERT 
            (unitcode
            ,using_unit
            ,longitude
            ,latitude
            ,direction
            ,velocity_mph
            ,ip_address
            ,gps_satellite_count
            ,data_source
            ,imei
            ,phone_number
            ,asset_tag
            ,date_updated
            ,date_last_communicated)
          VALUES (
            LTRIM(RTRIM(UT.unitcode)),
            UT.using_unit,
            UT.longitude,
            UT.latitude,
            UT.direction,
            UT.velocity_mph,
            UT.ip_address,
            UT.gps_satellite_count,
            UT.data_source,
            UT.imei,
            UT.phone_number,
            UT.asset_tag,
            UT.date_updated,
            UT.date_last_communicated
          );";
      try
      {
        using (IDbConnection db = new SqlConnection(Program.GetCS(Program.CS_Type.Tracking)))
        {
          db.Execute(query, new { UnitTracking = dt.AsTableValuedParameter("UnitTrackingData") });
        }
      }

      catch (Exception ex)
      {
        new ErrorLog(ex, query);
      }
    }

    

  }
}
