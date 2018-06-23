using HtmlAgilityPack;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace appie
{
    public class JobWord : IJob
    {
        readonly QueueThreadSafe<string> queue;
        readonly DictionaryThreadSafe<string, string> storeUrl;
        readonly DictionaryThreadSafe<string, string> storePath;

        static JobWord()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (se, ev) =>
            {
                Assembly asm = null;
                string comName = ev.Name.Split(',')[0];
                string resourceName = @"DLL\" + comName + ".dll";
                var assembly = Assembly.GetExecutingAssembly();
                resourceName = typeof(app).Namespace + "." + resourceName.Replace(" ", "_").Replace("\\", ".").Replace("/", ".");
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        //Debug.WriteLine(resourceName);
                    }
                    else
                    {
                        byte[] buffer = new byte[stream.Length];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                ms.Write(buffer, 0, read);
                            buffer = ms.ToArray();
                        }
                        asm = Assembly.Load(buffer);
                    }
                }
                return asm;
            };
        }

        public void f_stopAndFreeResource()
        {
        }
        private volatile JOB_STATE _state = JOB_STATE.NONE;
        public JOB_STATE State { get { return _state; } }
        public IJobStore StoreJob { get; }
        private volatile int Id = 0;
        public int f_getId() { return Id; }
        public void f_setId(int id) { Interlocked.CompareExchange(ref Id, Id, id); }
        readonly string _groupName = string.Empty;
        public string f_getGroupName() { return _groupName; }
        public JobWord(IJobStore _store)
        {
            this.StoreJob = _store;
            this.queue = new QueueThreadSafe<string>();
            this.storeUrl = new DictionaryThreadSafe<string, string>();
            this.storePath = new DictionaryThreadSafe<string, string>();
        }
        public void f_receiveMessage(Message m) { }
        public void f_sendMessage(Message m) { if (this.StoreJob != null) this.StoreJob.f_job_sendMessage(m); }

        public void f_runLoop(object state, bool timedOut)
        {
            JobInfo ti = (JobInfo)state;
            if (!timedOut)
            {
                ti.f_stopJob();
                return;
            }


            if (this.queue.Count > 0)
            {
                string s = this.queue.Dequeue(string.Empty);
                if (s.Length > 0)
                {
                    test_run_v1(s);
                    //test_run_v2(s);

                }
            }
        }

        #region [ TEST ]

        void test_run_v1(string word)
        {
            word = "forget";
            string url = string.Empty;

            //UrlService.GetAsync("https://dictionary.cambridge.org/dictionary/english/forget", (stream) =>
            //{
            //    oWordDefine wo = new oWordDefine(text);
            //    object rs = null;
            //    string s = string.Empty;
            //    using (var reader = new StreamReader(stream, Encoding.UTF8))
            //        s = reader.ReadToEnd();
            //    if (s.Length > 0)
            //        s = HttpUtility.HtmlDecode(s);
            //    if (s.Length > 0)
            //    {
            //        if (s.Contains(@"<span class=""ipa"">"))
            //            wo.PronunceUK = s.Split(new string[] { @"<span class=""ipa"">" }, StringSplitOptions.None)[1].Split('<')[0].Trim();

            //    }
            //    else return new UrlAnanyticResult() { Message = "Can not read" };
            //    return new UrlAnanyticResult() { Ok = true, Html = s, Result = wo };
            //}, (result) =>
            //{
            //    if (result.Result != null)
            //    {

            //    }
            //});

            url = string.Format("https://www.oxfordlearnersdictionaries.com/definition/english/{0}?q={0}", word);
            //url = "https://en.oxforddictionaries.com/definition/forget";
            UrlService.GetAsync(url, (stream) =>
            {
                oWordDefine wo = new oWordDefine(word);
                object rs = null;
                string s = string.Empty;
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    s = reader.ReadToEnd();
                if (s.Length > 0)
                    s = HttpUtility.HtmlDecode(s);
                if (s.Length > 0)
                {
                    #region

                    const char heading_char = '#'; // ■ ≡ ¶ ■
                    const string heading_text = "\r\n# ";

                    string htm = s, pro = string.Empty, type = string.Empty, mean_en = word.ToUpper();

                    HtmlNode nodes = f_word_speak_getPronunciationFromOxford_Nodes(htm);
                    pro = nodes.QuerySelectorAll("span[class=\"phon\"]").Select(x => x.InnerText).Where(x => !string.IsNullOrEmpty(x)).Take(1).SingleOrDefault();
                    type = nodes.QuerySelectorAll("span[class=\"pos\"]").Select(x => x.InnerText).Where(x => !string.IsNullOrEmpty(x)).Take(1).SingleOrDefault();
                    string[] pro_s = nodes.QuerySelectorAll("span[class=\"vp-g\"]").Select(x => x.InnerText).Where(x => !string.IsNullOrEmpty(x))
                        .Select(x => x.Replace(" BrE BrE", " = UK: ").Replace("; NAmE NAmE", "US: ").Replace("//", "/")).ToArray();
                    string[] word_links = pro_s.Select(x => x.Split('=')[0].Trim()).ToArray();
                    if (pro == null) pro = string.Empty;

                    if (type != null && type.Length > 0)
                        mean_en += " (" + type + ")";

                    if (!string.IsNullOrEmpty(pro))
                    {
                        if (pro.StartsWith("BrE")) pro = pro.Substring(3).Trim();
                        pro = pro.Replace("//", "/");
                    }

                    List<string> ls_Verb_Group = new List<string>();
                    var wgs = nodes.QuerySelectorAll("span[class=\"vp\"]").Select(x => x.InnerText_NewLine).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    foreach (string wi in wgs)
                    {
                        string[] a = wi.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                        ls_Verb_Group.Add(a[a.Length - 1]);
                    }
                    if (ls_Verb_Group.Count > 0)
                        mean_en += heading_text + "REF: " + string.Join("; ", ls_Verb_Group.ToArray());


                    if (word_links.Length > 0)
                        mean_en += "\r\n" + string.Join(Environment.NewLine, word_links).Replace("-ing", "V-ing").Trim();

                    string[] mp3 = nodes.QuerySelectorAll("div[data-src-mp3]")
                        .Select(x => x.GetAttributeValue("data-src-mp3", string.Empty))
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .ToArray();
                    if (mp3.Length > 0)
                    {
                        mean_en += "\r\n{\r\n" + string.Join(Environment.NewLine, mp3) + "\r\n}\r\n";

                         
                    }

                    string[] uns = nodes.QuerySelectorAll("span[class=\"un\"]").Select(x => x.InnerText_NewLine).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    string[] idoms = nodes.QuerySelectorAll("span[class=\"idm-g\"]").Select(x => x.InnerText_NewLine).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    string[] defines = nodes.QuerySelectorAll("li[class=\"sn-g\"]").Select(x => x.InnerText_NewLine).Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    if (defines.Length > 0)
                    {
                        mean_en += heading_text + "DEFINE:\r\n" +
                            string.Join(Environment.NewLine,
                                string.Join(Environment.NewLine, defines)
                                    .Split(new char[] { '\r', '\n' })
                                    .Select(x => x.Replace(".", ".\r\n").Trim())
                                    .Where(x => x.Length > 0)
                                    .ToArray())
                                    .Replace("\r\n[", ". ")
                                    .Replace("]", ":")

                                    .Replace("1\r\n", "\r\n- ")
                                    .Replace("2\r\n", "\r\n- ")
                                    .Replace("3\r\n", "\r\n- ")
                                    .Replace("4\r\n", "\r\n- ")
                                    .Replace("5\r\n", "\r\n- ")
                                    .Replace("6\r\n", "\r\n- ")
                                    .Replace("7\r\n", "\r\n- ")
                                    .Replace("8\r\n", "\r\n- ")
                                    .Replace("9\r\n", "\r\n- ")

                                    .Replace("1.", "\r\n+ ")
                                    .Replace("2.", "\r\n+ ")
                                    .Replace("3.", "\r\n+ ")
                                    .Replace("4.", "\r\n+ ")
                                    .Replace("5.", "\r\n+ ")
                                    .Replace("6.", "\r\n+ ")
                                    .Replace("7.", "\r\n+ ")
                                    .Replace("8.", "\r\n+ ")
                                    .Replace("9.", "\r\n+ ");
                    }

                    if (uns.Length > 0)
                        mean_en += heading_text + "NOTE:\r\n" + string.Join(Environment.NewLine, string.Join(Environment.NewLine, uns).Split(new char[] { '\r', '\n' }).Select(x => x.Replace(".", ".\r\n").Trim()).Where(x => x.Length > 0).ToArray());

                    if (idoms.Length > 0)
                        mean_en += heading_text + "IDOM:\r\n" + string.Join(Environment.NewLine, string.Join(Environment.NewLine, idoms).Split(new char[] { '\r', '\n' }).Select(x => x.Replace(".", ".\r\n").Trim()).Where(x => x.Length > 0).ToArray());

                    mean_en = Regex.Replace(mean_en, "[ ]{2,}", " ").Replace("\r\n’", "’");

                    mean_en = string.Join(Environment.NewLine,
                        mean_en.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                        .Select(x => x.Trim())
                        .Select(x => x.Length > 0 ?
                            (
                                (x[0] == '+' || x[0] == '-') ?
                                    (x[0].ToString() + " " + x[2].ToString().ToUpper() + x.Substring(3))
                                        : (x[0].ToString().ToUpper() + x.Substring(1))
                            ) : x)
                        .ToArray());

                    string[] sens = nodes.QuerySelectorAll("span[class=\"x\"]")
                        .Where(x => !string.IsNullOrEmpty(x.InnerText))
                        .Select(x => x.InnerText.Trim())
                        .Where(x => x.Length > 0)
                        .Select(x => "- " + x)
                        .ToArray();
                    if (sens.Length > 0)
                    {
                        string sen_text = string.Join(Environment.NewLine, sens);
                        mean_en += heading_text + "EXAMPLE:\r\n" + sen_text;
                    }

                    mean_en = mean_en.Replace("See full entry", string.Empty).Replace(Environment.NewLine, "|")
                        .Replace("’ ", @""" ").Replace(".’", @".""").Replace("’|", @"""|")
                        .Replace(" ‘", @" """)
                        .Replace("’", @"'");

                    mean_en = Regex.Replace(mean_en, @"[^\x20-\x7E]", string.Empty);

                    mean_en = mean_en.Replace("|", Environment.NewLine);
                    //mean_en = Regex.Replace(mean_en, @"[^0-9a-zA-Z;,|{}():/'#+-._\r\n]+!\?", " ");
                    mean_en = Regex.Replace(mean_en, "[ ]{2,}", " ");

                    #endregion
                }
                else return new UrlAnanyticResult() { Message = "Can not read" };
                return new UrlAnanyticResult() { Ok = true, Html = s, Result = wo };
            }, (result) =>
            {
                if (result.Result != null)
                {

                }
            });


            ;
        }

        void test_run_v2(string text)
        {
            
        }

        public static string f_word_speak_getURL(string word_en)
        {
            if (!string.IsNullOrEmpty(word_en))
            {
                if (word_en[word_en.Length - 1] == 's')
                    word_en = word_en.Substring(0, word_en.Length - 1);

                string url = string.Empty;
                //url = "https://s3.amazonaws.com/audio.oxforddictionaries.com/en/mp3/you_gb_1.mp3";
                //url = "https://ssl.gstatic.com/dictionary/static/sounds/oxford/you--_gb_1.mp3";
                //url = "https://ssl.gstatic.com/dictionary/static/sounds/20160317/you--_gb_1.mp3";

                url = string.Format("https://ssl.gstatic.com/dictionary/static/sounds/oxford/{0}--_gb_1.mp3", word_en);

                return url;
            }
            return string.Empty;
        }

        private static HtmlNode f_word_speak_getPronunciationFromOxford_Nodes(string s)
        {
            s = Regex.Replace(s, @"<script[^>]*>[\s\S]*?</script>", string.Empty);
            s = Regex.Replace(s, @"<style[^>]*>[\s\S]*?</style>", string.Empty);
            s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            s = Regex.Replace(s, @"(?s)(?<=<!--).+?(?=-->)", string.Empty).Replace("<!---->", string.Empty);

            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|meta|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|meta|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Load the document using HTMLAgilityPack as normal
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(s);

            return doc.DocumentNode;//
        }

        #endregion
    }
}
