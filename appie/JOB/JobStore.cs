using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace appie
{
    public class JobStore : IJobStore
    {
        const int job_exist_default = 2;

        #region [ LINK - MESSAGE ]

        readonly JobInfo job_Link;
        readonly JobInfo job_Message;
        readonly DictionaryThreadSafe<Guid, object> cacheJobResponseData;

        public void f_responseMessageFromJob(Message m)
        {
            if (m == null) return;
            if (m.Output.Ok) {
                var data = m.Output.GetData();
                if (data != null)
                {
                    cacheJobResponseData.Add(m.GetId(), data);
                    m.Output.SetData(null);
                }
            }
            job_Message.f_sendMessage(m);
        }

        public object f_responseMessageFromJob_getDataByID(Guid id) {
            if (cacheJobResponseData.ContainsKey(id))
                return cacheJobResponseData[id];
            return null;
        }

        public void f_responseMessageFromJob_removeData(Guid id) {
            if (cacheJobResponseData.ContainsKey(id))
                cacheJobResponseData.Remove(id);
        }

        public void f_responseMessageFromJob_clearAll() {
            cacheJobResponseData.Clear();
        }

        #endregion

        #region [ URL ]

        readonly DictionaryThreadSafe<string, string> urlOk;
        readonly DictionaryThreadSafe<string, string> urlFail;

        readonly ListThreadSafe<string> urlAll;
        readonly ListThreadSafe<string> urlPending;

        int urlCounter_Runtime = 0;
        int urlCounter_Result = 0;
        const int urlMax = 9;

        public event EventHandler OnUrlFetchComplete;

        private void f_url_Init()
        {
            urlOk.ReadFile("demo.bin");
            urlOk.Clear();
        }

        public int f_url_AddRange(string[] urls)
        {
            if (urlAll.Count < urlMax)
            {
                urls = urlAll.AddRangeIfNotExist(urls);
                if (urls.Length > 0) urlPending.AddRange(urls);
            }
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
                Tracer.WriteLine("CHECKING STATE CONPLETED: OK = {0}| ALL_URL = {1}", urlOk.Count, urlCounter_Runtime);
                return true;
            }
            return false;
        }

        public void f_url_Complete()
        {
            urlOk.WriteFile("demo.bin");

            Tracer.WriteLine("CRAWLE COMPLETE ...");
            OnUrlFetchComplete?.Invoke(this, new EventArgs() { });
        }

        #endregion

        #region [ JOB ]

        readonly DictionaryThreadSafe<int, AutoResetEvent> storeEvents;
        readonly DictionaryThreadSafe<int, JobInfo> storeJobs = new DictionaryThreadSafe<int, JobInfo>();
        readonly DictionaryThreadSafe<string, ListThreadSafe<int>> storeGroupJobs;
        readonly ListThreadSafe<int> listIdsStop;

        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        private volatile bool event_JobsStoping = false;
        public event EventHandler OnStopAll;

        public int f_job_countAll() { return storeJobs.Count + job_exist_default; }

        public void f_job_sendMessage(Message m)
        {
            //if (idJobReceiver > 0 && data != null && storeJobs.ContainsKey(idJobReceiver))
            //{
            //    JobInfo jo = null;
            //    if (storeJobs.TryGetValue(idJobReceiver, out jo) && jo != null)
            //        jo.f_postData(data);
            //}
        }

        public void f_restartAllJob()
        {
            f_stopAll();

            if (storeJobs.Count > 0)
            {
                JobInfo[] jobs = storeJobs.ValuesArray;
                for (int i = 0; i < jobs.Length; i++)
                    jobs[i].f_reStart();
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

        public int f_addJob(IJob job)
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
            int _id = storeJobs.Count + 1;

            storeJobs.Add(_id, jo);
            storeEvents.Add(_id, ev);

            string groupName = job.f_getGroupName();
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
                    Tracer.WriteLine("Sended to job[{0}] a signal to exit ...", i);
                    evs[i].Set();
                }
                Tracer.WriteLine("All {0} jobs received signal to exit ...", evs.Length);
                //WaitHandle.WaitAll(evs);
            }
        }

        public void f_freeResource()
        { 
            if (storeJobs.Count > 0)
            {
                JobInfo[] jobs = storeJobs.ValuesArray;
                for (int i = 0; i < jobs.Length; i++) 
                    jobs[i].f_freeResource(); 
            }
        }

        #endregion

        public JobStore()
        {
            cacheJobResponseData = new DictionaryThreadSafe<Guid, object>();
            job_Message = new JobInfo(new JobMessage(this), new AutoResetEvent(false));
            job_Link = new JobInfo(new JobLink(this), new AutoResetEvent(false)); 

            storeEvents = new DictionaryThreadSafe<int, AutoResetEvent>();
            //storeJobs = new DictionaryThreadSafe<int, JobInfo>();
            storeGroupJobs = new DictionaryThreadSafe<string, ListThreadSafe<int>>();
            listIdsStop = new ListThreadSafe<int>();

            urlFail = new DictionaryThreadSafe<string, string>();
            urlOk = new DictionaryThreadSafe<string, string>();

            urlPending = new ListThreadSafe<string>();
            urlAll = new ListThreadSafe<string>();

            f_url_Init();
        }

        ~JobStore()
        {
            f_stopAll();
            f_freeResource();

            job_Link.f_stopJob();
            job_Link.f_freeResource();

            job_Message.f_stopJob();
            job_Message.f_freeResource();

            f_responseMessageFromJob_clearAll();

            GC.Collect(); // Start .NET CLR Garbage Collection
            GC.WaitForPendingFinalizers(); // Wait for Garbage Collection to finish
        }
    }
}
