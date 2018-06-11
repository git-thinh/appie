using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace appie
{
    public class FetchUrl
    {
        static readonly object finishedLock = new object();
        const string PageUrl = @"http://www.pobox.com/~skeet/csharp/threads/threadpool.shtml";

        static void RUN()
        {
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

                Console.WriteLine("Waiting for response...");

                // Wait until everything's finished. Normally you'd want to
                // carry on doing stuff here, of course.
                Monitor.Wait(finishedLock);
            }
        }

        static void GetResponseCallback(IAsyncResult ar)
        {
            // Fetch our state information
            RequestResponseState state = (RequestResponseState)ar.AsyncState;

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
                ReportFinished(state.text.ToString());
                return;
            }

            // Nope - so decode the text and then call BeginRead again
            state.text.Append(state.encoding.GetString(state.buffer, 0, len));

            state.stream.BeginRead(state.buffer, 0, state.buffer.Length, new AsyncCallback(ReadCallback), state);
        }

        static void ReportFinished(string page)
        {
            Console.WriteLine("Read text of page. Length={0} characters.", page.Length);
            // Assume for convenience that the page length is over 50 characters!
            Console.WriteLine("First 50 characters:");
            Console.WriteLine(page.Substring(0, 50));
            Console.WriteLine("Last 50 characters:");
            Console.WriteLine(page.Substring(page.Length - 50));

            // Tell the main thread we've finished.
            lock (finishedLock)
            {
                Monitor.Pulse(finishedLock);
            }
        }


    }
}
