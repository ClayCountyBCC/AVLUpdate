using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using System.Data.SqlClient;

namespace AVLUpdate.Models.FleetComplete
{
  // some information here:
  // api documentation
  // http://hosted.fleetcomplete.com/v8_3_0/Integration/WebAPI/fleet-docs/dist/index.html#!/Asset/Asset_GetAssets

  public class FleetCompleteControl
  {
    private const int SecondsToWait = 30;
    private AccessToken Token { get; set; }
    private DateTime DataTimeOut { get; set; } = DateTime.MinValue;
    private bool IsExpired
    {
      get
      {
        return DataTimeOut < DateTime.Now;
      }
    }

    public FleetCompleteControl()
    {
      Token = AccessToken.Authenticate();
    }

    public FleetCompleteData Update()
    {
      // we only query this again if the IsExpired field is true, otherwise
      // we return an empty list.
      // We return if we actually updated anything, this will be used
      // to indicate that a refresh of the unit_tracking data is needed.
      try
      {
        if (IsExpired)
        {

          var fcd = FleetCompleteData.Get(Token);
          if (fcd == null || fcd.Data == null) return null;
          // let's remove the invalids          
          SaveFleetCompleteData(fcd);
          fcd.Data = (from d in fcd.Data
                      where d.IsValid
                      select d).ToList();
          DataTimeOut = DateTime.Now.AddSeconds(SecondsToWait);          
          return fcd;
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex);
      }
      return null;
    }

    private static DataTable CreateDataTable()
    {
      var dt = new DataTable("FleetCompleteData");
      dt.Columns.Add("id", typeof(string));
      dt.Columns.Add("device_id", typeof(string));
      dt.Columns.Add("asset_tag", typeof(string));
      dt.Columns.Add("vin", typeof(string));
      dt.Columns.Add("make", typeof(string));
      dt.Columns.Add("model", typeof(string));
      dt.Columns.Add("year", typeof(int));
      dt.Columns.Add("timestamp", typeof(DateTime));
      dt.Columns.Add("date_updated", typeof(DateTime));
      dt.Columns.Add("latitude", typeof(decimal));
      dt.Columns.Add("longitude", typeof(decimal));
      dt.Columns.Add("direction", typeof(int));
      dt.Columns.Add("velocity", typeof(float));
      return dt;
    }

    private bool SaveFleetCompleteData(FleetCompleteData fcd)
    {
      var dt = CreateDataTable();
      try
      {
        //var test = (from f in fcd.Data
        //            where f.DeviceID.Length == 0
        //            select f).ToList();

        foreach (Asset d in fcd.Data)
        {
          try
          {
            if (d.DeviceID == null) d.DeviceID = "";
            //if(d.Position == null || 
            //  d.DeviceID == null || 
            //  d.AssetTag == null || 
            //  d.Position.Latitude == null || 
            //  d.Position.Longitude == null ||
            //  d.Position.Direction == null ||
            //  d.VIN == null ||
            //  d.Make == null ||
            //  d.Model == null ||
            //  d.Year == null ||
            //  d.LastUpdatedTimeStamp == null)
            //{
            //  int i = 11;
            //}
          }
          catch(Exception exx)
          {
            new ErrorLog(exx);
          }

          dt.Rows.Add(
            d.DeviceID + "-" + d.AssetTag,
            d.DeviceID,
            d.AssetTag,
            d.VIN,
            d.Make,
            d.Model,
            d.Year,
            d.LastUpdatedTimeStampLocal,
            DateTime.Now,
            d.Position.Latitude,
            d.Position.Longitude,
            d.Position.Direction ?? 0,
            d.Position.Speed ?? 0);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex);
        return false;
      }

      string query = @"
        SET NOCOUNT, XACT_ABORT ON;
        USE Tracking;

        MERGE Tracking.dbo.fleetcomplete_data WITH (HOLDLOCK) AS F

        USING @FleetCompleteData AS FLD ON FLD.id = F.id

        WHEN MATCHED THEN
          
          UPDATE 
          SET
            device_id=FLD.device_id
            ,asset_tag=FLD.asset_tag            
            ,vin=FLD.vin
            ,make=FLD.make
            ,model=FLD.model
            ,year=FLD.year
            ,timestamp=FLD.timestamp
            ,date_updated=FLD.date_updated
            ,latitude=FLD.latitude
            ,longitude=FLD.longitude
            ,direction=FLD.direction
            ,velocity=FLD.velocity

        WHEN NOT MATCHED THEN

          INSERT 
            (
              id
              ,device_id
              ,asset_tag
              ,vin
              ,make
              ,model
              ,year
              ,timestamp
              ,date_updated
              ,latitude
              ,longitude
              ,direction
              ,velocity
            )
          VALUES (
              id
              ,FLD.device_id
              ,FLD.asset_tag
              ,FLD.vin
              ,FLD.make
              ,FLD.model
              ,FLD.year
              ,FLD.timestamp
              ,FLD.date_updated
              ,FLD.latitude
              ,FLD.longitude
              ,FLD.direction
              ,FLD.velocity
          );";

      try
      {
        using (IDbConnection db = new SqlConnection(Program.GetCS(Program.CS_Type.Tracking)))
        {
          db.Execute(query, new { FleetCompleteData = dt.AsTableValuedParameter("FleetCompleteData") });          
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex);
        return false;
      }
      return true;
    }

  }
}
