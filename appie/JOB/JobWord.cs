using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Web;

namespace appie
{
    public class JobWord : IJob
    {
        readonly QueueThreadSafe<string> queue;
        readonly DictionaryThreadSafe<string, string> storeUrl;
        readonly DictionaryThreadSafe<string, string> storePath;

        static JobWord()
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

        public IJobStore store { get; }
        public void f_freeResource()
        {
        }
        public JobWord(IJobStore _store)
        {
            this.store = _store;
            this.queue = new QueueThreadSafe<string>();
            this.storeUrl = new DictionaryThreadSafe<string, string>();
            this.storePath = new DictionaryThreadSafe<string, string>();
        }

        public void f_postData(object data)
        {
            if (data != null && data is string)
                this.queue.Enqueue(data as string);
        }

        public void f_runLoop(object state, bool timedOut)
        {
            JobInfo ti = (JobInfo)state;
            if (!timedOut)
            {
                ti.f_stopJob();
                return;
            }


            if (this.queue.Count > 0)
            {
                string s = this.queue.Dequeue(string.Empty);
                if (s.Length > 0)
                {
                    test_run_v1(s);
                    //test_run_v2(s);

                }
            }
        }

        #region [ TEST ]

        void test_run_v1(string text)
        { 
            UrlService.GetAsync("https://dictionary.cambridge.org/dictionary/english/forget", (stream) =>
            {
                object rs = null;
                string s = string.Empty;
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    s = reader.ReadToEnd();
                if (s.Length > 0) 
                    s = HttpUtility.HtmlDecode(s);
                if (s.Length > 0)
                {

                }
                else return new UrlAnanyticResult() { Message = "Can not read" };
                return new UrlAnanyticResult() { Ok = true, Html = s, Result = rs };
            }, (result) =>
            {
                if (result.Result != null) {

                }
            })
           ;
        }

        void test_run_v2(string text)
        {
            //IsBusy(true);
            GooTranslateService_v2.TranslateAsync(
                text, "en", "vi", string.Empty,
                (success, result, type) =>
                {
                    //SetResult(result, type);
                    //IsBusy(false);
                    Console.WriteLine("\r\n -> " + text + " (" + type + "): " + result);
                    Trace.WriteLine(text + "(" + type + "): " + result);
                });
        }

        #endregion
    }
}
