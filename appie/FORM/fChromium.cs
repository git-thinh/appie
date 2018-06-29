// fonts .svg .woff .ttf 


using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace appie
{
    public class fChromium : fBase, IRequestHandler
    {
        private void f_event_OnReceiveMessage(IFORM form, Message m)
        {
            switch (m.JobName)
            {
                case JOB_NAME.SYS_LINK:
                    switch (m.getAction())
                    {
                        case MESSAGE_ACTION.ITEM_SEARCH:
                            break;
                        case MESSAGE_ACTION.URL_REQUEST_CACHE:
                            break;
                    }
                    break;
            }
        }

        #region [ === FORM === ]

        public fChromium(IJobStore store) : base(store)
        {
            this.Text = "English";
            this.OnReceiveMessage += f_event_OnReceiveMessage;
            this.Shown += f_form_Shown;
            this.FormClosing += f_form_Closing;

            browser = new WebView(url, new BrowserSettings());
            browser.Dock = DockStyle.Fill;
            browser.RequestHandler = this;
            this.Controls.Add(browser);
            //this.WindowState = FormWindowState.Maximized;
            //this.Text = String.Format("Chromium: {0}, CEF: {1}, CefSharp: {2}, Environment: x86", CEF.ChromiumVersion, CEF.CefVersion, CEF.CefSharpVersion);

            var btn = new Button() { Location = new System.Drawing.Point(0, 0), Text = "DEV", Width = 45 };
            btn.Click += (se, ev) =>
            {
                browser.ShowDevTools();
            };
            this.Controls.Add(btn);
            btn.BringToFront();
        }

        private void f_form_Closing(object sender, FormClosingEventArgs e)
        {
        }

        private void f_form_Shown(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }

        #endregion

        #region [ === BROWSER === ]

        private readonly WebView browser;
        //const string url = "https://www.google.com/maps";
        //const string url = "http://web20office.com/crm/demo/system/login.php?r=/crm/demo";
        //const string url = "file:///G:/_EL/Document/data_el2/book/84-cau-truc-va-cau-vi-du-thong-dung-trong-tieng-anh-giao-tiep.pdf";
        const string url = "https://www.google.com";
        //const string url = "https://www.youtube.com/";
        //const string url = "https://drive.google.com/open?id=1TG-FDU0cZ48vaJCMcAO33iNOuNqgL9BH";
        //const string url = "https://drive.google.com/open?id=1B_DuOqTAQOcZjuls6bw9Tnx_0nd8qpr8";
        //const string url = "https://drive.google.com/file/d/1B_DuOqTAQOcZjuls6bw9Tnx_0nd8qpr8/view";
        //const string url = "https://drive.google.com/file/d/1TG-FDU0cZ48vaJCMcAO33iNOuNqgL9BH/view";

        string domain = url.Split('/')[2];

        #region [ IRequestHandler Members ]

        bool IRequestHandler.OnBeforeResourceLoad(IWebBrowser browser, IRequestResponse requestResponse)
        {
            //System.Diagnostics.Debug.WriteLine("OnBeforeResourceLoad");
            IRequest request = requestResponse.Request;
            string url = request.Url, s = string.Empty;
            Debug.WriteLine(url);
            //var headers = request.GetHeaders();
            if (url.Contains(".js")
                || url.Contains(domain) == false
                || url.Contains("font")
                || url.Contains(".png") || url.Contains(".jpeg") || url.Contains(".jpg") || url.Contains(".gif"))
            {
                Debug.WriteLine("----> " + url);
                return true;
            }

            #region

            ////IRequest request = requestResponse.Request;
            ////string url = request.Url, s = string.Empty;
            //            MemoryStream stream;
            //            byte[] bytes;
            //            if (url.EndsWith(".mp4"))
            //            {
            //                string id = Path.GetFileName(url);
            //                id = id.Substring(0, id.Length - 4);
            //                string desUrl = string.Format("https://drive.google.com/uc?export=download&id={0}", id);

            //                //stream = new System.IO.MemoryStream();
            //                ////bytes = System.Text.ASCIIEncoding.UTF8.GetBytes("");

            //                //FileStream file = new FileStream(@"E:\_cs\cef\cef_119_youtube\bin\x86\Debug\player\files\1.mp4", FileMode.Open, FileAccess.Read, FileShare.Read);
            //                //bytes = new byte[file.Length];
            //                //file.Read(bytes, 0, (int)file.Length);
            //                //file.Close();

            //                //stream.Write(bytes, 0, bytes.Length);

            //                //requestResponse.RespondWith(stream, "video/mp4");

            //                desUrl = "https://r6---sn-8qj-i5oed.googlevideo.com/videoplayback?source=youtube&ms=au%2Crdu&mt=1526202288&mv=m&mm=31%2C29&mn=sn-8qj-i5oed%2Csn-i3b7kn7d&requiressl=yes&key=yt6&itag=22&mime=video%2Fmp4&ipbits=0&signature=CFA4FBAB6DAF7D4E1E6F8643865E06BD13C9B2C9.4AE8093B9CC164EE634F1465807AE309CB9EC5C3&dur=234.289&expire=1526223993&pl=20&ratebypass=yes&pcm2cms=yes&fvip=2&lmt=1510741625396835&id=o-APLwY1H9955dAWnARW0t1FTqsoCs-_OffF4spks0P2AQ&ei=GQD4WtupH4mngQOysI3oCw&c=WEB&initcwndbps=960000&sparams=dur%2Cei%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpcm2cms%2Cpl%2Cratebypass%2Crequiressl%2Csource%2Cexpire&ip=14.177.123.70";

            //                requestResponse.Redirect(desUrl);
            //            }
            //            else
            //            {
            //                url = url.ToLower();
            //                #region
            //                switch (url)
            //                {
            //                    case "http://i.ytimg.com/crossdomain.xml":
            //                    case "https://drive.google.com/crossdomain.xml":
            //                        #region
            //                        stream = new MemoryStream();
            //                        s = @"<?xml version=""1.0""?>
            //<!DOCTYPE cross-domain-policy SYSTEM
            //""http://www.adobe.com/xml/dtds/cross-domain-policy.dtd"">
            //<cross-domain-policy>
            //   <site-control permitted-cross-domain-policies=""all""/>
            //   <allow-access-from domain=""*"" secure=""false""/>
            //   <allow-http-request-headers-from domain=""*"" headers=""*"" secure=""false""/>
            //</cross-domain-policy>";
            //                        s = @"<cross-domain-policy><allow-access-from domain=""*"" /></cross-domain-policy>";

            //                        bytes = ASCIIEncoding.UTF8.GetBytes("");
            //                        stream.Write(bytes, 0, bytes.Length);
            //                        requestResponse.RespondWith(stream, "text/xml");
            //                        #endregion
            //                        break;
            //                    case "http://l.longtailvideo.com/5/10/logo.png":
            //                        stream = new MemoryStream();
            //                        bytes = new byte[] { 0 };
            //                        stream.Write(bytes, 0, bytes.Length);
            //                        requestResponse.RespondWith(stream, "image/png");
            //                        break;
            //                    case "http://www.youtube.com/apiplayer":
            //                        stream = new System.IO.MemoryStream();
            //                        bytes = System.Text.ASCIIEncoding.UTF8.GetBytes("");
            //                        stream.Write(bytes, 0, bytes.Length);
            //                        requestResponse.RespondWith(stream, "text/html; charset=utf-8");
            //                        break;
            //                }



            //                ////if (request.Url.EndsWith("header.png"))
            //                ////{
            //                ////    MemoryStream stream = new System.IO.MemoryStream();

            //                ////    FileStream file = new FileStream(@"C:\tmp\header.png", FileMode.Open, FileAccess.Read, FileShare.Read);
            //                ////    byte[] bytes = new byte[file.Length];
            //                ////    file.Read(bytes, 0, (int)file.Length);
            //                ////    stream.Write(bytes, 0, (int)file.Length);
            //                ////    file.Close();

            //                ////    requestResponse.RespondWith(stream, "image/png");
            //                ////}
            //                #endregion
            //            }
            
            #endregion

            return false;
        }

        bool IRequestHandler.OnBeforeBrowse(IWebBrowser browser, IRequest request, NavigationType naigationvType, bool isRedirect)
        {
            Debug.WriteLine("GO ====> " + request.Url);
            domain = request.Url.Split('/')[2];
            return false;
        }

        void IRequestHandler.OnResourceResponse(IWebBrowser browser, string url, int status, string statusText, string mimeType, WebHeaderCollection headers)
        {
            //string content_type = headers.Get("Content-Type");
            ////if (url.EndsWith(".mp4")) { }
            ////System.Diagnostics.Debug.WriteLine("OnResourceResponse");
            //Debug.WriteLine(content_type + " === " + url);
        }

        public bool GetDownloadHandler(IWebBrowser browser, string mimeType, string fileName, long contentLength, ref IDownloadHandler handler)
        {
            return false;
        }

        public bool GetAuthCredentials(IWebBrowser browser, bool isProxy, string host, int port, string realm, string scheme, ref string username, ref string password)
        {
            return false;
        }

        #endregion

        void f_brow_Close()
        {
            browser.Dispose();
            CEF.Shutdown();
        }

        #endregion
    }
}
