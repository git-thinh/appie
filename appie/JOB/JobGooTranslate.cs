using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Speech.Synthesis;
using System.Threading;

namespace appie
{
    public class JobGooTranslate : IJob
    { 
        readonly QueueThreadSafe<string> queue;
        readonly DictionaryThreadSafe<string, string> storeUrl;
        readonly DictionaryThreadSafe<string, string> storePath;

        static JobGooTranslate()
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
        public JobGooTranslate(IJobStore _store)
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
                System.Trace.WriteLine("J{0} executes on thread {1}: SIGNAL -> STOP", ti.f_getId(), Thread.CurrentThread.GetHashCode().ToString());
                ti.f_stopJob();
                return;
            }


            if (this.queue.Count > 0)
            {
                string s = this.queue.Dequeue(string.Empty);
                if (s.Length > 0)
                {
                    test_run(s);

                    System.Trace.WriteLine("J{0} executes on thread {1}: Speech = {2}", ti.f_getId(), Thread.CurrentThread.GetHashCode().ToString(), s);
                }
            }
        }

        #region [ TEST ]
         
        void test_run(string text)
        { 
            //IsBusy(true);
            GTranslateService.TranslateAsync(
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
