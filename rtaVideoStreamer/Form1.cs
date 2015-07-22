using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using rtaNetworking.Streaming;
using System.Net;
using ETAG;
using System.Threading.Tasks;

namespace rtaVideoStreamer
{
    public partial class Form1 : Form
    {

        private ImageStreamingServer _Server;
        ETAG.ETAGReader reader;
      
        public Form1()
        {
            InitializeComponent();
            this.linkLabel1.Text = string.Format("http://{0}:8080", Environment.MachineName);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
         //  MjpegProcessor.
          //  _Server = new ImageStreamingServer("http://210.241.67.167/abs2mjpg/mjpg?resolution=352x240&camera=1", "admin", "pass");
          // _Server = new ImageStreamingServer("http://192.192.85.20:11000/getimage", "admin", "pass");

            //http://192.168.0.104/cgi/mjpg/mjpeg.cgi
//_Server = new ImageStreamingServer("http://192.168.0.104/cgi/mjpg/mjpeg.cgi", "admin", "admin");

            _Server = new ImageStreamingServer("http://192.192.85.33/axis-cgi/mjpg/video.cgi?camera=1", "root", "pass");
            _Server.Start(8080);

            reader = new ETAGReader("192.192.85.33", 50007);

            reader.OnEtagArriveEvent += reader_OnEtagArriveEvent;
        }

        void reader_OnEtagArriveEvent(EtagInfo info)
        {
            using (System.IO.StreamWriter wr = System.IO.File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "ETAG.log"))
            {
                wr.WriteLine(info.antenna + "," + info.tag_id);
                wr.Close();
            }


            if (info.tag_id == "0x00BFDE232520FF63991F027A" )
            {

                if(info.antenna==1)
                             _Server.Pause();
                if (info.antenna == 7)
                             _Server.Resume();

                //Task.Run(() =>
                //    {
                //        System.Threading.Thread.Sleep(10 * 1000);
                //        _Server.Resume();
                //    }
                //);

            }

            
                
            

          //  throw new NotImplementedException();
        }

        private DateTime time = DateTime.MinValue;

        private void timer1_Tick(object sender, EventArgs e)
        {
            int count = (_Server.Clients != null) ? _Server.Clients.Count() : 0;

            this.sts.Text = "Clients: " + count.ToString();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(this.linkLabel1.Text);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            _Server.Pause();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            _Server.Resume();
        }

    }


}
