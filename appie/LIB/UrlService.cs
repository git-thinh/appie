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

    public class UrlService
    {
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
    }

}
