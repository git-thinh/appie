using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace appie
{
    public class ApiJob
    {
        readonly DictionaryThreadSafe<long, AutoResetEvent> storeEvents;
        readonly DictionaryThreadSafe<long, JobInfo> storeJobs;
        readonly DictionaryThreadSafe<string, ListThreadSafe<long>> storeGroupJobs;

        public ApiJob()
        {
            storeEvents = new DictionaryThreadSafe<long, AutoResetEvent>();
            storeJobs = new DictionaryThreadSafe<long, JobInfo>();
            storeGroupJobs = new DictionaryThreadSafe<string, ListThreadSafe<long>>();
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
            JobInfo jo = new JobInfo(job, ev);

            storeJobs.Add(jo.Id, jo);
            storeEvents.Add(jo.Id, ev);

            if (!string.IsNullOrEmpty(groupName))
            {
                if (storeGroupJobs.ContainsKey(groupName))
                {
                    ListThreadSafe<long> ls = storeGroupJobs[groupName];
                    ls.Add(jo.Id);
                }
                else
                {
                    List<long> lsId = new List<long>() { jo.Id };
                    storeGroupJobs.Add(groupName, lsId);
                }
            }

            return jo.Id;
        }

        ~ApiJob()
        { 
            if (storeEvents.Count > 0)
            {
                AutoResetEvent[] evs = storeEvents.ValuesArray; 
                for(int i = 0; i < evs.Length; i++)
                {
                    Trace.WriteLine("Sended to job[{0}] a signal to exit ...", i);
                    evs[i].Set();
                }
                Trace.WriteLine("All {0} jobs received signal to exit ...", evs.Length);
                WaitHandle.WaitAll(evs);
            }
        }
    }

    // TaskInfo contains data that will be passed to the callback method.
    public class JobInfo
    {
        public long Id = 0;
        public string Name = "default";

        readonly IJob _job;
        readonly RegisteredWaitHandle _handle = null;

        static readonly Random _random = new Random();
        public JobInfo(IJob job, AutoResetEvent ev)
        {
            this._job = job;
            this.Name = Guid.NewGuid().ToString();
            int _id = 0;
            lock (_random)
                _id = _random.Next(1, 999);
            this.Id = long.Parse(DateTime.Now.ToString("yyMMddHHmmssfff")) + _id;

            // The TaskInfo for the task includes the registered wait
            // handle returned by RegisterWaitForSingleObject.  This
            // allows the wait to be terminated when the object has
            // been signaled once (see WaitProc).
            this._handle = ThreadPool.RegisterWaitForSingleObject(
                ev,
                new WaitOrTimerCallback(job.Run),
                this,
                1000,
                false);

            // send signal to method running on thread to stop
            //ev.Set(); 
        }

        public void Unregister()
        {
            if (this._handle != null)
                this._handle.Unregister(null);
        }

        public override string ToString()
        {
            return this.Id.ToString();
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
}
