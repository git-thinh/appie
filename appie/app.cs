﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace appie
{
    public class app {

        static app()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (se, ev) =>
            {
                Assembly asm = null;
                string comName = ev.Name.Split(',')[0];
                string resourceName = @"DLL\" + comName + ".dll";
                var assembly = Assembly.GetExecutingAssembly();
                resourceName = typeof(app).Namespace + "." + resourceName.Replace(" ", "_").Replace("\\", ".").Replace("/", ".");
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        //Debug.WriteLine(resourceName);
                    }
                    else
                    {
                        byte[] buffer = new byte[stream.Length];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                ms.Write(buffer, 0, read);
                            buffer = ms.ToArray();
                        }
                        asm = Assembly.Load(buffer);
                    }
                }
                return asm;
            };
        }

        static fCrawler fom;

        static QueueThreadSafe<msg> api_msg_queue = null;
        //static System.Threading.Timer api_msg_timer = null;
        //static ConcurrentDictionary<string, IthreadMsg> dicService = null;
        //static ConcurrentDictionary<string, msg> dicResponses = null;


        public static void postToAPI(msg m)
        {
            api_msg_queue.Enqueue(m);
        }

        public static void postToAPI(string api, string key, object input)
        {
            postToAPI(new msg() { API = api, KEY = key, Input = input });
        }

        public static void RUN() {




            //api_msg_queue = new ConcurrentQueue<msg>();
            //if (api_msg_timer == null)
            //    api_msg_timer = new System.Threading.Timer(new System.Threading.TimerCallback((obj) =>
            //    {
            //        if (api_msg_queue.Count > 0)
            //        {
            //            msg m = api_msg_queue.Dequeue();
            //            if(m != null)
            //            {
            //                if (!string.IsNullOrEmpty(m.API) && dicService.ContainsKey(m.API))
            //                {
            //                    IthreadMsg sv = dicService.Get(m.API);
            //                    if(sv != null)
            //                    {
            //                        ////new Thread(new ParameterizedThreadStart((object _sv) =>
            //                        ////{
            //                        ////    IthreadMsg so = (IthreadMsg)_sv;
            //                        ////    so.Execute(m);
            //                        ////})).Start(sv);
            //                        sv.Execute(m);
            //                    }
            //                }
            //            }
            //        }
            //    }), null, 100, 100);

            //dicResponses = new ConcurrentDictionary<string, msg>();
            //dicService = new ConcurrentDictionary<string, IthreadMsg>();

            //dicService.Add(_API.CRAWLER, new threadMsg(new api_crawler()));







            fom = new fCrawler();

            Application.EnableVisualStyles();
            //Application.Run(new fMedia());
            //Application.Run(new fMain());
            //Application.Run(new fEdit());
            Application.Run(new fBrowser());
        }

        public static IFORM get_Main() {
            return null;
        }
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 1000;
            try
            {
                // active SSL 1.1, 1.2, 1.3 for WebClient request HTTPS
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
                    | (SecurityProtocolType)3072
                    | (SecurityProtocolType)0x00000C00
                    | SecurityProtocolType.Tls;
            }
            catch
            {
                // active SSL 1.1, 1.2, 1.3 for WebClient request HTTPS
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 
                    | SecurityProtocolType.Tls; 
            }
            //app.RUN();


            //ApiChannel channel = new ApiChannel(new ApiFetchWorker()); 
            //channel.PostDataToWorker(new string[] {
            //    // "https://pronuncian.com/pronounce-th-sounds/",
            //    // "https://www.learning-english-online.net/pronunciation/the-english-th/",
            //    "https://dictionary.cambridge.org/grammar/british-grammar/",
            //});

            //Console.ReadLine();

            var jobs = new JobStore();

            jobs.f_addJob(new JobWebClient(jobs), "a");
            //jobs.f_addJob(new JobWebClient(), "a");
            //jobs.f_addJob(new JobWebClient(), "a");

            jobs.OnStopAll += (se,ev) => {
                Trace.WriteLine(">>>>> STOP ALL JOBS: DONE ...");
            };

            while (true)
            {
                Console.WriteLine("Input URL: ");
                string url = Console.ReadLine();
                jobs.f_url_AddRang(new string[] { "https://dictionary.cambridge.org/grammar/british-grammar/" });

                //Console.WriteLine("Enter to stop all...");
                //Console.ReadLine();
                //jobs.f_stopAll(); 
                //Console.WriteLine("Enter to restart all...");
                //Console.ReadLine();
                //jobs.f_restartAllJob(); 
            }
        }
    }
}
