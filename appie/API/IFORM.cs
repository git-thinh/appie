using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace appie
{
    public delegate void EventReceiveMessage(IFORM form, Guid[] ids);
    public interface IFORM
    {
        IJobStore JobStore { get; }
        void f_receiveMessage(Guid[] ids);
        event EventReceiveMessage OnReceiveMessage;
    }
}
