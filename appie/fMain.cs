using mshtml;
using SHDocVw;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace appie
{
    public class fMain : Form
    {
        const string url = "http://www.w3.org/";
        //const string url = "https://pronuncian.com/podcasts/episode221";
        //const string url = "https://www.youtube.com/watch?v=KN2jyw6D1ak";

        System.Windows.Forms.WebBrowser wbMaster;
        System.Windows.Forms.WebBrowser wbSlave;

        public fMain()
        {
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

            this.Controls.AddRange(new Control[] {
                wbMaster,
                wbSlave,
            });

            this.Shown += (s, e) =>
            {
                this.WindowState = FormWindowState.Maximized;

                //wbMaster.Navigated += (se, ev) => { HideScriptErrors(wbMaster, true); };
                //wbSlave.Navigated += (se, ev) => { HideScriptErrors(wbSlave, true); }; 

                var axWbMainV1 = (SHDocVw.WebBrowser_V1)wbMaster.ActiveXInstance;
                var axWbSlaveV1 = (SHDocVw.WebBrowser_V1)wbSlave.ActiveXInstance;

                axWbMainV1.DownloadComplete += () => {
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

                // Use WebBrowser_V1 events as BeforeNavigate2 doesn't work with WPF WebBrowser
                axWbMainV1.BeforeNavigate += (string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Cancel) =>
                {
                    if (!manualNavigation)
                        return;
                    Cancel = true;
                    axWbMainV1.Stop();
                    axWbSlaveV1.Navigate(URL, Flags, TargetFrameName, PostData, Headers);
                };

                axWbMainV1.FrameBeforeNavigate += (string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Cancel) =>
                {
                    if (!manualNavigation)
                        return;
                    Cancel = true;
                    axWbMainV1.Stop();
                    axWbSlaveV1.Navigate(URL, Flags, TargetFrameName, PostData, Headers);
                };

                axWbMainV1.NavigateComplete += (string URL) =>
                {
                    manualNavigation = true;
                };

                axWbMainV1.FrameNavigateComplete += (string URL) =>
                {
                    manualNavigation = true;
                };

                axWbMainV1.NewWindow += (string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Processed) =>
                {
                    if (!manualNavigation)
                        return;
                    Processed = true;
                    axWbMainV1.Stop();
                    axWbSlaveV1.Navigate(URL, Flags, String.Empty, PostData, Headers);
                };

                manualNavigation = false;
                axWbMainV1.Navigate(url);


                //// *** Always raise the ContextMenuClicked event 
                //// *** Using Custom Event Object  -   No Mouse Lockups 
                //HTMLDocument doc = axWbMainV1.Document as HTMLDocument;
                //DHTMLEventHandler Handler = new DHTMLEventHandler(doc);
                //Handler.Handler += new DHTMLEvent(this.Browser_ContextMenuStandardEvent);
                //doc.oncontextmenu = Handler;

                //// Set up the handler:  
                //// ...find the element I'm interested in... 
                //HTMLDocument doc = axWbMainV1.Document as HTMLDocument;
                //DHTMLEventHandler Handler = new DHTMLEventHandler(doc);
                //Handler.Handler += new DHTMLEvent(this.Browser_ContextMenuStandardEvent);

                //HTMLDocument doc = axWbMainV1.Document as HTMLDocument;
                //DHTMLEventHandler Handler = new DHTMLEventHandler(doc);
                //Handler.Handler += new DHTMLEvent(this.Browser_ContextMenuStandardEvent);
                //doc.oncontextmenu = Handler;

            };
        }
          
        void Explorer_DocumentComplete(object pDisp, ref object URL)
        {
            IWebBrowser2 b = (IWebBrowser2)pDisp;
            HTMLDocument doc2 = b.Document as HTMLDocument;
            DHTMLEventHandler eventHandler = new DHTMLEventHandler(doc2);
            eventHandler.Handler += new DHTMLEvent(this.Browser_ContextMenuStandardEvent);
            //Not triggered 
            ((mshtml.DispHTMLDocument)doc2).onclick = eventHandler;
            //Following works fine 
            ((mshtml.DispHTMLDocument)doc2).oncontextmenu = eventHandler;
            ((mshtml.DispHTMLDocument)doc2).onmouseover = eventHandler;
        }

        private void Browser_ContextMenuStandardEvent(mshtml.IHTMLEventObj e)
        { 
            MessageBox.Show("Context Menu Action (Event Object) Hooked");
            e.returnValue = false;
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
