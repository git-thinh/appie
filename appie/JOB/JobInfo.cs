using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace appie
{
    public class JobInfo
    {
        readonly string _groupName;
        readonly int _id;
        readonly IJobStore _api;
        readonly IJob _job;
        readonly AutoResetEvent _even;
        readonly static Random _random = new Random();

        private JOB_STATE state;
        private RegisteredWaitHandle handle;

        public JobInfo(int id, string groupName, IJob job, AutoResetEvent ev, IJobStore _api)
        {
            this._job = job;
            this._groupName = groupName;
            this._id = id;
            this._api = _api;
            this._even = ev;

            this.state = JOB_STATE.RUNNING;
            this.handle = ThreadPool.RegisterWaitForSingleObject(
                ev,
                new WaitOrTimerCallback(job.f_runLoop),
                this,
                JOB_CONST.JOB_TIMEOUT_RUN,
                false);
        }

        public void f_reStart()
        {
            if (this.handle != null)
                this.handle.Unregister(null);

            this._even.Reset();

            this.handle = ThreadPool.RegisterWaitForSingleObject(
                this._even,
                new WaitOrTimerCallback(_job.f_runLoop),
                this,
                JOB_CONST.JOB_TIMEOUT_RUN,
                false);

            this.state = JOB_STATE.RUNNING;
        }

        public void f_postData(object data)
        {
            if (this._job != null)
                this._job.f_postData(data);
        }

        public void f_freeResource()
        {
            if (this._job != null)
                this._job.f_freeResource();
        }

        public void f_stopJob()
        {
            if (this.handle != null)
                this.handle.Unregister(null);
            this.state = JOB_STATE.STOPED;
            this._api.f_job_eventAfterStop(this._id);
        }

        public JOB_STATE f_getState()
        {
            return this.state;
        }

        public AutoResetEvent f_getEvent() { return _even; }

        public int f_getId() { return _id; }

        public string f_getGroupName() { return _groupName; }

        public override string ToString() { return this._id.ToString(); }
    }
}
