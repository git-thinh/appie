using System;
using System.Collections.Generic;
using System.Text;

namespace appie
{
    public interface IJob
    {
        int ThreadId { get; set; }
        IApiChannel Channel { get; set; }

        /// <summary>
        /// Returns whether the worker thread has been asked to stop.
        /// This continues to return true even after the thread has stopped.
        /// </summary>
        bool Stopping { get; }

        /// <summary>
        /// Returns whether the worker thread has stopped.
        /// </summary>
        bool Stopped { get; }

        /// <summary>
        /// Tells the worker thread to stop, typically after completing its 
        /// current work item. (The thread is *not* guaranteed to have stopped
        /// by the time this method returns.)
        /// </summary>
        void Stop();

        /// <summary>
        /// Main work loop of the class.
        /// </summary>
        void Run(object state, bool timedOut);

        void PostDataToWorker(object data);
    }
}
