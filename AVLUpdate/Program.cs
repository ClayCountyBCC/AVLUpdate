﻿using System;
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
using System.Threading;
using System.Net;
using System.IO;

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

      while (DateTime.Now < endTime) // we want this program to run from 6 AM to 5:55 AM
      {
        try
        {
          // Goals
          // update the data from Airvantage every 5 minutes
          // update the data from GIS every 10 seconds
          // update the data from FleetComplete every 30 seconds?




          //var avd = avl.Update();
          //var nophone = (from a in avd
          //               where !a.subscriptions.First().mobileNumber.HasValue
          //               select a).ToList();
          //var labels = (from a in avd
          //              where a.labels.Count() > 1 || a.labels.Count() == 0
          //              select a).ToList();
          //var unitLocs = UnitLocation.Get();
          var i = 0;

          Thread.Sleep(10000);
        }catch(Exception ex)
        {
          new ErrorLog(ex);
        }
      }
   }

    public static string GetJSON(string url, WebHeaderCollection hc = null)
    {
      var wr = HttpWebRequest.Create(url);
      wr.ContentType = "application/json";
      if(hc != null) // Added this bit for the Fleet Complete Headers that are derived from the Authentication information.
      {
        foreach (string key in hc.AllKeys)
        {
          wr.Headers.Add(key, hc[key]);
        }        
      }
      string json = "";
      try
      {
        var response = wr.GetResponse();
        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
        {
          json = sr.ReadToEnd();
          return json;
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, json);
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
      FC_Client_Id = 10
    }

    public static string GetCS(CS_Type cs)
    {
      return ConfigurationManager.ConnectionStrings[cs.ToString()].ConnectionString;
    }
#endregion

  }
}