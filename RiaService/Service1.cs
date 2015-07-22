using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;

namespace RiaService
{
    public partial class Service1 : ServiceBase
    {
        ETAGReader reader;
        RiaObject obj;
        ServiceHost host;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
             reader = new ETAGReader("192.192.85.33", 50007);
             CCTVInfo[] infos = new CCTVInfo[]{

                 new CCTVInfo(){  MjpegUrl="http://192.192.85.33/axis-cgi/mjpg/video.cgi?camera=1", UserName= "root", Password="pass", StreamServerPort=9090}
               };
              obj= new RiaObject(infos, reader, 1, 7);
           // 0x00BFDE232520FF63991F027A
            RiaObject.AddWhiteList("0x00BFDE232520FF63991F027A");

            host = new ServiceHost(new Service1());
        }

        protected override void OnStop()
        {

            reader.Close();
            obj.Close();
            
        }
    }
}
