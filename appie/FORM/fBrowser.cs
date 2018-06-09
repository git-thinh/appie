﻿using FarsiLibrary.Win;
using mshtml;
using ProtoBuf;
using SHDocVw;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace appie
{
    public class fBrowser : Form
    {
        //const string URL_DEFAULT = "https://pronuncian.com/";
        //const string URL_DEFAULT = "https://pronuncian.com/";
        //const string URL_DEFAULT = "https://pronuncian.com/podcasts/";
        const string URL_DEFAULT = "https://pronuncian.com/podcasts/episode219";

        #region [ VARIABLE ]

        readonly Font font_Title = new Font("Arial", 11f, FontStyle.Regular);
        readonly Font font_TextView = new Font("Courier New", 11f, FontStyle.Regular);
        readonly Font font_LogView = new Font("Courier New", 9f, FontStyle.Regular);

        TextBox m_url_textBox;
        TabControl m_tab;
        System.Windows.Forms.WebBrowser m_browser;
        Panel m_header;
        Panel m_footer;
        TabPage m_tab_Browser;
        Label m_browser_MessageLabel;
        TextBox m_log_Text;

        TextBox m_link_search_textBox;
        ListBox m_link_items_listBox;

        TextBox m_history_search_textBox;
        ListBox m_history_items_listBox;

        HTMLDocument docMain = null;
        WebBrowser_V1 m_browser_ax = null;
        HTMLDocument m_browser_doc = null;

        SynchronizedCacheString dicHtml = new SynchronizedCacheString();

        #endregion

        public fBrowser()
        {
            this.Shown += f_form_Shown;

            #region [ Browser UI ]

            m_log_Text = new TextBox()
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical,
            };
            m_log_Text.MouseDoubleClick += (se, ev) => { m_log_Text.Text = string.Empty; };

            m_url_textBox = new TextBox()
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Height = 17,
                BackColor = Color.WhiteSmoke,
                Text = URL_DEFAULT,
            };

            m_browser = new System.Windows.Forms.WebBrowser()
            {
                Dock = DockStyle.Fill,
                ScriptErrorsSuppressed = true,
                IsWebBrowserContextMenuEnabled = false,
            };
            m_tab = new TabControl()
            {
                Dock = DockStyle.Fill,
            };
            m_tab_Browser = new TabPage()
            {
                Text = "Browser",
            };
            m_header = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 25,
                //BackColor = Color.DeepSkyBlue,
            };
            m_footer = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 17,
                //BackColor = Color.Orange,
            };

            m_browser_MessageLabel = new Label() { Dock = DockStyle.Fill, AutoSize = false, TextAlign = ContentAlignment.BottomLeft };
            Button btn_go = new Button() { Dock = DockStyle.Right, Text = "Go", Width = 69, };
            Button btn_back = new Button() { Dock = DockStyle.Right, Text = "Back", Width = 69, };
            Button btn_next = new Button() { Dock = DockStyle.Right, Text = "Next", Width = 69, };
            Button btn_google = new Button() { Dock = DockStyle.Right, Text = "Google", Width = 69, };
            Button btn_open = new Button() { Dock = DockStyle.Right, Text = "Open", Width = 69, };
            Panel panel_address = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(0, 2, 0, 2), };
            panel_address.Controls.AddRange(new Control[] {
                m_url_textBox,
                new Label() { Dock = DockStyle.Top, Height = 5 },
                btn_go,
                btn_back,
                btn_next,
                btn_google,
                btn_open,
            });

            btn_google.MouseClick += f_browser_google_MouseClick;
            btn_open.MouseClick += (se,ev)=> { f_package_openFile(); };
            #endregion

            #region [ TAB ]

            FATabStrip tab_detail = new FATabStrip() {
                Dock = DockStyle.Right,
                Width = 500,
                AlwaysShowClose = false,
                AlwaysShowMenuGlyph = false,
            };
            FATabStripItem tab_Log = new FATabStripItem("Log", false);
            FATabStripItem tab_Link       = new FATabStripItem("☰", false);
            FATabStripItem tab_Word       = new FATabStripItem("W", false);
            FATabStripItem tab_WordDetail = new FATabStripItem("WD", false);
            FATabStripItem tab_Writer     = new FATabStripItem("✍", false);
            FATabStripItem tab_BookMark   = new FATabStripItem("★", false);
            FATabStripItem tab_Search        = new FATabStripItem("Find", false);
            FATabStripItem tab_History    = new FATabStripItem("History", false);
            FATabStripItem tab_Media      = new FATabStripItem("Media", false);

            tab_Log.Controls.Add(m_log_Text);

            tab_detail.Items.AddRange(new FATabStripItem[] {
                tab_Link,
                tab_Word,
                tab_WordDetail,
                tab_Writer,
                tab_BookMark,
                tab_Search,
                tab_History,
                tab_Media,
                tab_Log ,
            });
            #endregion

            #region [ LINK ]

            m_link_search_textBox = new TextBox() {
                Dock = DockStyle.Top,
                BorderStyle = BorderStyle.FixedSingle,
            };

            m_link_items_listBox = new ListBox() {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Font = font_Title,
            };
            m_link_items_listBox.ValueMember = "Item1";
            m_link_items_listBox.DisplayMember = "Item2";

            tab_Link.Controls.AddRange(new Control[] {
                m_link_items_listBox,
                m_link_search_textBox,
            });

            m_link_items_listBox.SelectedIndexChanged += f_link_items_selectIndexChange;

            #endregion

            #region [ HISTORY ]

            m_history_search_textBox = new TextBox() {
                Dock = DockStyle.Top,
                BorderStyle = BorderStyle.FixedSingle,
            };

            m_history_items_listBox = new ListBox() {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Font = font_Title,
            };
            m_history_items_listBox.ValueMember = "Item1";
            m_history_items_listBox.DisplayMember = "Item2";

            tab_History.Controls.AddRange(new Control[] {
                m_history_items_listBox,
                m_history_search_textBox,
            });

            m_history_items_listBox.MouseDoubleClick += f_history_items_selectIndexChange;

            m_history_items_listBox.Items.AddRange(new Tuple<string, string>[] {
                new Tuple<string,string>("Youtube","https://www.youtube.com/"),
                new Tuple<string,string>("Google","https://www.google.com/search?q=english+pronunciation"),
                new Tuple<string,string>("pronuncian.com","https://pronuncian.com/pronounce-th-sounds/"),
                new Tuple<string,string>("learning-english-online.net","https://www.learning-english-online.net/pronunciation/the-english-th/"),
            });

            #endregion

            #region [ Add Control -> UI ]

            m_header.Controls.AddRange(new Control[] {
                panel_address,
                new Label() {
                    Text = "Address:",
                    Dock = DockStyle.Left,
                    Width = 50,
                    //BackColor = Color.Red,
                    TextAlign = ContentAlignment.MiddleLeft
                },
            });
            m_footer.Controls.AddRange(new Control[] {
                m_browser_MessageLabel,
            });
            m_tab_Browser.Controls.Add(m_browser);
            m_tab.Controls.AddRange(new Control[] {
                m_tab_Browser,
            });
            this.Controls.AddRange(new Control[] {
                m_tab,
                new Splitter() {
                    Dock = DockStyle.Right
                },
                tab_detail,
                m_header,
                m_footer,
            });

            #endregion            
        }

        private void f_history_items_selectIndexChange(object sender, EventArgs e)
        { 
        }

        void f_package_openFile() {

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "package");
            openFileDialog.Filter = "htm files (*.htm)|*.htm|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //brow_offline_mode = true;
                //brow_offline_tools.Visible = true;

                string fi_name = openFileDialog.FileName;
                string name = Path.GetFileName(fi_name);
                this.Text = fi_name;
                //brow_URL_Text.Tag = fi_name;

                dicHtml.ReadFile(fi_name);

                m_link_items_listBox.Items.Clear();

                string[] a = dicHtml.Keys.ToArray();
                string url_min = a.Select(x => new oLinkLen { Url = x, Len = x.Length }).MinBy(x => x.Len).Url;
                if (url_min[url_min.Length - 1] != '/')
                {
                    string[] aa = url_min.Split('/');
                    url_min = string.Join("/", aa.Where((x, k) => k < aa.Length - 1).ToArray()) + "/";
                }
                //brow_offline_url_path = url_min;

                foreach (string it in a)
                {
                    string tit = it.Replace(url_min, string.Empty).Replace('-', ' ');
                    if (tit.Length > 0)
                        tit = tit[0].ToString().ToUpper() + tit.Substring(1);
                    m_link_items_listBox.Items.Add(new Tuple<string, string>(it, tit));
                }
            }
        }

        void f_browser_loadHTML(string title, string htm) {
            if (htm == null) htm = string.Empty;

            m_tab_Browser.Text = title;

            //string htm = File.ReadAllText("demo3.html");
            string page = format_HTML(htm);
            page = File.ReadAllText("browser.html") + page;
            log(page);
            m_browser.DocumentText = page;
            File.WriteAllText("result_.html", page);
        }

        void f_link_items_selectIndexChange(object sender, EventArgs e)
        { 
            var it = m_link_items_listBox.SelectedItem as Tuple<string, string>;
            if (it != null
                && dicHtml.ContainsKey(it.Item1))
            {
                m_url_textBox.Text = it.Item1;
                f_browser_loadHTML(it.Item2, dicHtml.Get(it.Item1));
            }
        }

        private void f_form_Shown(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            //m_tab.Width = 999;

            // Set the WebBrowser to use an instance of the ScriptManager to handle method calls to C#.
            // wbMaster.ObjectForScripting = new ScriptManager(this);

            //var axWbMainV1 = (SHDocVw.WebBrowser_V1)wbMaster.ActiveXInstance;
            //var axWbSlaveV1 = (SHDocVw.WebBrowser_V1)wbSlave.ActiveXInstance;



            m_browser_ax = (SHDocVw.WebBrowser_V1)m_browser.ActiveXInstance;
            //m_browser.DocumentCompleted += (se, ev) =>
            //{
            //    if (m_browser.Document != null)
            //    {
            //        string url = m_browser.Url.ToString();
            //        m_tab_Browser.Text = m_browser.Document.Title;
            //        m_url_textBox.Text = url;
            //        m_browser_MessageLabel.Text = "Page loaded";
            //        log("DONE: " + url);

            //        m_browser_doc = m_browser_ax.Document as HTMLDocument;
            //        //DHTMLEventHandler eventHandler = new DHTMLEventHandler(docMain);
            //        //eventHandler.Handler += new DHTMLEvent(this.f_browser_document_onMouseOver);
            //        //((mshtml.DispHTMLDocument)docMain).onmouseover = eventHandler;
            //    }
            //};

            //// Use WebBrowser_V1 events as BeforeNavigate2 doesn't work with WPF WebBrowser
            //m_browser_ax.BeforeNavigate += (string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Cancel) =>
            //{
            //    //Cancel = true;
            //    //axWbMainV1.Stop();
            //    //axWbSlaveV1.Navigate(URL, Flags, TargetFrameName, PostData, Headers);
            //};

            //m_browser_ax.FrameBeforeNavigate += (string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Cancel) =>
            //{
            //    if (URL == "about:blank") return;

            //    log("FRAME GO: " + TargetFrameName + " = " + URL);
            //    Cancel = true;
            //    m_browser_ax.Stop();
            //    //axWbSlaveV1.Navigate(URL, Flags, TargetFrameName, PostData, Headers);
            //};



            f_browser_google_MouseClick(null, null);
        }
         

        void log(string text, bool clean = false)
        {
            if (clean) m_log_Text.Text = string.Empty;
            m_log_Text.Text += Environment.NewLine + Environment.NewLine + text;
        }

        void f_browser_Navigate(string url = "")
        {
            url = "https://www.google.com.vn/";
            url = "https://www.google.com/search?q=english+pronunciation";
            url = "https://pronuncian.com/";

            log("GO: " + url);
            m_browser.Navigate(url);
        }

        private void f_browser_google_MouseClick(object sender, MouseEventArgs e)
        {
            const string url = "https://pronuncian.com/";
            // const string url = "https://pronuncian.com/podcasts/";
            // const string url = "https://pronuncian.com/podcasts/episode219";

            //string para_url = m_url_textBox.Text.Trim();
            //HttpWebRequest w = (HttpWebRequest)WebRequest.Create(new Uri(para_url));
            //w.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36";
            //w.BeginGetResponse(asyncResult =>
            //{
            //    HttpWebResponse rs = (HttpWebResponse)w.EndGetResponse(asyncResult); //add a break point here 
            //    string url = rs.ResponseUri.ToString();

            //    if (rs.StatusCode == HttpStatusCode.OK)
            //    {
            //        string htm = string.Empty;
            //        using (StreamReader sr = new StreamReader(rs.GetResponseStream(), Encoding.UTF8))
            //            htm = sr.ReadToEnd();
            //        rs.Close();
            //        if (!string.IsNullOrEmpty(htm))
            //        {
            //            string page = htm;
            //            m_browser.crossThreadPerformSafely(() =>
            //            {
            //                m_browser.DocumentText = page;
            //            });
            //        }
            //    }
            //}, w);

            string htm = File.ReadAllText("demo3.html");
            string page = format_HTML(htm);
            page = File.ReadAllText("browser.html") + page;
            log(page);
            m_browser.DocumentText =  page;
            File.WriteAllText("result_.html", page);
        }

        private void f_browser_document_onMouseOver(IHTMLEventObj e)
        {
            log("MOUSE_OVER DOM: " + e.srcElement.tagName + " === " + e.srcElement.outerHTML);
        }

        private static string format_HTML(string s)
        {
            string si = string.Empty;
            s = Regex.Replace(s, @"<script[^>]*>[\s\S]*?</script>", string.Empty);
            //s = Regex.Replace(s, @"<style[^>]*>[\s\S]*?</style>", string.Empty);
            s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            s = Regex.Replace(s, @"(?s)(?<=<!--).+?(?=-->)", string.Empty).Replace("<!---->", string.Empty);

            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|meta|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"</?(?i:base|nav|form|input|fieldset|button|iframe|link|symbol|path|canvas|use|ins|svg|embed|object|frameset|frame|meta)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Remove attribute style="padding:10px;..."
            s = Regex.Replace(s, @"<([^>]*)(\sstyle="".+?""(\s|))(.*?)>", string.Empty);
            s = s.Replace(@">"">", ">");

            string[] lines = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
            s = string.Join(Environment.NewLine, lines);

            int pos = s.ToLower().IndexOf("<body");
            if (pos > 0) {
                s = s.Substring(pos + 5);
                pos = s.IndexOf('>') + 1;
                s = s.Substring(pos, s.Length - pos).Trim();
            }

            s = s
                .Replace(@" data-src=""", @" src=""")
                .Replace(@"src=""//", @"src=""http://");

            var mts = Regex.Matches(s, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
            if (mts.Count > 0) {
                foreach (Match mt in mts)
                {
                    s = s.Replace(mt.ToString(), string.Format("{0}{1}{2}", "<p class=box_img___>", mt.ToString(), "</p>"));
                }
            }

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
    }
}