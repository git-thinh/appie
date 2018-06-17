using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace appie
{
    public class JobFetch : IJob
    {
        public IApiChannel Channel { get; set; }

        public bool Stopped { get; set; }

        public bool Stopping { get; set; }

        public int ThreadId { get; set; }

        public void PostDataToWorker(object data)
        {

        }

        public void Run(object state, bool timedOut)
        {
            // The state object must be cast to the correct type, because the
            // signature of the WaitOrTimerCallback delegate specifies type Object.
            JobInfo ti = (JobInfo)state;
             
            if (!timedOut)
            {
                ////////////////////////////////////////////////////////////////
                // do someting before exit Job....

                // If the callback method executes because the WaitHandle is
                // signaled, stop future execution of the callback method
                // by unregistering the WaitHandle.
                ti.Unregister();

                Trace.WriteLine("{0} executes on thread {1}; cause = {2}. STOP ...",
                    ti.Name,
                    Thread.CurrentThread.GetHashCode().ToString(),
                    "SIGNALED"
                );

                return;
            }
            ////////////////////////////////////////////////////////////////
            // Do something while wait signal to exit Job

            Trace.WriteLine("{0} executes on thread {1}; cause = {2}. do something ...",
                ti.Name,
                Thread.CurrentThread.GetHashCode().ToString(),
                "TIMED OUT"
            ); 
        }

        public void Stop()
        {

        }
    }
}
