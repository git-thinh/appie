using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace appie
{
    public class htmlToText
    {
        #region Public Methods

        public string Convert(string path)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(path);

            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        public string ConvertHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            string s = sw.ToString(), si = string.Empty; // ♦
            string[] a = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
            StringBuilder bi = new StringBuilder();
            for (int i = 0; i < a.Length; i++)
            {
                bi.Append(Environment.NewLine);

                si = a[i];
                if (si[si.Length - 1] == ':' && si[0] != '■' && si[0] != '▪')
                {
                    bi.Append(Environment.NewLine);
                    bi.Append("□ ");
                    bi.Append(si);
                    bi.Append(Environment.NewLine);
                }
                else
                {
                    if (si[0] == '■') { bi.Append(Environment.NewLine); bi.Append(si); bi.Append(Environment.NewLine); }
                    else if (si[0] == '▪') { bi.Append(Environment.NewLine); bi.Append(si); }
                    else bi.Append(si);
                }
            }
            return bi.ToString();
        }

        public void ConvertTo(HtmlNode node, TextWriter outText)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "li":
                            // treat paragraphs as crlf
                            outText.Write("\r\n▪ " + node.InnerText.Trim());
                            break;
                        case "h2":
                        case "h3":
                        case "h4":
                        case "h5":
                            // treat paragraphs as crlf
                            outText.Write("\r\n■ " + node.InnerText.Trim());
                            break;
                        case "p":
                        case "div":
                        case "blockquote":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            if (node.HasChildNodes)
                                ConvertContentTo(node, outText);
                            break;
                        default:
                            if (node.HasChildNodes)
                                ConvertContentTo(node, outText);
                            break;
                    }

                    break;
            }
        }

        #endregion

        #region Private Methods

        private void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
        }

        #endregion
    }
}
