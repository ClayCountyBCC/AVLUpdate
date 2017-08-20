using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using AVLUpdate.Models;
using AVLUpdate.Models.AirVantange;
using AVLUpdate.Models.GIS;
using AVLUpdate.Models.Tracking;

namespace AVLUpdate
{
  class Program
  {
    public const int appId = 20010;

    static void Main()
    {
      // Main loop here
      DateTime endTime = DateTime.Today.AddHours(5).AddMinutes(55);
      if (DateTime.Now.Hour > 5)
      {
        endTime = DateTime.Today.AddDays(1).AddHours(5).AddMinutes(55);
      }
      var avl = new AirVantageControl();

      while (DateTime.Now < endTime)
      {
        try
        {
          var avd = avl.Get();
          var nophone = (from a in avd
                         where !a.subscriptions.First().mobileNumber.HasValue
                         select a).ToList();
          var unitLocs = UnitLocation.Get();
          var i = avd.Count();

        }catch(Exception ex)
        {
          new ErrorLog(ex);
        }
      }
   }

    #region " Data Code "

    public static List<T> Get_Data<T>(string query, CS_Type cs)
    {
      try
      {
        using (IDbConnection db = new SqlConnection(Get_ConnStr(cs)))
        {
          return (List<T>)db.Query<T>(query);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return null;
      }
    }

    public static List<T> Get_Data<T>(string query, DynamicParameters dbA, CS_Type cs)
    {
      try
      {
        using (IDbConnection db = new SqlConnection(Get_ConnStr(cs)))
        {
          return (List<T>)db.Query<T>(query, dbA);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return null;
      }
    }

    public static int Exec_Query(string query, DynamicParameters dbA, CS_Type cs)
    {
      try
      {
        using (IDbConnection db = new SqlConnection(Get_ConnStr(cs)))
        {
          return db.Execute(query, dbA);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return -1;
      }
    }

    public static int Exec_Scalar(string query, DynamicParameters dbA, CS_Type cs)
    {
      try
      {
        using (IDbConnection db = new SqlConnection(Get_ConnStr(cs)))
        {
          return db.ExecuteScalar<int>(query, dbA);
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return -1;
      }
    }

    public enum CS_Type : int
    {
      GIS = 1,
      Tracking = 2,
      LOG = 4,
      User = 8,
      Password = 16,
      client_id=32,
      client_secret=64
    }

    public static string Get_ConnStr(CS_Type cs)
    {
      return ConfigurationManager.ConnectionStrings[cs.ToString()].ConnectionString;
    }
#endregion

  }
}
