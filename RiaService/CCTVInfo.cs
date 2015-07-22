using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiaService
{
    public class CCTVInfo
    {
        public string ch { get; set; }
        public string MjpegUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public int StreamServerPort { get; set; }

        public RiaObject riaobject { get; set; }

    }
}
