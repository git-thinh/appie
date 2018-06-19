﻿using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Speech.Synthesis;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace appie
{
    public class JobSpeechEN : IJob
    {
        readonly QueueThreadSafe<string> queue;
        readonly static SpeechSynthesizer _speaker = new SpeechSynthesizer();
        readonly DictionaryThreadSafe<string, string> storeUrl;
        readonly DictionaryThreadSafe<string, string> storePath;


        public IJobStore store { get; }
        public void f_freeResource()
        {
            //System.Runtime.InteropServices.Marshal.FinalReleaseComObject(wplayer);

        }
        public JobSpeechEN(IJobStore _store)
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
                    //m_media.URL = "http://localhost:17909/?key=MP41332697176";
                    //m_media.URL = "https://r7---sn-jhjup-nbol.googlevideo.com/videoplayback?sparams=clen%2Cdur%2Cei%2Cgir%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpcm2%2Cpl%2Cratebypass%2Crequiressl%2Csource%2Cexpire&ip=113.20.96.116&ratebypass=yes&id=o-AOLJZSgGBWxgcZ-LY1egw0c_LmFlE8xK4tYaWJkfIec3&c=WEB&fvip=1&expire=1528362756&mm=31%2C29&ms=au%2Crdu&ei=o6IYW-afL82u4AKBibLoBA&pl=23&itag=18&mt=1528341082&mv=m&signature=4173045360861DAD91316A65BEA4AA7D68F7FF99.BCEA5E84BA94D5CA9D2C8F57236DEE2D75DD95FF&source=youtube&requiressl=yes&mime=video%2Fmp4&gir=yes&clen=246166&mn=sn-jhjup-nbol%2Csn-i3b7knld&initcwndbps=216250&ipbits=0&pcm2=yes&dur=5.596&key=yt6&lmt=1467908538636985";

                    //test_play_fileLocal("1.m4a");
                    //test_play_fileLocal("1.mp3");
                    //test_play_fileLocal("2.mp4");
                    //test_play_fileLocal_Seek("1.m4a");
                    //test_play_fileLocal_Loop("1.mp3");
                    //test_play_urlMP3Online("http://media.ch9.ms/ch9/2876/fd36ef30-cfd2-4558-8412-3cf7a0852876/AzureWebJobs103.mp3");
                    //test_play_stream_urlMP3Online("http://media.ch9.ms/ch9/2876/fd36ef30-cfd2-4558-8412-3cf7a0852876/AzureWebJobs103.mp3");
                    //test_play_stream_urlMP3Online("https://drive.google.com/uc?export=download&id=1u2wJYTB-hVWeZOLLd9CxcA9KCLuEanYg");
                    //test_play_stream_urlMP3Online_v2("http://media.ch9.ms/ch9/2876/fd36ef30-cfd2-4558-8412-3cf7a0852876/AzureWebJobs103.mp3");
                    //test_play_stream_urlMP3Online_v2("https://drive.google.com/uc?export=download&id=1u2wJYTB-hVWeZOLLd9CxcA9KCLuEanYg");

                    //test_play_fileLocal_cache("1.mp3");
                    //test_play_fileLocal_cache("1.m4a");
                    test_play_file_cache("http://media.ch9.ms/ch9/2876/fd36ef30-cfd2-4558-8412-3cf7a0852876/AzureWebJobs103.mp3", TYPE_SOURCE.FILE_MP3_ONLINE);

                    //////Invoke with:
                    ////var playThread = new Thread(timeout => test_play_stream_urlMP3Online_v3("http://translate.google.com/translate_tts?q=" + HttpUtility.UrlEncode(relatedLabel.Text), (int)timeout));
                    ////playThread.IsBackground = true;
                    ////playThread.Start(10000);
                    //////Terminate with:
                    ////if (waiting) stop.Set();

                    System.Trace.WriteLine("J{0} executes on thread {1}: Speech = {2}", ti.f_getId(), Thread.CurrentThread.GetHashCode().ToString(), s);
                }
            }
        }

        #region [ TEST ]

        void test_play_fileLocal(string url)
        {
            //string url = "1.m4a";
            //url = "1.mp3";
            //url = "2.mp4";
            using (var audioFile = new AudioFileReader(url))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        void test_play_file_cache(string url, TYPE_SOURCE type = TYPE_SOURCE.FILE_LOCAL)
        { 
            // on startup:
            var zap = new CachedSound(url, type);
            //var boom = new CachedSound("boom.wav");

            // later in the app...
            AudioPlaybackEngine.Instance.PlaySound(zap);
            //AudioPlaybackEngine.Instance.PlaySound(boom);
            //AudioPlaybackEngine.Instance.PlaySound("crash.wav");

            // on shutdown
            //AudioPlaybackEngine.Instance.Dispose(); 
        }

        void test_play_fileLocal_Loop(string url)
        {
            //WaveFileReader reader = new WaveFileReader(@"C:\Music\Example.wav");
            //LoopStream loop = new LoopStream(reader);
            //var waveOut = new WaveOut();
            //waveOut.Init(loop);
            //waveOut.Play();

            using (var audioFile = new AudioFileReader(url))
            {
                LoopStream loop = new LoopStream(audioFile);
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(loop);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        void test_play_fileLocal_Seek(string url, int seekSecondBegin = 45)
        {
            using (var audioFile = new AudioFileReader(url))
            {
                var trimmed = new OffsetSampleProvider(audioFile);
                trimmed.SkipOver = TimeSpan.FromSeconds(seekSecondBegin);
                trimmed.Take = TimeSpan.FromSeconds(10);

                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(trimmed);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        void test_play_urlMP3Online(string url)
        {
            //url = "http://media.ch9.ms/ch9/2876/fd36ef30-cfd2-4558-8412-3cf7a0852876/AzureWebJobs103.mp3";
            using (var mf = new MediaFoundationReader(url))
            using (var wo = new WaveOutEvent())
            {
                wo.Init(mf);
                wo.Play();
                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        void test_play_stream_urlMP3Online(string url)
        {
            //string url = "https://drive.google.com/uc?export=download&id=1u2wJYTB-hVWeZOLLd9CxcA9KCLuEanYg";

            Console.WriteLine("\r\n\r\nSTREAM BEGIN: " + url);

            using (Stream ms = new MemoryStream())
            {
                using (Stream stream = WebRequest.Create(url)
                    .GetResponse().GetResponseStream())
                {
                    byte[] buffer = new byte[32768];
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                }

                Console.WriteLine("STREAM DONE -> PLAY: " + url);

                ms.Position = 0;
                using (WaveStream blockAlignedStream =
                    new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(
                            new Mp3FileReader(ms))))
                {
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Init(blockAlignedStream);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////

        private Stream ms = new MemoryStream();
        public void test_play_stream_urlMP3Online_v2(string url)
        {
            new Thread(delegate (object o)
            {
                var response = WebRequest.Create(url).GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    byte[] buffer = new byte[65536]; // 64KB chunks
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var pos = ms.Position;
                        ms.Position = ms.Length;
                        ms.Write(buffer, 0, read);
                        ms.Position = pos;
                    }
                }
            }).Start();

            // Pre-buffering some data to allow NAudio to start playing
            while (ms.Length < 65536 * 10)
                Thread.Sleep(1000);

            ms.Position = 0;
            using (WaveStream blockAlignedStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(ms))))
            {
                using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                {
                    waveOut.Init(blockAlignedStream);
                    waveOut.Play();
                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////

        bool waiting = false;
        AutoResetEvent stop = new AutoResetEvent(false);
        public void test_play_stream_urlMP3Online_v3(string url, int timeout)
        {
            using (Stream ms = new MemoryStream())
            {
                using (Stream stream = WebRequest.Create(url)
                    .GetResponse().GetResponseStream())
                {
                    byte[] buffer = new byte[32768];
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                }
                ms.Position = 0;
                using (WaveStream blockAlignedStream =
                    new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(
                            new Mp3FileReader(ms))))
                {
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Init(blockAlignedStream);
                        waveOut.PlaybackStopped += (sender, e) =>
                        {
                            waveOut.Stop();
                        };
                        waveOut.Play();
                        waiting = true;
                        stop.WaitOne(timeout);
                        waiting = false;
                    }
                }
            }
        }

        //////Invoke with:
        ////var playThread = new Thread(timeout => test_play_stream_urlMP3Online_v3("http://translate.google.com/translate_tts?q=" + HttpUtility.UrlEncode(relatedLabel.Text), (int)timeout));
        ////playThread.IsBackground = true;
        ////playThread.Start(10000);

        //////Terminate with:
        ////if (waiting) stop.Set();

        #endregion

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