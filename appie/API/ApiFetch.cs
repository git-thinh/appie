using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace appie
{
    public class ApiFetch : IAPI
    {
        static readonly object _locker = new object();
        static bool _go = true;

        public int Id { get; set; }
        public bool Open { get; set; }
        public ApiChannelCanceler Canceler { get; set; }

        private Queue<string> queueData;
        private Dictionary<string, string> dicStore;

        public void Init()
        {
            queueData = new Queue<string>();
            dicStore = new Dictionary<string, string>();
        }
        public void Close()
        {
        }
        
        public void Pause()
        {
            // Let's now wake up the thread by setting _go=true and pulsing.
            lock (_locker)
            {
                _go = false;
                Monitor.Pulse(_locker);
            }
        }

        public void PostMessage(object data)
        {
            // Let's now wake up the thread by setting _go=true and pulsing.                
            lock (_locker)
            { 
                queueData.Enqueue(data as string); 
                Monitor.Pulse(_locker);
            }
        }

        public void Run()
        {
            //app.postToAPI(_API.MEDIA, _API.MEDIA_KEY_INITED, null);
            lock (_locker)
            {
                while (queueData.Count == 0 || _go == false)
                    Monitor.Wait(_locker);    // Lock is released while we’re waiting

                // do something ...
                string data = queueData.Dequeue(); 

                try
                {
                    HttpWebRequest w = (HttpWebRequest)WebRequest.Create(new Uri(data));
                    w.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36";
                    w.BeginGetResponse(asyncResult =>
                    {
                        string htm = string.Empty;
                        try
                        {
                            HttpWebResponse rs = (HttpWebResponse)w.EndGetResponse(asyncResult); //add a break point here 
                            string url = rs.ResponseUri.ToString();

                            if (rs.StatusCode == HttpStatusCode.OK)
                            {
                                using (StreamReader sr = new StreamReader(rs.GetResponseStream(), Encoding.UTF8))
                                    htm = sr.ReadToEnd();
                                rs.Close();
                            }
                        }
                        catch { }

                        if (!string.IsNullOrEmpty(htm))
                        {

                        }

                        Run();
                    }, w);
                }
                catch {
                    Run();
                }

            }// end lock
        }
    }
}
