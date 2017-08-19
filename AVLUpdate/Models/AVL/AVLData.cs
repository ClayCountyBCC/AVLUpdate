using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models.AVL
{
  class AVLData
  {
    public string uid { get; set; }
    public string name { get; set; }
    public gateway gateway { get; set; }
    public List<string> labels { get; set; }
    public List<subscription> subscriptions { get; set; } = new List<subscription>();

    public AVLData()
    {

    }

  }
}
