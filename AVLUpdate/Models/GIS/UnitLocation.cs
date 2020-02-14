using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;
using System.Linq;
using System.Data;
using Dapper;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.GIS
{
  public class UnitLocation
  {
    public enum TypeIdentifier : int
    {
      unknown = 0,
      phoneNumber = 1,
      imei = 2
    }

    public long deviceId { get; set; } = 0;
    public string deviceType { get; set; } = "";
    public TypeIdentifier deviceTypeIdentifier
    {
      get
      {
        switch (deviceType)
        {
          case "ESN/IMSI":
            imei = deviceId;
            return TypeIdentifier.imei;

          case "Phone Number":
            phoneNumber = deviceId;
            return TypeIdentifier.phoneNumber;

          default:
            return TypeIdentifier.unknown;
        }          
      }
    }
    public int direction { get; set; } = 0;
    public DateTime timestampUTC { get; set; }
    public DateTime timestampLocal
    {
      get
      {
        return timestampUTC.ToLocalTime();
      }
    }
    public int velocityKM { get; set; } = 0;
    public int velocityMPH { get
      {
        if (velocityKM == 0) return 0;
        return (int)(velocityKM * 0.621371);
      } }
    public string ipAddress { get; set; } = "";
    public decimal XCoord { get; set; } = 0;
    public decimal YCoord { get; set; } = 0;
    public int satelliteCount { get; set; } = 0;
    public long imei { get; set; } = 0;
    public long phoneNumber { get; set; } = 0;
    public long phoneNumberNormalized
    {
      get
      {
        return phoneNumber > 9999999999 ? phoneNumber - 10000000000 : phoneNumber;
      }
    }
    public Point Location
    {
      get
      {
        return new Point((double)XCoord, (double)YCoord);
      }
    }

    public UnitLocation()
    {

    }

    public static void GetAndSave()
    {
      string query = @"
        USE ClayWebGIS;
        SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
        SELECT 
          AS1.DeviceID deviceId, 
          AS1.DeviceType deviceType,
          AS1.TimeStampUTC timestampUTC, 
          AS1.Direction direction, 
          AS1.VelocityKM velocityKM, 
          AS1.GPSSatellites satelliteCount,
          AS1.Shape.STX XCoord,
          AS1.Shape.STY YCoord,
          AS1.IpAddress ipAddress
        FROM AVL_PS_CURRENT AS1 
        INNER JOIN 
          (SELECT 
            MAX(OBJECTID) AS OBJECTID, 
            DeviceID
          FROM AVL_PS_CURRENT 
          WHERE 
            DeviceID IS NOT NULL            
          GROUP BY DeviceID
        ) AS AVLGROUP ON AS1.OBJECTID=AVLGROUP.OBJECTID
        WHERE 
          AS1.DeviceID IS NOT NULL
          --AND DATEDIFF(hh, GETDATE(), AS1.TimeStampUTC) <= 5
        ORDER BY TimeStampUTC";
      // only return those locations we know are valid.
      try
      {
        var data = Program.Get_Data<UnitLocation>(query, Program.CS_Type.GIS);

        //if (data == null) return null;

        Save(data); // Save the data

        //var valid = (from u in data
        //             where u.deviceId > 0 &&
        //             u.timestampLocal < DateTime.Now.AddMinutes(30)
        //             select u).ToList();        
        //return valid;
      }
      catch(Exception ex)
      {
        new ErrorLog(ex, query);
        //return null;
      }
    }

    public static void Save(List<UnitLocation> data)
    {
      var dt = CreateDataTable();

      foreach (UnitLocation d in data)
      {
        try
        {
          dt.Rows.Add(
          d.deviceId.ToString(),
          d.deviceType,
          (Int16)d.direction,
          d.timestampLocal,
          (Int16)d.satelliteCount,
          (Int16)d.velocityMPH,
          d.ipAddress,
          d.Location.Latitude,
          d.Location.Longitude);
        }
        catch (Exception ex)
        {
          new ErrorLog(ex);
        }

      }

      string query = @"
        SET NOCOUNT, XACT_ABORT ON;
        USE Tracking;

        DECLARE @Now DATETIME = GETDATE();

        MERGE Tracking.dbo.avl_data WITH (HOLDLOCK) AS A

        USING @AVLData AS AVL ON A.device_id = AVL.device_id

        WHEN MATCHED THEN
          
          UPDATE 
          SET
            device_type = AVL.device_type
            ,direction=AVL.direction
            ,location_timestamp = AVL.location_timestamp
            ,satellite_count = AVL.satellite_count
            ,velocity=AVL.velocity
            ,ip_address = AVL.ip_address
            ,latitude=AVL.latitude
            ,longitude=AVL.longitude
            ,updated_on = @Now

        WHEN NOT MATCHED THEN

          INSERT 
            (
              device_id
              ,device_type
              ,direction
              ,location_timestamp
              ,satellite_count
              ,velocity
              ,ip_address
              ,latitude
              ,longitude
              ,updated_on
            )
          VALUES (
              AVL.device_id
              ,AVL.device_type
              ,AVL.direction
              ,AVL.location_timestamp
              ,AVL.satellite_count
              ,AVL.velocity
              ,AVL.ip_address
              ,AVL.latitude
              ,AVL.longitude
              ,@Now
          );";

      try
      {
        using (IDbConnection db = new SqlConnection(Program.GetCS(Program.CS_Type.Tracking)))
        {
          db.Execute(query, new { AVLData = dt.AsTableValuedParameter("AVLData") });
        }

      }
      catch (Exception ex)
      {
        new ErrorLog(ex);
      }
    }

    private static DataTable CreateDataTable()
    {
      var dt = new DataTable("AVLData");
      dt.Columns.Add("device_id", typeof(string));
      dt.Columns.Add("device_type", typeof(string));
      dt.Columns.Add("direction", typeof(Int16));
      dt.Columns.Add("location_timestamp", typeof(DateTime));
      dt.Columns.Add("satellite_count", typeof(Int16));
      dt.Columns.Add("velocity", typeof(Int16));
      dt.Columns.Add("ip_address", typeof(string));
      dt.Columns.Add("latitude", typeof(decimal));
      dt.Columns.Add("longitude", typeof(decimal));
      return dt;
    }
    
  }
}
