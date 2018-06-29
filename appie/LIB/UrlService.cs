using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace appie
{
    public delegate void UrlServiceCallBack(UrlAnanyticResult result);

    public class UrlServicePara
    {
        public Message Message { set; get; }
        public WebRequest Request { set; get; }
        public UrlServiceCallBack Callback { set; get; }
        public Func<Stream, UrlAnanyticResult> FuncAnalytic { set; get; }

        public UrlServicePara(WebRequest request, UrlServiceCallBack callback, Func<Stream, UrlAnanyticResult> funcAnalytic)
        {
            this.Request = request;
            this.FuncAnalytic = funcAnalytic;
            this.Callback = callback;
        }
        public UrlServicePara(Message msg, WebRequest request, UrlServiceCallBack callback, Func<Stream, UrlAnanyticResult> funcAnalytic)
        {
            this.Message = msg;
            this.Request = request;
            this.FuncAnalytic = funcAnalytic;
            this.Callback = callback;
        }
    }

    public class UrlAnanyticResult
    {
        public Message Msg { set; get; }
        public bool Ok { set; get; }
        public string Html { set; get; }
        public string Message { set; get; }
        public object Result { set; get; }

        public UrlAnanyticResult() {
            Ok = false;
            Html = string.Empty;
        }
    }

    // UrlService.GetAsync("http://...", (result) => { ;;; });
    public class UrlService
    {
        public static string js = File.ReadAllText("clean.js");
        public static string css = File.ReadAllText("css.css");

        public readonly static Func<Stream, UrlAnanyticResult> Func_GetHTML_UTF8_FORMAT_BROWSER = (stream) =>
        {
            string s = string.Empty;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                s = reader.ReadToEnd();
            if (s.Length > 0)
                s = HttpUtility.HtmlDecode(s);

            var mbody = Regex.Match(s, @"(?<=<body[^>]*>)[\s\S]*?(?=</body>)");
            if (mbody.Success) s = mbody.Value;

            string si = string.Empty;
            s = Regex.Replace(s, @"<script[^>]*>[\s\S]*?</script>", string.Empty);
            //s = Regex.Replace(s, @"<style[^>]*>[\s\S]*?</style>", string.Empty);
            s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            s = Regex.Replace(s, @"(?s)(?<=<!--).+?(?=-->)", string.Empty).Replace("<!---->", string.Empty);
            s = Regex.Replace(s, @"<form[^>]*>[\s\S]*?</form>", string.Empty);
            s = Regex.Replace(s, @"<select[^>]*>[\s\S]*?</select>", string.Empty);
            s = Regex.Replace(s, @"<script[^>]*>[\s\S]*?</script>", string.Empty);
            s = Regex.Replace(s, @"</?(?i:base|header|footer|nav|form|input|select|option|fieldset|button|iframe|link|symbol|path|canvas|use|ins|svg|embed|object|frameset|frame|meta)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            //// Remove attribute style="padding:10px;..."
            //s = Regex.Replace(s, @"<([^>]*)(\sstyle="".+?""(\s|))(.*?)>", string.Empty);
            //s = s.Replace(@">"">", ">");

            string[] lines = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
            s = string.Join(Environment.NewLine, lines);

            s = s
                //.Replace(@"<code>", @"<textarea>").Replace(@"</code>", @"</textarea>")
                //.Replace(@"<code>", string.Empty).Replace(@"</code>", string.Empty) 
                .Replace(@" data-src=""", @" src=""")
                .Replace(@"src=""//", @"src=""http://");

            //var mts = Regex.Matches(s, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
            //if (mts.Count > 0) 
            //    foreach (Match mt in mts) 
            //        s = s.Replace(mt.ToString(), string.Format("{0}{1}{2}", "<p class=box_img___>", mt.ToString(), "</p>")); 

            //s = Regex.Replace(s, @"(?<=<li[^>]*>)\s*<a.*?(?=</li>)", "", RegexOptions.Singleline);
            //s = s.Replace("<li></li>", string.Empty).Replace("<ul></ul>", string.Empty);

            //foreach (Match m in Regex.Matches(s, @"<ul[^>]*>[\s\S]*?</ul>"))
            //    s = s.Replace(m.Value, string.Empty);
            //foreach (Match m in Regex.Matches(s, @"<li[^>]*>[\s\S]*?</li>"))
            //    s = s.Replace(m.Value, string.Empty);
            //s = RemoveAllHtmlTag(s, new string[] { "ul", "li" });
            s = s.Substring(s.Split('>')[0].Length + 1);

            //foreach (Match m in Regex.Matches(s, @"<div [^>]*class=\""\s*menu.*?\""([\s\S]*?)</div>", RegexOptions.IgnoreCase)) s = s.Replace(m.Value, string.Empty);
            //foreach (Match m in Regex.Matches(s, @"<div [^>]*class=\""\s*search.*?\""([\s\S]*?)</div>", RegexOptions.IgnoreCase)) s = s.Replace(m.Value, string.Empty);
            //foreach (Match m in Regex.Matches(s, @"<div [^>]*class=\""\s*newsletters.*?\""([\s\S]*?)</div>", RegexOptions.IgnoreCase)) s = s.Replace(m.Value, string.Empty);

            //s = removeById_ClassName(s, "menu");
            //s = removeById_ClassName(s, "search");
            //s = removeById_ClassName(s, "_newsletter_");//news_newsletter_form

            return new UrlAnanyticResult() { Ok = true, Html = s };
        };

        public readonly static Func<Stream, UrlAnanyticResult> Func_GetHTML_UTF8 = (stream) =>
        {
            string s = string.Empty;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                s = reader.ReadToEnd();
            if (s.Length > 0)
                s = HttpUtility.HtmlDecode(s);
            return new UrlAnanyticResult() { Ok = true, Html = s };
        };

        public readonly static Func<Stream, UrlAnanyticResult> Func_GetHTML_ASCII = (stream) =>
        {
            string s = string.Empty;
            using (var reader = new StreamReader(stream, Encoding.ASCII))
                s = reader.ReadToEnd();
            if (s.Length > 0)
                s = HttpUtility.HtmlDecode(s);
            return new UrlAnanyticResult() { Ok = true, Html = s };
        };

        private const string RequestUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:55.0) Gecko/20100101 Firefox/55.0";

        public static void GetAsync(string url, Func<Stream, UrlAnanyticResult> func_analytic, UrlServiceCallBack callBack)
        {
            var request = f_CreateWebRequest(url);
            request.BeginGetResponse(f_UrlRequestCallBack, new UrlServicePara(request, callBack, func_analytic));
        }

        public static void GetAsync(string url, Message msg, Func<Stream, UrlAnanyticResult> func_analytic, UrlServiceCallBack callBack)
        {
            var request = f_CreateWebRequest(url);
            request.BeginGetResponse(f_UrlRequestCallBack, new UrlServicePara(msg, request, callBack, func_analytic));
        }

        static WebRequest f_CreateWebRequest(string url)
        {
            var create = (HttpWebRequest)WebRequest.Create(url);
            create.UserAgent = RequestUserAgent;
            create.Timeout = 50 * 1000;
            return create;
        }

        static void f_UrlRequestCallBack(IAsyncResult ar)
        {
            var pair = (UrlServicePara)ar.AsyncState;
            WebRequest request = pair.Request;
            UrlServiceCallBack callback = pair.Callback;
            Func<Stream, UrlAnanyticResult> funcAnalytic = pair.FuncAnalytic;
            Message msg = pair.Message;


            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(ar);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    callback(new UrlAnanyticResult() { Message = "Response is failed with code: " + response.StatusCode, Msg = msg });
                    return;
                }

                using (var stream = response.GetResponseStream())
                {
                    if (funcAnalytic != null)
                    {
                        UrlAnanyticResult rs = funcAnalytic(stream);
                        if (!string.IsNullOrEmpty(rs.Html))
                        {
                            //string domain = request.RequestUri.Host.Replace('.', '_');
                            string[] ad = request.RequestUri.Host.Split(new char[] { '.', ':' });
                            string domain = ad.Length > 2 ? ad[ad.Length - 2] : ad[0];
                            rs.Html = @"<html><head><meta charset=""utf-8"" /><meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" /><title></title><style type=""text/css"">" + css + "</style></head><body class=" + domain + ">" + rs.Html + @"<script type=""text/javascript""> " + js + " </script>";
                            rs.Html = rs.Html.Replace("[_URL_]", request.RequestUri.ToString());
                            
                            File.WriteAllText("result_.html", rs.Html);
                        }
                        rs.Msg = msg;
                        callback(rs);
                    }
                    else {
                        string s = string.Empty;
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                            s = reader.ReadToEnd();
                        if (s.Length > 0)
                            s = HttpUtility.HtmlDecode(s);
                        callback(new UrlAnanyticResult() { Ok = true, Html = s, Msg = msg });
                    }
                }
            }
            catch (Exception ex)
            {
                callback(new UrlAnanyticResult(){ Message = "Request failed.\r\n" + ex.Message, Msg = msg });
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }

        static string RemoveAllHtmlTag(string content, string[] removeTagArray)
        {
            string result = content;
            //string[] removeTagArray = new string[] { "b", "a", "script", "i", "ul", "li", "ol", "font", "span", "div", "u" };
            foreach (string removeTag in removeTagArray)
            {
                string regExpressionToRemoveBeginTag = string.Format("<{0}([^>]*)>", removeTag);
                Regex regEx = new Regex(regExpressionToRemoveBeginTag, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                result = regEx.Replace(result, "");

                string regExpressionToRemoveEndTag = string.Format("</{0}([^>]*)>", removeTag);
                regEx = new Regex(regExpressionToRemoveEndTag, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                result = regEx.Replace(result, "");
            }
            return result;
        }

        static string removeById_ClassName(string s, string Id_className)
        {
            foreach (Match m in Regex.Matches(s, @"<div .*?class=['""](.+?)['""].*?>(.+?)</div>"))
                if (m.Groups[1].Value.ToLower().Contains(Id_className))
                    s = s.Replace(m.Value, string.Empty);

            //foreach (Match m in Regex.Matches(s, @"<div .*?id=['""](.+?)['""].*?>(.+?)</div>"))
            foreach (Match m in Regex.Matches(s, @"<div[^>]*?ids*=s*[""\']?([^\'"" >]+?)[ \'""][^>]*?>(.+?)</div>"))
                if (m.Groups[1].Value.ToLower().Contains(Id_className))
                    s = s.Replace(m.ToString(), string.Empty);

            return s;
        }
    }

}
