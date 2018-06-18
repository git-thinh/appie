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

        private RegisteredWaitHandle _handle = null;
        private static readonly Random _random = new Random();

        public JobInfo(int id, string groupName, IJob job, AutoResetEvent ev, IJobStore _api)
        {
            this._job = job;
            this._groupName = groupName;
            this._id = id;
            this._api = _api;
            this._even = ev;

            this._handle = ThreadPool.RegisterWaitForSingleObject(
                ev,
                new WaitOrTimerCallback(job.Run),
                this,
                JOB_CONST.JOB_TIMEOUT_RUN,
                false);
        }

        public void ReStart()
        {
            if (this._handle != null)
                this._handle.Unregister(null);

            this._even.Reset();

            this._handle = ThreadPool.RegisterWaitForSingleObject(
                this._even,
                new WaitOrTimerCallback(_job.Run),
                this,
                JOB_CONST.JOB_TIMEOUT_RUN,
                false);
        }

        public void StopJob()
        {
            if (this._handle != null)
                this._handle.Unregister(null);
            this._api.f_job_eventAfterStop(this._id);
        }

        public AutoResetEvent GetEvent() { return _even; }

        public int GetId() { return _id; }

        public string GetGroupName() { return _groupName; }

        public override string ToString() { return this._id.ToString(); }
    }
}
