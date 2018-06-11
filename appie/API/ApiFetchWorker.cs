using System;
using System.Net;
using System.Text;
using System.Threading;

namespace appie
{ 
    /*
     * 
            + Why Thread.Abort/Interrupt should be avoided
            I don't use Thread.Abort/Interrupt routinely. I prefer a graceful shutdown which lets the thread do anything it wants to, 
            and keeps things orderly. I dislike aborting or interrupting threads for the following reasons:

            - They aren't immediate
            One of the reasons often given for not using the graceful shutdown pattern is that a thread could be waiting forever. 
            Well, the same is true if you abort or interrupt it. If it's waiting for input from a stream of some description, 
            you can abort or interrupt the thread and it will go right on waiting. If you only interrupt the thread, 
            it could go right on processing other tasks, too - it won't actually be interrupted until it enters the WaitSleepJoin state.

            - They can't be easily predicted
            While they don't happen quite as quickly as you might sometimes want, 
            aborts and interrupts do happen where you quite possibly don't want them to. 
            If you don't know where a thread is going to be interrupted or aborted, 
            it's hard to work out exactly how to get back to a consistent state. 
            Although finally blocks will be executed, you don't want to have to put them all over the place just in case of an abort or interrupt. 
            In almost all cases, the only time you don't mind a thread dying at any point in its operation is when the whole application is going down.

            - The bug described above
            Getting your program into an inconsistent state is one problem - getting it into a state which, 
            on the face of it, shouldn't even be possible is even nastier.
     
         */

    /// <summary>
    /// Skeleton for a worker thread. Another thread would typically set up
    /// an instance with some work to do, and invoke the Run method (eg with
    /// new Thread(new ThreadStart(job.Run)).Start())
    /// </summary>
    public class ApiFetchWorker : IApiWorker
    {
        #region [ MEMBER WORKER ]

        public int ThreadId { set; get; }
        public IApiChannel Channel { set; get; }

        /// <summary>
        /// Lock covering stopping and stopped
        /// </summary>
        readonly object stopLock = new object();
        /// <summary>
        /// Whether or not the worker thread has been asked to stop
        /// </summary>
        bool stopping = false;
        /// <summary>
        /// Whether or not the worker thread has stopped
        /// </summary>
        bool stopped = false;

        /// <summary>
        /// Returns whether the worker thread has been asked to stop.
        /// This continues to return true even after the thread has stopped.
        /// </summary>
        public bool Stopping
        {
            get
            {
                lock (stopLock)
                {
                    return stopping;
                }
            }
        }

        /// <summary>
        /// Returns whether the worker thread has stopped.
        /// </summary>
        public bool Stopped
        {
            get
            {
                lock (stopLock)
                {
                    return stopped;
                }
            }
        }

        /// <summary>
        /// Tells the worker thread to stop, typically after completing its 
        /// current work item. (The thread is *not* guaranteed to have stopped
        /// by the time this method returns.)
        /// </summary>
        public void Stop()
        {
            lock (stopLock)
            {
                stopping = true;
            }
        }

        /// <summary>
        /// Called by the worker thread to indicate when it has stopped.
        /// </summary>
        void SetStopped()
        {
            lock (stopLock)
            {
                stopped = true;
            }
        }

        #endregion

        bool Inited = false;
        /// <summary>
        /// Main work loop of the class.
        /// </summary>
        public void Run()
        {
            if (Inited == false)
            {
                Inited = true;
                lock (finishedLock)
                    Monitor.Wait(finishedLock);
            }

            try
            {
                while (!Stopping)
                {
                    // Insert work here. Make sure it doesn't tight loop!
                    // (If work is arriving periodically, use a queue and Monitor.Wait,
                    // changing the Stop method to pulse the monitor as well as setting
                    // stopping.)

                    // Note that you may also wish to break out *within* the loop
                    // if work items can take a very long time but have points at which
                    // it makes sense to check whether or not you've been asked to stop.
                    // Do this with just:
                    // if (Stopping)
                    // {
                    //     return;
                    // }
                    // The finally block will make sure that the stopped flag is set.

                    if (queueURL.Count == 0)
                    {
                        if (Interlocked.CompareExchange(ref responseCounter, 0, 0) == 0) {
                            if (dicHTML.Count > 0) {
                                if (Channel != null)
                                    Channel.RecieveDataFormWorker(dicHTML);
                                dicHTML.Clear();
                            }
                        }
                        lock (finishedLock)
                            Monitor.Wait(finishedLock);
                    }

                    string PageUrl = queueURL.Dequeue();
                    if (string.IsNullOrEmpty(PageUrl))
                        continue;

                    WebRequest request = WebRequest.Create(PageUrl);
                    RequestResponseState state = new RequestResponseState();
                    state.request = request;

                    // Lock the object we'll use for waiting now, to make
                    // sure we don't (by some fluke) do everything in the other threads
                    // before getting to Monitor.Wait in this one. If we did, the pulse
                    // would effectively get lost!
                    lock (finishedLock)
                    {
                        request.BeginGetResponse(new AsyncCallback(GetResponseCallback), state);

                        //Console.WriteLine("Waiting for response...");

                        // Wait until everything's finished. Normally you'd want to
                        // carry on doing stuff here, of course.
                        Monitor.Wait(finishedLock);
                    }
                }
            }
            finally
            {
                SetStopped();
            }
        }

