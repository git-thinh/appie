using System;
using System.Collections.Generic;
using System.Text;

namespace appie
{
    public interface IJob
    {
        IJobStore store { get; }

        void f_postData(object data);
        void f_freeResource();

        /// <summary>
        /// Main work loop of the class.
        /// </summary>
        void f_runLoop(object state, bool timedOut);
    }
}
