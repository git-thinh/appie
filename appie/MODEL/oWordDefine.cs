using System;
using System.Collections.Generic;
using System.Text;

namespace appie
{
    public class oWordDefine
    {
        public string Word { set; get; }
        public string PronunceUK { set; get; }
        public string PronunceUS { set; get; }

        public string MeanEN { set; get; }
        public string MeanVI { set; get; }

        public List<string> Sentences { set; get; }
        public List<string> MP3 { set; get; }

        public oWordDefine(string word) {
            Word = word;
            Sentences = new List<string>();
            MP3 = new List<string>();
        }
    }
}
