using System;
using System.Threading;

namespace appie
{
    public class JobTest : IJob
    {
        public IJobStore store { get; }
        public void f_freeResource() { }
        public void f_postData(object data) { }
        public void f_receiveMessage(Message m) { }

        public JobTest(IJobStore _store)
        {
            this.store = _store;
        }

        public void f_runLoop(object state, bool timedOut)
        {
            JobInfo ti = (JobInfo)state;
            if (!timedOut)
            {
                Tracer.WriteLine("J{0} executes on thread {1}: SIGNAL -> STOP", ti.f_getId(), Thread.CurrentThread.GetHashCode().ToString());
                ti.f_stopJob();
                return;
            }

            Tracer.WriteLine("J{0} executes on thread {1}: Do something ...", ti.f_getId(), Thread.CurrentThread.GetHashCode().ToString());
        }

        
    }
}
