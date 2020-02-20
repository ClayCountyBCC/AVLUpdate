using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace AVLUpdate.Models.CAD
{
  public class CadUnitLocation
  {
    public int avllogid { get; set; } = 0;
    public string unitcode { get; set; } = "";
    public DateTime location_timestamp { get; set; } = DateTime.MinValue;
    public string inci_id { get; set; } = "";
    public string status { get; set; } = "";
    public string avstatus { get; set; } = "";
    public decimal geox { get; set; } = 0;
    public decimal geoy { get; set; } = 0;
    public decimal speed { get; set; } = 0;
    public decimal heading { get; set; } = 0;

    public StatePlanePoint Location
    {
      get
      {
        return new StatePlanePoint((double)geox, (double)geoy);
      }
    }

    public CadUnitLocation() { }

    public static List<CadUnitLocation> Get()
    {
      string query = @"
        DECLARE @MaxAvlLogId BIGINT = (SELECT
             MAX(avllogid)
           FROM
             Tracking.dbo.cad_unit_location_data);

        SELECT
          avllogid
          ,[unitcode]
          ,[timestamp] location_timestamp
          ,[inci_id]
          ,[status]
          ,[avstatus]
          ,[geox]
          ,[geoy]
          ,[speed]
          ,[heading]
        FROM
          [cad].[dbo].[vwMaxAvllogidByUnit]
        WHERE
          avllogid > @MaxAvlLogId";
      var data = Program.Get_Data<CadUnitLocation>(query, Program.CS_Type.Tracking);
      return data;
    }

    public static void Save(List<CadUnitLocation> data)
    {
      var dt = CreateDataTable();

      foreach (CadUnitLocation d in data)
      {
        try
        {
          dt.Rows.Add(
            d.unitcode
            ,d.location_timestamp
            ,d.inci_id
            ,d.status
            ,d.avstatus
            ,d.Location.Latitude
            ,d.Location.Longitude
            ,d.speed
            ,d.heading
            ,d.avllogid
          );
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

        MERGE Tracking.dbo.cad_unit_location_data WITH (HOLDLOCK) AS C

        USING @CADUnitLocationData AS CAD ON C.unitcode = CAD.unitcode

        WHEN MATCHED THEN
          
          UPDATE 
          SET
            location_timestamp = CAD.location_timestamp
            ,inci_id = CAD.inci_id
            ,status = CAD.status
            ,avstatus = CAD.avstatus
            ,latitude = CAD.latitude
            ,longitude = CAD.longitude
            ,speed = CAD.speed
            ,heading = CAD.heading
            ,updated_on = @Now
            ,avllogid=CAD.avllogid

        WHEN NOT MATCHED THEN

          INSERT 
            (
              unitcode
              ,location_timestamp
              ,inci_id
              ,status
              ,avstatus
              ,latitude
              ,longitude
              ,speed
              ,heading
              ,updated_on
              ,avllogid
            )
          VALUES (
              CAD.unitcode
              ,CAD.location_timestamp
              ,CAD.inci_id
              ,CAD.status
              ,CAD.avstatus
              ,CAD.latitude
              ,CAD.longitude
              ,CAD.speed
              ,CAD.heading              
              ,@Now
              ,CAD.avllogid
          );";

      try
      {
        using (IDbConnection db = new SqlConnection(Program.GetCS(Program.CS_Type.Tracking)))
        {
          db.Execute(query, new { CADUnitLocationData = dt.AsTableValuedParameter("CADUnitLocationData") });
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
      var dt = new DataTable("CADUnitLocationData");
      dt.Columns.Add("unitcode", typeof(string));
      dt.Columns.Add("location_timestamp", typeof(DateTime));
      dt.Columns.Add("inci_id", typeof(string));
      dt.Columns.Add("status", typeof(string));
      dt.Columns.Add("avstatus", typeof(string));
      dt.Columns.Add("latitude", typeof(decimal));
      dt.Columns.Add("longitude", typeof(decimal));
      dt.Columns.Add("speed", typeof(decimal));
      dt.Columns.Add("heading", typeof(decimal));
      dt.Columns.Add("avllogid", typeof(int));
      return dt;
    }


  }
}
