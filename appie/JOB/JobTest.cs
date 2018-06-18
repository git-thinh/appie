using System;
using System.Threading;

namespace appie
{
    public class JobTest : IJob
    {
        public IJobStore store { get; }

        public JobTest(IJobStore _store)
        {
            this.store = _store;
        }

        public void Run(object state, bool timedOut)
        {
            JobInfo ti = (JobInfo)state;
            if (!timedOut)
            {
                Trace.WriteLine("J{0} executes on thread {1}: SIGNAL -> STOP", ti.GetId(), Thread.CurrentThread.GetHashCode().ToString());
                ti.StopJob();
                return;
            }

            Trace.WriteLine("J{0} executes on thread {1}: Do something ...", ti.GetId(), Thread.CurrentThread.GetHashCode().ToString());
        } 
    }
}
