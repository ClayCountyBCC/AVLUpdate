using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using AVLUpdate.Models;
using System.Configuration;

namespace AVLUpdate
{
  class Program
  {
    public const int appId = 20010;

    static void Main()
    {
    }


    public enum CS_Type : int
    {
      GIS = 1,
      Tracking = 2,
      LOG = 4
    }
    public static string Get_ConnStr(CS_Type cs)
    {
      return ConfigurationManager.ConnectionStrings[cs.ToString()].ConnectionString;
    }
  }
}
