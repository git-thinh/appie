using mshtml;
using SHDocVw;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using System.Security.Permissions;

namespace appie
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class fEdit : Form
    {
        bool browser_run_debug_mode = false;
        Panel panel_Footer;
        Label label_XPath;

        #region [ VARIABLE ]

        //const string url = "http://www.w3.org/";
        //const string url = "https://pronuncian.com/podcasts/episode221";
        //const string url = "https://www.youtube.com/watch?v=KN2jyw6D1ak";
        //const string url = "http://localhost:59537/?crawler_key=https%3a%2f%2fpronuncian.com%2fpodcasts%2fepisode210";
        const string url = "file:///E:/appie/appie/bin/Debug/demo.html";

        System.Windows.Forms.WebBrowser wbMaster;
        System.Windows.Forms.WebBrowser wbSlave;
        Panel panel_Header;
        Label label_ElementSRC;
        Label label_ElementDES;
        Label label_Event;
        Label label_Message;
        TextBox txt_URL;
        TextBox txt_Log;

        HTMLDocument docMain = null;
        SHDocVw.WebBrowser_V1 axWbMainV1;
        SHDocVw.WebBrowser_V1 axWbSlaveV1;
        bool manualNavigation = false;

        #endregion

        public fEdit()
        {
            #region [ UI ]
            txt_URL = new TextBox()
            {
                Dock = DockStyle.Top
            };
            txt_URL.KeyDown += f_url_textBox_KeyDown;
            txt_Log = new TextBox()
            {
                Dock = DockStyle.Right,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
            };
            txt_Log.MouseDoubleClick += (se, ev) => { txt_Log.Text = string.Empty; };

            wbMaster = new System.Windows.Forms.WebBrowser()
            {
                Dock = DockStyle.Fill,
                ScriptErrorsSuppressed = true,
                AllowWebBrowserDrop = false,
                IsWebBrowserContextMenuEnabled = false,
            };
            wbSlave = new System.Windows.Forms.WebBrowser()
            {
                Height = 200,
                Dock = DockStyle.Bottom,
                ScriptErrorsSuppressed = true,
                AllowWebBrowserDrop = false,
                IsWebBrowserContextMenuEnabled = false,
            };
            panel_Header = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 200,
                BackColor = Color.LightGray,
            };
            label_Message = new Label()
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.LightGray,
            };
            label_Event = new Label()
            {
                Dock = DockStyle.Left,
                Width = 99,
                BackColor = Color.Gray,
                TextAlign = ContentAlignment.TopRight,
                Padding = new Padding(0, 3, 5, 0),
            };
            label_ElementSRC = new Label()
            {
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.TopLeft,
                Height = 100,
                BackColor = Color.Yellow,
                Padding = new Padding(5),
            };
            label_ElementDES = new Label()
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(5),
            };
            label_XPath = new Label() { Dock = DockStyle.Top, AutoSize = false, Text = "XPATH: ...", Height = 55, BackColor = Color.Orange };
            panel_Header.Controls.AddRange(new Control[] {
                label_ElementDES,
                label_ElementSRC,
                label_Event,
                label_XPath,
            });

            panel_Footer = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 24,
                BackColor = Color.DeepSkyBlue,
            };
            Button btn_ele_save = new Button() { Text = "SAVE", Dock = DockStyle.Left, BackColor = Color.LightGray, };
            Button btn_ele_get_xpath_next = new Button() { Text = "Get XPath NEXT >>>", Dock = DockStyle.Left, BackColor = Color.LightGray, Width = 199 };
            Button btn_ele_get_xpath_prev = new Button() { Text = "<<< Get XPath PREV", Dock = DockStyle.Left, BackColor = Color.LightGray, Width = 199, };
            Button btn_ele_set_style = new Button() { Text = "Set Style", Dock = DockStyle.Left, BackColor = Color.LightGray, };
            panel_Footer.Controls.AddRange(new Control[] { 
                btn_ele_save,
                btn_ele_get_xpath_next,
                btn_ele_get_xpath_prev,
                btn_ele_set_style,
            });
            btn_ele_get_xpath_next.MouseClick += Btn_ele_get_xpath_next_MouseClick;
            btn_ele_get_xpath_prev.MouseClick += Btn_ele_get_xpath_prev_MouseClick;
            btn_ele_save.MouseClick += Btn_ele_save_MouseClick;
            btn_ele_set_style.MouseClick += Btn_ele_set_style_MouseClick;

            this.Controls.AddRange(new Control[] {
                wbMaster,
                wbSlave,
                panel_Header,
                label_Message,
                txt_URL,
                new Splitter(){ Dock = DockStyle.Right },
                txt_Log,
                panel_Footer,
            });

            #endregion

            this.Shown += f_form_Shown;
        }

        private void Btn_ele_set_style_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void f_save_XPATH()
        { 
            string s = File.ReadAllText("demo.html");
            string htm = string.Empty, page = string.Empty;

            HtmlAgilityPack.HtmlDocument docAgi = new HtmlAgilityPack.HtmlDocument();
            docAgi.LoadHtml(s);

            //var eleContent = docAgi.DocumentNode.QuerySelector("article div.body.entry-content");
            var eleContent = docAgi.DocumentNode.QuerySelector(XPATH_RUNTIME);
            if (eleContent != null)
            {
                htm = eleContent.OuterHtml;
                //htm = RemoveAttributes(htm);

                htm = Regex.Replace(htm, @"</?(?i:base|ins|svg|iframe)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

                htm = Regex.Replace(htm, @"<([^>]*)(\sstyle="".+?""(\s|))(.*?)>", string.Empty);
                htm = htm.Replace(@">"">", ">");

                List<string> lsClass = new List<string>();
                var mts = Regex.Matches(htm, " class=([\"'])(?:(?=(\\\\?))\\2.)*?\\1");
                if (mts.Count > 0)
                {
                    for (int i = 0; i < mts.Count; i++)
                        lsClass.Add(mts[i].Value.Substring(7).Replace(@"""", string.Empty).Trim());
                    lsClass = lsClass.Distinct().ToList();
                }

                htm = htm.Replace("><", ">" + Environment.NewLine + "<");

                string template = File.ReadAllText("template.html"), css = string.Empty, js = string.Empty;
                page = template
                    .Replace("/*[{CSS}]*/", css)
                    .Replace("<!--[{HTML}]-->", htm)
                    .Replace("/*[{JS}]*/", js)
                    .Replace("<!--[{INPUT_LOAD_COMPLETE}]-->", @"<INPUT TYPE=""hidden"" ID=""_____INPUT_LOAD_COMPLETE"">");
            }

            browser_load_complete = false;
            wbMaster.DocumentText = page;
            File.WriteAllText("result.html", page);
        }

        private void Btn_ele_save_MouseClick(object sender, MouseEventArgs e)
        {
            f_save_XPATH();
        }


        private void f_form_Shown(object sender, EventArgs e)
        {

            this.WindowState = FormWindowState.Maximized;
            txt_Log.Width = this.Width / 2;

            // Ensure that ScriptErrorsSuppressed is set to false.
            wbMaster.ScriptErrorsSuppressed = browser_run_debug_mode ? false : true;

            // Set the WebBrowser to use an instance of the ScriptManager to handle method calls to C#.
            wbMaster.ObjectForScripting = new ScriptManager(this);

            //var axWbMainV1 = (SHDocVw.WebBrowser_V1)wbMaster.ActiveXInstance;
            //var axWbSlaveV1 = (SHDocVw.WebBrowser_V1)wbSlave.ActiveXInstance;

            axWbMainV1 = (SHDocVw.WebBrowser_V1)wbMaster.ActiveXInstance;
            axWbSlaveV1 = (SHDocVw.WebBrowser_V1)wbSlave.ActiveXInstance;



            //axWbMainV1.DownloadComplete += f_axWbMainV1__DownloadComplete;




            ////manualNavigation = false;

            ////// Use WebBrowser_V1 events as BeforeNavigate2 doesn't work with WPF WebBrowser
            ////axWbMainV1.BeforeNavigate += (string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Cancel) =>
            ////{
            ////    if (!manualNavigation)
            ////        return;
            ////    Cancel = true;
            ////    axWbMainV1.Stop();
            ////    axWbSlaveV1.Navigate(URL, Flags, TargetFrameName, PostData, Headers);
            ////};

            ////axWbMainV1.FrameBeforeNavigate += (string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Cancel) =>
            ////{
            ////    if (!manualNavigation)
            ////        return;
            ////    Cancel = true;
            ////    axWbMainV1.Stop();
            ////    axWbSlaveV1.Navigate(URL, Flags, TargetFrameName, PostData, Headers);
            ////};

            ////axWbMainV1.NavigateComplete += (string URL) =>
            ////{
            ////    manualNavigation = true;
            ////    txt_Log.Text += Environment.NewLine + Environment.NewLine + "=====> axWbMainV1.NavigateComplete";
            ////};

            ////axWbMainV1.FrameNavigateComplete += (string URL) =>
            ////{
            ////    manualNavigation = true;
            ////};

            ////axWbMainV1.NewWindow += (string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Processed) =>
            ////{
            ////    if (!manualNavigation)
            ////        return;
            ////    Processed = true;
            ////    axWbMainV1.Stop();
            ////    axWbSlaveV1.Navigate(URL, Flags, String.Empty, PostData, Headers);
            ////};

            ////manualNavigation = false;
            //////axWbMainV1.Navigate(url);
            //wbMaster.Navigate("about:blank");

            f_web_loadHTML();
        }

        private void Browser_ContextMenuStandardEvent(mshtml.IHTMLEventObj e)
        {
            label_Event.Text = string.Empty;
            label_ElementSRC.Text = string.Empty;
            label_ElementDES.Text = string.Empty;
            //label_Message.Text = "Context Menu Action (Event Object) Hooked: " + e.type + " = " + e.srcElement.innerHTML;

            switch (e.type)
            {
                case "mouseover":
                    break;
                case "contextmenu":
                    label_Event.Text = e.type + " = " + e.srcElement.tagName;
                    label_ElementSRC.Text = "SRC: " + e.srcElement.outerHTML;
                    label_ElementDES.Text = "DES: " + e.toElement.outerHTML;
                    break;
                case "click":
                    label_Event.Text = e.type + " = " + e.srcElement.tagName;
                    label_ElementSRC.Text = "SRC: " + e.srcElement.outerHTML;
                    label_ElementDES.Text = "DES: " + e.toElement.outerHTML;
                    break;
            }

            e.returnValue = false;
            //e.returnValue = true;
        }

        void MyToolBar_onmousedown(IHTMLEventObj e)
        {
            label_Event.Text = e.type + " = " + e.srcElement.tagName;
            label_ElementSRC.Text = "SRC: " + e.srcElement.outerHTML;
            label_ElementDES.Text = "DES: " + e.toElement.outerHTML;

            //e.srcElement.style.backgroundColor = "yellow";
        }

        bool browser_load_complete = false;

        private void f_axWbMainV1__DownloadComplete()
        {
            if (docMain == null) docMain = axWbMainV1.Document as HTMLDocument;
            if (browser_load_complete) return;

            IHTMLElement _____INPUT_LOAD_COMPLETE = docMain.getElementById("_____INPUT_LOAD_COMPLETE");
            if (_____INPUT_LOAD_COMPLETE != null)
            {
                txt_Log.Text += Environment.NewLine + Environment.NewLine + "=====> axWbMainV1.DownloadComplete";

                browser_load_complete = true;
                f_browser_main_documentReady();
            }
        }

        void f_browser_main_documentReady()
        {
            txt_Log.Text += Environment.NewLine + Environment.NewLine + Environment.NewLine + " DOCUMENT READY ...";


            //////IWebBrowser2 b = (IWebBrowser2)pDisp;
            ////HTMLDocument doc2 = axWbMainV1.Document as HTMLDocument;
            ////DHTMLEventHandler eventHandler = new DHTMLEventHandler(doc2);
            ////eventHandler.Handler += new DHTMLEvent(this.Browser_ContextMenuStandardEvent);
            //////Not triggered 
            ////((mshtml.DispHTMLDocument)doc2).onclick = eventHandler;
            //////Following works fine 
            ////((mshtml.DispHTMLDocument)doc2).oncontextmenu = eventHandler;
            ////((mshtml.DispHTMLDocument)doc2).onmouseover = eventHandler;

            /////////////////////////////////////////////////////////////////////////////////////

            //HTMLDocument htmlDoc = axWbMainV1.Document as HTMLDocument;
            ////////Explorer is Object of SHDocVw.WebBrowserClass
            ////////HTMLDocument htmlDoc = (HTMLDocument)this.Explorer.IWebBrowser_Document;
            //////HTMLDocument htmlDoc = axWbMainV1.Document as HTMLDocument;
            ////////inject Script
            //////htmlDoc.parentWindow.execScript("alert('hello world !!')", "javascript");
            //////((mshtml.HTMLDocumentEvents2_Event)htmlDoc).onmousedown += new HTMLDocumentEvents2_onmousedownEventHandler(MyToolBar_onmousedown);

            /////////////////////////////////////////////////////////////////////////////////////

            //txt_Log.Text = string.Empty;
            ////////get all the images of document
            //////IHTMLElementCollection imgs = htmlDoc.images;
            //////foreach (HTMLImgClass imgTag in imgs)
            //////{
            //////    //MessageBox.Show(imgTag.src);
            //////    txt_Log.Text += Environment.NewLine + imgTag.src;
            //////}

            ////IHTMLElementCollection frames = (IHTMLElementCollection)htmlDoc.getElementsByTagName("frame"); 
            ////if (frames != null)
            ////{
            ////    foreach (IHTMLElement frm in frames)
            ////    {
            ////        txt_Log.Text += Environment.NewLine + Environment.NewLine + frm.outerHTML;
            ////        ((HTMLFrameElement)frm).contentWindow.execScript("alert('Hello From Frame')", "javascript");
            ////    }
            ////}

            //////IHTMLElementCollection elcol = htmlDoc.getElementsByTagName("iframe");
            //////foreach (IHTMLElement iel in elcol)
            //////{
            //////    txt_Log.Text += Environment.NewLine + Environment.NewLine + iel.outerHTML;
            //////    //HTMLFrameElement frm = (HTMLFrameElement)iel;
            //////    //DispHTMLDocument doc = (DispHTMLDocument)((SHDocVw.IWebBrowser2)frm).Document;
            //////    //DOMEventHandler onmousedownhandler = new DOMEventHandler(doc);
            //////    //onmousedownhandler।Handler += new DOMEvent(Mouse_Down);
            //////    //doc.onmousedown = onmousedownhandler;
            //////}


            ////IHTMLElement element = null;
            //////element = htmlDoc.getElementById("article-56f5dc86ac962c7992bb9267");
            ////IHTMLElementCollection articles = htmlDoc.getElementsByTagName("article");
            ////if (articles.length > 0)
            ////{
            ////    element = articles.item(null, 0) as IHTMLElement;
            ////    if (element != null)
            ////    {
            ////        IHTMLDOMNode nd = (IHTMLDOMNode)element;
            ////        IHTMLAttributeCollection attribs = (IHTMLAttributeCollection)nd.attributes;
            ////        try
            ////        {
            ////            foreach (IHTMLDOMAttribute2 att in attribs)
            ////            {
            ////                if (((IHTMLDOMAttribute)att).specified)
            ////                {
            ////                    txt_Log.Text += Environment.NewLine + Environment.NewLine + att.name + " === " + att.value;
            ////                }
            ////            }
            ////        }
            ////        catch { }
            ////    }
            ////}

            /////////////////////////////////////////////////////////////////////////////////////

            /////////////////////////////////////////////////////////////////////////////////////

        }

        private void f_web_loadHTML()
        {
            string text = File.ReadAllText("demo.html"),
                result = string.Empty;

            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            Regex regx = new Regex(@"((?:.(?!<\s*body[^>]*>))+.<\s*body[^>]*>)|(<\s*/\s*body\s*\>.+)", options);

            Match match = regx.Match(text);

            if (match.Success)
            {
                result = text.Substring(match.Value.Length, text.Length - match.Value.Length);
                result = File.ReadAllText("edit.html").Replace("<!--[{HTML}]-->", result);
                
                wbMaster.DocumentText = result;
            }

            return;


            string s = File.ReadAllText("demo.html");
            string htm = string.Empty, page = string.Empty;

            HtmlAgilityPack.HtmlDocument docAgi = new HtmlAgilityPack.HtmlDocument();
            docAgi.LoadHtml(s);

            var eleContent = docAgi.DocumentNode.QuerySelector("article div.body.entry-content");
            if (eleContent != null)
            {
                htm = eleContent.OuterHtml;
                //htm = RemoveAttributes(htm);

                htm = Regex.Replace(htm, @"</?(?i:base|ins|svg|iframe)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

                htm = Regex.Replace(htm, @"<([^>]*)(\sstyle="".+?""(\s|))(.*?)>", string.Empty);
                htm = htm.Replace(@">"">", ">");

                List<string> lsClass = new List<string>();
                var mts = Regex.Matches(htm, " class=([\"'])(?:(?=(\\\\?))\\2.)*?\\1");
                if (mts.Count > 0)
                {
                    for (int i = 0; i < mts.Count; i++)
                        lsClass.Add(mts[i].Value.Substring(7).Replace(@"""", string.Empty).Trim());
                    lsClass = lsClass.Distinct().ToList();
                }

                htm = htm.Replace("><", ">" + Environment.NewLine + "<");

                string template = File.ReadAllText("template.html"), css = string.Empty, js = string.Empty;
                page = template
                    .Replace("/*[{CSS}]*/", css)
                    .Replace("<!--[{HTML}]-->", htm)
                    .Replace("/*[{JS}]*/", js)
                    .Replace("<!--[{INPUT_LOAD_COMPLETE}]-->", @"<INPUT TYPE=""hidden"" ID=""_____INPUT_LOAD_COMPLETE"">");
            }

            browser_load_complete = false;
            wbMaster.DocumentText = page;
            File.WriteAllText("result.html", page);
        }

        string RemoveAttributes(string value)
        {
            var attributeClean = new System.Text.RegularExpressions.Regex(@"\<[a-z]+\b([^>]+?)\s?\/?\>", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            while (attributeClean.IsMatch(value))
            {
                var match = attributeClean.Match(value);
                value = value.Remove(match.Index, match.Length);
            }
            return value;
        }

        private void f_url_textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                manualNavigation = false;
                axWbMainV1.Navigate(txt_URL.Text.Trim());
            }
        }

        public void f_browser_main_document_Ready()
        {
            txt_Log.Text += Environment.NewLine + "====>>> DOCUMENT READY ...";
            wbMaster.Document.InvokeScript("test_js", new String[] { "called from client code" });

            docMain = axWbMainV1.Document as HTMLDocument;
            //((mshtml.HTMLDocumentEvents2_Event)docMain).onmouseup += new HTMLDocumentEvents2_onmouseupEventHandler(f_event_browser_main_onMouseUp);

            ////HTMLDocument htmlDoc = axWbMainV1.Document as HTMLDocument;
            ////Explorer is Object of SHDocVw.WebBrowserClass
            ////HTMLDocument htmlDoc = (HTMLDocument)this.Explorer.IWebBrowser_Document;
            ////HTMLDocument htmlDoc = axWbMainV1.Document as HTMLDocument;

            ////inject Script
            ////docMain.parentWindow.execScript("alert('hello world !!')", "javascript");
            ////((mshtml.HTMLDocumentEvents2_Event)docMain).onmousedown += new HTMLDocumentEvents2_onmousedownEventHandler(MyToolBar_onmousedown);
            //((mshtml.HTMLDocumentEvents2_Event)docMain).onmouseup += new HTMLDocumentEvents2_onmouseupEventHandler(MyToolBar_onmousedown); 


            //IWebBrowser2 b = (IWebBrowser2)pDisp;
            //HTMLDocument doc2 = axWbMainV1.Document as HTMLDocument;
            DHTMLEventHandler eventHandler = new DHTMLEventHandler(docMain);
            eventHandler.Handler += new DHTMLEvent(this.f_event_browser_main_onMouseOver);
            //////Not triggered 
            ////((mshtml.DispHTMLDocument)docMain).onclick = eventHandler;
            //////Following works fine 
            ////((mshtml.DispHTMLDocument)docMain).oncontextmenu = eventHandler;
            ((mshtml.DispHTMLDocument)docMain).onmouseover = eventHandler;
        }

        public string browser_main_ele_current_id = string.Empty;
        public string browser_main_ele_current_tagName = string.Empty;
        private void f_event_browser_main_onMouseOver(IHTMLEventObj e)
        {
            browser_main_ele_current_tagName = e.srcElement.tagName;
            browser_main_ele_current_id = Guid.NewGuid().ToString();
            e.srcElement.setAttribute("xpath_id", browser_main_ele_current_id);

            label_Event.Text = e.type + " = " + e.srcElement.tagName;
            label_ElementSRC.Text = "SRC: " + e.srcElement.outerHTML;
            label_ElementDES.Text = "DES: " + e.toElement.outerHTML;

            this.Text = browser_main_ele_current_id;
        }
        
        public void f_log_Write(string text)
        {
            txt_Log.Text += Environment.NewLine + text;
        }

        public void f_speech(string text)
        {
            txt_Log.Text += Environment.NewLine + "API SPEECH: " + text;
        }

        public void f_browser_xpath_update(string xpath) {
            XPATH_SELECT_CURRENT = xpath;
            label_XPath.Text = xpath;

            //int len = 2;
            //string html = File.ReadAllText("demo.html");


            //HtmlAgilityPack.HtmlDocument docAgi = new HtmlAgilityPack.HtmlDocument();
            //docAgi.LoadHtml(html);

            //HtmlAgilityPack.HtmlNode node = getNodeByXpath(docAgi, xpath, len);

            //if (node != null)
            //{
            //    this.Text = XPATH_RUNTIME;
            //    string src = node.OuterHtml;
            //    //txt_Log.Text = src;
            //    wbSlave.DocumentText = src;
            //}
            //else {
            //    this.Text = "CANNOT FIND: " + XPATH_RUNTIME;
            //}
        }

        string XPATH_SELECT_CURRENT = string.Empty;
        string XPATH_RUNTIME = string.Empty;
        int XPATH_RUNTIME_LEN = 0;
        HtmlAgilityPack.HtmlNode getNodeByXpath(HtmlAgilityPack.HtmlDocument docAgi, string xpath, int len)
        {
            //txt_Log.Text = string.Empty;

            try
            {
                string[] a = xpath.Split('/');

                if (a.Length <= len) return null;

                XPATH_RUNTIME = string.Join("/", a.Where((x, k) => k < a.Length - len).ToArray());
                HtmlAgilityPack.HtmlNode node = docAgi.DocumentNode.SelectNodes(XPATH_RUNTIME).First();
                XPATH_RUNTIME_LEN = len;

                return node;
            }
            catch {
                len++;
                return getNodeByXpath(docAgi, xpath, len);
            }

            return null;
        }

        private void Btn_ele_get_xpath_next_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void Btn_ele_get_xpath_prev_MouseClick(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(XPATH_SELECT_CURRENT)) return;

            wbMaster.DocumentText = string.Empty;
            txt_Log.Text = string.Empty;

            
            string html = File.ReadAllText("demo.html");

            string[] a = XPATH_SELECT_CURRENT.Split('/').Where(x => x.Contains(".") == true).ToArray();
            if (XPATH_RUNTIME_LEN <= 0)
                XPATH_RUNTIME_LEN = a.Length - 1;
            else
                XPATH_RUNTIME_LEN = XPATH_RUNTIME_LEN - 1;

            if (XPATH_RUNTIME_LEN <= 0) return;

            XPATH_RUNTIME = a[XPATH_RUNTIME_LEN];

            HtmlAgilityPack.HtmlDocument docAgi = new HtmlAgilityPack.HtmlDocument();
            docAgi.LoadHtml(html);

            label_Message.Text = XPATH_RUNTIME;
            try
            {
                HtmlAgilityPack.HtmlNode node = docAgi.DocumentNode.QuerySelector(XPATH_RUNTIME);
                if (node != null)
                {
                    string src = node.OuterHtml;
                    //txt_Log.Text = XPATH_RUNTIME;
                    txt_Log.Text += Environment.NewLine + Environment.NewLine + XPATH_SELECT_CURRENT;
                    txt_Log.Text += Environment.NewLine + Environment.NewLine + src;

                    string htm = src;
                    htm = Regex.Replace(htm, @"</?(?i:base|ins|svg|iframe)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase); 
                    htm = Regex.Replace(htm, @"<([^>]*)(\sstyle="".+?""(\s|))(.*?)>", string.Empty);
                    htm = htm.Replace(@">"">", ">");

                    wbMaster.DocumentText = @"<style type=""text/css""> body { padding: 20px 10px; margin: 0; font-size: 1.5em; } p { line-height: 1.7em; } a { text-decoration: none; } </style>" + htm;
                }
                else
                {
                    txt_Log.Text += Environment.NewLine + Environment.NewLine + "CANNOT FIND: " + XPATH_RUNTIME;
                    this.Text = "CANNOT FIND: " + XPATH_RUNTIME;
                }
            }
            catch { }

        }


        #region [ JAVASCRIPT ]

        // This nested class must be ComVisible for the JavaScript to be able to call it.
        [ComVisible(true)]
        public class ScriptManager
        {
            // Variable to store the form of type Form1.
            private fEdit mForm;

            // Constructor.
            public ScriptManager(fEdit form)
            {
                // Save the form so it can be referenced later.
                mForm = form;
            }

            public string get_ele_current_id()
            {
                return string.Format(@"{0}[xpath_id=""{1}""]", mForm.browser_main_ele_current_tagName, mForm.browser_main_ele_current_id);
            }

            public void log(string message)
            {
                mForm.f_log_Write(message);
            }

            public void update_XPath(string xpath)
            {
                mForm.f_browser_xpath_update(xpath);
            }

            public void speech(string message)
            {
                mForm.f_speech(message);
            }

            public void document_Ready()
            {
                mForm.f_browser_main_document_Ready();
            }

            // This method can be called from JavaScript.
            public void MethodToCallFromScript()
            {
                // Call a method on the form.
                mForm.DoSomething();
            }

            // This method can also be called from JavaScript.
            public void AnotherMethod(string message)
            {
                MessageBox.Show(message);
            }
        }

        // This method will be called by the other method (MethodToCallFromScript) that gets called by JavaScript.
        public void DoSomething()
        {
            // Indicate success.
            txt_Log.Text += Environment.NewLine + "It worked!";
        }

        #endregion
    }

}
