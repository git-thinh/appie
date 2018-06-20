using System;
using System.Collections.Generic;
using System.Text;

namespace appie
{
    public interface IJob
    {
        IJobStore store { get; }
        void f_receiveMessage(Message m);

        void f_postData(object data);
        void f_freeResource();
        
        void f_runLoop(object state, bool timedOut);
    }
}
