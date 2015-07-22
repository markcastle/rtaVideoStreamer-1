using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//using System.Windows.Forms;
using System.IO;
 
using System.Windows.Forms;

// -------------------------------------------------
// Developed By : Ragheed Al-Tayeb
// e-Mail       : ragheedemail@gmail.com
// Date         : April 2012
// -------------------------------------------------

namespace RiaService
{

    /// <summary>
    /// Provides a streaming server that can be used to stream any images source
    /// to any client.
    /// </summary>
    public class ImageStreamingServer:IDisposable
    {

        private List<Socket> _Clients;
        private Thread _Thread;
        private MjpegDecoder decoder;
        const int ReplaySize = 100;
        public ImageStreamingServer():this(Screen.Snapshots(600,450,true))
        {

        }
        Queue<byte[]> queue=new Queue<byte[]>();
        public ImageStreamingServer(IEnumerable<Image> imagesSource)
        {

            _Clients = new List<Socket>();
            _Thread = null;

            this.ImagesSource = imagesSource;
            this.Interval = 50;

        }
        int decoderLevel = 0;
        string username, password,url;
        bool IsStill = false;
        System.Timers.Timer tmr = new System.Timers.Timer(1000*20);
        public ImageStreamingServer(string url )
        {
            _Clients = new List<Socket>();
            _Thread = null;
            decoderLevel = 1;
            this.url = url;
            this.decoder = new MjpegDecoder();
            decoder.ParseStream(new Uri(url));
            decoder.Error += decoder_Error;
            this.Interval = 50;
            decoder.OnJpegEvent += decoder_OnJpegEvent;

            tmr.Elapsed += tmr_Elapsed;
            tmr.Start();
        }

        bool IsInTimer = false;

        void tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                 
                if (this.lastUpdateTime == null || IsInTimer)
                    return;

