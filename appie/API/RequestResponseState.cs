using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace appie
{
    class RequestResponseState
    {
        // In production code, you may well want to make these properties,
        // particularly if it's not a private class as it is in this case.
        internal WebRequest request;
        internal WebResponse response;
        internal Stream stream;
        internal byte[] buffer = new byte[16384];
        internal Encoding encoding;
        internal StringBuilder text = new StringBuilder();
    }
}
