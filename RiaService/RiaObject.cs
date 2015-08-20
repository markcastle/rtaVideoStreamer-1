using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiaService
{
     //  public delegate void OnETagComingEventHandler(object sender, string etagid);


   public enum ConstExecuteMode
    {
        Normal,
        Stop
    }
   public  class RiaObject
    {
   //    public event OnETagComingEventHandler OnEtagComingEvent;
       public static System.Collections.Generic.Dictionary<string, string> dictWhiteListLookUp=new Dictionary<string,string>();
       static System.Collections.Generic.List<RiaObject> objList = new List<RiaObject>();
        static ConstExecuteMode ExecuteMode = ConstExecuteMode.Normal;
       ImageStreamingServer[] _Servers;
       System.Collections.Generic.Dictionary<string, ImageStreamingServer> dictChStreamServer = new Dictionary<string, ImageStreamingServer>();
       int EnterChno, LeaveChno;
       public RiaObject(CCTVInfo[] cctvInfos/*string CCTVMJpegString,string cctvUserName,string cctvUserPwd,int MjpegBroadcasePort*/, ETAGReader reader, int enterChno, int leaveChno)
       {

           this.EnterChno = enterChno;
           this.LeaveChno = leaveChno;
           _Servers=new ImageStreamingServer[cctvInfos.Length];
           for (int i=0;i<cctvInfos.Length;i++)
           {
               cctvInfos[i].riaobject = this;
              
               _Servers[i] = new ImageStreamingServer(cctvInfos[i].MjpegUrl, cctvInfos[i].UserName, cctvInfos[i].Password);
               _Servers[i].Start(cctvInfos[i].StreamServerPort);

               dictChStreamServer.Add(cctvInfos[i].ch, _Servers[i]);
           }
         //  _Server = new ImageStreamingServer("http://192.192.85.33/axis-cgi/mjpg/video.cgi?camera=1", "root", "pass");
          // _Server = new ImageStreamingServer(CCTVMJpegString, cctvUserName, cctvUserPwd);

           
       //    _Server = new ImageStreamingServer(CCTVMJpegString, cctvUserName, cctvUserPwd);
         //  _Server.Start(MjpegBroadcasePort);

          //  reader = new ETAGReader("192.192.85.33", 50007);

            reader.OnEtagArriveEvent += reader_OnEtagArriveEvent;
            objList.Add(this);
        }
       public byte[] GetImage(string ch)
       {
           if(dictChStreamServer.ContainsKey(ch))
               return dictChStreamServer[ch].GetImage();
           else
             throw new Exception("channl:"+ch+" not found!");
       }

       public static void DisableETagEffect()
       {
           ExecuteMode = ConstExecuteMode.Stop;
           foreach (RiaObject obj in objList)
               obj.Resume();
       }

       public static ConstExecuteMode GetExecutionMode()
       {
           return ExecuteMode;
       }

       public static void EnableETagEffect()
       {
           ExecuteMode = ConstExecuteMode.Normal;
           //foreach (RiaObject obj in objList)
           //    obj._Server.Resume();
       }
       public static void AddWhiteList(string etagid)
       {

           etagid = etagid.ToUpper();
           if (!dictWhiteListLookUp.ContainsKey(etagid))
               dictWhiteListLookUp.Add(etagid, etagid );
       }
       public void Close()
       {
           foreach (ImageStreamingServer streamServer in _Servers)
           {
               streamServer.Stop();
               streamServer.Dispose();
           }
           

       }

       

       public void Pause()
       {
           foreach (ImageStreamingServer streamServer in _Servers)
               streamServer.Pause();
       }

       public void Resume()
       {

           foreach (ImageStreamingServer streamServer in _Servers)
               streamServer.Resume();
       }
        void reader_OnEtagArriveEvent(EtagInfo info)
        {
            //using (System.IO.StreamWriter wr = System.IO.File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "ETAG.log"))
            //{
            //    wr.WriteLine(info.antenna + "," + info.tag_id);
            //    wr.Close();
            //}

            Console.WriteLine(info.antenna + "," + info.tag_id);
         //   if (info.tag_id == "0x00BFDE232520FF63991F027A" )

            if (ExecuteMode == ConstExecuteMode.Stop)
            {
                Resume();
                return;
            }



            if (dictWhiteListLookUp.ContainsKey(   info.tag_id.ToUpper()))
            {

                if(info.antenna==EnterChno)
                             Pause();
                if (info.antenna == LeaveChno)
                             Resume();

                //Task.Run(() =>
                //    {
                //        System.Threading.Thread.Sleep(10 * 1000);
                //        _Server.Resume();
                //    }
                //);

            }

            
                
            

          //  throw new NotImplementedException();
        }
    }

   //internal class CCTVInfo
   //{
   //    public string IP { get; set; }
   //    public str
   //}

   
}
