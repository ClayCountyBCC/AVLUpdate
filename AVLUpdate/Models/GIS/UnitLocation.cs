using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;
using System.Linq;
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

    public static List<UnitLocation> Get()
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
          AND DATEDIFF(hh, GETDATE(), AS1.TimeStampUTC) <= 5
        ORDER BY TimeStampUTC";
      // only return those locations we know are valid.
      try
      {
        var data = Program.Get_Data<UnitLocation>(query, Program.CS_Type.GIS);
        if (data == null) return null;
        var valid = (from u in data
                     where u.Location.IsValid && u.deviceId > 0
                     select u).ToList();
        return valid;
      }
      catch(Exception ex)
      {
        new ErrorLog(ex, query);
        return null;
      }

    }

  }
}
