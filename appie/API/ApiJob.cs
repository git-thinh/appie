using System;
using System.Collections.Generic;
using System.Threading;

namespace appie
{
    public class ApiJob: IApiJob
    {
        readonly DictionaryThreadSafe<int, AutoResetEvent> storeEvents;
        readonly DictionaryThreadSafe<int, JobInfo> storeJobs;
        readonly DictionaryThreadSafe<string, ListThreadSafe<int>> storeGroupJobs;
        readonly ListThreadSafe<int> listIdsStop;
        bool event_JobsStoping = false;

        public event EventHandler OnStopAll; 

        public ApiJob()
        {
            storeEvents = new DictionaryThreadSafe<int, AutoResetEvent>();
            storeJobs = new DictionaryThreadSafe<int, JobInfo>();
            storeGroupJobs = new DictionaryThreadSafe<string, ListThreadSafe<int>>();
            listIdsStop = new ListThreadSafe<int>();
        }

        public void event_stopAllJob()
        {
            OnStopAll?.Invoke(this, new EventArgs() { });
            event_JobsStoping = false;
        }

        public void f_restartAllJob()
        {
            f_stopAll();

            if (storeJobs.Count > 0)
            {
                JobInfo[] jobs = storeJobs.ValuesArray;
                for (int i = 0; i < jobs.Length; i++)
                    jobs[i].ReStart();
            }
        }

        public void eventAfter_stopJob(int id)
        {
            listIdsStop.Add(id);
            if (listIdsStop.Count == storeJobs.Count && event_JobsStoping == false)
            {
                event_JobsStoping = true;
                event_stopAllJob();
            }
        }


        public long f_addJob(IJob job, string groupName = null)
        {
            // The main thread uses AutoResetEvent to signal the
            // registered wait handle, which executes the callback
            // method: new AutoResetEvent(???)
            //          + true = signaled -> thread continous run
            //          + false = non-signaled -> thread must wait
            //      EventWaitHandle có ba phương thức chính bạn cần quan tâm:
            //      – Close: giải phóng các tài nguyên được sử dụng bởi WaitHandle.
            //      – Reset: chuyển trạng thái của event thành non-signaled.
            //      – Set: chuyển trạng thái của event thành signaled.
            //      – WaitOne([parameters]): Chặn thread hiện tại cho đến khi trạng thái của event được chuyển sang signaled.

            AutoResetEvent ev = new AutoResetEvent(false);
            int _id = storeJobs.Count + 1;
            JobInfo jo = new JobInfo(_id, groupName, job, ev, this);

            storeJobs.Add(_id, jo);
            storeEvents.Add(_id, ev);

            if (!string.IsNullOrEmpty(groupName))
            {
                if (storeGroupJobs.ContainsKey(groupName))
                {
                    ListThreadSafe<int> ls = storeGroupJobs[groupName];
                    ls.Add(_id);
                }
                else
                {
                    List<int> lsId = new List<int>() { _id };
                    storeGroupJobs.Add(groupName, lsId);
                }
            }

            return _id;
        }

        public void f_stopAll() {
            listIdsStop.Clear();
            if (storeEvents.Count > 0)
            {
                AutoResetEvent[] evs = storeEvents.ValuesArray;
                for (int i = 0; i < evs.Length; i++)
                {
                    Trace.WriteLine("Sended to job[{0}] a signal to exit ...", i);
                    evs[i].Set();
                }
                Trace.WriteLine("All {0} jobs received signal to exit ...", evs.Length);
                //WaitHandle.WaitAll(evs);
            }
        }


        ~ApiJob()
        {
            f_stopAll();
        }
    }

    // TaskInfo contains data that will be passed to the callback method.
    public class JobInfo
    {
        readonly string _groupName;

        readonly int _id;
        readonly IApiJob _api;
        readonly IJob _job;
        readonly AutoResetEvent _even;
        private RegisteredWaitHandle _handle = null;

        private static readonly Random _random = new Random();
        public JobInfo(int id, string groupName, IJob job, AutoResetEvent ev, IApiJob _api)
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
                30,//1000 , 15
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
                30,//1000 , 15
                false);
        }

        public void StopJob()
        {
            if (this._handle != null)
                this._handle.Unregister(null);
            this._api.eventAfter_stopJob(this._id);
        }

        public int GetId() { return _id; }

        public string GetGroupName() { return _groupName; }

        public override string ToString()
        {
            return this._id.ToString();
        }
    }

    public interface IJob
    {
        int ThreadId { get; set; }
        IApiChannel Channel { get; set; }

        /// <summary>
        /// Returns whether the worker thread has been asked to stop.
        /// This continues to return true even after the thread has stopped.
        /// </summary>
        bool Stopping { get; }

        /// <summary>
        /// Returns whether the worker thread has stopped.
        /// </summary>
        bool Stopped { get; }

        /// <summary>
        /// Tells the worker thread to stop, typically after completing its 
        /// current work item. (The thread is *not* guaranteed to have stopped
        /// by the time this method returns.)
        /// </summary>
        void Stop();

        /// <summary>
        /// Main work loop of the class.
        /// </summary>
        void Run(object state, bool timedOut);

        void PostDataToWorker(object data);
    }

    public interface IApiJob
    { 
        void eventAfter_stopJob(int id);
    } 
}
