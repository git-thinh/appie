using mshtml;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace appie
{

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
