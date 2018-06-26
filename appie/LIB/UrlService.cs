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
        public WebRequest Request { set; get; }
        public UrlServiceCallBack Callback { set; get; }
        public Func<Stream, UrlAnanyticResult> FuncAnalytic { set; get; }

        public UrlServicePara(WebRequest request, UrlServiceCallBack callback, Func<Stream, UrlAnanyticResult> funcAnalytic)
        {
            this.Request = request;
            this.FuncAnalytic = funcAnalytic;
            this.Callback = callback;
        }
    }

    public class UrlAnanyticResult
    {
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
        public readonly static Func<Stream, UrlAnanyticResult> Func_GetHTML_UTF8_FORMAT_BROWSER = (stream) =>
        {
            string s = string.Empty;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                s = reader.ReadToEnd();
            if (s.Length > 0)
                s = HttpUtility.HtmlDecode(s);
            
            string si = string.Empty;
            s = Regex.Replace(s, @"<script[^>]*>[\s\S]*?</script>", string.Empty);
            //s = Regex.Replace(s, @"<style[^>]*>[\s\S]*?</style>", string.Empty);
            s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            s = Regex.Replace(s, @"(?s)(?<=<!--).+?(?=-->)", string.Empty).Replace("<!---->", string.Empty);

            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|meta|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"</?(?i:base|header|footer|nav|form|input|select|option|fieldset|button|iframe|link|symbol|path|canvas|use|ins|svg|embed|object|frameset|frame|meta)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

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

            //s = s
            //    .Replace(@" data-src=""", @" src=""")
            //    .Replace(@"src=""//", @"src=""http://");

            var mts = Regex.Matches(s, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
            if (mts.Count > 0) 
                foreach (Match mt in mts) 
                    s = s.Replace(mt.ToString(), string.Format("{0}{1}{2}", "<p class=box_img___>", mt.ToString(), "</p>")); 

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

            #region
            s =  
@"<html>
<head>
    <meta charset=""utf-8"" />
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
    <title></title>
    <style type=""text/css"">
        body {
            padding: 20px 10px;
            margin: 0;
            font-size: 1.5em;
        }

        p {
            line-height: 1.7em;
        }
        
        a {
            text-decoration: none;
        } 

        img {
            max-width: 20% !important;
        }
        p.box_img___ {
            display:none;
            text-align:right;
        }
        p.box_img___ img{
        }
    </style> 
</head>
<body>" + s +
@"
<script type=""text/javascript"">
    
    var htm;
    findMain(document.body.childNodes, 0);
    //console.log('OK: ', htm == null ? 'NULL -> find H1' : htm.length);
    if (htm == null) findH1();
    if (htm == null) {
        //console.log('FAILLL: ');
        cleanHtml(document.getElementsByTagName('*'));
    } else document.body.innerHTML = htm;

    function findH1() {
        var h1s = document.getElementsByTagName('h1');
        if (h1s.length > 0) {
            var it = h1s[h1s.length - 1].parentNode;
            while (it.parentNode.tagName != 'BODY') {
                it = it.parentNode;
            }
            var bs = document.getElementsByClassName(it.className);
            if (bs.length > 1) {
                for (var i = 0; i < bs.length; i++) {
                    cleanHtml(bs[i].getElementsByTagName('*'));
                    if (htm == null)
                        htm = bs[i].innerHTML;
                    else
                        htm += bs[i].innerHTML;
                }
            } else {
                //console.log('OK = ' + h1s.length, it);
                cleanHtml(it.getElementsByTagName('*'));
                htm = it.innerHTML;
            }
        }
    }

    function findMain(elements) {
        for (var i = 0; i < elements.length; i++) {
            var it = elements[i], css = it.className, id = it.id, tagName = it.tagName, text = it.textContent, ok = false;

            if (it.hasChildNodes() == false) {
                //console.log('REMOVE: ', it);
                it.parentNode.removeChild(it);
            } else {
                if (css == null) css = ''; else css = css.toLowerCase();
                if (id == null) id = ''; else id = id.toLowerCase();
                if (text == null) text = ''; else text = text.trim();
                if (id.length == 0 && css.length == 0 || text.length == 0) continue;

                ok = is_content(id, css, tagName, text);

                if (ok) {
                    var h1 = it.getElementsByTagName('h1').length;
                    if (htm == null && h1 > 0) {
                        //console.log('OK = ' + h1, it);
                        cleanHtml(it.getElementsByTagName('*'));
                        htm = it.innerHTML;
                    }
                    //return;
                } else {
                    //console.log(id + ': ' + css + ' remove = ' + is_remove(id, css), htm);
                    if (htm != null || is_remove(id, css)) {
                        //console.log('remove: ', it);
                        it.parentNode.removeChild(it);
                    } else
                        findMain(it.childNodes);
                }
            }
        }
    }

    function is_content(id, css, tagName, text) {
        var ok = (id.length > 0 && id.indexOf('main') != -1) || (css.length > 0 && css.indexOf('main') != -1)
            || (id.length > 0 && id.indexOf('content') != -1) || (css.length > 0 && css.indexOf('content') != -1);
        if (ok) {
            if (
            id.indexOf('menu') != -1
            || css.indexOf('menu') != -1
            || id.indexOf('search') != -1
            || css.indexOf('search') != -1
            || id.indexOf('header') != -1
            || css.indexOf('header') != -1
            || id.indexOf('footer') != -1
            || css.indexOf('footer') != -1
            || id.indexOf('cookie') != -1
            || css.indexOf('cookie') != -1
            || id.indexOf('feedback') != -1
            || css.indexOf('feedback') != -1
            || id.indexOf('error') != -1
            || css.indexOf('error') != -1
            ) return false;
            return true;
        }
        return false;
    }

    function cleanHtml(elements) {
        for (var i = 0; i < elements.length; i++) {
            var it = elements[i], css = it.className, id = it.id, tagName = it.tagName, text = it.textContent, ok = false;
            if (is_remove(id, css)) it.parentNode.removeChild(it);
        }
    }

    function is_remove(id, css) {
        return id.indexOf('search') != -1
            || css.indexOf('search') != -1
            || id.indexOf('header') != -1
            || css.indexOf('header') != -1
            || id.indexOf('footer') != -1
            || css.indexOf('footer') != -1
            || id.indexOf('cookie') != -1
            || css.indexOf('cookie') != -1
            || id.indexOf('newsletter') != -1
            || css.indexOf('newsletter') != -1
            || id.indexOf('breadcrumb') != -1
            || css.indexOf('breadcrumb') != -1
            || id.indexOf('metadata') != -1
            || css.indexOf('metadata') != -1
            || id.indexOf('feedback') != -1
            || css.indexOf('feedback') != -1
            || id.indexOf('sidebar') != -1
            || css.indexOf('sidebar') != -1
            || id.indexOf('page-background') != -1
            || css.indexOf('page-background') != -1
            || id.indexOf('share-') != -1
            || css.indexOf('share-') != -1
            || id.indexOf('share-') != -1
            || css.indexOf('-action') != -1
            || id.indexOf('nav-') != -1
            || css.indexOf('nav-') != -1
            || id.indexOf('error') != -1
            || css.indexOf('error') != -1;
    }

    </script>";
            #endregion

            File.WriteAllText("result_.html", s);

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

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(ar);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    callback(new UrlAnanyticResult() { Message = "Response is failed with code: " + response.StatusCode });
                    return;
                }

                using (var stream = response.GetResponseStream())
                {
                    if (funcAnalytic != null)
                    {
                        UrlAnanyticResult rs = funcAnalytic(stream);
                        callback(rs);
                    }
                    else {
                        string s = string.Empty;
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                            s = reader.ReadToEnd();
                        if (s.Length > 0)
                            s = HttpUtility.HtmlDecode(s);
                        callback(new UrlAnanyticResult() { Ok = true, Html = s });
                    }
                }
            }
            catch (Exception ex)
            {
                callback(new UrlAnanyticResult(){ Message = "Request failed.\r\n" + ex.Message });
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
