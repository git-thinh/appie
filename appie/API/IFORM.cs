using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace appie
{
    public delegate void EventReceiveMessage(IFORM form, Guid id);
    public interface IFORM
    {
        //void api_responseMsg(object sender, threadMsgEventArgs e);
        //void f_form_freeResource();
         
        IJobStore JobStore { get; }
        void f_receiveMessage(Guid id);
        event EventReceiveMessage OnReceiveMessage;
    }
}
