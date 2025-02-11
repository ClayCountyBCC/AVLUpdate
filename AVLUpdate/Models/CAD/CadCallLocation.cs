using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace AVLUpdate.Models.CAD
{
  public class CadCallLocation
  {
    public string inci_id { get; set; }
    public decimal geox { get; set; }
    public decimal geoy { get; set; }
    public StatePlanePoint Location
    {
      get
      {
        return new StatePlanePoint((double)geox, (double)geoy);
      }
    }

    public CadCallLocation()
    {
    }

    public static void UpdateActiveCallLocations()
    {
      var d = GetActive();
      Save(d);
    }

    public static void UpdateClosedCallLocations()
    {
      var d = GetClosed();
      Save(d);
    }

    public static List<CadCallLocation> GetActive()
    {
      // This function will return any call locations that haven't been converted yet.
      string query = @"
WITH ValidIncidents AS (
SELECT DISTINCT
  L.inci_id
FROM cad.dbo.log L
INNER JOIN cad.dbo.incident I ON I.inci_id = L.inci_id AND I.inci_id != '' AND I.cancelled=0
WHERE
  L.transtype IN ('D','E','A')
)

SELECT
  I.inci_id
  ,geox
  ,geoy
FROM
  cad.dbo.incident I
  INNER JOIN ValidIncidents VI ON I.inci_id = VI.inci_id
  LEFT OUTER JOIN Tracking.dbo.call_locations CL ON I.inci_id = CL.inci_id
WHERE
  CL.inci_id IS NULL
  AND geoy > 0
  AND geoy > 0";
      var data = Program.Get_Data<CadCallLocation>(query, Program.CS_Type.Tracking);
      return data;
    }

    public static List<CadCallLocation> GetClosed()
    {
      // This function will return any call locations that haven't been converted yet.
      string query = @"
        SELECT 
          I.inci_id
          ,geox
          ,geoy
        FROM cad.dbo.inmain I
        LEFT OUTER JOIN Tracking.dbo.call_locations CL ON I.inci_id = CL.inci_id
        WHERE
          CL.inci_id IS NULL
          AND geoy > 0 
          AND geoy > 0";
      var data = Program.Get_Data<CadCallLocation>(query, Program.CS_Type.Tracking);
      return data;
    }


    public static void Save(List<CadCallLocation> data)
    {
      var dt = CreateDataTable();

      foreach (CadCallLocation d in data)
      {
        try
        {
          dt.Rows.Add(
            d.inci_id
            , d.Location.Latitude
            , d.Location.Longitude
          );
        }
        catch (Exception ex)
        {
          new ErrorLog(ex);
        }

      }

      string query = @"
        SET NOCOUNT, XACT_ABORT ON;       

        INSERT INTO Tracking.dbo.call_locations (inci_id, latitude, longitude)
        SELECT
          inci_id
          ,latitude
          ,longitude
        FROM @CallLocationData";

      try
      {
        using (IDbConnection db = new SqlConnection(Program.GetCS(Program.CS_Type.Tracking)))
        {
          db.Execute(query, new { CallLocationData = dt.AsTableValuedParameter("CallLocationData") });
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex);
        return;
      }
      return;
    }

    private static DataTable CreateDataTable()
    {
      var dt = new DataTable("CallLocationData");
      dt.Columns.Add("inci_id", typeof(string));
      dt.Columns.Add("latitude", typeof(decimal));
      dt.Columns.Add("longitude", typeof(decimal));
      return dt;
    }
  }
}
