using System;
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
      if (IsExpired)
      {

        var fcd = FleetCompleteData.Get(Token);
        DataTimeOut = DateTime.Now.AddSeconds(SecondsToWait);
        return fcd;
      }
      return null;
    }
  }
}
