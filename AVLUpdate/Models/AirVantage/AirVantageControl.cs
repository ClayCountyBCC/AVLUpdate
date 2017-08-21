using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.AirVantange
{
  public class AirVantageControl
  {
    private AccessToken Token { get; set; }

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

    public List<AirVantageData> Get()
    {
      if (Token.isExpired)
      {
        Token = AccessToken.Authenticate();
        if (Token.isExpired) return null;
      }
      return AirVantageData.Get(Token).ToList();
    }



  }
}
