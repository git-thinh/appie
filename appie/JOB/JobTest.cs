using System;
using System.Threading;

namespace appie
{
    public class JobTest : IJob
    {
        public IJobStore StoreJob { get; }
        public void f_freeResource() { }
        public void f_sendMessage(Message m) { if (this.StoreJob != null) this.StoreJob.f_job_sendMessage(m); }
        public void f_receiveMessage(Message m) { }

        private volatile int Id = 0;
        public int f_getId() { return Id; }
        public void f_setId(int id) { Interlocked.CompareExchange(ref Id, Id, id); }
        readonly string _groupName = string.Empty;
        public string f_getGroupName() { return _groupName; }
        public JobTest(IJobStore _store)
        {
            this.StoreJob = _store;
        }

        public void f_runLoop(object state, bool timedOut)
        {
            JobInfo ti = (JobInfo)state;
            if (!timedOut)
            {
                Tracer.WriteLine("J{0} executes on thread {1}: SIGNAL -> STOP", Id, Thread.CurrentThread.GetHashCode().ToString());
                ti.f_stopJob();
                return;
            }

            Tracer.WriteLine("J{0} executes on thread {1}: Do something ...", Id, Thread.CurrentThread.GetHashCode().ToString());
        }

        
    }
}
