using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using AVLUpdate.Models;
using AVLUpdate.Models.AirVantage;
using AVLUpdate.Models.GIS;
using AVLUpdate.Models.Tracking;
using AVLUpdate.Models.FleetComplete;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Diagnostics;

namespace AVLUpdate
{
  public class Program
  {
    public const int appId = 20010;
    public const string GISDataErrorEmailAddresses = "Daniel.McCartney@claycountygov.com;";

    public static void Main()
    {
      //var now = DateTime.Now.ToUniversalTime();
      //var tickstart = DateTime.Parse("1/1/1970").ToUniversalTime();
      //double i = now.Subtract(tickstart).TotalMilliseconds;


      // Main loop here
      DateTime endTime = DateTime.Today.AddHours(5);
      if (DateTime.Now.Hour >= 5)
      {
        endTime = DateTime.Today.AddDays(1).AddHours(5); // 5 am
      }
      // init the base objects.      
      var utc = new UnitTrackingControl();      
      var avl = new AirVantageControl();      
      var fcc = new FleetCompleteControl();

      while (DateTime.Now < endTime) // we want this program to run from 6 AM to 5:55 AM
      {
        try
        {
          // pull in the current state of the unit_tracking_data table 
          // this will also update the most recent unitUsing data.
          utc.UpdateTrackingData();

          utc.UpdateAirVantage(avl.Update()); // update the data from Airvantage every 5 minutes
          // we update the AirVantage data before we update the GIS/AVL data because
          // we might've updated a unit's imei / phone number in the mean time.
          utc.UpdateGISUnitLocations(UnitLocation.Get());// update the data from GIS every 10 seconds

          utc.UpdateFleetComplete(fcc.Update()); // update the fleet complete data every 30 seconds.

          utc.Save(Program.CS_Type.Tracking); // Save the data to SQL
          //utc.Save(Program.CS_Type.GISTracking);
          Thread.Sleep(7000); // this may not be needed if we await/async these calls.
        }
        catch (Exception ex)
        {
          new ErrorLog(ex);
        }
      }
   }

    public static string GetJSON(string url, WebHeaderCollection hc = null )
    {
      ServicePointManager.ReusePort = true;
      ServicePointManager.Expect100Continue = true;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
      var wr = HttpWebRequest.Create(url);
      wr.Timeout = 40000;
      wr.Proxy = null;
      wr.ContentType = "application/json";      
      if (hc != null) // Added this bit for the Fleet Complete Headers that are derived from the Authentication information.
      {
        foreach (string key in hc.AllKeys)
        {
          wr.Headers.Add(key, hc[key]);
        }
      }      

      string json = "";
      try
      {
        using (var response = wr.GetResponse())
        {
          if (response != null)
          {
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
              json = sr.ReadToEnd();
              return json;
            }
          }
        }
        return null;
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, url + '\n' + json);
        return null;
      }
    }

    #region " Data Code "

    public static List<T> Get_Data<T>(string query, CS_Type cs)
    {
      try
      {
        using (IDbConnection db = new SqlConnection(GetCS(cs)))
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
        using (IDbConnection db = new SqlConnection(GetCS(cs)))
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
        using (IDbConnection db = new SqlConnection(GetCS(cs)))
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
        using (IDbConnection db = new SqlConnection(GetCS(cs)))
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
      LOG = 3,
      AV_User = 4,
      AV_Password = 5,
      AV_Client_Id = 6,
      AV_Client_Secret = 7,
      FC_User = 8,
      FC_Password = 9,
      FC_Client_Id = 10,
      GISTracking=11
    }

    public static string GetCS(CS_Type cs)
    {
      return ConfigurationManager.ConnectionStrings[cs.ToString()].ConnectionString;
    }
#endregion

  }
}
