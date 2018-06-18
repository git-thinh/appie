using System;
using System.Collections.Generic;
using System.Threading;

namespace appie
{
    public class JobStore : IJobStore
    {
        readonly DictionaryThreadSafe<int, AutoResetEvent> storeEvents;
        readonly DictionaryThreadSafe<int, JobInfo> storeJobs;
        readonly DictionaryThreadSafe<string, ListThreadSafe<int>> storeGroupJobs;
        readonly ListThreadSafe<int> listIdsStop;

        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        private volatile bool event_JobsStoping = false;
        public event EventHandler OnStopAll;

        public JobStore()
        {
            storeEvents = new DictionaryThreadSafe<int, AutoResetEvent>();
            storeJobs = new DictionaryThreadSafe<int, JobInfo>();
            storeGroupJobs = new DictionaryThreadSafe<string, ListThreadSafe<int>>();
            listIdsStop = new ListThreadSafe<int>();
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

        public void f_eventAfter_stopJob(int id)
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

        ~JobStore()
        {
            f_stopAll();
        }
    }
}
