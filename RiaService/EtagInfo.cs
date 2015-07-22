using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiaService
{
   public class EtagInfo
    {

      public  string tag_id{get;set;}
      public int  antenna { get; set; }
      public int rssi { get; set; }

      public override string ToString()
      {
          return string.Format("tagid:{0} atenna:{1} rssi:{2}", tag_id, antenna, rssi);
      }
    }
}
