using System;
using System.Collections.Generic;
using System.Text;

namespace appie
{
    public interface IJobStore
    {
        #region [ JOB - MESSAGE ]

        /// <summary>
        /// Call this event after stop executing job
        /// </summary>
        /// <param name="id"></param>
        void f_job_eventAfterStop(int id);
        void f_job_sendMessage(Message m);
        int f_job_countAll();
        int[] f_job_getIdsByName(string job_name);

        List<Message> f_msg_getMessageDatas(Guid[] ids);

        #endregion

        #region [ FORM ]

        void f_form_Add(IFORM form);
        void f_form_Remove(IFORM form);

        #endregion

        #region [ RESPONSE ]

        void f_responseMessageFromJob(Message m);
        object f_responseMessageFromJob_getDataByID(Guid id);
        void f_responseMessageFromJob_removeData(Guid id);
        void f_responseMessageFromJob_clearAll();

        #endregion

        #region [ URL ]

        int f_url_AddRange(string[] urls);        
        string f_url_getUrlPending();
        bool f_url_stateJobIsComplete(int id);
        void f_url_Complete();        
        int f_url_countPending();
        int f_url_countResult(string url, string message, bool isSuccess = true);

        #endregion
    }
}
