using AxWMPLib;
using System;
using System.IO;
using System.Reflection;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;

namespace appie
{
    public class JobSpeechEN : IJob
    {
        static JobSpeechEN()
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
         
        //readonly AxWindowsMediaPlayer media;
        readonly QueueThreadSafe<string> queue;
        readonly static SpeechSynthesizer _speaker = new SpeechSynthesizer();
        readonly DictionaryThreadSafe<string, string> storeUrl;
        readonly DictionaryThreadSafe<string, string> storePath;


        readonly System.Media.SoundPlayer player = new System.Media.SoundPlayer();
        readonly WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();

        public IJobStore store { get; }
        public void f_freeResource() {
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(wplayer);
        }
        public JobSpeechEN(IJobStore _store)
        {
            this.store = _store;
            this.queue = new QueueThreadSafe<string>();
            this.storeUrl = new DictionaryThreadSafe<string, string>();
            this.storePath = new DictionaryThreadSafe<string, string>();
             
        }

        private void f_media_event_PlayStateChange(object sender, _WMPOCXEvents_PlayStateChangeEvent e)
        {
        }

        public void f_postData(object data) {
            if (data != null && data is string)
                this.queue.Enqueue(data as string);
        }

        public void f_runLoop(object state, bool timedOut)
        {
            JobInfo ti = (JobInfo)state;
            if (!timedOut)
            {
                Trace.WriteLine("J{0} executes on thread {1}: SIGNAL -> STOP", ti.f_getId(), Thread.CurrentThread.GetHashCode().ToString());
                ti.f_stopJob();
                return;
            }
             

            if (this.queue.Count > 0)
            {
                string s = this.queue.Dequeue(string.Empty);
                if (s.Length > 0)
                {
                    //media.settings.volume = 100;
                    ////m_media.uiMode = "none";
                    //media.URL = "1.m4a";
                    //m_media.URL = "2.mp4";
                    //m_media.URL = "http://localhost:17909/?key=MP41332697176";
                    //m_media.URL = "https://r7---sn-jhjup-nbol.googlevideo.com/videoplayback?sparams=clen%2Cdur%2Cei%2Cgir%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpcm2%2Cpl%2Cratebypass%2Crequiressl%2Csource%2Cexpire&ip=113.20.96.116&ratebypass=yes&id=o-AOLJZSgGBWxgcZ-LY1egw0c_LmFlE8xK4tYaWJkfIec3&c=WEB&fvip=1&expire=1528362756&mm=31%2C29&ms=au%2Crdu&ei=o6IYW-afL82u4AKBibLoBA&pl=23&itag=18&mt=1528341082&mv=m&signature=4173045360861DAD91316A65BEA4AA7D68F7FF99.BCEA5E84BA94D5CA9D2C8F57236DEE2D75DD95FF&source=youtube&requiressl=yes&mime=video%2Fmp4&gir=yes&clen=246166&mn=sn-jhjup-nbol%2Csn-i3b7knld&initcwndbps=216250&ipbits=0&pcm2=yes&dur=5.596&key=yt6&lmt=1467908538636985";

                    ////System.Media.SoundPlayer player = new System.Media.SoundPlayer(); 
                    //player.SoundLocation = "1.mp3";
                    //player.Play();

                    wplayer.URL = "1.mp3";
                    wplayer.controls.play();

                    Trace.WriteLine("J{0} executes on thread {1}: Speech = {2}", ti.f_getId(), Thread.CurrentThread.GetHashCode().ToString(), s);
                }
            }
        }

        enum SPEECH_COMMAND
        {
            SPEECH,
            STOP,
            REPEAT,
        }

        enum SPEECH_STATE
        {
            SPEECH_ONCE,
            SPEECH_REPEATE,
            STOP,
            FREE,
        }
    }


}
