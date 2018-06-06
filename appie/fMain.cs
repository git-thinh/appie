using mshtml;
using SHDocVw;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace appie
{
    public class fMain : Form
    {
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

        private void Browser_ContextMenuStandardEvent(mshtml.IHTMLEventObj e)
        {
            //label_Message.Text = "Context Menu Action (Event Object) Hooked: " + e.type + " = " + e.srcElement.innerHTML;
            switch (e.type)
            {
                case "mouseover":
                    break;
                case "contextmenu":
                    label_Event.Text = e.type;
                    label_ElementSRC.Text = "SRC: " + e.srcElement.innerHTML;
                    label_ElementDES.Text = "DES: " + e.toElement.innerHTML;
                    break;
                case "click":
                    label_Event.Text = e.type; 
                    label_ElementSRC.Text = "SRC: " + e.fromElement.innerHTML;
                    label_ElementDES.Text = "DES: " + e.toElement.innerHTML;
                    break;
            }

            e.returnValue = false;
            //e.returnValue = true;
        }

        public fMain()
        {
            #region [ UI ]

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
            panel_Header.Controls.AddRange(new Control[] {
                label_ElementDES,
                label_ElementSRC,
                label_Event,
            });
            this.Controls.AddRange(new Control[] {
                wbMaster,
                wbSlave,
                panel_Header,
                label_Message,
            });
            
            #endregion

            this.Shown += (s, e) =>
            {
                this.WindowState = FormWindowState.Maximized;

                //wbMaster.Navigated += (se, ev) => { HideScriptErrors(wbMaster, true); };
                //wbSlave.Navigated += (se, ev) => { HideScriptErrors(wbSlave, true); }; 

                var axWbMainV1 = (SHDocVw.WebBrowser_V1)wbMaster.ActiveXInstance;
                var axWbSlaveV1 = (SHDocVw.WebBrowser_V1)wbSlave.ActiveXInstance;

                axWbMainV1.DownloadComplete += () =>
                {
                    //IWebBrowser2 b = (IWebBrowser2)pDisp;
                    HTMLDocument doc2 = axWbMainV1.Document as HTMLDocument;
                    DHTMLEventHandler eventHandler = new DHTMLEventHandler(doc2);
                    eventHandler.Handler += new DHTMLEvent(this.Browser_ContextMenuStandardEvent);
                    //Not triggered 
                    ((mshtml.DispHTMLDocument)doc2).onclick = eventHandler;
                    //Following works fine 
                    ((mshtml.DispHTMLDocument)doc2).oncontextmenu = eventHandler;
                    ((mshtml.DispHTMLDocument)doc2).onmouseover = eventHandler;
                };

                var manualNavigation = false;

                //////// Use WebBrowser_V1 events as BeforeNavigate2 doesn't work with WPF WebBrowser
                //////axWbMainV1.BeforeNavigate += (string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Cancel) =>
                //////{
                //////    if (!manualNavigation)
                //////        return;
                //////    Cancel = true;
                //////    axWbMainV1.Stop();
                //////    axWbSlaveV1.Navigate(URL, Flags, TargetFrameName, PostData, Headers);
                //////};

                //////axWbMainV1.FrameBeforeNavigate += (string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Cancel) =>
                //////{
                //////    if (!manualNavigation)
                //////        return;
                //////    Cancel = true;
                //////    axWbMainV1.Stop();
                //////    axWbSlaveV1.Navigate(URL, Flags, TargetFrameName, PostData, Headers);
                //////};

                //////axWbMainV1.NavigateComplete += (string URL) =>
                //////{
                //////    manualNavigation = true;
                //////};

                //////axWbMainV1.FrameNavigateComplete += (string URL) =>
                //////{
                //////    manualNavigation = true;
                //////};

                //////axWbMainV1.NewWindow += (string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Processed) =>
                //////{
                //////    if (!manualNavigation)
                //////        return;
                //////    Processed = true;
                //////    axWbMainV1.Stop();
                //////    axWbSlaveV1.Navigate(URL, Flags, String.Empty, PostData, Headers);
                //////};

                manualNavigation = false;
                //axWbMainV1.Navigate(url);

                wbMaster.DocumentText = File.ReadAllText("demo.html");
            };
        }


    }

    // From http://west-wind.com/WebLog/posts/393.aspx
    // The delegate:
    public delegate void DHTMLEvent(IHTMLEventObj e);
    ///
    /// Generic Event handler for HTML DOM objects.
    /// Handles a basic event object which receives an IHTMLEventObj which
    /// applies to all document events raised.
    ///
    [ComVisible(true)]
    public class DHTMLEventHandler
    {
        public DHTMLEvent Handler;
        HTMLDocument Document;
        public DHTMLEventHandler(mshtml.HTMLDocument doc)
        {
            this.Document = doc;
        }

        [DispId(0)]
        public void Call()
        {
            Handler(Document.parentWindow.@event);
        }
    }
}