                IsInTimer = true;
                if (DateTime.Now.Subtract(this.lastUpdateTime).TotalSeconds > 20)
                {

                    System.Threading.Thread.Sleep(1000);
                    try
                    {
                        // if (decoder != bull)
                        decoder.Error -= decoder_Error;
                        decoder.OnJpegEvent -= decoder_OnJpegEvent;
                        decoder.StopStream();

                    }
                    catch { ;}
                    lastUpdateTime = DateTime.Now;
                    if (decoderLevel == 1)
                    {
                        this.decoder = new MjpegDecoder();
                        decoder.Error += decoder_Error;
                        decoder.OnJpegEvent += decoder_OnJpegEvent;
                        this.Interval = 50;
                        decoder.ParseStream(new Uri(url));


                    }
                    else if (decoderLevel == 2)
                    {
                        this.decoder = new MjpegDecoder();
                        decoder.Error += decoder_Error;
                        this.Interval = 50;
                        decoder.OnJpegEvent += decoder_OnJpegEvent;
                        decoder.ParseStream(new Uri(url), username, password);


                    }
                }

            }
            catch { ;}
            finally
            {
                IsInTimer = false;
            }
            //throw new NotImplementedException();
        }

        
        public ImageStreamingServer(string url,string username,string password)
        {
            decoderLevel = 2;
            this.url = url;
            this.username = username;
            this.password = password;
            _Clients = new List<Socket>();
            _Thread = null;

            this.decoder = new MjpegDecoder();
            decoder.ParseStream(new Uri(url),username,password);
            decoder.Error += decoder_Error;
            this.Interval = 50;
            decoder.OnJpegEvent += decoder_OnJpegEvent;
            tmr.Elapsed += tmr_Elapsed;
            tmr.Start();
        }


       
        void decoder_Error(object sender, RiaService.ErrorEventArgs e)
        {
           
          //  tmr.Start();
           // throw new NotImplementedException();
        }

        public void Pause()
        {
            temp = queue.ToArray();
            inx = 0;
            IsStill = true;
        }
        public void Resume()
        {
            IsStill = false;
        }
        private byte[] JpegBytes;
        private byte[][] temp;
        int inx = 0;
        DateTime lastUpdateTime=DateTime.Now;

        public byte[] GetImage()
        {
            return JpegBytes;
        }
        void decoder_OnJpegEvent(byte[] data)
        {
            lastUpdateTime = DateTime.Now;
           
            if (!IsStill)
            {

                JpegBytes = data;

                queue.Enqueue(data);
                while (queue.Count > ReplaySize)
                    queue.Dequeue();
            }
            else
            {
                if (temp == null)
                    return;

                if (inx == temp.GetLength(0))
                    inx = 0;
                JpegBytes = temp[inx++];

            }
         
            //throw new NotImplementedException();
        }



        /// <summary>
        /// Gets or sets the source of images that will be streamed to the 
        /// any connected client.
        /// </summary>
        public IEnumerable<Image> ImagesSource { get; set; }

        /// <summary>
        /// Gets or sets the interval in milliseconds (or the delay time) between 
        /// the each image and the other of the stream (the default is . 
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// Gets a collection of client sockets.
        /// </summary>
        public IEnumerable<Socket> Clients { get { return _Clients; } }

        /// <summary>
        /// Returns the status of the server. True means the server is currently 
        /// running and ready to serve any client requests.
        /// </summary>
        public bool IsRunning { get { return (_Thread != null && _Thread.IsAlive); } }

        /// <summary>
        /// Starts the server to accepts any new connections on the specified port.
        /// </summary>
        /// <param name="port"></param>
        /// 
        public void Start(int port)
        {

            lock (this)
            {
                _Thread = new Thread(new ParameterizedThreadStart(ServerThread));
                _Thread.IsBackground = true;
                _Thread.Start(port);
            }

        }

        /// <summary>
        /// Starts the server to accepts any new connections on the default port (8080).
        /// </summary>
        public void Start()
        {
            this.Start(8080);
        }

        public void Stop()
        {

            if (this.IsRunning)
            {
                try
                {
                    _Thread.Join();
                    _Thread.Abort();
                }
                finally
                {

                    lock (_Clients)
                    {
                        
                        foreach (var s in _Clients)
                        {
                            try
                            {
                                s.Close();
                            }
                            catch { }
                        }
                        _Clients.Clear();

                    }

                    _Thread = null;
                }
            }
        }

        /// <summary>
        /// This the main thread of the server that serves all the new 
        /// connections from clients.
        /// </summary>
        /// <param name="state"></param>
        private void ServerThread(object state)
        {

            try
            {
                Socket Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                Server.Bind(new IPEndPoint(IPAddress.Any,(int)state));
                Server.Listen(10);

                System.Diagnostics.Debug.WriteLine(string.Format("Server started on port {0}.", state));
                
                foreach (Socket client in Server.IncommingConnectoins())
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), client);
            
            }
            catch { }

            this.Stop();
        }

        /// <summary>
        /// Each client connection will be served by this thread.
        /// </summary>
        /// <param name="client"></param>
        private void ClientThread(object client)
        {

            Socket socket = (Socket)client;

            System.Diagnostics.Debug.WriteLine(string.Format("New client from {0}",socket.RemoteEndPoint.ToString()));

            lock (_Clients)
                _Clients.Add(socket);

            try
            {
                using (MjpegWriter wr = new MjpegWriter(new NetworkStream(socket, true)))
                {

                    // Writes the response header to the client.
                    wr.WriteHeader();
                    if (decoder != null)
                    {
                        while (true)
                        {
                          
                            if (this.Interval > 0)
                                Thread.Sleep(this.Interval);
                            if (JpegBytes != null)
                                wr.Write(JpegBytes);
                        }
                    }
                    else
                    {
                        // Streams the images from the source to the client.
                        foreach (var imgStream in Screen.Streams(this.ImagesSource))
                        {


                            wr.Write(imgStream);
                        }
                    }
                   

                }
            }
            catch { }
            finally
            {
                lock (_Clients)
                    _Clients.Remove(socket);
            }
        }


        #region IDisposable Members

        public void Dispose()
        {
            this.Stop();
            decoder.StopStream();
        }

        #endregion
    }

    static class SocketExtensions
    {

        public static IEnumerable<Socket> IncommingConnectoins(this Socket server)
        {
            while(true)
                yield return server.Accept();
        }

    }


    static class Screen
    {


        public static IEnumerable<Image> Snapshots()
        {
            return Screen.Snapshots(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height,true);
        }

        /// <summary>
        /// Returns a 
        /// </summary>
        /// <param name="delayTime"></param>
        /// <returns></returns>
        public static IEnumerable<Image> Snapshots(int width,int height,bool showCursor)
        {
            Size size = new Size(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            
            Bitmap srcImage = new Bitmap(size.Width, size.Height);
            Graphics srcGraphics = Graphics.FromImage(srcImage);

            bool scaled = (width != size.Width || height != size.Height);

            Bitmap dstImage = srcImage;
            Graphics dstGraphics = srcGraphics;

            if(scaled)
            {
                dstImage = new Bitmap(width, height);
                dstGraphics = Graphics.FromImage(dstImage);
            }

            Rectangle src = new Rectangle(0, 0, size.Width, size.Height);
            Rectangle dst = new Rectangle(0, 0, width, height);
            Size curSize = new Size(32, 32);

            while (true)
            {
                srcGraphics.CopyFromScreen(0, 0, 0, 0, size);

                if (showCursor)
                    Cursors.Default.Draw(srcGraphics,new Rectangle(Cursor.Position,curSize));

                if (scaled)
                    dstGraphics.DrawImage(srcImage, dst, src, GraphicsUnit.Pixel);
       
                yield return dstImage;

            }

            srcGraphics.Dispose();
            dstGraphics.Dispose();

            srcImage.Dispose();
            dstImage.Dispose();

            yield break;
        }

        internal static IEnumerable<MemoryStream> Streams(this IEnumerable<Image> source)
        {
            MemoryStream ms = new MemoryStream();

            foreach (var img in source)
            {
                ms.SetLength(0);
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                yield return ms;
            }

            ms.Close();
            ms = null;

            yield break;
        }

    }
}
