using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace appie
{
    public class JobWebClient : IJob
    {
        readonly IJobStore _store;
        readonly Object _lock = new object();

        private bool isDownloading = false;
        private int Id = 0;

        public JobWebClient(IJobStore store)
        {
            _store = store;
        }

        public void Run(object state, bool timedOut)
        {
            JobInfo ti = (JobInfo)state;
            if (!timedOut)
            {
                ti.StopJob();
                return;
            }
            
            lock (_lock)
                if (isDownloading)
                    return;

            string _url = _store.f_url_getUrlPending();
            if (_url.Length == 0) return;

            Interlocked.CompareExchange(ref Id, ti.GetId(), 0);
            Trace.WriteLine("J{0} -> {1}", Id, _url);

            HttpWebRequest w = (HttpWebRequest)WebRequest.Create(new Uri(_url));
            w.BeginGetResponse(asyncResult =>
            {
                string url = ((HttpWebRequest)asyncResult.AsyncState).RequestUri.ToString();
                string data = string.Empty;
                bool isSuccess = true;
                try
                {
                    HttpWebResponse rs = (HttpWebResponse)w.EndGetResponse(asyncResult);
                    if (rs.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(rs.GetResponseStream(), System.Text.Encoding.UTF8))
                            data = sr.ReadToEnd();
                        rs.Close();
                    }

                    if (!string.IsNullOrEmpty(data))
                    {
                        data = HttpUtility.HtmlDecode(data);
                        data = format_HTML(data);

                        string[] urls = get_UrlHtml(url, data);
                        if (urls.Length > 0)
                            _store.f_url_AddRange(urls);
                    }
                    else
                    {
                        isSuccess = false;
                        data = "REQUEST_FAIL";
                    }
                }
                catch (Exception ex)
                {
                    data = ex.Message;
                    isSuccess = false;
                }
                                
                Trace.WriteLine("J{0} <- {1}", Id, _url);

                _store.f_url_countResult(_url, data, isSuccess);

                int urlCounter = _store.f_url_countPending();
                if (urlCounter == 0)
                {
                    bool end = _store.f_url_stateJobIsComplete(Id);
                    if (end) _store.f_url_Complete();
                    return;
                }

                lock (_lock)
                    isDownloading = false;
            }, w);
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
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
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
