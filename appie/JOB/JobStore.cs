using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace appie
{
    public class JobStore : IJobStore
    {
        #region [ MESSAGE ]

        public void f_responseMessageFromJob(Message m)
        {
            if (m == null) return;
            if (m.Type == MESSAGE_TYPE.RESPONSE)
            {
                var data = m.Output.GetData();
                if (data != null)
                {
                    if (!msgCacheData.ContainsKey(m.GetMessageId()))
                        msgCacheData.Add(m.GetMessageId(), data);
                    m.Output.SetData(null);
                    if (!msgCache.ContainsKey(m.GetMessageId()))
                        msgCache.Add(m.GetMessageId(), m);
                }
            }
            msgInfo.f_sendMessage(m);
        }

        public object f_responseMessageFromJob_getDataByID(Guid id)
        {
            if (msgCacheData.ContainsKey(id))
                return msgCacheData[id];
            return null;
        }

        public void f_responseMessageFromJob_removeData(Guid id)
        {
            if (msgCacheData.ContainsKey(id))
                msgCacheData.Remove(id);
        }

        public void f_responseMessageFromJob_clearAll()
        {
            msgCacheData.Clear();
        }

        public Message f_msg_getMessageData(Guid id)
        {
            Message m = null;
            if (msgCache.TryGetValue(id, out m) && m != null)
            {
                object data;
                if (msgCacheData.TryGetValue(id, out data))
                    m.Output.SetData(data);
            }
            return m;
        }

        public List<Message> f_msg_getMessageDatas(Guid[] ids)
        {
            List<Message> ls = new List<Message>() { };
            for (int i = 0; i < ids.Length; i++)
            {
                Message m;
                if (msgCache.TryGetValue(ids[i], out m) && m != null)
                {
                    object data;
                    if (msgCacheData.TryGetValue(ids[i], out data))
                        m.Output.SetData(data);
                    ls.Add(m);
                }
            }
            return ls;
        }

        private void f_msg_Clear()
        {
            if (msgInfo.f_getState() != JOB_STATE.RUNNING)
                msgInfo.f_stopJob();
            msgInfo.f_stopAndFreeResource();

            msgCache.Clear();
            msgCacheData.Clear();
        }

        #endregion

        #region [ URL ]

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

        public IJob[] f_job_getByID(int[] ids)
        {
            JobInfo[] jis = jobInfos.GetValues(ids);
            IJob[] jobs = jis.Select(x => x.f_getJob()).ToArray();
            return jobs;
        }

        public int[] f_job_getIdsByName(string job_name)
        {
            if (jobGroups.ContainsKey(job_name))
                return jobGroups[job_name].ToArray();
            return new int[] { };
        }

        public int f_job_countAll() { return jobInfos.Count + job_exist_default; }

        public void f_job_sendMessage(Message m)
        {
            if (!msgCache.ContainsKey(m.GetMessageId()))
                msgCache.Add(m.GetMessageId(), m);
            msgInfo.f_sendMessage(m);
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

        private void f_addGroupJobName(IJob job)
        {
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

        public IFORM f_form_Get(int id)
        {
            IFORM fom = null;
            if (storeForms.TryGetValue(id, out fom) && fom != null)
                return fom;
            return null;
        }

        public void f_form_Add(IFORM form)
        {
            int key = form.f_getFormID();
            if (!storeForms.ContainsKey(key))
                storeForms.Add(key, form);
        }

        public void f_form_Remove(IFORM form)
        {
            int key = form.f_getFormID();
            if (storeForms.ContainsKey(key))
                storeForms.Remove(key);
        }

        public void f_form_Clear()
        {
            storeForms.Clear();
        }

        #endregion

        #region [ === VARIABLE === ]

        #region [ VAR: JOB ]
        const int job_exist_default = 2;
        readonly DictionaryThreadSafe<int, AutoResetEvent> jobEvents;
        readonly DictionaryThreadSafe<int, JobInfo> jobInfos;
        readonly DictionaryThreadSafe<string, ListThreadSafe<int>> jobGroups;

        // Volatile is used as hint to the compiler that this data member will be accessed by multiple threads.
        private volatile bool event_JobsStoping = false;
        readonly ListThreadSafe<int> listIdsStop;

        public event EventHandler OnStopAll;
        #endregion

        #region [ VAR: MESSAGE ]
        readonly JobInfo msgInfo;
        readonly DictionaryThreadSafe<Guid, object> msgCacheData;
        readonly DictionaryThreadSafe<Guid, Message> msgCache;
        #endregion

        #region [ VAR: FORM ]
        readonly DictionaryThreadSafe<int, IFORM> storeForms;
        #endregion

        #region [ VAR: URL ]
        readonly DictionaryThreadSafe<string, string> urlOk;
        readonly DictionaryThreadSafe<string, string> urlFail;

        readonly ListThreadSafe<string> urlAll;
        readonly ListThreadSafe<string> urlPending;

        int urlCounter_Runtime = 0;
        int urlCounter_Result = 0;
        const int urlMax = 9;

        public event EventHandler OnUrlFetchComplete;
        #endregion

        //#region [ VAR: LINK ]
        //readonly JobInfo job_Link;
        //#endregion

        #endregion

        public JobStore()
        {
            #region [ JOB ]
            jobEvents = new DictionaryThreadSafe<int, AutoResetEvent>();
            jobInfos = new DictionaryThreadSafe<int, JobInfo>();
            jobGroups = new DictionaryThreadSafe<string, ListThreadSafe<int>>();
            listIdsStop = new ListThreadSafe<int>();
            #endregion

            #region [ MESSAGE ]
            msgCacheData = new DictionaryThreadSafe<Guid, object>();
            msgCache = new DictionaryThreadSafe<Guid, Message>();
            msgInfo = new JobInfo(new JobMessage(this), new AutoResetEvent(false));
            f_addGroupJobName(msgInfo.f_getJob());
            #endregion

            #region [ FORM ]
            storeForms = new DictionaryThreadSafe<int, IFORM>();
            #endregion

            #region [ URL ]
            urlFail = new DictionaryThreadSafe<string, string>();
            urlOk = new DictionaryThreadSafe<string, string>();
            urlPending = new ListThreadSafe<string>();
            urlAll = new ListThreadSafe<string>();
            f_url_Init();
            #endregion

            f_addJob(new JobLink(this));

            //#region [ LINK ]
            //job_Link = new JobInfo(new JobLink(this), new AutoResetEvent(false));
            //f_addGroupJobName(job_Link.f_getJob());
            //#endregion
        }

        ~JobStore()
        {
            #region [ JOB ]
            f_stopAll();
            f_freeResource();
            #endregion

            #region [ MESSAGE ]
            msgInfo.f_stopJob();
            msgInfo.f_stopAndFreeResource();
            f_responseMessageFromJob_clearAll();
            #endregion

            #region [ FORM ]
            f_form_Clear();
            #endregion

            #region [ URL ]
            #endregion

            //#region [ LINK ]
            //job_Link.f_stopJob();
            //job_Link.f_stopAndFreeResource();
            //#endregion

            GC.Collect(); // Start .NET CLR Garbage Collection
            GC.WaitForPendingFinalizers(); // Wait for Garbage Collection to finish
        }
    }
}
