using System;
using System.Net;
using System.Threading;

namespace appie
{
    public class JobWebClient : IJob
    {
        readonly Object _lock = new object();
        readonly WebClient _client = null;

        private bool isDownloading = false;

        public JobWebClient() {
            _client = new WebClient();
            _client.DownloadStringCompleted += (sender, args) =>
            {
                Trace.WriteLine("JOB[{0}] executes on thread {1}: done ...");

                if (args.Cancelled)
                    Trace.WriteLine("Canceled");
                else if (args.Error != null)
                    Trace.WriteLine("Exception: " + args.Error.Message);
                else
                {
                    Trace.WriteLine(args.Result.Length + " chars were downloaded");
                    // We could update the UI from here...
                }

                lock (_lock)
                    isDownloading = false;
            };
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
            Trace.WriteLine("JOB[{0}] executes on thread {1}: downloading ...", ti.GetId(), Thread.CurrentThread.GetHashCode().ToString());

            lock (_lock)
            {
                if (isDownloading) return;
                isDownloading = true;
            }

            _client.DownloadStringAsync(new Uri("http://www.linqpad.net"));
        }
    }
}
