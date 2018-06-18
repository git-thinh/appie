using System;
using System.Collections.Generic;
using System.Text;

namespace appie
{
    public interface IJobStore
    {
        void f_eventAfter_stopJob(int id);
    }
}
