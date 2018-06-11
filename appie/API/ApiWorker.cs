using System;
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
    public class ApiWorker : IApiWorker
    {
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

        /// <summary>
        /// Main work loop of the class.
        /// </summary>
        public void Run()
        {
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
                }
            }
            finally
            {
                SetStopped();
            }
        }
         
        public void PostDataToWorker(object data) { }

    }
}
