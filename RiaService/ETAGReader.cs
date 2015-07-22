using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace RiaService
{

    public delegate void EtagEventHandler(EtagInfo info);
    public class ETAGReader
    {
        public event EtagEventHandler OnEtagReportEvent;
        public event EtagEventHandler OnEtagArriveEvent;
        public event EtagEventHandler OnEtagDepartEvent;
        public string IP { get; set; }
        public int  CmdPort { get; set; }
        public int  EventPort { get; set; }
        TcpClient tcp,evttcp;
        string ConnectionID;
        System.Threading.Thread ThEventTcpTask;
        System.Threading.Timer tmr ;//= new System.Threading.Timer();

      public   ETAGReader(string ip, int cmdport )
        {
            this.IP = ip;
            this.CmdPort = cmdport;
           // this.EventPort = eventport;

            init_tcp();
            Console.WriteLine("Set operation mode to active");
            SetOperationModeActive();

            Console.WriteLine("Version:" + GetVersion());
            EventPort = GetEventPort();
            Console.WriteLine("Event port:" + EventPort);
            Console.WriteLine("model:" + GetModel());
            Console.WriteLine("Antenna detect count:"+GetAtennaDetectCount());
            Console.WriteLine("Antenna port count:" + GetAtennaPortCount());
            Console.WriteLine("Error Status:" + GetErrorStatus());

            tmr = new System.Threading.Timer(new System.Threading.TimerCallback(TmrCallback));
            tmr.Change(0, 10000);
        }
      public void Close()
      {
          try
          {
              tcp.Close();
              evttcp.Close();
          }catch{;}
      }
      void TmrCallback(object stateobj)
      {
          if (this.tcp != null && !tcp.Connected)
          {
              Console.WriteLine("Etag reader reconnecting....");
              init_tcp();
          }
      }
      bool IsInInitTCP;
        void init_tcp()
        {

            if (IsInInitTCP)
                return;
            IsInInitTCP = true;
           
            
            //while (true)
            //{
                try
                {
                    if (tcp != null)
                    {
                        tcp.Close();

                    }
                    if (evttcp != null)
                    {
                        evttcp.Close();

                        if (ThEventTcpTask.IsAlive)
                        {
                            ThEventTcpTask.Abort();
                            ThEventTcpTask.Join();
                        }
                    }
                    tcp = new TcpClient();
                    tcp.Connect(IP, CmdPort);
                    evttcp = new TcpClient();
                    evttcp.Connect(IP, GetEventPort());
                    ThEventTcpTask = new System.Threading.Thread(EventTcpTask);
                    System.IO.StreamReader rd = new System.IO.StreamReader(evttcp.GetStream());
                    string s = rd.ReadLine();
                    Console.WriteLine(s);
                    ConnectionID = s.Split('=')[1].Trim();
                    RegisterReportEvent();
                    RegisterReportArrive();
                    RegisterReportDepart();
                    ThEventTcpTask.Start();
                  //  break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "," + ex.StackTrace);
                }

              //  System.Threading.Thread.Sleep(1000);

            //}


            IsInInitTCP = false;
            
        }
        private void RegisterReportDepart()
        {
            if (!tcp.Connected)
            {

                try
                {
                    init_tcp();

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            lock (tcp)
            {
                System.IO.StreamWriter wr = new System.IO.StreamWriter(tcp.GetStream());

                wr.Write(string.Format("reader.events.register(id={0},name=event.tag.depart)\x0d\x0a\x0d\x0a", ConnectionID));
                wr.Flush();
                string res = GetReturnString();

                //  Console.WriteLine(res);

            }
        }
        private void RegisterReportArrive()
        {

            if (!tcp.Connected)
            {

                try
                {
                    init_tcp();

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            lock (tcp)
            {
                System.IO.StreamWriter wr = new System.IO.StreamWriter(tcp.GetStream());
                wr.Write("tag.reporting.arrive_fields=tag_id antenna rssi\x0d\x0a\x0d\x0a");
                wr.Flush();
                string res = GetReturnString();
                wr.Write(string.Format("reader.events.register(id={0},name=event.tag.arrive)\x0d\x0a\x0d\x0a", ConnectionID));
                wr.Flush();
                 res = GetReturnString();

                 Console.WriteLine(res);

            }
        }

        public void RegisterReportEvent()
        {
           // reader.events.register(id=20,name=event.tag.report)
            if (!tcp.Connected)
            {

                try
                {
                    init_tcp();

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            lock (tcp)
            {
                System.IO.StreamWriter wr = new System.IO.StreamWriter(tcp.GetStream());
                
                    wr.Write(string.Format("reader.events.register(id={0},name=event.tag.report)\x0d\x0a\x0d\x0a",ConnectionID));
                    wr.Flush();
                    string res = GetReturnString();
                
                //  Console.WriteLine(res);
                
            }
        }


        EtagInfo ToEtagInfo(string str)
        {
            string[] pairs = str.Split(new char[] { ' ' });    //[1].Trim().Split(new char[]{',',' '});
            EtagInfo info=new EtagInfo();
          foreach (string s in pairs)
          {
              string value, key;
              if (!s.Contains('='))
                  continue;
              string temp = s.Trim().TrimEnd(new char[] { ',' });
               string[] res =temp.Split(new char[] {'=' });
              key = res[0];
              value = res[1];
              if (key == "tag_id")
                  info.tag_id = value;
              else if (key == "rssi")
                  info.rssi = System.Convert.ToInt32(value);
              else if(key=="antenna")
                  info.antenna = System.Convert.ToInt32(value);
          }
          return info;

        }

        public void EventTcpTask( )
        {
            System.IO.StreamReader rd = new System.IO.StreamReader(evttcp.GetStream());
            while (true)
            {
                try
                {
                    string s = rd.ReadLine();
                    //  Console.WriteLine(rd.ReadLine());
                    if (s.StartsWith("event.tag.report"))
                    {
                        if (this.OnEtagReportEvent != null)
                        {
                            this.OnEtagReportEvent(ToEtagInfo(s));
                        }
                    }
                    else if (s.StartsWith("event.tag.arrive"))
                    {
                        if (this.OnEtagArriveEvent != null)
                        {
                            this.OnEtagArriveEvent(ToEtagInfo(s));
                        }
                    }
                    else if (s.StartsWith("event.tag.depart"))
                    {
                        if (this.OnEtagDepartEvent != null)
                        {
                            this.OnEtagDepartEvent(ToEtagInfo(s));
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "," + ex.StackTrace);
                };
               

            }
            
        }
        public string ReadTag()
        {// tag.read_id() 


            if (!tcp.Connected)
            {

                try
                {
                    init_tcp();

                }
                catch(Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            lock (tcp)
            {
                System.IO.StreamWriter wr = new System.IO.StreamWriter(tcp.GetStream());
                

                wr.Write("tag.read_id()\x0d\x0a\x0d\x0a");
                wr.Flush();
                string res = GetReturnString();
                //  Console.WriteLine(res);
                if (res.TrimStart(new char[] { 'o', 'k' }).Trim() == "")
                    return "";
                else
                return res.TrimStart(new char[] { 'o', 'k' }).Trim().Split( new char[]{'='})[1];
           
               }
        }
        public string GetErrorStatus()
        {
            if (!tcp.Connected)
            {

                try
                {
                    init_tcp();

                }
                catch
                {
                }
            }
            lock (tcp)
            {
                System.IO.StreamWriter wr = new System.IO.StreamWriter(tcp.GetStream());

                wr.Write("diag.error_status\x0d\x0a\x0d\x0a");
                wr.Flush();
                string res = GetReturnString();
                //  Console.WriteLine(res);
                return  res.TrimStart(new char[] { 'o', 'k' }).Trim();
            }
        }
        public int GetAtennaPortCount()
        {
            //antennas.detected
            if (!tcp.Connected)
            {

                try
                {
                    init_tcp();

                }
                catch
                {
                }
            }
            lock (tcp)
            {
                System.IO.StreamWriter wr = new System.IO.StreamWriter(tcp.GetStream());

                wr.Write("antennas.port_count\x0d\x0a\x0d\x0a");
                wr.Flush();
                string res = GetReturnString();
                //  Console.WriteLine(res);
                return System.Convert.ToInt32(res.TrimStart(new char[] { 'o', 'k' }).Trim());
            }
        }
        public int GetAtennaDetectCount()
        {
            //antennas.detected
            if (!tcp.Connected)
            {

                try
                {
                    init_tcp();

                }
                catch
                {
                }
            }
            lock (tcp)
            {
                System.IO.StreamWriter wr = new System.IO.StreamWriter(tcp.GetStream());

                wr.Write("antennas.detected\x0d\x0a\x0d\x0a");
                wr.Flush();
                string res = GetReturnString();
                //  Console.WriteLine(res);
                return  (res.TrimStart(new char[] { 'o', 'k' }).Trim().Split(new char[]{' '}).Length);            //  System.Convert.ToInt32(res.TrimStart(new char[] { 'o', 'k' }).Trim());
            }
        }
        public string GetVersion()
        {
            if (!tcp.Connected)
            {

                try
                { init_tcp(); }
                catch
                {
                }
            }
            lock (tcp)
            {
                System.IO.StreamWriter wr = new System.IO.StreamWriter(tcp.GetStream());

                wr.Write("version.sw\x0d\x0a\x0d\x0a");
                wr.Flush();
                string res = GetReturnString();
                //  Console.WriteLine(res);
                return  res.TrimStart(new char[] { 'o', 'k' }).Trim();
            }
        }

        public string GetModel()
        {
            if (!tcp.Connected)
            {

                try
                {
                    init_tcp();

                }
                catch
                {
                }
            }
            lock (tcp)
            {
                System.IO.StreamWriter wr = new System.IO.StreamWriter(tcp.GetStream());

                wr.Write("info.model\x0d\x0a\x0d\x0a");
                wr.Flush();
                string res = GetReturnString();
                //  Console.WriteLine(res);
                return  res.TrimStart(new char[] { 'o', 'k' });
            }
         
        }

        public int  GetEventPort()
        {
            if (!tcp.Connected)
            {

                try
                {
                    init_tcp();
                    
                }
                catch
                {
                }
            }
                lock (tcp)
                {
                    System.IO.StreamWriter wr = new System.IO.StreamWriter(tcp.GetStream());
                
                    wr.Write("com.network.reader.event_port\x0d\x0a\x0d\x0a");
                    wr.Flush();
                    string res = GetReturnString();
                //  Console.WriteLine(res);
                  return System.Convert.ToInt32(res.TrimStart(new char[] { 'o', 'k' }).Trim());
                }
         

               
        }

        public  void  SetOperationModeActive()
        {

            if (!tcp.Connected)
            {

                try
                {
                    init_tcp();

                }
                catch
                {
                }
            }
            lock (tcp)
            {
                System.IO.StreamWriter wr = new System.IO.StreamWriter(tcp.GetStream());

                wr.Write(" setup.operating_mode = active \x0d\x0a\x0d\x0a");
                wr.Flush();
                GetReturnString();
                //  Console.WriteLine(res);
              //  return res.TrimStart(new char[] { 'o', 'k' });
            }
        }
        public  string  GetReturnString()
        {
            System.IO.StreamReader rd = new System.IO.StreamReader(tcp.GetStream());
            StringBuilder sb = new StringBuilder();
           while(true){
            int ch = rd.Read();
            if (ch == 0x0d && rd.Read() == 0x0a && rd.Read() == 0x0d && rd.Read() == 0x0a)
                break;
            sb.Append((char)ch);
           }

           string res = sb.ToString();
            if(res.StartsWith("ok"))
                return res;
            else
                throw new Exception(res);
           
        }


    }
}
