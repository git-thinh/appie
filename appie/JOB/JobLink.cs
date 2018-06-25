using System;
using System.Threading;
using System.Linq;

namespace appie
{
    public class JobLink : IJob
    {
        readonly QueueThreadSafe<Message> msg;
        readonly ListThreadSafe<oLink> list;

        private volatile JOB_STATE _state = JOB_STATE.NONE;
        public JOB_STATE State { get { return _state; } }
        public IJobStore StoreJob { get; }
        public void f_stopAndFreeResource() { }
        public void f_sendMessage(Message m) { if (this.StoreJob != null) this.StoreJob.f_job_sendMessage(m); }

        private volatile int Id = 0;
        public int f_getId() { return Id; }
        public void f_setId(int id) { Interlocked.Add(ref Id, id); }
        readonly string _groupName = JOB_NAME.SYS_LINK;
        public string f_getGroupName() { return _groupName; }
        public JobLink(IJobStore _store)
        {
            this.StoreJob = _store;
            list = new ListThreadSafe<oLink>();
            msg = new QueueThreadSafe<Message>();
        }

        public void f_receiveMessage(Message m) {
            msg.Enqueue(m);
        }

        private volatile bool _inited = false;
        private void f_Init() {
            list.ReadFile("link.dat");
            // Tracer.WriteLine("J{0} executes on thread {1}: INIT ...");
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
                // Tracer.WriteLine("J{0} executes on thread {1}: SIGNAL -> STOP ...", Id, Thread.CurrentThread.GetHashCode().ToString());
                ti.f_stopJob();
                return;
            }

            // Tracer.WriteLine("J{0} executes on thread {1}:DO SOMETHING ...", Id, Thread.CurrentThread.GetHashCode().ToString());
            // Do something ...

            if (msg.Count > 0) {
                Message m = msg.Dequeue(null);
                if (m != null) {
                    switch (m.getAction()) {
                        case MESSAGE_ACTION.ITEM_SEARCH:
                            oLink[] a = new oLink[] { };
                            if (m.Input != null)
                            {
                                string key = m.Input as string;
                                a = list.Where(x => x.Link.Contains(key) || x.Title.Contains(key) || x.Tags.Contains(key), false, int.MaxValue);
                                m.Output.Counter = a.Length;
                            }
                            else {
                                a = list.Take(10).ToArray();
                                m.Output.Counter = list.Count;
                            }

                            m.Type = MESSAGE_TYPE.RESPONSE;
                            m.JobName = this._groupName;

                            m.Output.Ok = true;
                            m.Output.PageSize = 10;
                            m.Output.PageNumber = 1;
                            m.Output.Total = list.Count;
                            m.Output.SetData(a);

                            this.StoreJob.f_responseMessageFromJob(m);

                            break;
                    }
                }
            }
        }
         
    }
}
