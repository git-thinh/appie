using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace appie
{
    public class JobStore : IJobStore
    {
        const int job_exist_default = 2;
        readonly JobInfo job_Link;

        #region [ MESSAGE ]

        readonly JobInfo msgInfo;
        readonly DictionaryThreadSafe<Guid, object> msgCacheData;
        readonly DictionaryThreadSafe<Guid,Message> msgCache;

        public void f_responseMessageFromJob(Message m)
        {
            if (m == null) return;
            if (m.Output.Ok) {
                var data = m.Output.GetData();
                if (data != null)
                {
                    msgCacheData.Add(m.GetMessageId(), data);
                    m.Output.SetData(null);
                    msgCache.Add(m.GetMessageId(), m);
                }
            }
            msgInfo.f_sendMessage(m);
        }

        public object f_responseMessageFromJob_getDataByID(Guid id) {
            if (msgCacheData.ContainsKey(id))
                return msgCacheData[id];
            return null;
        }

        public void f_responseMessageFromJob_removeData(Guid id) {
            if (msgCacheData.ContainsKey(id))
                msgCacheData.Remove(id);
        }

        public void f_responseMessageFromJob_clearAll() {
            msgCacheData.Clear();
        }

        public List<Message> f_msg_getMessageDatas(Guid[] ids) {
            List<Message> ls = new List<Message>() { };
            for (int i = 0; i < ids.Length; i++)
            {
                Message m;
                if (msgCache.TryGetValue(ids[i], out m) && m != null) {
                    object data;
                    if (msgCacheData.TryGetValue(ids[i], out data))
                        m.Output.SetData(data);
                    ls.Add(m);
                }
            }
            return ls;
        }

        private void f_msg_Clear() {
            if (msgInfo.f_getState() != JOB_STATE.RUNNING)
                msgInfo.f_stopJob();
            msgInfo.f_stopAndFreeResource();

            msgCache.Clear();
            msgCacheData.Clear();
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

        readonly DictionaryThreadSafe<int, AutoResetEvent> jobEvents;
        readonly DictionaryThreadSafe<int, JobInfo> jobInfos;
        readonly DictionaryThreadSafe<string, ListThreadSafe<int>> jobGroups;

        // Volatile is used as hint to the compiler that this data member will be accessed by multiple threads.
        private volatile bool event_JobsStoping = false;
        readonly ListThreadSafe<int> listIdsStop;

        public event EventHandler OnStopAll;

        public int[] f_job_getIdsByName(string job_name) {
            if (jobGroups.ContainsKey(job_name)) 
                return jobGroups[job_name].ToArray(); 
            return new int[] { };
        }

        public int f_job_countAll() { return jobInfos.Count + job_exist_default; }

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

            if (jobInfos.Count > 0)
            {
                JobInfo[] jobs = jobInfos.ValuesArray;
                for (int i = 0; i < jobs.Length; i++)
                    jobs[i].f_reStart();
            }
        }

        public void f_job_eventAfterStop(int id)
        {
            listIdsStop.Add(id);
            if (listIdsStop.Count == jobInfos.Count && event_JobsStoping == false)
            {
                event_JobsStoping = true;
                Thread.Sleep(JOB_CONST.JOB_TIMEOUT_STOP_ALL);
                OnStopAll?.Invoke(this, new EventArgs() { });
                event_JobsStoping = false;
            }
        }

        private void f_addGroupJobName(IJob job) {
            int _id = job.f_getId();

            string groupName = job.f_getGroupName();
            if (!string.IsNullOrEmpty(groupName))
            {
                if (jobGroups.ContainsKey(groupName))
                {
                    ListThreadSafe<int> ls = jobGroups[groupName];
                    ls.Add(_id);
                }
                else
                {
                    List<int> lsId = new List<int>() { _id };
                    jobGroups.Add(groupName, lsId);
                }
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
            int _id = job.f_getId();

            jobInfos.Add(_id, jo);
            jobEvents.Add(_id, ev);
            f_addGroupJobName(job);

            return _id;
        }

        public void f_stopAll()
        {
            listIdsStop.Clear();
            if (jobEvents.Count > 0)
            {
                AutoResetEvent[] evs = jobEvents.ValuesArray;
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
            if (jobInfos.Count > 0)
            {
                JobInfo[] jobs = jobInfos.ValuesArray;
                for (int i = 0; i < jobs.Length; i++) 
                    jobs[i].f_stopAndFreeResource(); 
            }
        }

        #endregion

        #region [ FORM ]

        public void f_form_Add(IFORM form)
        { 
        }

        public void f_form_Remove(IFORM form)
        {
        }

        public void f_form_Clear()
        {

        }

        #endregion

        public JobStore()
        {
            jobEvents = new DictionaryThreadSafe<int, AutoResetEvent>();
            jobInfos = new DictionaryThreadSafe<int, JobInfo>();
            jobGroups = new DictionaryThreadSafe<string, ListThreadSafe<int>>();
            listIdsStop = new ListThreadSafe<int>();

            msgCacheData = new DictionaryThreadSafe<Guid, object>();
            msgCache = new DictionaryThreadSafe<Guid, Message>();

            msgInfo = new JobInfo(new JobMessage(this), new AutoResetEvent(false));
            job_Link = new JobInfo(new JobLink(this), new AutoResetEvent(false));
            f_addGroupJobName(msgInfo.f_getJob());
            f_addGroupJobName(job_Link.f_getJob());


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
            job_Link.f_stopAndFreeResource();

            msgInfo.f_stopJob();
            msgInfo.f_stopAndFreeResource();

            f_responseMessageFromJob_clearAll();

            GC.Collect(); // Start .NET CLR Garbage Collection
            GC.WaitForPendingFinalizers(); // Wait for Garbage Collection to finish
        }
    }
}
