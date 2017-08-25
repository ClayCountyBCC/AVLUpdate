using System;
using System.Collections.Generic;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using AVLUpdate.Models.AirVantange;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.Tracking
{
  public class UnitTracking 
  {
    // this matches what is in the unit_tracking_data
    // table in the Tracking database
    public string unitcode { get; set; } = "";
    public string usingUnit { get; set; } = null;
    public DateTime dateUpdated { get; set; } = DateTime.Now;
    public decimal longitude { get; set; } = 0;
    public decimal latitude { get; set; } = 0;
    public int direction { get; set; } = 0;
    public int velocityMPH { get; set; } = 0;
    public int gpsSatelliteCount { get; set; } = 0;
    public string ipAddress { get; set; } = "";
    public long imei { get; set; } = 0;
    public long phoneNumber { get; set; } = 0;
    public DateTime dateLastCommunicated { get; set; } = DateTime.Now;

    public UnitTracking()
    {

    }

    public UnitTracking(AirVantageData avd)
    {
      if(avd.labels.Count() == 1)
      {
        // if there are not exactly 1 label, we should error out
        unitcode = avd.labels.First().Trim();
      }
      else
      {
        return;
      }
      if(avd.subscriptions.Count() > 0)
      {
        var s = avd.subscriptions.First();
        if (s.mobileNumber.HasValue && s.mobileNumber.Value != phoneNumber)
        {
          phoneNumber = s.mobileNumber.Value;
        }
      }
      if (avd.gateway.imei.HasValue && avd.gateway.imei.Value != imei)
      {
        imei = avd.gateway.imei.Value;
      }
      Insert();
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

    public void Update()
    {
      string query = @"
        UPDATE unit_tracking_data
        SET 
          unit_using=@unitUsing,
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
        WHERE unitcode=@unitcode";
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

    public void Update(AirVantageData avd)
    {
      bool changed = false;
      if(avd.gateway.imei.HasValue && avd.gateway.imei.Value != imei)
      {
        imei = avd.gateway.imei.Value;
        changed = true;
      }
      if(avd.subscriptions.Count() > 0)
      {
        var s = avd.subscriptions.First();
        if(s.mobileNumber.HasValue && s.mobileNumber.Value != phoneNumber)
        {
          phoneNumber = s.mobileNumber.Value;
          changed = true;
        }
      }
      if (changed)
      {
        Update();
      }
    }

    public void Insert()
    {
      if (unitcode.Length == 0) return;
      string query = @"
        INSERT INTO unit_tracking_data
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
        )";
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

  }
}
