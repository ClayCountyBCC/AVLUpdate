using System;
using System.Collections.Generic;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using AVLUpdate.Models.AirVantage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.Tracking
{
  public class UnitTracking
  {
    // this matches what is in the unit_tracking_data
    // table in the Tracking database
    // anytime we get a new location, in addition to saving it to the unit_tracking_data table, 
    // we need to insert it into the tracking_data table.
    public string unitcode { get; set; } = "";
    public string usingUnit { get; set; } = null; // this is only updated by the UpdateUnitsUsing function
    public DateTime dateUpdated { get; set; } = DateTime.Now;
    public decimal longitude { get; set; } = 0;
    public decimal latitude { get; set; } = 0;
    public int direction { get; set; } = 0;
    public int velocityMPH { get; set; } = 0;
    public string ipAddress { get; set; } = "";
    public int gpsSatelliteCount { get; set; } = 0;
    public long imei { get; set; } = 0;
    public long phoneNumber { get; set; } = 0;
    public string assetTag { get; set; } = "";
    public DateTime dateLastCommunicated { get; set; } = DateTime.Now;
    public bool isChanged { get; set; } = false;

    public UnitTracking()
    {

    }

    public static void UpdateAirVantageData(AirVantageData avd)
    {
      var ut = new UnitTracking();
      if(avd.labels.Count() == 1)
      {
        // if there are not exactly 1 label, we should error out
        ut.unitcode = avd.labels.First().Trim();
      }
      else
      {
        return;
      }
      if(avd.subscriptions.Count() > 0)
      {
        var s = avd.subscriptions.First();
        if (s.mobileNumber.HasValue && s.mobileNumber.Value != ut.phoneNumber)
        {
          ut.phoneNumber = s.mobileNumber.Value;
        }
      }
      if (avd.gateway.imei.HasValue && avd.gateway.imei.Value != ut.imei)
      {
        ut.imei = avd.gateway.imei.Value;
      }
      ut.Merge();
    }

    public static List<UnitTracking> Get()
    {
      string query = @"
        WITH UsingUnit(unitcode, unit_using) AS (
          SELECT 
            UTD.unitcode, 
            LTRIM(RTRIM(REPLACE(ISNULL(UD.basedname, ''), 'USING', ''))) CurrentUnit 
          FROM unit_tracking_data UTD 
          INNER JOIN cad.dbo.undisp UD ON UTD.unitcode = UD.unitcode AND 
            UD.basedname like '%USING%' 
        )

        SELECT 
          UTD.unitcode,
          UU.unitcode unitUsing,
          UTD.date_updated dateUpdated,
          UTD.longitude,
          UTD.latitude,
          UTD.direction,
          UTD.velocity_mph velocityMPH,
          UTD.ip_address ipAddress,
          UTD.gps_satellite_count gpsSatelliteCount,
          UTD.imei,
          UTD.phone_number phoneNumber,
          UTD.asset_tag assetTag,
          UTD.date_last_communicated dateLastCommunicated
        FROM unit_tracking_data UTD
        LEFT OUTER JOIN UsingUnit UU ON UTD.unitcode=UU.unit_using
        ORDER BY unitcode ASC";
      try
      {
        return Program.Get_Data<UnitTracking>(query, Program.CS_Type.Tracking);
      }
      catch(Exception ex)
      {
        new ErrorLog(ex, query);
        return null;
      }
    }

    public void Merge()
    {
      if (unitcode.Length == 0) return;
      string query = @"
        SET NOCOUNT, XACT_ABORT ON;

        MERGE unit_tracking_data WITH (HOLDLOCK) AS UTD

        USING (SELECT @unitCode unitCode) AS newUnit
              ON UTD.unitcode = newUnit.unitCode

        WHEN MATCHED THEN
          
          UPDATE 
          SET             
            date_updated=@dateUpdated,
            date_last_communicated=@dateLastCommunicated,
            longitude=@longitude,
            latitude=@latitude,
            direction=@direction,
            velocity_mph=@velocityMPH,
            gps_satellite_count=@gpsSatelliteCount,
            ip_address=@ipAddress,
            imei=@imei,
            phone_number=@phoneNumber

        WHEN NOT MATCHED THEN

          INSERT 
            (unitcode
            ,longitude
            ,latitude
            ,direction
            ,velocity_mph
            ,ip_address
            ,gps_satellite_count
            ,imei
            ,phone_number
            ,date_last_communicated)
          VALUES (
            @unitCode,
            @longitude,
            @latitude,
            @direction,
            @velocityMPH,
            @ipAddress,
            @gpsSatelliteCount,
            @imei,
            @phoneNumber,
            @dateLastCommunicated
          );";
      try
      {
        using (IDbConnection db = new SqlConnection(Program.GetCS(Program.CS_Type.Tracking)))
        {
          db.Execute(query, this);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
      }
    }

    public static void UpdateUnitsUsing()
    {
      string query = @"
        SET NOCOUNT ON;
        UPDATE UTD
        SET UTD.using_unit=LTRIM(RTRIM(REPLACE(UD.basedname, 'USING', '')))
        FROM unit_tracking_data UTD
        LEFT OUTER JOIN cad.dbo.undisp UD ON UTD.unitcode = UD.unitcode AND 
          UD.basedname like '%USING%'";
      try
      {
        using (IDbConnection db = new SqlConnection(Program.GetCS(Program.CS_Type.Tracking)))
        {
          db.Execute(query);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
      }
    }
  }
}
