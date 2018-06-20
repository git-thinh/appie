using System;
using System.Threading;

namespace appie
{
    public class JobLink : IJob
    {
        readonly ListThreadSafe<oLink> list;
        public IJobStore store { get; }
        public void f_freeResource() { }
        public void f_postData(object data) { }

        public JobLink(IJobStore _store)
        {
            this.store = _store;
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
                //Tracer.WriteLine("J{0} executes on thread {1}: SIGNAL -> STOP ...", ti.f_getId(), Thread.CurrentThread.GetHashCode().ToString());
                ti.f_stopJob();
                return;
            }

            //Tracer.WriteLine("J{0} executes on thread {1}:DO SOMETHING ...", ti.f_getId(), Thread.CurrentThread.GetHashCode().ToString());
            // Do something ...
        } 
    }
}
