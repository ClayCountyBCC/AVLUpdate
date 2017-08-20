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
