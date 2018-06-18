using System;
using System.Threading;

namespace appie
{
    public class JobTest : IJob
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
                // If the callback method executes because the WaitHandle is
                // signaled, stop future execution of the callback method
                // by unregistering the WaitHandle.
                ti.StopJob();
                return;
            }

            ////////////////////////////////////////////////////////////////
            // Do something while wait signal to exit Job
            Trace.WriteLine("JOB[{0}] executes on thread {1}: Do something ...", ti.GetId(), Thread.CurrentThread.GetHashCode().ToString());
        }

        public void Stop()
        {
        }
    }
}
