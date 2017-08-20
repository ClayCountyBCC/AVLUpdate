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
  public class Unit
  {
    public string unitcode { get; set; }
    public string source { get; set; }
    public long imei { get; set; }
    public long phonenumber { get; set; }
    public Unit()
    {

    }

    public static List<Unit> Get()
    {
      string query = @"
        SELECT 
          unitcode,
          source,
          imei,
          phonenumber
        FROM valid_unit_list
        ORDER BY unitcode ASC";
      try
      {
        return Program.Get_Data<Unit>(query, Program.CS_Type.Tracking);
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return null;
      }
    }

    public static void UpdateBaseUnits()
    {
      string query = @"
        INSERT INTO valid_unit_list(unitcode, source) (
            SELECT DISTINCT 
              unitcode, 
              'PS' 
            FROM cad.dbo.unit 
            WHERE 
              status='A' 
            AND unitcode NOT IN (
              SELECT DISTINCT unitcode 
              FROM valid_unit_list)
        )";
      try
      {
        using (IDbConnection db = new SqlConnection(Program.Get_ConnStr(Program.CS_Type.Tracking)))
        {
          db.Execute(query);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
      }
    }

    public void Update()
    {
      string query = @"
        UPDATE valid_unit_list
        SET 
          imei=@imei, 
          phonenumber=@phonenumber
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
