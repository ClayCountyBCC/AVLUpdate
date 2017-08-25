using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AVLUpdate.Models.Tracking;

namespace AVLUpdate.Models.AirVantange
{
  public class AirVantageControl
  {
    private const int MinutesToWait = 5;
    private AccessToken Token { get; set; }
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

    private void Update()
    {
      // we only query this again if the IsExpired field is true, otherwise
      // we return an empty list.
      if (IsExpired) 
      {
        if (Token.isExpired)
        {
          Token = AccessToken.Authenticate();
          if (Token.isExpired) return;
        }

        var avd = AirVantageData.Get(Token).ToList();
        DataTimeOut = DateTime.Now.AddMinutes(MinutesToWait);
        foreach (AirVantageData a in avd)
        {
          a.UpdateUnitTracking();
        }
        return;
      }    
    }

  }
}
