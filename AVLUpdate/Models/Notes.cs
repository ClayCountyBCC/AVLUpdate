using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVLUpdate.Models
{
  class Notes
  {
    // regex pattern to match notes: \s+\[\d\d/\d\d/\d\d\s\d\d:\d\d:\d\d\s\w+]
    // regex pattern to match unit comments: \[\w+\-\w+\] {\w+}\s+
    // both patterns together, any matches should be removed: \s+\[\d\d/\d\d/\d\d\s\d\d:\d\d:\d\d\s\w+]|\[\w+\-\w+\] {\w+}\s+
  }
}
