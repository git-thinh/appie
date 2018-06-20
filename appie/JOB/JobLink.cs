using System;
using System.Threading;
using System.Linq;

namespace appie
{
    public class JobLink : IJob
    {
        readonly ListThreadSafe<oLink> list;

        public IJobStore StoreJob { get; }
        public void f_freeResource() { }
        public void f_sendMessage(Message m) { if (this.StoreJob != null) this.StoreJob.f_job_sendMessage(m); }

        private volatile int Id = 0;
        public int f_getId() { return Id; }
        public void f_setId(int id) { Interlocked.CompareExchange(ref Id, Id, id); }
        readonly string _groupName = JOB_NAME.SYS_LINK;
        public string f_getGroupName() { return _groupName; }
        public JobLink(IJobStore _store)
        {
            this.StoreJob = _store;
            list = new ListThreadSafe<oLink>();
        }

        public void f_receiveMessage(Message m) { }

        private volatile bool _inited = false;
        private void f_Init() {
            list.ReadFile("link.dat");
            //Tracer.WriteLine("J{0} executes on thread {1}: INIT ...");
        }

        public void f_runLoop(object state, bool timedOut)
        {
            if (!_inited) {
                _inited = true;
                f_Init();
                return;
            }

            JobInfo ti = (JobInfo)state;
            if (!timedOut)
            {
                //Tracer.WriteLine("J{0} executes on thread {1}: SIGNAL -> STOP ...", Id, Thread.CurrentThread.GetHashCode().ToString());
                ti.f_stopJob();
                return;
            }

            //Tracer.WriteLine("J{0} executes on thread {1}:DO SOMETHING ...", Id, Thread.CurrentThread.GetHashCode().ToString());
            // Do something ...
        }
         
    }
}
