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
        readonly DictionaryThreadSafe<int, bool> urlState;
        readonly QueueThreadSafe<string> urlRemain;
        int counterJobStateDone = 0;

        public void f_url_AddRang(string[] urls) {
            string[] a1 = urlOk.KeysArray,
                a2 = urlFail.KeysArray;

            urls = urls.Where(x =>
                        !a1.Any(p => p == x)
                        && !a2.Any(p => p == x)
                        && x.StartsWith("https://dictionary.cambridge.org/grammar/british-grammar/")).ToArray();

            if (urls.Length > 0)
            {
                urlRemain.EnqueueAll(urls);
                Interlocked.Add(ref counterJobStateDone, 0);
            }
        }

        const string str_empty = "";
        public string f_url_Dequeue() {
            return urlRemain.Dequeue(str_empty);
        }

        public void f_url_updateFail(string url, string message) {
            urlFail.Add(url, message);
        }

        public void f_url_updateSuccess(string url, string html) {
            urlOk.Add(url, html);
        }

        public bool f_url_stateJobIsComplete()
        {
            if (Interlocked.CompareExchange(ref counterJobStateDone, storeJobs.Count, 0) == 0
                && urlOk.Count > 0)
            {
                return true;
            }
            Interlocked.Increment(ref counterJobStateDone);
            return false;
        }

        public void f_url_Complete()
        {
            Interlocked.Add(ref counterJobStateDone, 0);
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
            urlState = new DictionaryThreadSafe<int, bool>();
            urlRemain = new QueueThreadSafe<string>();
        }

        ~JobStore()
        {
            f_stopAll();
        }
    }
}
