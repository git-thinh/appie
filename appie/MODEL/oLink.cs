using System;
using System.Collections.Generic;
using System.Text;

namespace appie
{
    public class oLink
    {
        public string Title { set; get; }
        public string Link { set; get; }
        public string Tags { set; get; }

        public oLink() {
            Title = string.Empty;
            Link = string.Empty;
            Tags = string.Empty;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.Title, this.Link);
        }

        public string TitleDomain()
        {
            string[] a = Link.Split('/');
            return string.Format("{0} | {1}", this.Title, a.Length > 2 ? a[2] : string.Empty);
        }
    }

    public class oLinkLen
    {
        public string Url { set; get; }
        public int Len { set; get; }

        public override string ToString()
        {
            return string.Format("{0}| {1}", this.Len, this.Url);
        }
    }

    public class oLinkTag
    {
        public string Tag { set; get; }
        public int Count { set; get; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.Tag, this.Count);
        }
    }
}
