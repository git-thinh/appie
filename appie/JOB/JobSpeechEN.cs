using System;
using System.Speech.Synthesis;
using System.Threading;

namespace appie
{
    public class JobSpeechEN : IJob
    {
        readonly QueueThreadSafe<string> queue;
        readonly static SpeechSynthesizer _speaker = new SpeechSynthesizer();

        public IJobStore store { get; }
        public void f_freeResource() { }
        public JobSpeechEN(IJobStore _store)
        {
            this.store = _store;
            this.queue = new QueueThreadSafe<string>();
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
                    switch (s)
                    {
                        case "STOP":
                            break;
                        case "REPEA":
                            break;
                    }
                    _speaker.Speak(s);

                    Trace.WriteLine("J{0} executes on thread {1}: Speech = {2}", ti.f_getId(), Thread.CurrentThread.GetHashCode().ToString(), s);
                }
            }
        }

        enum SPEECH_COMMAND {
            SPEECH,
            STOP,
            REPEAT,
        }
    }


}
