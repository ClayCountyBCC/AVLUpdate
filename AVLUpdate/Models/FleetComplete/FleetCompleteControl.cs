using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.FleetComplete
{
  // some information here:
  // api documentation
  // http://hosted.fleetcomplete.com/v8_3_0/Integration/WebAPI/fleet-docs/dist/index.html#!/Asset/Asset_GetAssets
  
  public class FleetCompleteControl
  {
    private const int SecondsToWait = 30;
    private AccessToken Token { get; set; }
    private DateTime DataTimeOut { get; set; } = DateTime.MinValue;
    private bool IsExpired
    {
      get
      {
        return DataTimeOut < DateTime.Now;
      }
    }

    public FleetCompleteControl()
    {
      Token = AccessToken.Authenticate();
    }

    public FleetCompleteData Update()
    {
      // we only query this again if the IsExpired field is true, otherwise
      // we return an empty list.
      // We return if we actually updated anything, this will be used
      // to indicate that a refresh of the unit_tracking data is needed.
      try
      {
        if (IsExpired)
        {

          var fcd = FleetCompleteData.Get(Token);
          if (fcd == null || fcd.Data == null) return null;
          // let's remove the invalids
          fcd.Data = (from d in fcd.Data
                      where d.IsValid
                      select d).ToList();
          DataTimeOut = DateTime.Now.AddSeconds(SecondsToWait);
          return fcd;
        }
      }
      catch(Exception ex)
      {
        new ErrorLog(ex);
      }
      return null;
    }
  }
}
