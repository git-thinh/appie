using HtmlAgilityPack;
using ProtoBuf;
using System;
using System.Collections; 
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net; 
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace appie
{
    public class api_crawler : api_base, IAPI
    {
        #region [ VARIABLE ]

        static bool CRAWLER_KEY_STOP = false;

        static int crawlCounter = 0;
        static int crawlPending = 0;
        static int crawlResult = 0;
        const int crawlMaxThread = 5;
        static Dictionary<string, string> dicHtml = new Dictionary<string, string>();

        static List<string> listUrl = new List<string>();
        static readonly object _lockUrl = new object();
        static readonly BackgroundWorker[] tasks = new BackgroundWorker[crawlMaxThread];

        static string domain_current = string.Empty;
        static string url_sub_path_current = string.Empty;
        static string setting_URL_CONTIANS = string.Empty;
        static string setting_PARA1 = string.Empty;
        static string setting_PARA2 = string.Empty;

        #endregion

        public msg Execute(msg m)
        {
            if (m == null) return m;

            switch (m.KEY)
            {
                case _API.CRAWLER_KEY_STOP:
                    f_CRAWLER_KEY_STOP(m);
                    break;
                case _API.CRAWLER_KEY_REGISTER_PATH:
                    f_CRAWLER_KEY_REGISTER_PATH(m);
                    break;
                case _API.CRAWLER_KEY_REQUEST_LINK:
                    f_CRAWLER_KEY_REQUEST_LINK(m);
                    break;
                case _API.CRAWLER_KEY_CONVERT_PACKAGE_TO_HTML:
                    #region
                    ////path_package = (string)m.Input;
                    ////if (!string.IsNullOrEmpty(path_package) && File.Exists(path_package))
                    ////{
                    ////    //var dicRaw = new Dictionary<string, string>();
                    ////    //var dicCon = new Dictionary<string, string>();
                    ////    //var list_XPath = new List<string>();

                    ////    //using (var fileStream = File.OpenRead(path_package))
                    ////    //    dicRaw = Serializer.Deserialize<Dictionary<string, string>>(fileStream);

                    ////    ////foreach (var kv in dicRaw)
                    ////    ////{
                    ////    ////    string s = kv.Value;
                    ////    ////    doc = new HtmlDocument();
                    ////    ////    doc.LoadHtml(s);
                    ////    ////    foreach (var h1 in doc.DocumentNode.SelectNodes("//h1"))
                    ////    ////    {
                    ////    ////        //d1.Add(kv.Key, h1.ParentNode.InnerText);
                    ////    ////        //d2.Add(kv.Key, h1.ParentNode.ParentNode.InnerText);
                    ////    ////        //d3.Add(kv.Key, h1.ParentNode.ParentNode.ParentNode.InnerText);
                    ////    ////        list_XPath.Add(h1.XPath);
                    ////    ////        break;
                    ////    ////    }
                    ////    ////}

                    ////    //foreach (var kv in dicRaw)
                    ////    //{
                    ////    //    string s = kv.Value, si = string.Empty;
                    ////    //    doc = new HtmlDocument();
                    ////    //    doc.LoadHtml(s);
                    ////    //    var ns = doc.DocumentNode.SelectNodes("/html[1]/body[1]/div[3]/article[1]/div[1]/div[1]/div[1]/div[1]/article[1]");
                    ////    //    if (ns != null && ns.Count > 0)
                    ////    //    {
                    ////    //        si = ns[0].InnerHtml;
                    ////    //        dicCon.Add(kv.Key, si);
                    ////    //    }
                    ////    //}

                    ////    //using (var file = File.Create("crawler.htm.bin"))
                    ////    //    Serializer.Serialize<Dictionary<string, string>>(file, dicCon);

                    ////}
                    #endregion
                    break;
                case _API.CRAWLER_KEY_CONVERT_PACKAGE_TO_TEXT:
                    #region
                    ////path_package = (string)m.Input;
                    ////if (!string.IsNullOrEmpty(path_package) && File.Exists(path_package))
                    ////{
                    ////    var dicRaw = new Dictionary<string, string>();
                    ////    var dicText = new Dictionary<string, string>();

                    ////    using (var fileStream = File.OpenRead(path_package))
                    ////        dicRaw = Serializer.Deserialize<Dictionary<string, string>>(fileStream);

                    ////    foreach (var kv in dicRaw)
                    ////    {
                    ////        string s = new htmlToText().ConvertHtml(kv.Value).Trim();
                    ////        dicText.Add(kv.Key, s);
                    ////    }

                    ////    using (var file = File.Create("crawler.txt.bin"))
                    ////        Serializer.Serialize<Dictionary<string, string>>(file, dicText);

                    ////}
                    #endregion
                    break;
            }

            m.Output.Ok = true;
            m.Output.Data = null;
            return m;
        }

        #region [ METHOD ]

        public bool Open { get; set; } = false;

        public void Init()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 1000;
            // active SSL 1.1, 1.2, 1.3 for WebClient request HTTPS
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | (SecurityProtocolType)3072 | (SecurityProtocolType)0x00000C00 | SecurityProtocolType.Tls;

            for (int i = 0; i < crawlMaxThread; i++)
            {
                tasks[i] = new BackgroundWorker();
                tasks[i].DoWork += (se, ev) =>
                {
                    string para_url = (string)ev.Argument;
                    ev.Result = para_url;
                    #region

                    bool running = true;
                    HttpWebRequest w = null;
                    try
                    {
                        w = (HttpWebRequest)WebRequest.Create(new Uri(para_url));
                        w.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36";
                        w.BeginGetResponse(asyncResult =>
                        {
                            try
                            {
                                HttpWebResponse rs = (HttpWebResponse)w.EndGetResponse(asyncResult); //add a break point here  
                                if (rs.StatusCode == HttpStatusCode.NotFound)
                                {
                                    running = false;
                                    return;
                                }
                                string url = rs.ResponseUri.ToString();

                                if (rs.StatusCode == HttpStatusCode.OK)
                                {
                                    StreamReader sr = new StreamReader(rs.GetResponseStream(), Encoding.UTF8);
                                    string htm = sr.ReadToEnd();
                                    sr.Close();
                                    rs.Close();
                                    if (!string.IsNullOrEmpty(htm))
                                    {
                                        htm = HttpUtility.HtmlDecode(htm);
                                        htm = format_HTML(htm);

                                        if (!dicHtml.ContainsKey(url))
                                        {
                                            dicHtml.Add(url, htm);
                                            Interlocked.Increment(ref crawlResult);
                                        }

                                        var us = get_Urls(url, htm);
                                        if (us.Url_Html.Length > 0)
                                            lock (_lockUrl)
                                                listUrl.AddRange(us.Url_Html);
                                    }
                                }
                                running = false;
                            }
                            catch
                            {
                                running = false;
                                w.Abort();
                            }
                        }, w);
                        w.Timeout = 5000;
                    }
                    catch
                    {
                        running = false;
                        if (w != null)
                            w.Abort();
                    }
                    while (running) {; }

                    #endregion
                };
                tasks[i].RunWorkerCompleted += (se, ev) =>
                {
                    if (CRAWLER_KEY_STOP)
                        f_CRAWLER_KEY_STOP_reset();

                    Interlocked.Decrement(ref crawlCounter);
                    if (Interlocked.CompareExchange(ref crawlCounter, 0, 0) == 0)
                    {
                        Execute(new msg() { API = _API.CRAWLER, KEY = _API.CRAWLER_KEY_REQUEST_LINK });
                    }
                    else
                    {
                        string para_url = (string)ev.Result;
                        response_toMain(new msg() { API = _API.CRAWLER, KEY = _API.CRAWLER_KEY_REQUEST_LINK, Log = crawlResult + "|" + crawlPending + ": " + para_url });
                    }
                };
            }

            if (!Directory.Exists("package")) Directory.CreateDirectory("package");
        }

        public void Close()
        {
        }

        //////////////////////////////////////////////////////////////////////

        void f_CRAWLER_KEY_STOP_reset()
        {
            lock (_lockUrl)
                listUrl.Clear();

            Interlocked.Exchange(ref crawlCounter, 0);
        }

        msg f_CRAWLER_KEY_STOP(msg m)
        {
            CRAWLER_KEY_STOP = true;
            string[] rs_out = dicHtml.Keys.ToArray();
            response_toMain(new msg() { API = _API.CRAWLER, KEY = _API.CRAWLER_KEY_REQUEST_LINK, Log = "Crawle complete result: " + rs_out.Length + " links. Writing file ..." });
            if (Interlocked.CompareExchange(ref crawlResult, 1, 1) > 1)
                write_file_contentHTML();
            response_toMain(new msg() { API = _API.CRAWLER, KEY = _API.CRAWLER_KEY_REQUEST_LINK_COMPLETE, Input = rs_out });
            return m;
        }

        msg f_CRAWLER_KEY_REGISTER_PATH(msg m)
        {
            CRAWLER_KEY_STOP = false;
            domain_current = string.Empty;
            setting_URL_CONTIANS = string.Empty;
            setting_PARA1 = string.Empty;
            setting_PARA2 = string.Empty;

            if (m.Input != null)
            {
                Interlocked.Exchange(ref crawlResult, 0);

                oLinkSetting st = (oLinkSetting)m.Input;
                string para_url = st.Url;

                if (st.Settings != null && st.Settings.Count > 0)
                {
                    st.Settings.TryGetValue("URL_CONTIANS", out setting_URL_CONTIANS);
                    st.Settings.TryGetValue("PARA1", out setting_PARA1);
                    st.Settings.TryGetValue("PARA2", out setting_PARA2);
                }
                string[] a = para_url.Split('/');
                domain_current = a[2].ToLower();
                if (domain_current.StartsWith("www.")) domain_current = domain_current.Substring(4);
                if (a.Length > 3)
                    url_sub_path_current = a[3];

                dicHtml.Clear();
                listUrl.Clear();

                read_file_contentHTML();

                HttpWebRequest w = (HttpWebRequest)WebRequest.Create(new Uri(para_url));
                w.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36";
                w.BeginGetResponse(asyncResult =>
                {
                    HttpWebResponse rs = (HttpWebResponse)w.EndGetResponse(asyncResult); //add a break point here 
                    string url = rs.ResponseUri.ToString();
                    response_toMain(new msg() { API = _API.CRAWLER, KEY = _API.CRAWLER_KEY_REQUEST_LINK, Log = url });

                    if (rs.StatusCode == HttpStatusCode.OK)
                    {
                        string htm = string.Empty;
                        StreamReader sr = new StreamReader(rs.GetResponseStream(), Encoding.UTF8);
                        htm = sr.ReadToEnd();
                        sr.Close();
                        rs.Close();
                        if (!string.IsNullOrEmpty(htm))
                        {
                            htm = HttpUtility.HtmlDecode(htm);
                            htm = format_HTML(htm);

                            if (!dicHtml.ContainsKey(url))
                            {
                                dicHtml.Add(url, htm);
                                Interlocked.Increment(ref crawlResult);
                            }

                            var us = get_Urls(url, htm);

                            if (CRAWLER_KEY_STOP)
                            {
                                f_CRAWLER_KEY_STOP_reset();
                                return;
                            }

                            if (us.Url_Html.Length > 0)
                            {
                                lock (_lockUrl)
                                    listUrl.AddRange(us.Url_Html);
                                Execute(new msg() { API = _API.CRAWLER, KEY = _API.CRAWLER_KEY_REQUEST_LINK });
                            }
                            else
                                Execute(new msg() { API = _API.CRAWLER, KEY = _API.CRAWLER_KEY_REQUEST_LINK_COMPLETE, Input = dicHtml.Keys.ToArray() });
                        }
                    }
                }, w);
            }

            return m;
        }

        msg f_CRAWLER_KEY_REQUEST_LINK(msg m)
        {
            if (CRAWLER_KEY_STOP)
                f_CRAWLER_KEY_STOP_reset();

            string[] urls = new string[] { };
            string[] uri_ok = dicHtml.Keys.ToArray();
            lock (_lockUrl)
            {
                listUrl = listUrl.Where(x => !uri_ok.Any(o => o == x)).Distinct().ToList();
                urls = listUrl.Take(crawlMaxThread).ToArray();
                listUrl = listUrl.Where(x => !urls.Any(o => o == x)).ToList();
                Interlocked.Exchange(ref crawlPending, listUrl.Count);
                Interlocked.Exchange(ref crawlCounter, urls.Length);
            }

            //if (Interlocked.CompareExchange(ref crawlPending, 0, 0) == 0)
            if (Interlocked.CompareExchange(ref crawlCounter, 0, 0) == 0)
            {
                string[] rs_out = dicHtml.Keys.ToArray();
                response_toMain(new msg() { API = _API.CRAWLER, KEY = _API.CRAWLER_KEY_REQUEST_LINK, Log = "Crawle complete result: " + rs_out.Length + " links. Writing file ..." });
                if (Interlocked.CompareExchange(ref crawlResult, 1, 1) > 1)
                    write_file_contentHTML();
                response_toMain(new msg() { API = _API.CRAWLER, KEY = _API.CRAWLER_KEY_REQUEST_LINK_COMPLETE, Input = rs_out });
                return m;
            }
            else
            {
                for (int i = 0; i < urls.Length; i++)
                    tasks[i].RunWorkerAsync(urls[i]);
            }

            return m;
        }

        void write_file_contentHTML()
        {
            try
            {
                if (dicHtml.Count > 0)
                {
                    string fi_name = "package/" + domain_current;
                    if (!string.IsNullOrEmpty(url_sub_path_current))
                        fi_name += "___" + url_sub_path_current;
                    fi_name += ".htm";

                    if (File.Exists(fi_name))
                        File.Delete(fi_name);

                    // Using Protobuf-net, I suddenly got an exception about an unknown wire-type
                    // https://stackoverflow.com/questions/2152978/using-protobuf-net-i-suddenly-got-an-exception-about-an-unknown-wire-type
                    using (var file = new FileStream(fi_name, FileMode.Truncate))
                    {
                        // write
                        Serializer.Serialize<Dictionary<string, string>>(file, dicHtml);

                        // SetLength after writing your data:
                        // file.SetLength(file.Position);
                    }

                    //using (var file = File.Create(fi_name))
                    //{
                    //    Serializer.Serialize<Dictionary<string, string>>(file, dicHtml);
                    //    file.Close();
                    //}
                    dicHtml.Clear();
                }
            }
            catch { }
        }

        void read_file_contentHTML()
        {
            if (!string.IsNullOrEmpty(domain_current))
            {
                string fi_name = "package/" + domain_current;
                if (!string.IsNullOrEmpty(url_sub_path_current))
                    fi_name += "___" + url_sub_path_current;
                fi_name += ".htm";

                if (File.Exists(fi_name))
                {
                    using (var file = File.OpenRead(fi_name))
                    {
                        dicHtml = Serializer.Deserialize<Dictionary<string, string>>(file);
                        file.Close();
                    }
                }
            }
        }

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

            if (!string.IsNullOrEmpty(setting_URL_CONTIANS))
                foreach (string key in setting_URL_CONTIANS.Split('|'))
                    u_html = u_html.Where(x => x.Contains(key)).ToArray();

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

        private static string format_HTML(string s)
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

        #endregion
    }

}
