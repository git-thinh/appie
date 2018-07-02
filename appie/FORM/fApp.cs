using FarsiLibrary.Win;
using Gecko;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace appie
{
    public class fApp : fBase 
    {
        readonly Font font_Title = new Font("Arial", 11f, FontStyle.Regular);
        readonly Font font_TextView = new Font("Courier New", 11f, FontStyle.Regular);
        readonly Font font_LogView = new Font("Courier New", 9f, FontStyle.Regular);

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

        public fApp(IJobStore store) : base(store)
        {
            this.Text = "English";
            this.OnReceiveMessage += f_event_OnReceiveMessage;
            this.Shown += f_form_Shown;
            this.FormClosing += f_form_Closing;

            f_brow_Init();
            f_tab_Init();
        }

        private void f_form_Closing(object sender, FormClosingEventArgs e)
        {
            f_brow_Close();
        }

        private void f_form_Shown(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            f_brow_Go(brow_URL);
        }

        #endregion

        #region [ === BROWSER === ]

        const string DOMAIN_GOOGLE = "www.google.com.vn";
        const string DOMAIN_BING = "www.bing.com";
        const string DOM_CONTENT_LOADED = "DOM_CONTENT_LOADED";
        const int TOOLBAR_HEIGHT = 28;
        const int SHORTCUTBAR_HEIGHT = 17;

        string brow_URL = "https://www.google.com.vn";
        //string brow_URL = "https://dictionary.cambridge.org/grammar/british-grammar/do-or-make";
        //string brow_URL = "https://dictionary.cambridge.org";
        //string brow_URL = "https://www.bing.com";
        //string brow_URL = "https://www.bing.com/search?go=Submit&qs=ds&form=QBLH&q=hello";
        //string brow_URL = "https://developers.google.com/web/tools/chrome-devtools/network-performance/";
        //string brow_URL = "https://www.google.com/maps";
        //string brow_URL = "http://web20office.com/crm/demo/system/login.php?r=/crm/demo";
        //string brow_URL = "file:///G:/_EL/Document/data_el2/book/84-cau-truc-va-cau-vi-du-thong-dung-trong-tieng-anh-giao-tiep.pdf";
        //string brow_URL = "https://www.youtube.com/";
        //string brow_URL = "https://drive.google.com/open?id=1TG-FDU0cZ48vaJCMcAO33iNOuNqgL9BH";
        //string brow_URL = "https://drive.google.com/open?id=1B_DuOqTAQOcZjuls6bw9Tnx_0nd8qpr8";
        //string brow_URL = "https://drive.google.com/file/d/1B_DuOqTAQOcZjuls6bw9Tnx_0nd8qpr8/view";
        //string brow_URL = "https://drive.google.com/file/d/1TG-FDU0cZ48vaJCMcAO33iNOuNqgL9BH/view";

        string brow_Domain;
        bool brow_ImportPlugin = false, 
            brow_EnabelJS = true, 
            brow_EnableCSS = false,
            brow_EnableImg = false,
            brow_AutRequest = false;

        TextBoxWaterMark brow_UrlTextBox;
        GeckoWebBrowser browser;
        ControlTransparent brow_Transparent;
        Panel brow_ShortCutBar;

        void f_brow_Init()
        {
            brow_Domain = brow_URL.Split('/')[2];
            browser = new GeckoWebBrowser();
            browser.Dock = DockStyle.Fill;
            browser.NavigationError += (s, e) =>
            {
                Debug.WriteLine("StartDebugServer error: 0x" + e.ErrorCode.ToString("X"));
                browser.Dispose();
            };
            browser.DocumentCompleted += (s, e) =>
            {
                Debug.WriteLine("StartDebugServer completed");
                browser.Dispose();
            };

            brow_Transparent = new ControlTransparent() { Location = new Point(0, 0), Size = new Size(999, 999) };

            Panel toolbar = new Panel() { Dock = DockStyle.Top, Height = TOOLBAR_HEIGHT, BackColor = SystemColors.Control, Padding = new Padding(3, 3, 0, 3) };
            brow_ShortCutBar = new Panel() { Dock = DockStyle.Top, Height = SHORTCUTBAR_HEIGHT, BackColor = SystemColors.Control, Padding = new Padding(0) };

            brow_UrlTextBox = new TextBoxWaterMark() { WaterMark = "HTTP://...", Dock = DockStyle.Fill, Height = 20, Font = font_Title, BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Control };
            brow_UrlTextBox.KeyDown += (se, ev) =>
            {
                if (ev.KeyCode == Keys.Enter)
                {
                    f_brow_Go(brow_UrlTextBox.Text.Trim());
                }
            };
            brow_UrlTextBox.MouseDoubleClick += (se, ev) =>
            {
                brow_UrlTextBox.Text = string.Empty;
            };

            var btn_ToggleTab = new Button() { Text = tab_IconToggle, Width = 19, Height = 20, Dock = DockStyle.Right };
            btn_ToggleTab.MouseClick += (se, ev) => { f_tab_Toggle(); };
            toolbar.Controls.AddRange(new Control[] { brow_UrlTextBox,
                new Label() { Dock = DockStyle.Right, Width = 100 },
                btn_ToggleTab
            });
            this.Controls.AddRange(new Control[] { brow_Transparent, browser, brow_ShortCutBar, toolbar, });
        }

        //private void f_brow_onLoadCompleted(object sender, LoadCompletedEventArgs url)
        //{
        //    string s = string.Format("LOAD_COMPLETED: ===== {0}", url.Url);
        //    Debug.WriteLine(s);
        //    f_brow_onDOMContentLoaded();
        //}

        //private void f_brow_onBrowserConsoleMessage(object sender, ConsoleMessageEventArgs e)
        //{
        //    string s = string.Format("LOG: ===== Line {0}, Source: {1}, Message: {2}", e.Line, e.Source, e.Message);
        //    Debug.WriteLine(s);
        //}

        void f_brow_Go(string url)
        {
            url = url.Trim();

            if ((url.IndexOf(' ') == -1 && url.IndexOf('.') != -1) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                //brow_URL = url;
                //brow_Domain = brow_URL.Split('/')[2];
                browser.Navigate(url);
            }
            else
            {
                f_brow_Go("https://www.google.com.vn/search?q=" + HttpUtility.UrlEncode(url));
                //f_brow_Go("https://www.bing.com/search?q=" + HttpUtility.UrlEncode(url));
            }
        }

        void f_brow_onBeforeBrowse()
        {
            brow_Transparent.crossThreadPerformSafely(() => brow_Transparent.BringToFront());
        }

        void f_brow_onDOMContentLoaded()
        {
            //brow_Transparent.crossThreadPerformSafely(() => brow_Transparent.SendToBack());
            //this.crossThreadPerformSafely(() =>
            //{
            //    this.Text = string.Format("{0} | {1}", browser.Title, brow_URL);
            //});
        }

        #region [ IRequestHandler Members ]

        //bool IRequestHandler.OnBeforeResourceLoad(IWebBrowser browser, IRequestResponse requestResponse)
        //{
        //    return false;

        //    //System.Diagnostics.Debug.WriteLine("OnBeforeResourceLoad");
        //    //var headers = request.GetHeaders();
        //    string url = requestResponse.Request.Url;
        //    if (url.StartsWith("chrome-devtools://") == false)
        //    {
        //        if (brow_ImportPlugin == false && (url.Contains(".js") || url.Contains("/js/")))
        //        {
        //            MemoryStream stream;
        //            byte[] bytes;
        //            switch (brow_Domain)
        //            {
        //                case DOMAIN_GOOGLE:
        //                case DOMAIN_BING:
        //                    stream = new System.IO.MemoryStream();
        //                    bytes = ASCIIEncoding.ASCII.GetBytes(@"document.addEventListener('DOMContentLoaded', function (event) { var a = document.querySelectorAll('img'); for (var i = 0; i < a.length; i++) { a[i].remove(); }; console.log('DOM_CONTENT_LOADED'); }); ");
        //                    stream.Write(bytes, 0, bytes.Length);
        //                    requestResponse.RespondWith(stream, "text/javascript; charset=utf-8");
        //                    break;
        //                default:
        //                    stream = new System.IO.MemoryStream();
        //                    FileStream file = new FileStream(@"plugin.js", FileMode.Open, FileAccess.Read, FileShare.Read);
        //                    bytes = new byte[file.Length];
        //                    file.Read(bytes, 0, (int)file.Length);
        //                    stream.Write(bytes, 0, (int)file.Length);
        //                    file.Close();
        //                    requestResponse.RespondWith(stream, "text/javascript; charset=utf-8");
        //                    break;
        //            }
        //            Debug.WriteLine("----> JS === " + url);
        //            brow_ImportPlugin = true;
        //            return false;
        //        }

        //        if (url.Contains(".js") || url.Contains("/js/")
        //            || url.Contains(brow_Domain) == false
        //            || url.Contains("font") || url.Contains(".svg") || url.Contains(".woff") || url.Contains(".ttf")
        //            || url.Contains("/image") || url.Contains(".png") || url.Contains(".jpeg") || url.Contains(".jpg") || url.Contains(".gif"))
        //        {
        //            Debug.WriteLine("----> " + url);
        //            return true;
        //        }
        //        Debug.WriteLine(url);
        //    }

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

        //    return false;
        //}

        //bool IRequestHandler.OnBeforeBrowse(IWebBrowser browser, IRequest request, NavigationType naigationvType, bool isRedirect)
        //{
        //    string url = request.Url;
        //    if (url == "about:blank"
        //        || url.Contains("youtube.com/embed/")
        //        || url.Contains("facebook.com/plugins/"))
        //        return true;

        //    if (url.StartsWith("chrome-devtools://")) return false;

        //    Debug.WriteLine("GO ====> " + request.Url);

        //    brow_URL = request.Url;
        //    brow_Domain = brow_URL.Split('/')[2];
        //    brow_UrlTextBox.crossThreadPerformSafely(() => brow_UrlTextBox.Text = brow_URL);

        //    brow_ImportPlugin = false;

        //    f_brow_onBeforeBrowse();

        //    return false;
        //}

        //void IRequestHandler.OnResourceResponse(IWebBrowser browser, string url, int status, string statusText, string mimeType, WebHeaderCollection headers)
        //{
        //    //string content_type = headers.Get("Content-Type");
        //    ////if (url.EndsWith(".mp4")) { }
        //    ////System.Diagnostics.Debug.WriteLine("OnResourceResponse");
        //    //Debug.WriteLine(content_type + " === " + url);
        //}

        //public bool GetDownloadHandler(IWebBrowser browser, string mimeType, string fileName, long contentLength, ref IDownloadHandler handler)
        //{
        //    return false;
        //}

        //public bool GetAuthCredentials(IWebBrowser browser, bool isProxy, string host, int port, string realm, string scheme, ref string username, ref string password)
        //{
        //    return false;
        //}

        #endregion

        void f_brow_Close()
        {
            browser.Dispose();
            Xpcom.Shutdown();
        }

        #endregion

        #region [  === TAB === ]

        #region [ TAB: MAIN ]

        const string tab_IconToggle = "☰";
        FATabStrip tab_Main;
        FATabStripItem tab_Note;
        FATabStripItem tab_Link;
        FATabStripItem tab_Setting;

        void f_tab_Init() {
            tab_Main = new FATabStrip() { Dock = DockStyle.Right, Width = 399, AlwaysShowClose = false, AlwaysShowMenuGlyph = false };
            tab_Link = new FATabStripItem() { Title = "Link", CanClose = false };
            tab_Note = new FATabStripItem() { Title = "Note", CanClose = false };
            tab_Setting = new FATabStripItem() { Title = "Setting", CanClose = false };  

            tab_Main.Items.AddRange(new FATabStripItem[] {
                tab_Link,
                tab_Note,
                tab_Setting, 
            });

            this.Controls.AddRange(new Control[] {
                new Splitter() { Dock = DockStyle.Right, MinExtra = 0, MinSize = 0 },
                tab_Main,
            });
             
            f_tab_SettingInit();
            f_tab_LinkInit();
            f_tab_LinkNote();
        }

        void f_tab_Toggle() {
            if (tab_Main.Width != 0) {
                tab_Main.Tag = tab_Main.Width;
                tab_Main.Width = 0;
            } else {
                if (tab_Main.Tag != null)
                    tab_Main.Width = (int)tab_Main.Tag;
                else tab_Main.Width = 399;
            }
        }

        #endregion

        #region [ TAB:LINK ]

        TextBoxWaterMark tab_LinkSearchTextBox;
        System.Windows.Forms.TreeView tab_LinkTreeView;

        void f_tab_LinkInit() {
            Panel barSearch = new Panel() {
                Height = 23,
                Dock = DockStyle.Top,
                //BackColor = Color.Gray,
                Padding = new Padding(9,1,0,0),
            };
            tab_LinkSearchTextBox = new TextBoxWaterMark() {
                WaterMark = "Search Link",
                Dock = DockStyle.Right,
                Height = 19,
                BorderStyle = BorderStyle.None,
                WaterMarkForeColor = Color.Gray,
                WaterMarkActiveForeColor = Color.DarkGray,
            };
            barSearch.Controls.AddRange(new Control[] {
                tab_LinkSearchTextBox
            });

            tab_LinkTreeView = new System.Windows.Forms.TreeView() {
                Dock = DockStyle.Fill,
                Font = font_Title,
                BorderStyle = BorderStyle.None,
            };

            Panel barFooter = new Panel()
            {
                Height = 24,
                Dock = DockStyle.Bottom,
                BackColor = Color.Gray,
            };

            tab_Link.Controls.AddRange(new Control[] {
                tab_LinkTreeView,
                barSearch,
                barFooter,
                new Label() { Dock = DockStyle.Left, Width = 1, BackColor = Color.LightGray }
            });
        }

        #endregion

        #region [ TAB:NOTE ]

        void f_tab_LinkNote() {

        }

        #endregion

        #region [ TAB: SETTING ]

        void f_tab_SettingInit() {

            var btn_Devtool = new Button() { Text = "Dev", Width = 45, Height = 20, Dock = DockStyle.Top };
            btn_Devtool.Click += (se, ev) =>
            {
                //browser.ShowDevTools();
            };

            var btn_EnableJS = new Button() { Text = "JS", Width = 45, Height = 20, Dock = DockStyle.Top, BackColor = Color.OrangeRed };
            btn_EnableJS.MouseClick += (se, ev) => {
                brow_EnabelJS = brow_EnabelJS ? false : true;
                if (brow_EnabelJS)
                    btn_EnableJS.BackColor = Color.OrangeRed;
                else
                    btn_EnableJS.BackColor = SystemColors.Control;
            };
            var btn_EnableCSS = new Button() { Text = "CSS", Width = 45, Height = 20, Dock = DockStyle.Top };
            btn_EnableCSS.MouseClick += (se, ev) => {
                brow_EnableCSS = brow_EnableCSS ? false : true;
                if (brow_EnableCSS)
                    btn_EnableCSS.BackColor = Color.OrangeRed;
                else
                    btn_EnableCSS.BackColor = SystemColors.Control;
            };
            var btn_EnableImg = new Button() { Text = "Image", Width = 45, Height = 20, Dock = DockStyle.Top, BackColor = Color.OrangeRed  };
            btn_EnableImg.MouseClick += (se, ev) => {
                brow_EnableImg = brow_EnableImg ? false : true;
                if (brow_EnableImg)
                    btn_EnableImg.BackColor = Color.OrangeRed;
                else
                    btn_EnableImg.BackColor = SystemColors.Control;
            };
            var btn_EnableAutoCache = new Button() { Text = "AutoRequest", Width = 79, Height = 20, Dock = DockStyle.Top };
            btn_EnableAutoCache.MouseClick += (se, ev) => {
                brow_AutRequest = brow_AutRequest ? false : true;
                if (brow_AutRequest)
                    btn_EnableAutoCache.BackColor = Color.OrangeRed;
                else
                    btn_EnableAutoCache.BackColor = SystemColors.Control;
            };

            tab_Setting.Controls.AddRange(new Control[] {
                btn_Devtool , btn_EnableCSS, btn_EnableJS, btn_EnableImg, btn_EnableAutoCache,
            });
        }

        #endregion
         
        #endregion
    }
     
}
