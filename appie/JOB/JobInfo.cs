using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace appie
{
    public class JobInfo
    {
        readonly IJob Job;
        readonly AutoResetEvent _even;
        readonly static Random _random = new Random();

        private JOB_STATE state;
        private RegisteredWaitHandle handle;

        public JobInfo(IJob job, AutoResetEvent ev)
        {
            job.f_setId(job.StoreJob.f_job_countAll() + 1);

            this.Job = job;
            this._even = ev;

            this.state = JOB_STATE.RUNNING;
            this.handle = ThreadPool.RegisterWaitForSingleObject(
                ev,
                new WaitOrTimerCallback(job.f_runLoop),
                this,
                JOB_CONST.JOB_TIMEOUT_RUN,
                false);
        }

        public void f_receiveMessage(Message m)
        {
            if (Job != null)
                Job.f_receiveMessage(m);
        }

        public void f_reStart()
        {
            if (this.handle != null)
                this.handle.Unregister(null);

            this._even.Reset();

            this.handle = ThreadPool.RegisterWaitForSingleObject(
                this._even,
                new WaitOrTimerCallback(Job.f_runLoop),
                this,
                JOB_CONST.JOB_TIMEOUT_RUN,
                false);

            this.state = JOB_STATE.RUNNING;
        }

        public void f_sendMessage(Message m) { if (this.Job.StoreJob != null) this.Job.StoreJob.f_job_sendMessage(m); }

        public void f_freeResource()
        {
            if (this.Job != null)
                this.Job.f_freeResource();
        }

        public void f_stopJob()
        {
            if (this.handle != null)
                this.handle.Unregister(null);
            this.state = JOB_STATE.STOPED;
            this.Job.StoreJob.f_job_eventAfterStop(this.Job.f_getId());
        }

        public JOB_STATE f_getState()
        {
            return this.state;
        }

        public AutoResetEvent f_getEvent() { return _even; }
        
        public string f_getGroupName() { return this.Job.f_getGroupName(); }

        public override string ToString() { return this.Job.f_getId().ToString(); }
    }
}
