using System;
using System.Collections.Generic;
using System.Text;

namespace appie
{
    public interface IJob
    {
        IJobStore StoreJob { get; } 

        int f_getId();
        void f_setId(int id);
        string f_getGroupName();

        void f_receiveMessage(Message m);
        void f_sendMessage(Message m);

        void f_freeResource();
        
        void f_runLoop(object state, bool timedOut);
    }
}
