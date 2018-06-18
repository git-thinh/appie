using System;
using System.Net;
using System.Threading;

namespace appie
{
    public class JobWebClient : IJob
    {
        readonly IJobStore _store;
        readonly Object _lock = new object();
        readonly WebClient _client = null;

        private bool isDownloading = false;

        private volatile string URL = string.Empty;

        public JobWebClient(IJobStore store)
        {
            _store = store;
            _client = new WebClient();
            _client.DownloadStringCompleted += (sender, args) =>
            {
                Trace.WriteLine("DONE = {0}", URL);

                if (args.Cancelled)
                {
                    // Trace.WriteLine("Canceled");
                    _store.f_url_updateFail(URL, "CANCELED");
                }
                else if (args.Error != null)
                {
                    // Trace.WriteLine("Exception: " + args.Error.Message);
                    _store.f_url_updateFail(URL, args.Error.Message);
                }
                else
                {
                    // Trace.WriteLine(args.Result.Length + " chars were downloaded");
                    _store.f_url_updateSuccess(URL, args.Result);
                }

                //string url = _store.f_url_Dequeue();
                //if (string.IsNullOrEmpty(url))
                //{
                //    bool end = _store.f_url_stateJobIsComplete();
                //    if (end) _store.f_url_Complete();
                //    return;
                //}
                //URL = string.Copy(url);

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
            //Trace.WriteLine("JOB[{0}] executes on thread {1}: waiting URL ...", ti.GetId(), Thread.CurrentThread.GetHashCode().ToString());

            lock (_lock)
                if (isDownloading)
                    return;

            string url = _store.f_url_Dequeue();
            if (url.Length == 0) return;
            Trace.WriteLine("JOB[{0}] -> {1}", ti.GetId(), url);
            URL = string.Copy(url);
            _client.DownloadStringAsync(new Uri(url));
        }
    }
}
