using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.FleetComplete
{
  public class Error
  {
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public string ExceptionName { get; set; }
    public int Severity { get; set; }

    public Error()
    {

    }
  }
}
      