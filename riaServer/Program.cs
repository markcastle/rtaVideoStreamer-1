using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RiaService;
using System.ServiceModel;

namespace riaServer
{
    class Program
    {
        static void Main(string[] args)
        {

        RestService service=    new RestService();
            ServiceHost  host=new ServiceHost(service);
            host.Open();
          // CCTVInfo [] info=new CCTVInfo[]{  
          //    new CCTVInfo(){ ch=1, MjpegUrl= "http://192.192.85.33/axis-cgi/mjpg/video.cgi?camera=1", UserName= "root", Password= "pass", StreamServerPort= 9090},
          //    new CCTVInfo(){ ch=2, MjpegUrl= "http://192.192.85.33:204/axis-cgi/mjpg/video.cgi?camera=1", UserName= "root", Password= "pass", StreamServerPort= 9091},
          //    new CCTVInfo(){ ch=3 ,MjpegUrl= "http://192.192.85.33/axis-cgi/mjpg/video.cgi?camera=1", UserName= "root", Password= "pass", StreamServerPort= 9092},
          //    new CCTVInfo(){ ch=4, MjpegUrl= "http://192.192.85.33:204/axis-cgi/mjpg/video.cgi?camera=1", UserName= "root", Password= "pass", StreamServerPort= 9093}
          
          //};


          //ETAGReader reader= new ETAGReader("192.192.85.33", 50007);
          //RiaObject obj = new RiaObject(info, reader, 1, 7);
           char c;
          //RiaObject.AddWhiteList("0X00BFDE232520FF63991F027B");
          //RiaObject.AddWhiteList("0X110514000000000000000386");
          while(true)
            {
                c = Console.ReadKey().KeyChar;
                switch (c)
                {
                    case '0':
                        RiaObject.DisableETagEffect();
                        break;
                    case '1':
                        RiaObject.EnableETagEffect();
                        break;
                }

            }
        }
    }
}
