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
    public string unitUsing { get; set; } = null;
    public DateTime dateUpdated { get; set; }
    public decimal longitude { get; set; }
    public decimal latitude { get; set; }
    public DateTime dateLastCommunicated { get; set; }

    public UnitTracking()
    {

    }

    public static List<UnitTracking> Get()
    {
      string query = @"
        SELECT 
          unitcode,
          unit_using unitUsing,
          date_update dateUpdated,
          longitude,
          latitude,
          date_last_communicated dateLastCommunicated
        FROM unit_tracking_data
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
          latitude=@latitude
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
