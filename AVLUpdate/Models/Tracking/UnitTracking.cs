using System;
using System.Collections.Generic;
using Dapper;
using System.Data;
using System.Data.SqlClient;
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
    public string usingUnit { get; set; } = null;
    public DateTime dateUpdated { get; set; } = DateTime.Now;
    public decimal longitude { get; set; } = 0;
    public decimal latitude { get; set; } = 0;
    public int direction { get; set; } = 0;
    public int velocityMPH { get; set; } = 0;
    public string ipAddress { get; set; } = "";
    public int gpsSatelliteCount { get; set; } = 0;
    public string dataSource { get; set; }
    public long imei { get; set; } = 0;
    public long phoneNumber { get; set; } = 0;
    public long phoneNumberNormalized
    {
      get
      {
        return phoneNumber > 9999999999 ? phoneNumber - 10000000000 : phoneNumber;
      }
    }
    public string assetTag { get; set; } = "";
    public DateTime dateLastCommunicated { get; set; } = DateTime.Now;
    public bool isChanged { get; set; } = false;
    public string inci_id { get; set; }
    public string cadUnitStatus { get; set; }
    public DateTime? cadUnitStatusTime { get; set; }


    public UnitTracking()
    {

    }

    public UnitTracking(AirVantage.AirVantageData a)
    {
      isChanged = true;
      unitcode = a.unitcode.Trim();
      imei = a.imei;
      phoneNumber = a.phone_number_normalized;
      dataSource = "AV";
    }

    public UnitTracking(FleetComplete.Asset a)
    {
      isChanged = true;
      unitcode = a.AssetTag;
      assetTag = a.AssetTag;
      dataSource = "FC";
      dateLastCommunicated = a.LastUpdatedTimeStampLocal;
      dateUpdated = a.LastUpdatedTimeStampLocal;
      try
      {
        if (a.Position != null && a.Position.Latitude != 0)
        {
          latitude = a.Position.Latitude;
          longitude = a.Position.Longitude;
          direction = (int)a.Position.Direction;
          velocityMPH = a.Position.Speed ?? 0;
        }
      }
      catch(Exception ex)
      {
        new ErrorLog(ex, "Updating FleetComplete UnitTracking");
        return;
      }
    }

    public void UpdateFleetCompleteData(FleetComplete.Asset a)
    {
      try
      {
        if (dataSource == "AVL" &&
          DateTime.Now.Subtract(dateLastCommunicated).TotalSeconds < 60)
        {
          // If we've had a location from the GIS system in the last 60 seconds,
          // we should ignore this location because it's not as precise
          // as our GIS location.
          return;
        }
        if(a.Position.Longitude == 0 || a.Position.Latitude == 0)
        {
          // we didn't get a location for this unit so let's not do anything.
          return;
        }
        isChanged = true;
        dateLastCommunicated = a.LastUpdatedTimeStampLocal;
        dateUpdated = a.LastUpdatedTimeStampLocal;
        latitude = a.Position.Latitude;
        longitude = a.Position.Longitude;
        direction = (int)a.Position.Direction;
        velocityMPH = a.Position.Speed ?? 0;
      }
      catch(Exception ex)
      {
        new ErrorLog(ex, "UpdateFleetCompleteData");
      }
    }

    public void UpdateAirVantageData(AirVantage.AirVantageData a)
    {
      if (a.imei > 0 && a.imei != imei)
      {
        imei = a.imei;
        isChanged = true;
      }
      if (a.phone_number > 0 && a.phone_number_normalized != phoneNumberNormalized)
      {
        phoneNumber = a.phone_number_normalized;
        isChanged = true;
      }
    }

    public static List<UnitTracking> Get()
    {
      string query = @"
        WITH UsingUnitCount AS (

          SELECT 
            LTRIM(RTRIM(REPLACE(ISNULL(UD.basedname, ''), 'USING', ''))) unitcode
          FROM unit_tracking_data UTD 
          INNER JOIN cad.dbo.undisp UD ON UTD.unitcode = UD.unitcode AND 
            UD.basedname like '%USING%' 
          GROUP BY LTRIM(RTRIM(REPLACE(ISNULL(UD.basedname, ''), 'USING', '')))
          HAVING COUNT(*) > 1

        
        ),UsingUnit(unitcode, unit_using) AS (

          SELECT 
            UTD.unitcode, 
            LTRIM(RTRIM(REPLACE(ISNULL(UD.basedname, ''), 'USING', ''))) CurrentUnit 
          FROM unit_tracking_data UTD 
          INNER JOIN cad.dbo.undisp UD ON UTD.unitcode = UD.unitcode 
            AND UD.basedname like '%USING%' 
          LEFT OUTER JOIN UsingUnitCount UC ON UC.unitcode = LTRIM(RTRIM(REPLACE(ISNULL(UD.basedname, ''), 'USING', '')))
          WHERE
            UC.unitcode IS NULL

        )
       

        SELECT 
          UTD.unitcode,
          CASE WHEN UUC.unitcode IS NOT NULL 
          THEN 'ERROR' 
          ELSE
            UU.unitcode 
          END usingUnit,
          UTD.date_updated dateUpdated,
          UTD.longitude,
          UTD.latitude,
          UTD.direction,
          UTD.velocity_mph velocityMPH,
          UTD.ip_address ipAddress,
          UTD.gps_satellite_count gpsSatelliteCount,
          UTD.data_source dataSource,
          UTD.imei,
          UTD.phone_number phoneNumber,
          UTD.asset_tag assetTag,
          UTD.date_last_communicated dateLastCommunicated,
          CASE WHEN UD.inci_id = '' THEN NULL ELSE UD.inci_id END inci_id,
          CASE WHEN UD.status = ''THEN NULL ELSE UD.status END cadUnitStatus,
          UD.statustime cadUnitStatusTime
        FROM unit_tracking_data UTD
        LEFT OUTER JOIN cad.dbo.undisp UD ON UTD.unitcode = UD.unitcode
        LEFT OUTER JOIN UsingUnit UU ON UTD.unitcode=UU.unit_using
        LEFT OUTER JOIN UsingUnitCount UUC ON UTD.unitcode = UUC.unitcode        
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
        
  }
}
