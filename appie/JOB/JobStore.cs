using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace appie
{
    public class JobStore : IJobStore
    {
        #region [ URL ]

        readonly DictionaryThreadSafe<string, string> urlOk;
        readonly DictionaryThreadSafe<string, string> urlFail;
        readonly DictionaryThreadSafe<int, bool> urlStateJob;

        readonly ListThreadSafe<string> urlAll;
        readonly ListThreadSafe<string> urlPending;

        int urlCounter_Runtime = 0;
        int urlCounter_Result = 0;
        const int urlMax = 100;

        public int f_url_AddRange(string[] urls)
        {
            if (urlAll.Count < 100)
            {
                urls = urlAll.AddRangeIfNotExist(urls);
                if (urls.Length > 0) urlPending.AddRange(urls);
            }
            urlStateJob.Clear();
            return urls.Length;
        }

        public string f_url_getUrlPending()
        {
            if (Interlocked.CompareExchange(ref urlCounter_Runtime, urlMax, urlMax) == urlMax)
                return string.Empty;
            string url = urlPending.Dequeue(string.Empty);

            if (url.Length > 0)
                Interlocked.Increment(ref urlCounter_Runtime);

            return url;
        }

        public int f_url_countPending()
        {
            if (Interlocked.CompareExchange(ref urlCounter_Runtime, urlMax, urlMax) == urlMax)
                return 0;

            return urlPending.Count;
        }

        public int f_url_countResult(string url, string message, bool isSuccess = true)
        {
            if (isSuccess)
                urlOk.Add(url, message);
            else
                urlFail.Add(url, message);

            return Interlocked.Increment(ref urlCounter_Result);
        }

        public bool f_url_stateJobIsComplete(int id)
        {
            if (Interlocked.CompareExchange(ref urlCounter_Runtime, urlCounter_Runtime, urlCounter_Result) == urlCounter_Result
                && urlCounter_Result != 0)
            {
                Trace.WriteLine("CHECKING STATE CONPLETED: OK = {0}| ALL_URL = {1}", urlOk.Count, urlCounter_Runtime);
                return true;
            }
            return false;
        }

        public void f_url_Complete()
        {
            urlStateJob.Clear();
            Trace.WriteLine("CRAWLE COMPLETE ...");
        }

        #endregion

        #region [ JOB ]

        readonly DictionaryThreadSafe<int, AutoResetEvent> storeEvents;
        readonly DictionaryThreadSafe<int, JobInfo> storeJobs;
        readonly DictionaryThreadSafe<string, ListThreadSafe<int>> storeGroupJobs;
        readonly ListThreadSafe<int> listIdsStop;

        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        private volatile bool event_JobsStoping = false;
        public event EventHandler OnStopAll;

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

        public void f_job_eventAfterStop(int id)
        {
            listIdsStop.Add(id);
            if (listIdsStop.Count == storeJobs.Count && event_JobsStoping == false)
            {
                event_JobsStoping = true;
                Thread.Sleep(JOB_CONST.JOB_TIMEOUT_STOP_ALL);
                OnStopAll?.Invoke(this, new EventArgs() { });
                event_JobsStoping = false;
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

        public void f_stopAll()
        {
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

        #endregion

        public JobStore()
        {
            storeEvents = new DictionaryThreadSafe<int, AutoResetEvent>();
            storeJobs = new DictionaryThreadSafe<int, JobInfo>();
            storeGroupJobs = new DictionaryThreadSafe<string, ListThreadSafe<int>>();
            listIdsStop = new ListThreadSafe<int>();

            urlFail = new DictionaryThreadSafe<string, string>();
            urlOk = new DictionaryThreadSafe<string, string>();
            urlStateJob = new DictionaryThreadSafe<int, bool>();

            urlPending = new ListThreadSafe<string>();
            urlAll = new ListThreadSafe<string>();
        }

        ~JobStore()
        {
            f_stopAll();
        }
    }
}
