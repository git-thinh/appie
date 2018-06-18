using System;
using System.Collections.Generic;
using System.Text;

namespace appie
{
    public interface IJobStore
    {
        /// <summary>
        /// Call this event after stop executing job
        /// </summary>
        /// <param name="id"></param>
        void f_job_eventAfterStop(int id);
        void f_job_postData(int idJobReceiver, object data);

        ///////////////////////////////////////////////////////////////////



        ///////////////////////////////////////////////////////////////////

        int f_url_AddRange(string[] urls);        
        string f_url_getUrlPending();
        bool f_url_stateJobIsComplete(int id);
        void f_url_Complete();        
        int f_url_countPending();
        int f_url_countResult(string url, string message, bool isSuccess = true);
    }
}
