using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using AVLUpdate.Models.Tracking;

namespace AVLUpdate.Models.AirVantage
{
  public class AirVantageControl
  {
    private const int SecondsToWait = 5 * 60;
    private AccessToken Token { get; set; } = null;
    private DateTime DataTimeOut { get; set; } = DateTime.MinValue;    
    private bool IsExpired
    {
      get
      {
        return DataTimeOut < DateTime.Now;
      }
    }
    // The airvantage data will tell us the phone number
    // and IMEI of the units.  The phone number is tied
    // to the sim card, the IMEI is tied to the AVL
    // hardware.
    // Really, it only matters if an AVL breaks and 
    // has to be replaced.

    public AirVantageControl()
    {
      Token = AccessToken.Authenticate();      
    }

    public List<AirVantageData> Update()
    {
      // we only query this again if the IsExpired field is true, otherwise
      // we return an empty list.
      // We return if we actually updated anything, this will be used
      // to indicate that a refresh of the unit_tracking data is needed.
      try
      {
        var avlNew = new List<AirVantageData>();
        if (IsExpired)
        {
          if (Token != null && Token.isExpired)
          {
            Token = AccessToken.Authenticate();
            if (Token.isExpired) return avlNew;
          }

          var avd = AirVantageData.Get(Token);
          if (avd == null) return avlNew;
          DataTimeOut = DateTime.Now.AddSeconds(SecondsToWait);
          return avd.ToList(); ;
        }
        return avlNew;
      }
      catch(Exception ex)
      {
        new ErrorLog(ex);
        return null;
      }
    }

  }
}
