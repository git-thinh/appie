using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

            f_brow_Init();
        }

        private void f_form_Closing(object sender, FormClosingEventArgs e)
        {
            f_brow_Close();
        }

        private void f_form_Shown(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }

        #endregion

        #region [ === BROWSER === ]

        const string DOMAIN_GOOGLE = "www.google.com";
        const string DOM_CONTENT_LOADED = "DOM_CONTENT_LOADED";

        //string brow_URL = "https://www.google.com";
        string brow_URL = "https://developers.google.com/web/tools/chrome-devtools/network-performance/";
        //string brow_URL = "https://www.google.com/maps";
        //string brow_URL = "http://web20office.com/crm/demo/system/login.php?r=/crm/demo";
        //string brow_URL = "file:///G:/_EL/Document/data_el2/book/84-cau-truc-va-cau-vi-du-thong-dung-trong-tieng-anh-giao-tiep.pdf";
        //string brow_URL = "https://www.youtube.com/";
        //string brow_URL = "https://drive.google.com/open?id=1TG-FDU0cZ48vaJCMcAO33iNOuNqgL9BH";
        //string brow_URL = "https://drive.google.com/open?id=1B_DuOqTAQOcZjuls6bw9Tnx_0nd8qpr8";
        //string brow_URL = "https://drive.google.com/file/d/1B_DuOqTAQOcZjuls6bw9Tnx_0nd8qpr8/view";
        //string brow_URL = "https://drive.google.com/file/d/1TG-FDU0cZ48vaJCMcAO33iNOuNqgL9BH/view";

        string brow_Domain;
        bool importPlugin = false;

        TextBoxWaterMark brow_UrlTextBox;
        WebView browser;
        ControlTransparent brow_Transparent;

        void f_brow_Init()
        {
            brow_Domain = brow_URL.Split('/')[2];
            browser = new WebView(brow_URL, new BrowserSettings());
            browser.Dock = DockStyle.Fill;
            browser.RequestHandler = this;            
            browser.ConsoleMessage += f_brow_onBrowserConsoleMessage;

            brow_Transparent = new ControlTransparent() { Location = new Point(0, 0), Size = new Size(999,999) };

            Panel footer = new Panel() { Dock = DockStyle.Bottom, Height = 32, BackColor = Color.WhiteSmoke, Padding = new Padding(9, 9, 9, 3) };

            brow_UrlTextBox = new TextBoxWaterMark() { WaterMark = "HTTP://...", Dock = DockStyle.Fill, Height = 20 };
            brow_UrlTextBox.KeyDown += (se, ev) =>
            {
                if (ev.KeyCode == Keys.Enter)
                {
                    f_brow_Go(brow_UrlTextBox.Text.Trim());
                }
            };

            var btn = new Button() { Location = new System.Drawing.Point(0, 0), Text = "DEV", Width = 45, Height = 20, Dock = DockStyle.Right };
            btn.Click += (se, ev) =>
            {
                browser.ShowDevTools();
            };
            btn.BringToFront();

            footer.Controls.AddRange(new Control[] { brow_UrlTextBox, btn });
            this.Controls.AddRange(new Control[] { brow_Transparent, browser, footer });
        }
        
        private void f_brow_onBrowserConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            string s = string.Format("LOG: ===== Line {0}, Source: {1}, Message: {2}", e.Line, e.Source, e.Message);
            Debug.WriteLine(s);
            switch (e.Message) {
                case DOM_CONTENT_LOADED:
                    f_brow_onDOMContentLoaded();
                    break;
            }
        }

        void f_brow_Go(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                brow_URL = url;
                brow_Domain = brow_URL.Split('/')[2];
                browser.Load(brow_Domain);
            }
        }

        void f_brow_onBeforeBrowse()
        {
            brow_Transparent.crossThreadPerformSafely(() => brow_Transparent.BringToFront());
        }

        void f_brow_onDOMContentLoaded()
        {
            brow_Transparent.crossThreadPerformSafely(() => brow_Transparent.SendToBack());
        }

        #region [ IRequestHandler Members ]

        bool IRequestHandler.OnBeforeResourceLoad(IWebBrowser browser, IRequestResponse requestResponse)
        {
            //System.Diagnostics.Debug.WriteLine("OnBeforeResourceLoad");
            //var headers = request.GetHeaders();
            string url = requestResponse.Request.Url;
            if (url.StartsWith("chrome-devtools://") == false)
            {
                if (importPlugin == false && (url.Contains(".js") || url.Contains("/js/")))
                {
                    MemoryStream stream;
                    byte[] bytes;
                    switch (brow_Domain)
                    {
                        case DOMAIN_GOOGLE:
                            stream = new System.IO.MemoryStream();
                            bytes = ASCIIEncoding.ASCII.GetBytes(@"document.addEventListener('DOMContentLoaded', function (event) { var a = document.querySelectorAll('img'); for (var i = 0; i < a.length; i++) { a[i].remove(); }; console.log('DOM_CONTENT_LOADED'); }); ");
                            stream.Write(bytes, 0, bytes.Length);
                            requestResponse.RespondWith(stream, "text/javascript; charset=utf-8");
                            break;
                        default:
                            stream = new System.IO.MemoryStream();
                            FileStream file = new FileStream(@"plugin.js", FileMode.Open, FileAccess.Read, FileShare.Read);
                            bytes = new byte[file.Length];
                            file.Read(bytes, 0, (int)file.Length);
                            stream.Write(bytes, 0, (int)file.Length);
                            file.Close();
                            requestResponse.RespondWith(stream, "text/javascript; charset=utf-8");
                            break;
                    }
                    Debug.WriteLine("----> JS === " + url);
                    importPlugin = true;
                    return false;
                }

                if (url.Contains(".js") || url.Contains("/js/")
                    || url.Contains(brow_Domain) == false
                    || url.Contains("font") || url.Contains(".svg") || url.Contains(".woff") || url.Contains(".ttf")
                    || url.Contains("/image") || url.Contains(".png") || url.Contains(".jpeg") || url.Contains(".jpg") || url.Contains(".gif"))
                {
                    Debug.WriteLine("----> " + url);
                    return true;
                }
                Debug.WriteLine(url);
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
            if (request.Url.StartsWith("chrome-devtools://") == false)
            {
                Debug.WriteLine("GO ====> " + request.Url);

                brow_URL = request.Url;
                brow_Domain = brow_URL.Split('/')[2];
                brow_UrlTextBox.crossThreadPerformSafely(() => brow_UrlTextBox.Text = brow_URL);

                importPlugin = false;
                return false;
            }

            f_brow_onBeforeBrowse();
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
