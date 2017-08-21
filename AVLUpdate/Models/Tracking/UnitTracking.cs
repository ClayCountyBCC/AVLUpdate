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
  class UnitTracking
  {
    public string unitcode { get; set; }
    public string usingUnit { get; set; } = null;
    public DateTime dateUpdated { get; set; }
    public decimal longitude { get; set; }
    public decimal latitude { get; set; }
    public int direction { get; set; }
    public int velocityMPH { get; set; }
    public int gpsSatelliteCount { get; set; }
    public string ipAddress { get; set; }
    public DateTime dateLastCommunicated { get; set; }

    public UnitTracking()
    {

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
          ip_address=@ipAddress
        WHERE unitcode=@unitcode AND source=@source";
      try
      {
        using (IDbConnection db = new SqlConnection(Program.Get_ConnStr(Program.CS_Type.Tracking)))
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
