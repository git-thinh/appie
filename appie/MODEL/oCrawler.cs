using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace appie
{ 
    public class oLinkLen
    {
        public string Url { set; get; }
        public int Len { set; get; }
    }

    [ProtoContract]
    public class oLink
    {
        [ProtoMember(1)]
        public string uri { set; get; }

        [ProtoMember(2)]
        public string text { set; get; }

        [ProtoMember(3)]
        public string url { set; get; }

        [ProtoMember(4)]
        public bool crawled { set; get; }

        public override string ToString()
        {
            return string.Format("{0} = {1}",url , text);
        }
    }

    public class oLinkSetting
    {
        public string Url { set; get; }
        public Dictionary<string, string> Settings { set; get; }

        public oLinkSetting()
        {
            Url = string.Empty;
            Settings = new Dictionary<string, string>();
        }
    }

    public class oLinkExtract
    {
        public string[] Url_Html { set; get; }
        public string[] Url_Image { set; get; }
        public string[] Url_Audio { set; get; }
        public string[] Url_Youtube { set; get; }

        public oLinkExtract()
        {
            Url_Html = new string[] { };
            Url_Image = new string[] { };
            Url_Audio = new string[] { };
            Url_Youtube = new string[] { };
        }
    }
}
