using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

/* 
 * AutoResxTranslator
 * by Salar Khalilzadeh
 * 
 * https://autoresxtranslator.codeplex.com/
 * Mozilla Public License v2

//IsBusy(true);
GTranslateService.TranslateAsync(
    text, "en", "vi", string.Empty,
    (success, result, type) =>
    {
        //SetResult(result, type);
        //IsBusy(false);
        Console.WriteLine(text + "(" + type + "): " + result);
        Trace.WriteLine(text + "(" + type + "): " + result);
    });

 */
namespace appie
{
    public class GooTranslateService_v2
    {
        private const string RequestUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:55.0) Gecko/20100101 Firefox/55.0";
        private const string RequestGoogleTranslatorUrl = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&hl=en&dt=t&dt=bd&dj=1&source=icon&tk=467103.467103&q={2}";


        public delegate void TranslateCallBack(bool succeed, string result, string type);
        public static void TranslateAsync(
            string text,
            string sourceLng,
            string destLng,
            string textTranslatorUrlKey,
            TranslateCallBack callBack)
        {
            var request = CreateWebRequest(text, sourceLng, destLng, textTranslatorUrlKey);
            request.BeginGetResponse(
                TranslateRequestCallBack,
                new KeyValuePair<WebRequest, TranslateCallBack>(request, callBack));
        }

        public static bool Translate(
            string text,
            string sourceLng,
            string destLng,
            string textTranslatorUrlKey,
            out string result,
            out string type)
        {
            var request = CreateWebRequest(text, sourceLng, destLng, textTranslatorUrlKey);
            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    result = "Response is failed with code: " + response.StatusCode;
                    type = string.Empty;
                    return false;
                }

                using (var stream = response.GetResponseStream())
                {
                    string _output, _type;
                    var succeed = ReadGoogleTranslatedResult(stream, out _output, out _type);
                    result = _output;
                    type = _type;
                    return succeed;
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
                type = string.Empty;
                return false;
            }
        }

        static WebRequest CreateWebRequest(
            string text,
            string lngSourceCode,
            string lngDestinationCode,
            string textTranslatorUrlKey)
        {
            text = HttpUtility.UrlEncode(text);

            var url = string.Format(RequestGoogleTranslatorUrl, lngSourceCode, lngDestinationCode, text);


            var create = (HttpWebRequest)WebRequest.Create(url);
            create.UserAgent = RequestUserAgent;
            create.Timeout = 50 * 1000;
            return create;
        }

        private static void TranslateRequestCallBack(IAsyncResult ar)
        {
            var pair = (KeyValuePair<WebRequest, TranslateCallBack>)ar.AsyncState;
            var request = pair.Key;
            var callback = pair.Value;
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(ar);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    callback(false, "Response is failed with code: " + response.StatusCode, string.Empty);
                    return;
                }

                using (var stream = response.GetResponseStream())
                {
                    string output, type;
                    var succeed = ReadGoogleTranslatedResult(stream, out output, out type);

                    callback(succeed, output, type);
                }
            }
            catch (Exception ex)
            {
                callback(false, "Request failed.\r\n" + ex.Message, string.Empty);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }

        /// <summary>
        ///  the main trick :)
        /// </summary>
        static bool ReadGoogleTranslatedResult(Stream rawdata, out string result, out string type)
        {
            string text;
            using (var reader = new StreamReader(rawdata, Encoding.UTF8))
            {
                text = reader.ReadToEnd();
            }

            try
            {
                result = string.Empty;
                type = string.Empty;

                AutoResxTranslator it = Newtonsoft.Json.JsonConvert.DeserializeObject<AutoResxTranslator>(text);
                if (it.sentences.Length > 0 && it.sentences[0].trans != null)
                    result = it.sentences[0].trans;
                if (it.dict.Length > 0)
                {
                    type = it.dict[0].pos;
                    if (it.dict[0].terms != null && it.dict[0].terms.Length > 0)
                        result += "; " + string.Join("; ", it.dict[0].terms);
                }

                ////dynamic obj = SimpleJson.DeserializeObject(text);

                //JObject obj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(text);

                //var final = "";
                //// the number of lines
                //int lines = obj[0].Count;
                //for (int i = 0; i < lines; i++)
                //{
                //    // the translated text.
                //    final += (obj[0][i][0]).ToString();
                //}
                //result = final;

                //type = string.Empty;
                //if (obj.Count > 1)
                //{
                //    type = (obj[1][0][0]).ToString();
                //    try
                //    {
                //        result = result + "; " + string.Join("; ", ((JObject[])(obj[1][0][1])).Select(x => x.ToString()).ToArray());
                //    }
                //    catch { }
                //}
                return true;
            }
            catch (Exception ex)
            {
                result = ex.Message;
                type = string.Empty;
                return false;
            }
        }

    }

    public class AutoResxTranslator
    {
        public sentences[] sentences { set; get; }
        public dict[] dict { set; get; }
    }

    public class sentences
    {
        public string trans { set; get; }
        public string orig { set; get; }
    }

    public class dict
    {
        public string pos { set; get; }
        public string[] terms { set; get; }
    }

    //	public class MYCLASSNAME 
    //	{
    //		string Sj(string a)
    //		{
    //			return a;
    //		}
    //		void Tj(string a,string b) {
    //			for (FIXME_VAR_TYPE c = 0; c < b.length - 2; c += 3)
    //			{
    //				FIXME_VAR_TYPE d = b.charAt(c + 2), d = "a" <= d ? d.charCodeAt(0) - 87 : Number(d), d = "+" == b.charAt(c + 1) ? a >>> d : a << d; a = "+" == b.charAt(c) ? a + d & 4294967295 : a ^ d
    //			}
    //			return a;
    //		}

    //		void Vj( string a)
    //		{
    //			FIXME_VAR_TYPE b;
    //			if (null !== Uj) b = Uj;
    //			else
    //			{
    //				b = Sj(String.fromCharCode(84));
    //				FIXME_VAR_TYPE c = Sj(String.fromCharCode(75));
    //				b = [b(), b()];
    //				b[1] = c();
    //				b = (Uj = window[b.join(c())] || "") || ""

    //	}
    //			FIXME_VAR_TYPE d = Sj(String.fromCharCode(116)),
    //				c = Sj(String.fromCharCode(107)),
    //				d = [d(), d()];
    //			d[1] = c();
    //			c = "&" + d.join("") +
    //				"=";
    //			d = b.split(".");
    //			b = Number(d[0]) || 0;
    //			for (FIXME_VAR_TYPE e = [], f = 0, g = 0; g < a.Length; g++)
    //			{
    //				FIXME_VAR_TYPE l = a.charCodeAt(g);
    //				128 > l ? e[f++] = l : (2048 > l ? e[f++] = l >> 6 | 192 : (55296 == (l & 64512) && g + 1 < a.length && 56320 == (a.charCodeAt(g + 1) & 64512) ? (l = 65536 + ((l & 1023) << 10) + (a.charCodeAt(++g) & 1023), e[f++] = l >> 18 | 240, e[f++] = l >> 12 & 63 | 128) : e[f++] = l >> 12 | 224, e[f++] = l >> 6 & 63 | 128), e[f++] = l & 63 | 128)
    //    }
    //		a = b;
    //    for (f = 0; f<e.length; f++) a += e[f], a = Tj(a, "+-a^+6");
    //		a = Tj(a, "+-3^+b+-f");
    //		a ^= Number(d[1]) || 0;
    //    0 > a && (a = (a & 2147483647) + 2147483648);
    //    a %= 1E6;
    //    return c + (a.toString() + "." +
    //        (a ^ b))
    //}
    //}
}
