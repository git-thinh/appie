using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

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

                    lock (finishedLock)
                        Monitor.Wait(finishedLock);

                    if (listURL.Count == 0)
                    {
                        //if (Channel != null)
                        //    Channel.RecieveDataFormWorker(dicHTML); 
                        //dicHTML.Clear();
                        dicHTML.RemoveValueEmpty();
                        dicHTML.WriteFile("demo.bin");
                        Console.WriteLine("---------------------> COMPLETE: " + dicHTML.Count.ToString());
                        Interlocked.Exchange(ref requestCounter, 0);
                    }
                    else
                    {
                        string[] urls = listURL.Splice(20).ToArray(); 
                        PostDataToWorker(urls);
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

            Type type = data.GetType();
            if (type.Name == "String[]")
            {
                string[] urls = data as string[];

                if (urls != null && urls.Length > 0)
                {
                    Interlocked.Exchange(ref responseCounter, 0);

                    for (int i = 0; i < urls.Length; i++)
                    {
                        ////if (Interlocked.CompareExchange(ref requestCounter, 5, 5) == 5)
                        ////{
                        ////    break;
                        ////}

                        if (dicHTML.ContainsKey(urls[i])) continue;

                        Interlocked.Increment(ref responseCounter);

                        dicHTML.Add(urls[i], string.Empty);
                        try
                        {
                            HttpWebRequest w = (HttpWebRequest)WebRequest.Create(new Uri(urls[i]));
                            w.Timeout = 5000;
                            w.BeginGetResponse(asyncResult =>
                            {
                                string htm = string.Empty, url = ((HttpWebRequest)asyncResult.AsyncState).RequestUri.ToString();


                                Interlocked.Increment(ref requestCounter);
                                Console.WriteLine(string.Format("{0}: {1}", requestCounter, url));

                                try
                                {
                                    HttpWebResponse rs = (HttpWebResponse)w.EndGetResponse(asyncResult); //add a break point here 
                                                                                                         //url = rs.ResponseUri.ToString();

                                if (rs.StatusCode == HttpStatusCode.OK)
                                    {
                                        using (StreamReader sr = new StreamReader(rs.GetResponseStream(), Encoding.UTF8))
                                            htm = sr.ReadToEnd();
                                        rs.Close();
                                    }

                                    if (!string.IsNullOrEmpty(htm))
                                    {
                                        htm = HttpUtility.HtmlDecode(htm);
                                        htm = format_HTML(htm);
                                        dicHTML.AddOrUpdate(url, htm);

                                        string[] us = get_UrlHtml(url, htm);
                                        if (us.Length > 0)
                                        {
                                            us = us.Where(x => !dicHTML.Keys.Any(xi => xi == x)).ToArray();
                                            if (us.Length > 0)
                                                listURL.AddRange(us, true);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(string.Format("----> FAIL: {0}| {1}", requestCounter, url));
                                }

                                Interlocked.Decrement(ref responseCounter);
                                if (Interlocked.CompareExchange(ref responseCounter, 0, 0) == 0) 
                                    lock (finishedLock)
                                        Monitor.Pulse(finishedLock); 
                            }, w); 
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("----> FAIL: {0}| {1}", requestCounter, urls[i]));

                            Interlocked.Decrement(ref responseCounter);
                            if (Interlocked.CompareExchange(ref responseCounter, 0, 0) == 0) 
                                lock (finishedLock)
                                    Monitor.Pulse(finishedLock); 
                        }
                    } // end for
                }
            }
        }

        static ConcurrentList<string> listURL = new ConcurrentList<string>();
        static SynchronizedCacheString  dicHTML = new SynchronizedCacheString();
        static readonly object finishedLock = new object();
        static int responseCounter = 0;
        static int requestCounter = 0;

        static oLinkExtract get_Urls(string url, string htm)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htm);

            string[] auri = url.Split('/');
            string uri_root = string.Join("/", auri.Where((x, k) => k < 3).ToArray());
            string uri_path1 = string.Join("/", auri.Where((x, k) => k < auri.Length - 2).ToArray());
            string uri_path2 = string.Join("/", auri.Where((x, k) => k < auri.Length - 3).ToArray());

            var lsURLs = doc.DocumentNode
                .SelectNodes("//a")
                .Where(p => p.InnerText != null && p.InnerText.Trim().Length > 0)
                .Select(p => p.GetAttributeValue("href", string.Empty))
                .Select(x => x.IndexOf("../../") == 0 ? uri_path2 + x.Substring(5) : x)
                .Select(x => x.IndexOf("../") == 0 ? uri_path1 + x.Substring(2) : x)
                .Where(x => x.Length > 1 && x[0] != '#')
                .Select(x => x[0] == '/' ? uri_root + x : (x[0] != 'h' ? uri_root + "/" + x : x))
                .ToList();

            //string[] a = htm.Split(new string[] { "http" }, StringSplitOptions.None).Where((x, k) => k != 0).Select(x => "http" + x.Split(new char[] { '"', '\'' })[0]).ToArray();
            //lsURLs.AddRange(a);

            var u_html = lsURLs
                 .Where(x => x.IndexOf(uri_root) == 0)
                 .GroupBy(x => x)
                 .Select(x => x.First())
                 //.Where(x =>
                 //    !x.EndsWith(".pdf")
                 //    || !x.EndsWith(".txt")

                 //    || !x.EndsWith(".ogg")
                 //    || !x.EndsWith(".mp3")
                 //    || !x.EndsWith(".m4a")

                 //    || !x.EndsWith(".gif")
                 //    || !x.EndsWith(".png")
                 //    || !x.EndsWith(".jpg")
                 //    || !x.EndsWith(".jpeg")

                 //    || !x.EndsWith(".doc")
                 //    || !x.EndsWith(".docx")
                 //    || !x.EndsWith(".ppt")
                 //    || !x.EndsWith(".pptx")
                 //    || !x.EndsWith(".xls")
                 //    || !x.EndsWith(".xlsx"))
                 .ToArray();

            //if (!string.IsNullOrEmpty(setting_URL_CONTIANS))
            //    foreach (string key in setting_URL_CONTIANS.Split('|'))
            //        u_html = u_html.Where(x => x.Contains(key)).ToArray();

            //var u_audio = lsURLs.Where(x => x.EndsWith(".mp3")).Distinct().ToArray();
            //var u_img = lsURLs.Where(x => x.EndsWith(".gif") || x.EndsWith(".jpeg") || x.EndsWith(".jpg") || x.EndsWith(".png")).Distinct().ToArray();
            //var u_youtube = lsURLs.Where(x => x.Contains("youtube.com/")).Distinct().ToArray();

            return new oLinkExtract()
            {
                Url_Html = u_html,
                //Url_Audio = u_audio,
                //Url_Image = u_img,
                //Url_Youtube = u_youtube
            };
        }

        static string format_HTML_bak(string s)
        {
            string si = string.Empty;
            s = Regex.Replace(s, @"<script[^>]*>[\s\S]*?</script>", string.Empty);
            s = Regex.Replace(s, @"<style[^>]*>[\s\S]*?</style>", string.Empty);
            s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            s = Regex.Replace(s, @"(?s)(?<=<!--).+?(?=-->)", string.Empty).Replace("<!---->", string.Empty);

            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|meta|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"</?(?i:base|ins|svg|embed|object|frameset|frame|meta|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Remove attribute style="padding:10px;..."
            s = Regex.Replace(s, @"<([^>]*)(\sstyle="".+?""(\s|))(.*?)>", string.Empty);
            s = s.Replace(@">"">", ">");

            string[] lines = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
            s = string.Join(Environment.NewLine, lines);
            return s;

            //HtmlDocument doc = new HtmlDocument();
            //doc.LoadHtml(s);
            //string tagName = string.Empty, tagVal = string.Empty;
            //foreach (var node in doc.DocumentNode.SelectNodes("//*"))
            //{
            //    if (node.InnerText == null || node.InnerText.Trim().Length == 0)
            //    {
            //        node.Remove();
            //        continue;
            //    }

            //    tagName = node.Name.ToUpper();
            //    if (tagName == "A")
            //        tagVal = node.GetAttributeValue("href", string.Empty);
            //    else if (tagName == "IMG")
            //        tagVal = node.GetAttributeValue("src", string.Empty);

            //    //node.Attributes.RemoveAll();
            //    node.Attributes.RemoveAll_NoRemoveClassName();

            //    if (tagVal != string.Empty)
            //    {
            //        if (tagName == "A") node.SetAttributeValue("href", tagVal);
            //        else if (tagName == "IMG") node.SetAttributeValue("src", tagVal);
            //    }
            //}

            //si = doc.DocumentNode.OuterHtml;
            ////string[] lines = si.Split(new char[] { '\r', '\n' }, StringSplitOptions.None).Where(x => x.Trim().Length > 0).ToArray();
            //string[] lines = si.Split(new char[] { '\r', '\n' }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
            //si = string.Join(Environment.NewLine, lines);
            //return si;
        }

        static string format_HTML(string s)
        {
            string si = string.Empty;
            s = Regex.Replace(s, @"<script[^>]*>[\s\S]*?</script>", string.Empty);
            s = Regex.Replace(s, @"<style[^>]*>[\s\S]*?</style>", string.Empty);
            s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            s = Regex.Replace(s, @"(?s)(?<=<!--).+?(?=-->)", string.Empty).Replace("<!---->", string.Empty);

            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|meta|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"</?(?i:base|nav|form|input|fieldset|button|link|symbol|path|canvas|use|ins|svg|embed|object|frameset|frame|meta|noscript)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Remove attribute style="padding:10px;..."
            s = Regex.Replace(s, @"<([^>]*)(\sstyle="".+?""(\s|))(.*?)>", string.Empty);
            s = s.Replace(@">"">", ">");

            string[] lines = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
            s = string.Join(Environment.NewLine, lines);

            int pos = s.ToLower().IndexOf("<body");
            if (pos > 0)
            {
                s = s.Substring(pos + 5);
                pos = s.IndexOf('>') + 1;
                s = s.Substring(pos, s.Length - pos).Trim();
            }

            s = s
                .Replace(@" data-src=""", @" src=""")
                .Replace(@"src=""//", @"src=""http://");

            var mts = Regex.Matches(s, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
            if (mts.Count > 0) 
                foreach (Match mt in mts) 
                    s = s.Replace(mt.ToString(), string.Format("{0}{1}{2}", "<p class=box_img___>", mt.ToString(), "</p>"));
            s = s.Replace("</body>", string.Empty).Replace("</html>", string.Empty).Trim();

            return s;

            //HtmlDocument doc = new HtmlDocument();
            //doc.LoadHtml(s);
            //string tagName = string.Empty, tagVal = string.Empty;
            //foreach (var node in doc.DocumentNode.SelectNodes("//*"))
            //{
            //    if (node.InnerText == null || node.InnerText.Trim().Length == 0)
            //    {
            //        node.Remove();
            //        continue;
            //    }

            //    tagName = node.Name.ToUpper();
            //    if (tagName == "A")
            //        tagVal = node.GetAttributeValue("href", string.Empty);
            //    else if (tagName == "IMG")
            //        tagVal = node.GetAttributeValue("src", string.Empty);

            //    //node.Attributes.RemoveAll();
            //    node.Attributes.RemoveAll_NoRemoveClassName();

            //    if (tagVal != string.Empty)
            //    {
            //        if (tagName == "A") node.SetAttributeValue("href", tagVal);
            //        else if (tagName == "IMG") node.SetAttributeValue("src", tagVal);
            //    }
            //}

            //si = doc.DocumentNode.OuterHtml;
            ////string[] lines = si.Split(new char[] { '\r', '\n' }, StringSplitOptions.None).Where(x => x.Trim().Length > 0).ToArray();
            //string[] lines = si.Split(new char[] { '\r', '\n' }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
            //si = string.Join(Environment.NewLine, lines);
            //return si;
        }

        static string[] get_UrlHtml(string url, string htm)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htm);

            string[] auri = url.Split('/');
            string uri_root = string.Join("/", auri.Where((x, k) => k < 3).ToArray());
            string uri_path1 = string.Join("/", auri.Where((x, k) => k < auri.Length - 2).ToArray());
            string uri_path2 = string.Join("/", auri.Where((x, k) => k < auri.Length - 3).ToArray());

            var lsURLs = doc.DocumentNode
                .SelectNodes("//a")
                .Where(p => p.InnerText != null && p.InnerText.Trim().Length > 0)
                .Select(p => p.GetAttributeValue("href", string.Empty))
                .Select(x => x.IndexOf("../../") == 0 ? uri_path2 + x.Substring(5) : x)
                .Select(x => x.IndexOf("../") == 0 ? uri_path1 + x.Substring(2) : x)
                .Where(x => x.Length > 1 && x[0] != '#')
                .Select(x => x[0] == '/' ? uri_root + x : (x[0] != 'h' ? uri_root + "/" + x : x))
                .Select(x => x.Split('#')[0])
                .ToList();

            //string[] a = htm.Split(new string[] { "http" }, StringSplitOptions.None).Where((x, k) => k != 0).Select(x => "http" + x.Split(new char[] { '"', '\'' })[0]).ToArray();
            //lsURLs.AddRange(a);

            //????????????????????????????????????????????????????????????????????????????????
            uri_root = "https://dictionary.cambridge.org/grammar/british-grammar/";

            var u_html = lsURLs
                 .Where(x => x.IndexOf(uri_root) == 0)
                 .GroupBy(x => x)
                 .Select(x => x.First())
                 //.Where(x =>
                 //    !x.EndsWith(".pdf")
                 //    || !x.EndsWith(".txt")

                 //    || !x.EndsWith(".ogg")
                 //    || !x.EndsWith(".mp3")
                 //    || !x.EndsWith(".m4a")

                 //    || !x.EndsWith(".gif")
                 //    || !x.EndsWith(".png")
                 //    || !x.EndsWith(".jpg")
                 //    || !x.EndsWith(".jpeg")

                 //    || !x.EndsWith(".doc")
                 //    || !x.EndsWith(".docx")
                 //    || !x.EndsWith(".ppt")
                 //    || !x.EndsWith(".pptx")
                 //    || !x.EndsWith(".xls")
                 //    || !x.EndsWith(".xlsx"))
                 .Distinct()
                 .ToArray();

            //if (!string.IsNullOrEmpty(setting_URL_CONTIANS))
            //    foreach (string key in setting_URL_CONTIANS.Split('|'))
            //        u_html = u_html.Where(x => x.Contains(key)).ToArray();

            //var u_audio = lsURLs.Where(x => x.EndsWith(".mp3")).Distinct().ToArray();
            //var u_img = lsURLs.Where(x => x.EndsWith(".gif") || x.EndsWith(".jpeg") || x.EndsWith(".jpg") || x.EndsWith(".png")).Distinct().ToArray();
            //var u_youtube = lsURLs.Where(x => x.Contains("youtube.com/")).Distinct().ToArray();

            return u_html;
        }

    }
}
