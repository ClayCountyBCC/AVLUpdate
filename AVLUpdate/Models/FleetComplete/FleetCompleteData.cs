using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.FleetComplete
{
  public class FleetCompleteData
  {
    public int StatusCode { get; set; }
    public List<Asset> Data { get; set; } = new List<Asset>();
    public List<Error> Errors { get; set; } = new List<Error>();

    public FleetCompleteData()
    {

    }

  }
}


//{
//  "StatusCode": 100,

//  "Errors": [
//    {
//      "Message": "string",
//      "StackTrace": "string",
//      "ExceptionName": "string",
//      "Severity": 0
//    }
//  ]
//}