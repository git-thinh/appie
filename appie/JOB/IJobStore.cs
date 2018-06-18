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
        void f_url_AddRang(string[] urls);

        /// <summary>
        /// Dequeue url from queue contain urls remain(be not crawled)
        /// </summary>
        /// <returns></returns>
        
        string f_url_Dequeue();

        bool f_url_stateJobIsComplete();
        void f_url_Complete();

        void f_url_updateFail(string url, string message);

        void f_url_updateSuccess(string url, string html);
    }
}