        public void PostDataToWorker(object data)
        {
            if (data == null) return;
            if (!Inited)
                Thread.Sleep(500);

            Type type = data.GetType();
            if (type.Name == "String")
            {
                string url = data as string;

                if (!string.IsNullOrEmpty(url))
                {
                    queueURL.Enqueue(url);
                    Interlocked.Increment(ref responseCounter);
                    lock (finishedLock)
                        Monitor.Pulse(finishedLock);
                }
            }
            else if (type.Name == "String[]")
            {
                string[] urls = data as string[];

                if (urls != null && urls.Length > 0)
                {
                    queueURL.EnqueueItems(urls);
                    Interlocked.Add(ref responseCounter, urls.Length);
                    lock (finishedLock)
                        Monitor.Pulse(finishedLock);
                }
            }
        }
         

        #region [ FETCH ]

        static SynchronizedQueue<string> queueURL = new SynchronizedQueue<string>();
        static SynchronizedDictionary<string, string> dicHTML = new SynchronizedDictionary<string, string>();

        static readonly object finishedLock = new object();
        static int responseCounter = 0;

        //static void RUN()
        //{
        //    WebRequest request = WebRequest.Create(PageUrl);
        //    RequestResponseState state = new RequestResponseState();
        //    state.request = request;

        //    // Lock the object we'll use for waiting now, to make
        //    // sure we don't (by some fluke) do everything in the other threads
        //    // before getting to Monitor.Wait in this one. If we did, the pulse
        //    // would effectively get lost!
        //    lock (finishedLock)
        //    {
        //        request.BeginGetResponse(new AsyncCallback(GetResponseCallback), state);

        //        Console.WriteLine("Waiting for response...");

        //        // Wait until everything's finished. Normally you'd want to
        //        // carry on doing stuff here, of course.
        //        Monitor.Wait(finishedLock);
        //    }
        //}

        static void GetResponseCallback(IAsyncResult ar)
        {
            // Fetch our state information
            RequestResponseState state = (RequestResponseState)ar.AsyncState;
            try
            {
                // Fetch the response which has been generated
                state.response = state.request.EndGetResponse(ar);

                // Store the response stream in the state
                state.stream = state.response.GetResponseStream();

                // Stash an Encoding for the text. I happen to know that
                // my web server returns text in ISO-8859-1 - which is
                // handy, as we don't need to worry about getting half
                // a character in one read and the other half in another.
                // (Use a Decoder if you want to cope with that.)
                //state.encoding = Encoding.GetEncoding(28591);
                state.encoding = Encoding.UTF8;

                // Now start reading from it asynchronously
                state.stream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(ReadCallback), state);
            }
            catch(Exception ex)
            {
                string message = ex.Message;
                string url = state.request.RequestUri.ToString();
                fetchFinished(url, string.Empty);
            }
        }

        static void ReadCallback(IAsyncResult ar)
        {
            // Fetch our state information
            RequestResponseState state = (RequestResponseState)ar.AsyncState;

            // Find out how much we've read
            int len = state.stream.EndRead(ar);

            // Have we finished now?
            if (len == 0)
            {
                // Dispose of things we can get rid of
                ((IDisposable)state.response).Dispose();
                ((IDisposable)state.stream).Dispose();
                fetchFinished(state.request.RequestUri.ToString(), state.text.ToString());
                return;
            }

            // Nope - so decode the text and then call BeginRead again
            state.text.Append(state.encoding.GetString(state.buffer, 0, len)); 
            state.stream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(ReadCallback), state);
        }

        static void fetchFinished(string url, string page)
        {
            //Console.WriteLine("Read text of page. Length={0} characters.", page.Length);
            //// Assume for convenience that the page length is over 50 characters!
            //Console.WriteLine("First 50 characters:");
            //Console.WriteLine(page.Substring(0, 50));
            //Console.WriteLine("Last 50 characters:");
            //Console.WriteLine(page.Substring(page.Length - 50));

            Interlocked.Decrement(ref responseCounter);
            dicHTML.Add(url, page);

            // Tell the main thread we've finished.
            lock (finishedLock)
            {
                Monitor.Pulse(finishedLock);
            }
        }

        #endregion
    }
}
