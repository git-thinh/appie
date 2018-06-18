using System;
using System.Collections.Generic;
using System.Text;

namespace appie
{
    public interface IJobStore
    {
        /// <summary>
        /// Call this event after stop job executing
        /// </summary>
        /// <param name="id"></param>
        void f_job_eventAfterStop(int id);

        /// <summary>
        /// Add range news array urls
        /// </summary>
        /// <param name="urls"></param>
        int f_url_AddRange(string[] urls);

        /// <summary>
        /// Dequeue url from queue contain urls remain(be not crawled)
        /// </summary>
        /// <returns></returns>
        
        string f_url_getUrlPending();
        bool f_url_stateJobIsComplete(int id);
        void f_url_Complete();        
        int f_url_countPending();
        int f_url_countResult(string url, string message, bool isSuccess = true);
    }
}
