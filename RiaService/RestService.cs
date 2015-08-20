using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace RiaService
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class RestService:IRestService
    {
        RiaObject obj;

        public RestService()
        {
            CCTVInfo[] info = new CCTVInfo[]{  
              new CCTVInfo(){ ch="1", MjpegUrl= "http://192.192.85.33/axis-cgi/mjpg/video.cgi?camera=1", UserName= "root", Password= "pass", StreamServerPort= 9090},
              new CCTVInfo(){ ch="2", MjpegUrl= "http://192.192.85.33:204/axis-cgi/mjpg/video.cgi?camera=1", UserName= "root", Password= "pass", StreamServerPort= 9091},
              new CCTVInfo(){ ch="3" ,MjpegUrl= "http://192.192.85.33/axis-cgi/mjpg/video.cgi?camera=1", UserName= "root", Password= "pass", StreamServerPort= 9092},
              new CCTVInfo(){ ch="4", MjpegUrl= "http://192.192.85.33:204/axis-cgi/mjpg/video.cgi?camera=1", UserName= "root", Password= "pass", StreamServerPort= 9093}
          
          };


            ETAGReader reader = new ETAGReader("192.192.85.33", 50007);
              obj = new RiaObject(info, reader, 1, 7);
            
            RiaObject.AddWhiteList("0X00BFDE232520FF63991F027B");
            RiaObject.AddWhiteList("0X110514000000000000000386");
        }

        public System.IO.Stream jpg(string ch)
        {
            MemoryStream ms = new MemoryStream();
            byte[] data = obj.GetImage(ch);
            ms.Write(data,0,data.Length);
            //bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            ms.Position = 0;
            WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";
            return ms;

        }


        public void EnableETagEffect(bool IsEnable)
        {
            if (IsEnable)
                RiaObject.EnableETagEffect();
            else
                RiaObject.DisableETagEffect();
        }

        public bool GetIsEnableETagEffect()
        {

            return (RiaObject.GetExecutionMode() == ConstExecuteMode.Normal);
           // throw new NotImplementedException();
        }
    }
}
