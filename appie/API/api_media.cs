using Newtonsoft.Json.Linq;
using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using YoutubeExplode.Internal;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using YoutubeExplode.Models.ClosedCaptions;

namespace appel
{
    public class api_media : api_base, IAPI
    {
        #region [ VARIABLE ]

        static readonly string path_data = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

        public bool Open { set; get; } = false;

        public bool SearchMediaCaptionCC { set; get; } = false;

        public bool StoreMediaCaptionCC_Filter { set; get; } = false;

        #endregion

        #region [ API ]

        public void Init()
        {



            if (!Directory.Exists(path_data)) Directory.CreateDirectory(path_data);

            wait_search = new AutoResetEvent(false);
            dicMediaStore = new ConcurrentDictionary<long, oMedia>();
            dicMediaImage = new ConcurrentDictionary<long, Bitmap>();
            dicMediaSearch = new ConcurrentDictionary<long, oMedia>();

            if (File.Exists(file_media))
                using (var file = File.OpenRead(file_media))
                {
                    dicMediaStore = Serializer.Deserialize<ConcurrentDictionary<long, oMedia>>(file);
                    file.Close();
                }

            f_proxy_Start();

            f_word_Init();
        }

        public msg Execute(msg m)
        {
            if (m != null && Open)
            {
                if (m.Output == null) m.Output = new msgOutput();
                switch (m.KEY)
                {
                    case _API.MEDIA_KEY_INITED:
                        #region  
                        break;
                    #endregion

                    case _API.MEDIA_KEY_WORD_TRANSLATER:
                        f_word_MEDIA_KEY_WORD_TRANSLATER(m);
                        break;
                    case _API.MEDIA_KEY_PLAY_AUDIO:
                        #region
                        if (true)
                        {
                            long mediaId = (long)m.Input;
                            string urlSrc = f_media_fetchUriSource(mediaId, MEDIA_TYPE.M4A);
                            if (!string.IsNullOrEmpty(urlSrc))
                            {
                                m.Output.Ok = true;
                                m.Output.Data = f_proxy_getUriProxy(mediaId, MEDIA_TYPE.M4A);
                                response_toMain(m);
                            }
                            else
                            {
                                // cannot fetch uri
                            }
                        }
                        break;
                    #endregion
                    case _API.MEDIA_KEY_PLAY_VIDEO:
                        #region
                        if (true)
                        {
                            long mediaId = (long)m.Input;
                            string urlSrc = f_media_fetchUriSource(mediaId, MEDIA_TYPE.MP4);
                            if (!string.IsNullOrEmpty(urlSrc))
                            {
                                m.Output.Ok = true;
                                m.Output.Data = f_proxy_getUriProxy(mediaId, MEDIA_TYPE.MP4);
                                response_toMain(m);
                            }
                            else
                            {
                                // cannot fetch uri
                            }
                        }
                        break;
                    #endregion
                    case _API.MEDIA_KEY_TEXT_INFO:
                        #region
                        if (true)
                        {
                            long mediaId = (long)m.Input;
                            oMedia mi = null;
                            MEDIA_TAB tab = MEDIA_TAB.TAB_STORE;
                            if (m.Log != null) tab = (MEDIA_TAB)(int.Parse(m.Log));

                            if (tab == MEDIA_TAB.TAB_STORE)
                                dicMediaStore.TryGetValue(mediaId, out mi);
                            else if (tab == MEDIA_TAB.TAB_SEARCH)
                                dicMediaSearch.TryGetValue(mediaId, out mi);

                            if (mi != null)
                            {
                                m.Output.Ok = true;
                                m.Output.Data = mi.Text;
                                response_toMain(m);
                            }
                        }
                        break;
                    #endregion
                    case _API.MEDIA_KEY_WORD_LIST:
                        #region
                        if (true)
                        {
                            long mediaId = (long)m.Input;
                            oMedia mi = null;
                            MEDIA_TAB tab = MEDIA_TAB.TAB_STORE;
                            if (m.Log != null) tab = (MEDIA_TAB)(int.Parse(m.Log));

                            if (tab == MEDIA_TAB.TAB_STORE)
                                dicMediaStore.TryGetValue(mediaId, out mi);
                            else if (tab == MEDIA_TAB.TAB_SEARCH)
                                dicMediaSearch.TryGetValue(mediaId, out mi);

                            if (mi != null)
                            {
                                m.Output.Ok = true;
                                m.Output.Data = mi.Words;
                                response_toMain(m);
                            }
                        }
                        break;
                    #endregion
                    case _API.MEDIA_KEY_WORD_CAPTION_CC_DOWNLOAD_ANALYTIC:
                        #region

                        if (m.Input != null && m.Input is long)
                        {
                            long mediaId = (long)m.Input;
                            oMedia mi = f_media_getInfo(mediaId);
                            if (mi != null
                                && mi.Paths != null
                                && !string.IsNullOrEmpty(mi.Paths.YoutubeID))
                            {
                                string videoId = string.Empty;
                                videoId = mi.Paths.YoutubeID;

                                YoutubeClient _client = new YoutubeClient();
                                var cap = _client.GetVideoClosedCaptionTrackInfosAsync(videoId);
                                if (cap.Count > 0)
                                {
                                    var cen = cap.Where(x => x.Language.Code == "en").Take(1).SingleOrDefault();
                                    if (cen != null)
                                    {
                                        mi.SubtileEnglish = _client.GetStringAsync(cen.Url);

                                        f_media_writeFile();

                                        m.KEY = _API.MEDIA_KEY_WORD_LIST;
                                        m.Log = ((int)MEDIA_TAB.TAB_STORE).ToString();
                                        Execute(m);
                                    }
                                }
                            }
                        }

                        break;
                    #endregion
                    case _API.MEDIA_KEY_SEARCH_STORE:
                        #region 
                        if (true)
                        {
                            string input = (string)m.Input;
                            if (input == null) input = string.Empty;
                            StoreMediaCaptionCC_Filter = m.Log == "CC" ? true : false;

                            oMediaSearchLocalResult resultSearch = new oMediaSearchLocalResult();

                            List<long> lsSearch = new List<long>();
                            int min = (m.PageNumber - 1) * m.PageSize,
                                max = m.PageNumber * m.PageSize,
                                count = 0;
                            foreach (var kv in dicMediaStore)
                            {
                                if (kv.Value.Title.ToLower().Contains(input)
                                    || kv.Value.Description.ToLower().Contains(input))
                                {
                                    if (StoreMediaCaptionCC_Filter)
                                    {
                                        // only find media have caption CC
                                        if (string.IsNullOrEmpty(kv.Value.SubtileEnglish) == true)
                                            continue;
                                    }
                                    else
                                    {
                                        // only find media do not have caption CC
                                        if (string.IsNullOrEmpty(kv.Value.SubtileEnglish) == false)
                                            continue;
                                    }

                                    if (count >= min && count < max)
                                        lsSearch.Add(kv.Key);
                                    count++;
                                }
                            }
                            resultSearch.TotalItem = dicMediaStore.Count;
                            resultSearch.PageSize = m.PageSize;
                            resultSearch.PageNumber = m.PageNumber;
                            resultSearch.CountResult = count;
                            resultSearch.MediaIds = lsSearch;

                            m.Counter = count;
                            m.Output.Ok = true;
                            m.Output.Data = resultSearch;
                            response_toMain(m);
                        }
                        break;
                    #endregion
                    case _API.MEDIA_KEY_DOWNLOAD_PHOTO:
                        break;

                    case _API.MEDIA_KEY_FILTER_BOOKMAR_STAR:
                        #region

                        if (true)
                        {
                            //string input = (string)m.Input;
                            //if (input == null) input = string.Empty;
                            oMediaSearchLocalResult resultSearch = new oMediaSearchLocalResult();

                            List<long> lsSearch = new List<long>();
                            int min = (m.PageNumber - 1) * m.PageSize,
                                max = m.PageNumber * m.PageSize,
                                count = 0;
                            foreach (var kv in dicMediaStore)
                            {
                                if (kv.Value.Star)
                                {
                                    if (count >= min && count < max)
                                        lsSearch.Add(kv.Key);
                                    count++;
                                }
                            }
                            resultSearch.TotalItem = dicMediaStore.Count;
                            resultSearch.PageSize = m.PageSize;
                            resultSearch.PageNumber = m.PageNumber;
                            resultSearch.CountResult = count;
                            resultSearch.MediaIds = lsSearch;

                            m.Counter = count;
                            m.Output.Ok = true;
                            m.Output.Data = resultSearch;
                            response_toMain(m);
                        }

                        break;
                    #endregion
                    case _API.MEDIA_KEY_UPDATE_BOOKMARK_STAR:
                        #region

                        if (m.Input != null)
                        {
                            long mediaId = (long)m.Input;
                            oMedia mi = null;
                            if (dicMediaStore.TryGetValue(mediaId, out mi) && mi != null)
                            {
                                if (mi.Star)
                                    mi.Star = false;
                                else
                                    mi.Star = true;
                                f_media_writeFile();

                                m.Log = string.Format("Update bookmark set be {0} for {1}: {2}", mi.Star, mi.Title, "SUCCESSFULLY");
                                notification_toMain(m);
                            }
                        }

                        break;
                    #endregion
                    case _API.MEDIA_KEY_UPDATE_LENGTH:
                        break;
                    case _API.MEDIA_KEY_UPDATE_INFO:
                        break;
                    case _API.MEDIA_KEY_UPDATE_CAPTION:
                        break;
                    //case _API.MEDIA_YOUTUBE_INFO:
                    #region

                    //string videoId = "RQPSzkMNwcw";
                    //var _client = new YoutubeClient();
                    //// Get data
                    //var Video = _client.GetVideoAsync(videoId);
                    //var Channel = _client.GetVideoAuthorChannelAsync(videoId);
                    //var MediaStreamInfos = _client.GetVideoMediaStreamInfosAsync(videoId);
                    //var ClosedCaptionTrackInfos = _client.GetVideoClosedCaptionTrackInfosAsync(videoId);
                    //List<Video> video_result = _client.SearchVideosAsync("learn english subtitle");

                    ////////videoId = (string)msg.Input;
                    ////////url = string.Format("https://www.youtube.com/get_video_info?video_id={0}&el=embedded&sts=&hl=en", videoId);
                    ////////w = (HttpWebRequest)WebRequest.Create(new Uri(url));
                    //////////w.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36";
                    ////////w.BeginGetResponse(asyncResult =>
                    ////////{
                    ////////    HttpWebResponse rs = (HttpWebResponse)w.EndGetResponse(asyncResult); //add a break point here 
                    ////////    StreamReader sr = new StreamReader(rs.GetResponseStream(), Encoding.UTF8);
                    ////////    string query = sr.ReadToEnd();
                    ////////    sr.Close();
                    ////////    rs.Close();

                    ////////    oVideo video = null;
                    ////////    List<ClosedCaptionTrackInfo> listCaptionTrackInfo = new List<ClosedCaptionTrackInfo>();
                    ////////    #region [ VIDEO INFO - CAPTION - SUBTITLE ] 

                    ////////    if (!string.IsNullOrEmpty(query))
                    ////////    {
                    ////////        //query = HttpUtility.HtmlDecode(query);

                    ////////        //////////////////////////////////////////////////////
                    ////////        // GET VIDEO INFO

                    ////////        var videoInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    ////////        var rawParams = query.Split('&');
                    ////////        foreach (var rawParam in rawParams)
                    ////////        {
                    ////////            var param = HttpUtility.UrlDecode(rawParam);

                    ////////            // Look for the equals sign
                    ////////            var equalsPos = param.IndexOf('=');
                    ////////            if (equalsPos <= 0)
                    ////////                continue;

                    ////////            // Get the key and value
                    ////////            var key = param.Substring(0, equalsPos);
                    ////////            var value = equalsPos < param.Length
                    ////////                ? param.Substring(equalsPos + 1)
                    ////////                : string.Empty;

                    ////////            // Add to dictionary
                    ////////            videoInfo[key] = value;
                    ////////        }

                    ////////        // Extract values
                    ////////        var title = videoInfo["title"];
                    ////////        var author = videoInfo["author"];
                    ////////        double length_seconds = 0;
                    ////////        double.TryParse(videoInfo["length_seconds"], out length_seconds);
                    ////////        TimeSpan duration = TimeSpan.FromSeconds(length_seconds);
                    ////////        long viewCount = 0;
                    ////////        long.TryParse(videoInfo["view_count"], out viewCount);
                    ////////        var keywords = videoInfo["keywords"].Split(',');

                    ////////        //////////////////////////////////////////////////////
                    ////////        // CAPTION - SUBTITLE

                    ////////        // Extract captions metadata
                    ////////        var playerResponseRaw = videoInfo["player_response"];
                    ////////        var playerResponseJson = JToken.Parse(playerResponseRaw);
                    ////////        var captionTracksJson = playerResponseJson.SelectToken("$..captionTracks").EmptyIfNull();

                    ////////        // Parse closed caption tracks 
                    ////////        foreach (var captionTrackJson in captionTracksJson)
                    ////////        {
                    ////////            // Extract values
                    ////////            var code = captionTrackJson["languageCode"].Value<string>();
                    ////////            var name = captionTrackJson["name"]["simpleText"].Value<string>();
                    ////////            var language = new Language(code, name);
                    ////////            var isAuto = captionTrackJson["vssId"].Value<string>()
                    ////////                .StartsWith("a.", StringComparison.OrdinalIgnoreCase);
                    ////////            var url_caption = captionTrackJson["baseUrl"].Value<string>();

                    ////////            // Enforce format
                    ////////            url_caption = SetQueryParameter(url_caption, "format", "3");

                    ////////            var closedCaptionTrackInfo = new ClosedCaptionTrackInfo(url_caption, language, isAuto);
                    ////////            listCaptionTrackInfo.Add(closedCaptionTrackInfo);
                    ////////        }

                    ////////        ///////////////////////////////////////////////////////////////
                    ////////        // GET VIDEO WATCH PAGE
                    ////////        using (WebClient webWatchPage = new WebClient())
                    ////////        {
                    ////////            webWatchPage.Encoding = Encoding.UTF8;
                    ////////            s = webWatchPage.DownloadString(string.Format("https://www.youtube.com/watch?v={0}&disable_polymer=true&hl=en", videoId));

                    ////////            s = Regex.Replace(s, @"<script[^>]*>[\s\S]*?</script>", string.Empty);
                    ////////            s = Regex.Replace(s, @"<style[^>]*>[\s\S]*?</style>", string.Empty);
                    ////////            s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
                    ////////            s = Regex.Replace(s, @"(?s)(?<=<!--).+?(?=-->)", string.Empty).Replace("<!---->", string.Empty);

                    ////////            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
                    ////////            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
                    ////////            //s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|meta|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    ////////            s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

                    ////////            // Load the document using HTMLAgilityPack as normal
                    ////////            doc = new HtmlDocument();
                    ////////            doc.LoadHtml(s);

                    ////////            // Fizzler for HtmlAgilityPack is implemented as the
                    ////////            // QuerySelectorAll extension method on HtmlNode
                    ////////            var watchPage = doc.DocumentNode;

                    ////////            // Extract values 
                    ////////            var uploadDate = watchPage.QuerySelector("meta[itemprop=\"datePublished\"]")
                    ////////                .GetAttributeValue("content", "1900-01-01")
                    ////////                .ParseDateTimeOffset("yyyy-MM-dd");
                    ////////            var likeCount = watchPage.QuerySelector("button.like-button-renderer-like-button").InnerText
                    ////////                .StripNonDigit().ParseLongOrDefault();
                    ////////            var dislikeCount = watchPage.QuerySelector("button.like-button-renderer-dislike-button").InnerText
                    ////////                .StripNonDigit().ParseLongOrDefault();
                    ////////            var description = watchPage.QuerySelector("p#eow-description").TextEx();

                    ////////            var statistics = new Statistics(viewCount, likeCount, dislikeCount);
                    ////////            var thumbnails = new ThumbnailSet(videoId);

                    ////////            video = new oVideo(videoId, author, uploadDate, title, description, thumbnails, duration, keywords, statistics);
                    ////////        }
                    ////////    }

                    ////////    #endregion                            
                    ////////    string vinfo = $"Id: {video.Id} | Title: {video.Title} | Author: {video.Author}";

                    ////////    //////////////////////////////////////////////////////////////////////////////////////////////

                    ////////    Channel channel = null;
                    ////////    PlayerContext playerContext = null;
                    ////////    #region [ MEDIA STREAM INFO SET - CHANNEL INFO ]

                    ////////    using (WebClient requestGetVideoEmbedPage = new WebClient())
                    ////////    {
                    ////////        requestGetVideoEmbedPage.Encoding = Encoding.UTF8;
                    ////////        string rawGetVideoEmbedPage = requestGetVideoEmbedPage.DownloadString(string.Format("https://www.youtube.com/embed/{0}?disable_polymer=true&hl=en", videoId));

                    ////////        //////s = Regex.Replace(s, @"<script[^>]*>[\s\S]*?</script>", string.Empty);
                    ////////        //////s = Regex.Replace(s, @"<style[^>]*>[\s\S]*?</style>", string.Empty);
                    ////////        //////s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
                    ////////        //////s = Regex.Replace(s, @"(?s)(?<=<!--).+?(?=-->)", string.Empty).Replace("<!---->", string.Empty);
                    ////////        ////////s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|meta|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    ////////        //////s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    ////////        //////// Load the document using HTMLAgilityPack as normal
                    ////////        //////doc = new HtmlDocument();
                    ////////        //////doc.LoadHtml(s);
                    ////////        // Fizzler for HtmlAgilityPack is implemented as the QuerySelectorAll extension method on HtmlNode
                    ////////        // var watchPage = doc.DocumentNode;

                    ////////        // Get embed page config
                    ////////        var part = rawGetVideoEmbedPage.SubstringAfter("yt.setConfig({'PLAYER_CONFIG': ").SubstringUntil(",'");
                    ////////        JToken configJson = JToken.Parse(part);

                    ////////        // Extract values
                    ////////        var sourceUrl = configJson["assets"]["js"].Value<string>();
                    ////////        var sts_value = configJson["sts"].Value<string>();

                    ////////        // Extract values
                    ////////        var channelPath = configJson["args"]["channel_path"].Value<string>();
                    ////////        var id = channelPath.SubstringAfter("channel/");
                    ////////        var title = configJson["args"]["expanded_title"].Value<string>();
                    ////////        var logoUrl = configJson["args"]["profile_picture"].Value<string>();

                    ////////        channel = new Channel(id, title, logoUrl);

                    ////////        // Check if successful
                    ////////        if (sourceUrl.IsBlank() || sts_value.IsBlank())
                    ////////            throw new Exception("Could not parse player context.");

                    ////////        // Append host to source url
                    ////////        sourceUrl = "https://www.youtube.com" + sourceUrl;
                    ////////        playerContext = new PlayerContext(sourceUrl, sts_value);
                    ////////    }

                    ////////    #endregion

                    ////////    //////////////////////////////////////////////////////////////////////////////////////////////

                    ////////    string sts = playerContext.Sts;
                    ////////    MediaStreamInfoSet streamInfoSet = null;
                    ////////    #region [ STREAM VIDEO INFO FROM EMBED/DETAIL PAGE ]

                    ////////    Dictionary<string, string> videoInfo_EmbeddedOrDetailPage = new Dictionary<string, string>();
                    ////////    using (WebClient requestGetVideoInfo = new WebClient())
                    ////////    {
                    ////////        requestGetVideoInfo.Encoding = Encoding.UTF8;
                    ////////        string rawGetVideoInfo = requestGetVideoInfo.DownloadString(
                    ////////            string.Format("https://www.youtube.com/get_video_info?video_id={0}&el={1}&sts={2}&hl=en", videoId, "embedded", sts));

                    ////////        // Get video info
                    ////////        videoInfo_EmbeddedOrDetailPage = SplitQuery(rawGetVideoInfo);

                    ////////        // If can't be embedded - try another value of el
                    ////////        if (videoInfo_EmbeddedOrDetailPage.ContainsKey("errorcode"))
                    ////////        {
                    ////////            var errorReason = videoInfo_EmbeddedOrDetailPage["reason"];
                    ////////            if (errorReason.Contains("&feature=player_embedded"))
                    ////////            {
                    ////////                string rawGetVideoInfo_DetailPage = string.Empty;
                    ////////                using (WebClient requestGetVideoInfo_DetailPage = new WebClient())
                    ////////                {
                    ////////                    requestGetVideoInfo_DetailPage.Encoding = Encoding.UTF8;
                    ////////                    rawGetVideoInfo_DetailPage = requestGetVideoInfo_DetailPage.DownloadString(
                    ////////                        string.Format("https://www.youtube.com/get_video_info?video_id={0}&el={1}&sts={2}&hl=en", videoId, "detailpage", sts));
                    ////////                }
                    ////////                videoInfo_EmbeddedOrDetailPage = SplitQuery(rawGetVideoInfo_DetailPage);
                    ////////            }
                    ////////        }

                    ////////        // Check error
                    ////////        if (videoInfo_EmbeddedOrDetailPage.ContainsKey("errorcode"))
                    ////////        {
                    ////////            var errorCode = videoInfo_EmbeddedOrDetailPage["errorcode"].ParseInt();
                    ////////            var errorReason = videoInfo_EmbeddedOrDetailPage["reason"];

                    ////////            throw new VideoUnavailableException(videoId, errorCode, errorReason);
                    ////////        }
                    ////////    }

                    ////////    // Check if requires purchase
                    ////////    if (videoInfo_EmbeddedOrDetailPage.ContainsKey("ypc_vid"))
                    ////////    {
                    ////////        var previewVideoId = videoInfo_EmbeddedOrDetailPage["ypc_vid"];
                    ////////        throw new Exception(string.Format("Video [{0}] requires purchase and cannot be processed." + Environment.NewLine + "Free preview video [{1}] is available.", videoId, previewVideoId));
                    ////////    }

                    ////////    streamInfoSet = GetVideoMediaStreamInfosAsync(videoInfo_EmbeddedOrDetailPage);
                    ////////    var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
                    ////////    var normalizedFileSize = NormalizeFileSize(streamInfo.Size);

                    ////////    #endregion

                    ////////    string vstreamInfo = $"Quality: {streamInfo.VideoQualityLabel} | Container: {streamInfo.Container} | Size: {normalizedFileSize}";

                    ////////    //////////////////////////////////////////////////////////////////////////////////////////////

                    ////////    ////// Compose file name, based on metadata
                    ////////    ////var fileExtension = streamInfo.Container.GetFileExtension();
                    ////////    ////var fileName = $"{video.Title}.{fileExtension}";
                    ////////    ////// Replace illegal characters in file name
                    ////////    //////fileName = fileName.Replace(Path.GetInvalidFileNameChars(), '_');
                    ////////    ////// Download video
                    ////////    ////Console.WriteLine($"Downloading to [{fileName}]...");
                    ////////    ////Console.WriteLine('-'.Repeat(100));
                    ////////    ////var progress = new Progress<double>(p => Console.Title = $"YoutubeExplode Demo [{p:P0}]");
                    ////////    ////await client.DownloadMediaStreamAsync(streamInfo, fileName, progress);
                    ////////}, w);
                    #endregion
                    //    break;

                    #region [ SEARCH ONLINE ]

                    case _API.MEDIA_KEY_PLAY_VIDEO_ONLINE:
                        #region
                        if (true)
                        {
                            long mediaId = (long)m.Input;
                            string urlSrc = f_search_fetchUriSource(mediaId, MEDIA_TYPE.MP4);
                            if (!string.IsNullOrEmpty(urlSrc))
                            {
                                m.Output.Ok = true;
                                m.Output.Data = f_proxy_getUriProxy(mediaId, MEDIA_TYPE.MP4) + "&online=true";
                                response_toMain(m);
                            }
                            else
                            {
                                // cannot fetch uri
                            }
                        }
                        break;
                    #endregion
                    case _API.MEDIA_KEY_SEARCH_ONLINE_CACHE_CLEAR:
                        #region
                        Bitmap bitremove = null;
                        foreach (long mid in dicMediaSearch.Keys)
                            dicMediaImage.TryRemove(mid, out bitremove);
                        dicMediaSearch.Clear();

                        notification_toMain(new msg() { API = m.API, KEY = m.KEY, Log = "Clear all cache of result search successfully!" });
                        Execute(new msg() { API = m.API, KEY = _API.MEDIA_KEY_SEARCH_ONLINE_CACHE, Input = string.Empty });

                        break;
                    #endregion
                    case _API.MEDIA_KEY_SEARCH_ONLINE_SAVE_TO_STORE:
                        #region
                        if (true)
                        {
                            long mediaId = (long)m.Input;
                            oMedia mi = null;
                            if (dicMediaSearch.TryGetValue(mediaId, out mi) && mi != null)
                            {
                                if (!dicMediaStore.ContainsKey(mediaId))
                                {
                                    dicMediaStore.TryAdd(mediaId, mi);

                                    oMedia del_mi;
                                    dicMediaSearch.TryRemove(mediaId, out del_mi);

                                    f_media_writeFile();

                                    if (dicMediaImage.ContainsKey(mediaId))
                                    {
                                        //string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "photo");
                                        //if (Directory.Exists(dir) == false) Directory.CreateDirectory(dir);
                                        //string filename = Path.Combine(dir, mediaId.ToString() + ".jpg");
                                        //if (!File.Exists(filename))
                                        //{
                                        Bitmap bitmap = null;
                                        if (dicMediaImage.TryGetValue(mediaId, out bitmap) && bitmap != null)
                                            //bitmap.Save(filename, ImageFormat.Jpeg);
                                            dicMediaImage.TryRemove(mediaId, out bitmap);
                                        //}
                                    }

                                    notification_toMain(new appel.msg()
                                    {
                                        API = _API.MSG_MEDIA_SEARCH_SAVE_TO_STORE,
                                        Log = string.Format("{0} save successfully!", mi.Title),
                                    });
                                }
                            }
                        }
                        break;
                    #endregion
                    case _API.MEDIA_KEY_TEXT_VIDEO_ONLINE:
                        #region
                        if (true)
                        {
                            long mediaId = (long)m.Input;
                            oMedia mi = null;
                            if (dicMediaSearch.TryGetValue(mediaId, out mi) && mi != null)
                            {
                                string content = mi.Title;
                                if (!string.IsNullOrEmpty(mi.Description))
                                    content += Environment.NewLine + mi.Description;

                                if (mi.Keywords != null && mi.Keywords.Count > 0)
                                    content += Environment.NewLine + string.Join(" ", mi.Keywords);

                                string text = mi.SubtileEnglish;
                                if (!string.IsNullOrEmpty(text))
                                {
                                    text = Regex.Replace(text, @"<[^>]*>", String.Empty);
                                    text = HttpUtility.HtmlDecode(text);
                                    text = text.Replace('\r', ' ').Replace('\n', ' ');
                                    string[] a = text.Split(new char[] { '.' })
                                        .Select(x => x.Trim())
                                        .Where(x => x != string.Empty)
                                        .ToArray();
                                    text = string.Join(Environment.NewLine, a);
                                    text = text.Replace("?", "?" + Environment.NewLine);
                                    a = text.Split(new char[] { '\r', '\n' })
                                       .Select(x => x.Trim())
                                       .Where(x => x != string.Empty)
                                       .ToArray();
                                    text = string.Empty;
                                    foreach (string ti in a)
                                        if (ti[ti.Length - 1] == '?') text += ti + Environment.NewLine;
                                        else text += ti + "." + Environment.NewLine;
                                }

                                content += Environment.NewLine +
                                    "------------------------------------------------------------------"
                                    + Environment.NewLine + Environment.NewLine +
                                    text;

                                m.Output.Ok = true;
                                m.Output.Data = content;
                                response_toMain(m);

                                Execute(new msg() { API = _API.MEDIA, KEY = _API.MEDIA_KEY_PLAY_VIDEO_ONLINE, Input = mediaId });
                            }
                        }
                        break;
                    #endregion
                    case _API.MEDIA_KEY_SEARCH_ONLINE_CACHE:
                        #region

                        if (true)
                        {
                            string input = (string)m.Input;
                            if (input == null) input = string.Empty;
                            oMediaSearchLocalResult resultSearch = new oMediaSearchLocalResult();

                            List<long> lsSearch = new List<long>();
                            int min = (m.PageNumber - 1) * m.PageSize,
                                max = m.PageNumber * m.PageSize,
                                count = 0;
                            foreach (var kv in dicMediaSearch)
                            {
                                if (kv.Value.Title.ToLower().Contains(input)
                                    || kv.Value.Description.ToLower().Contains(input))
                                {
                                    if (count >= min && count < max)
                                        lsSearch.Add(kv.Key);
                                    count++;
                                }
                            }
                            resultSearch.TotalItem = dicMediaStore.Count;
                            resultSearch.PageSize = m.PageSize;
                            resultSearch.PageNumber = m.PageNumber;
                            resultSearch.CountResult = count;
                            resultSearch.MediaIds = lsSearch;

                            m.Counter = count;
                            m.Output.Ok = true;
                            m.Output.Data = resultSearch;
                            response_toMain(m);
                        }

                        break;
                    #endregion
                    case _API.MEDIA_KEY_SEARCH_ONLINE_STOP:
                        #region
                        // wait_search.WaitOne();
                        // wait_search.Reset(); // Need to pause the background thread
                        // wait_search.Set(); // Need to continue the background thread
                        stop_search = true;
                        break;
                    #endregion
                    case _API.MEDIA_KEY_SEARCH_ONLINE_NEXT:
                        #region
                        if (true)
                        {
                            string input = (string)m.Input;
                            if (input == null) input = string.Empty;
                            List<long> lsSearch = new List<long>();

                            if (!string.IsNullOrEmpty(input))
                            {
                                var _client = new YoutubeClient();
                                bool hasCC_Subtitle = true;
                                int page_query = m.PageNumber;
                                long[] aIDs = new long[] { };

                                do
                                {
                                    if (stop_search) break;

                                    page_query++;
                                    aIDs = f_media_searchYoutubeOnline(_client, input, page_query);
                                    lsSearch.AddRange(aIDs);

                                    //if (aIDs.Length > 0)
                                    //{
                                    //    notification_toMain(new appel.msg()
                                    //    {
                                    //        API = _API.MSG_MEDIA_SEARCH_RESULT,
                                    //        Log = string.Format("Page {0} -> Total items: {1}: Search online [ {2} ] ...", page_query, lsSearch.Count, input),
                                    //    });
                                    //}

                                    //if (page_query == 2)
                                    //{
                                    //    new Thread(new ParameterizedThreadStart((object so) =>
                                    //    {
                                    //        Execute(new msg()
                                    //        {
                                    //            API = _API.MEDIA,
                                    //            KEY = _API.MEDIA_KEY_SEARCH_ONLINE_CACHE,
                                    //            Input = so
                                    //        });
                                    //    })).Start(input);
                                    //}

                                }
                                while (aIDs.Length != 0);
                                stop_search = false;


                                notification_toMain(new appel.msg()
                                {
                                    API = _API.MSG_MEDIA_SEARCH_RESULT,
                                    Log = string.Format(">>> Search online [ {0} ] completed", input),
                                });

                                //m.KEY = _API.MEDIA_KEY_SEARCH_ONLINE_CACHE;
                                ////m_first.Input = input;
                                //m.Input = string.Empty;
                                //m.Counter = lsSearch.Count;
                                //m.Output.Ok = true;
                                //m.Output.Data = new oMediaSearchLocalResult()
                                //{
                                //    CountResult = lsSearch.Count,
                                //    MediaIds = lsSearch.Take(m.PageSize).ToList(),
                                //    TotalItem = lsSearch.Count,
                                //};
                                //response_toMain(m);
                            }
                        }
                        break;
                    #endregion
                    case _API.MEDIA_KEY_SEARCH_ONLINE:
                        #region 
                        if (true)
                        {
                            string input = (string)m.Input;
                            if (input == null) input = string.Empty;
                            List<long> lsSearch = new List<long>();

                            if (!string.IsNullOrEmpty(input))
                            {
                                SearchMediaCaptionCC = m.Log == "CC" ? true : false;

                                var _client = new YoutubeClient();
                                int page_query = 1;
                                long[] aIDs = f_media_searchYoutubeOnline(_client, input, page_query);
                                lsSearch.AddRange(aIDs);

                                //while (lsSearch.Count < 10)
                                //{
                                //    page_query++;
                                //    aIDs = f_media_searchYoutubeOnline(_client, input, page_query, hasCC_Subtitle);
                                //    lsSearch.AddRange(aIDs);
                                //}

                                var m_first = m.clone(m.Input);
                                m_first.KEY = _API.MEDIA_KEY_SEARCH_ONLINE_CACHE;
                                //m_first.Input = input;
                                m_first.Input = string.Empty;
                                m_first.Counter = lsSearch.Count;
                                m_first.Output.Ok = true;
                                m_first.Output.Data = new oMediaSearchLocalResult()
                                {
                                    CountResult = lsSearch.Count,
                                    MediaIds = lsSearch.Take(m.PageSize).ToList(),
                                    TotalItem = lsSearch.Count,
                                };
                                response_toMain(m_first);

                                new Thread(new ParameterizedThreadStart((object so) =>
                                {
                                    Execute(new msg()
                                    {
                                        API = _API.MEDIA,
                                        KEY = _API.MEDIA_KEY_SEARCH_ONLINE_NEXT,
                                        PageNumber = page_query,
                                        Input = so
                                    });
                                })).Start(input);
                            }
                        }
                        break;

                        #endregion

                        #endregion
                }
            }
            return m;
        }

        public void Close()
        {
            proxy_Close();
        }

        #endregion

        #region [ WORD ]

        //public static List<GRAMMAR> listGrammar = new List<GRAMMAR>()
        //{
        //    new GRAMMAR(){
        //        Type = "too...for",
        //        Example = "This structure is too easy for you to remember|He ran too fast for me to follow",
        //        Explain = "quá....để cho ai làm gì...",
        //        KeyWord = new string[]{ "too","for" },
        //        Struct = "S + V + too + adj/adv + (for someone) + to do something"
        //    },
        //    new GRAMMAR(){
        //        Type = "so...that",
        //        Example = "This box is so heavy that I cannot take it|He speaks so soft that we can’t hear anything",
        //        Explain = "quá... đến nỗi mà...",
        //        KeyWord = new string[]{ "","" },
        //        Struct = "S + V + so + adj/ adv + that + S + V"
        //    }
        //};

        /*
        WHO   (ai)
        WHOM  (ai)
        WHOSE (của ai) : chỉ sở hữu 
        WHAT  (gì, cái gỉ) : chỉ đồ vật, sự việc, hay con vật.
        WHICH (nào, cái nào): chỉ sự chọn lựa; chỉ đồ vật, sự việc hay con vật. 
        WHEN  (khi nào) : chỉ thời gian - 
        WHERE (đâu, ở đâu) : chỉ nơi chốn - 
        WHY   (tại sao) : chỉ lí do hoặc nguyên nhân. - 
        HOW   (thể nào, cách nào) : chỉ trạng thái, phương tiện hay phương pháp.
          */
        public const string WORDS_QUESTION_WH = "who,whom,whose,what,which,when,where,why,how";
        public const string WORDS_VERB_TOBE = "am,is,are,'m,'s,'re,amn't,isn't,aren't,was,were,wasn't,weren't";
        public const string WORDS_VERB_MODAL = "can,can’t,cannot,could,must,mustn’t,have to,may,might,will,would,shall,should,ought to,dare,need,needn’t,used to";
        public const string WORDS_VERB_INFINITIVE = "";
        public const string WORDS_SPECIALIZE = "";

        public readonly Dictionary<string, string> DEFINE = new Dictionary<string, string>() {
            { "bare-infinitive","động từ nguyên thể không “to” "}
        };

        public readonly Dictionary<string, string> dicGRAMMAR = new Dictionary<string, string>() {
            { "ought not to have","ought not to have + Vpp: diễn tả một sự không tán đồng về một hành động đã làm trong quá khứ, không phải làm điều gì đó"}
            ,{ "able to","was/were able to: Nếu câu nói hàm ý một sự thành công trong việc thực hiện hành động (succeeded in doing)" }

        };
        public readonly Dictionary<string, string> IDOM = new Dictionary<string, string>() { };

        public const string WORDS_VERBS_IRREGULAR =
        #region [ WORDS_VERBS_IRREGULAR ]

                                                    "| abide ; abode , abided ; abode , abided ; " +                    //-"lưu trú ,  lưu lại"
                                                    "| arise ; arose ; arisen ; " +                                     //-phát sinh
                                                    "| awake ; awoke ; awoken ; " +                                     //-"đánh thức ,  thức"
                                                    "| be ; was , were ; been ; " +                                     //-"thì ,  là ,  bị ,  ở"
                                                    "| bear ; bore ; borne ; " +                                        //-"mang ,  chịu đựng"
                                                    "| become ; became ; become ; " +                                   //-trở nên
                                                    "| befall ; befell ; befallen ; " +                                 //-xảy đến
                                                    "| begin ; began ; begun ; " +                                      //-bắt đầu
                                                    "| behold ; beheld ; beheld ; " +                                   //-ngắm nhìn
                                                    "| bend ; bent ; bent ; " +                                         //-bẻ cong
                                                    "| beset ; beset ; beset ; " +                                      //-bao quanh
                                                    "| bespeak ; bespoke ; bespoken ; " +                               //-chứng tỏ
                                                    "| bid ; bid ; bid ; " +                                            //-trả giá
                                                    "| bind ; bound ; bound ; " +                                       //-"buộc ,  trói"
                                                    "| bleed ; bled ; bled ; " +                                        //-chảy máu
                                                    "| blow ; blew ; blown ; " +                                        //-thổi
                                                    "| break ; broke ; broken ; " +                                     //-đập vỡ
                                                    "| breed ; bred ; bred ; " +                                        //-"nuôi ,  dạy dỗ"
                                                    "| bring ; brought ; brought ; " +                                  //-mang đến
                                                    "| broadcast ; broadcast ; broadcast ; " +                          //-phát thanh
                                                    "| build ; built ; built ; " +                                      //-xây dựng
                                                    "| burn ; burnt , burned ; burnt , burned ; " +                     //-"đốt ,  cháy"
                                                    "| buy ; bought ; bought ; " +                                      //-mua
                                                    "| cast ; cast ; cast ; " +                                         //-"ném ,  tung"
                                                    "| catch ; caught ; caught ; " +                                    //-"bắt ,  chụp"
                                                    "| chide ; chid , chided ; chid , chidden , chided ; " +            //-"mắng ,  chửi"
                                                    "| choose ; chose ; chosen ; " +                                    //-"chọn ,  lựa"
                                                    "| cleave ; clove , cleaved ; cloven , cleaved ; " +                //-"chẻ ,  tách hai"
                                                    "| cleave ; clave ; cleaved ; " +                                   //-dính chặt
                                                    "| come ; came ; come ; " +                                         //-"đến ,  đi đến"
                                                    "| cost ; cost ; cost ; " +                                         //-có giá là
                                                    "| crow ; crew , crewed ; crowed ; " +                              //-gáy (gà)
                                                    "| cut ; cut ; cut ; " +                                            //-"cắn ,  chặt"
                                                    "| deal ; dealt ; dealt ; " +                                       //-giao thiệp
                                                    "| dig ; dug ; dug ; " +                                            //-dào
                                                    "| dive ; dove , dived ; dived ; " +                                //-"lặn ,  lao xuống"
                                                    "| draw ; drew ; drawn ; " +                                        //-"vẽ ,  kéo"
                                                    "| dream ; dreamt , dreamed ; dreamt , dreamed ; " +                //-mơ thấy
                                                    "| drink ; drank ; drunk ; " +                                      //-uống
                                                    "| drive ; drove ; driven ; " +                                     //-lái xe
                                                    "| dwell ; dwelt ; dwelt ; " +                                      //-"trú ngụ ,  ở"
                                                    "| eat ; ate ; eaten ; " +                                          //-ăn
                                                    "| fall ; fell ; fallen ; " +                                       //-"ngã ,  rơi"
                                                    "| feed ; fed ; fed ; " +                                           //-"cho ăn ,  ăn ,  nuôi"
                                                    "| feel ; felt ; felt ; " +                                         //-cảm thấy
                                                    "| fight ; fought ; fought ; " +                                    //-chiến đấu
                                                    "| find ; found ; found ; " +                                       //-"tìm thấy ,  thấy"
                                                    "| flee ; fled ; fled ; " +                                         //-chạy trốn
                                                    "| fling ; flung ; flung ; " +                                      //-tung ;  quang
                                                    "| fly ; flew ; flown ; " +                                         //-bay
                                                    "| forbear ; forbore ; forborne ; " +                               //-nhịn
                                                    "| forbid ; forbade , forbad ; forbidden ; " +                      //-"cấm ,  cấm đoán"
                                                    "| forecast ; forecast , forecasted ; forecast , forecasted ; " +   //-tiên đoán
                                                    "| foresee ; foresaw ; forseen ; " +                                //-thấy trước
                                                    "| foretell ; foretold ; foretold ; " +                             //-đoán trước
                                                    "| forget ; forgot ; forgotten ; " +                                //-quên
                                                    "| forgive ; forgave ; forgiven ; " +                               //-tha thứ
                                                    "| forsake ; forsook ; forsaken ; " +                               //-ruồng bỏ
                                                    "| freeze ; froze ; frozen ; " +                                    //-(làm) đông lại
                                                    "| get ; got ; got , gotten ; " +                                   //-có được
                                                    "| gild ; gilt , gilded ; gilt , gilded ; " +                       //-mạ vàng
                                                    "| gird ; girt , girded ; girt , girded ; " +                       //-đeo vào
                                                    "| give ; gave ; given ; " +                                        //-cho
                                                    "| go ; went ; gone ; " +                                           //-đi
                                                    "| grind ; ground ; ground ; " +                                    //-"nghiền ,  xay"
                                                    "| grow ; grew ; grown ; " +                                        //-"mọc ,  trồng"
                                                    "| hang ; hung ; hung ; " +                                         //-"móc lên ,  treo lên"
                                                    "| hear ; heard ; heard ; " +                                       //-nghe
                                                    "| heave ; hove , heaved ; hove , heaved ; " +                      //-trục lên
                                                    "| hide ; hid ; hidden ; " +                                        //-"giấu ,  trốn ,  nấp"
                                                    "| hit ; hit ; hit ; " +                                            //-đụng
                                                    "| hurt ; hurt ; hurt ; " +                                         //-làm đau
                                                    "| inlay ; inlaid ; inlaid ; " +                                    //-"cẩn ,  khảm"
                                                    "| input ; input ; input ; " +                                      //-đưa vào (máy điện toán)
                                                    "| inset ; inset ; inset ; " +                                      //-"dát ,  ghép"
                                                    "| keep ; kept ; kept ; " +                                         //-giữ
                                                    "| kneel ; knelt , kneeled ; knelt , kneeled ; " +                  //-quỳ
                                                    "| knit ; knit , knitted ; knit , knitted ; " +                     //-đan
                                                    "| know ; knew ; known ; " +                                        //-"biết ,  quen biết"
                                                    "| lay ; laid ; laid ; " +                                          //-"đặt ,  để"
                                                    "| lead ; led ; led ; " +                                           //-"dẫn dắt ,  lãnh đạo"
                                                    "| leap ; leapt ; leapt ; " +                                       //-"nhảy ,  nhảy qua"
                                                    "| learn ; learnt , learned ; learnt , learned ; " +                //-"học ,  được biết"
                                                    "| leave ; left ; left ; " +                                        //-"ra đi ,  để lại"
                                                    "| lend ; lent ; lent ; " +                                         //-cho mượn (vay)
                                                    "| let ; let ; let ; " +                                            //-"cho phép ,  để cho"
                                                    "| lie ; lay ; lain ; " +                                           //-nằm
                                                    "| light ; lit , lighted ; lit ,  lighted ; " +                     //-thắp sáng
                                                    "| lose ; lost ; lost ; " +                                         //-"làm mất ,  mất"
                                                    "| make ; made ; made ; " +                                         //-"chế tạo ,  sản xuất"
                                                    "| mean ; meant ; meant ; " +                                       //-có nghĩa là
                                                    "| meet ; met ; met ; " +                                           //-gặp mặt
                                                    "| mislay ; mislaid ; mislaid ; " +                                 //-để lạc mất
                                                    "| misread ; misread ; misread ; " +                                //-đọc sai
                                                    "| misspell ; misspelt ; misspelt ; " +                             //-viết sai chính tả
                                                    "| mistake ; mistook ; mistaken ; " +                               //-"phạm lỗi ,  lầm lẫn"
                                                    "| misunderstand ; misunderstood ; misunderstood ; " +              //-hiểu lầm
                                                    "| mow ; mowed ; mown , mowed ; " +                                 //-cắt cỏ
                                                    "| outbid ; outbid ; outbid ; " +                                   //-trả hơn giá
                                                    "| outdo ; outdid ; outdone ; " +                                   //-làm giỏi hơn
                                                    "| outgrow ; outgrew ; outgrown ; " +                               //-lớn nhanh hơn
                                                    "| output ; output ; output ; " +                                   //-cho ra (dữ kiện)
                                                    "| outrun ; outran ; outrun ; " +                                   //-chạy nhanh hơn ;  vượt giá
                                                    "| outsell ; outsold ; outsold ; " +                                //-bán nhanh hơn
                                                    "| overcome ; overcame ; overcome ; " +                             //-khắc phục
                                                    "| overeat ; overate ; overeaten ; " +                              //-ăn quá nhiều
                                                    "| overfly ; overflew ; overflown ; " +                             //-bay qua
                                                    "| overhang ; overhung ; overhung ; " +                             //-"nhô lên trên ,  treo lơ lửng"
                                                    "| overhear ; overheard ; overheard ; " +                           //-nghe trộm
                                                    "| overlay ; overlaid ; overlaid ; " +                              //-phủ lên
                                                    "| overpay ; overpaid ; overpaid ; " +                              //-trả quá tiền
                                                    "| overrun ; overran ; overrun ; " +                                //-tràn ngập
                                                    "| oversee ; oversaw ; overseen ; " +                               //-trông nom
                                                    "| overshoot ; overshot ; overshot ; " +                            //-đi quá đích
                                                    "| oversleep ; overslept ; overslept ; " +                          //-ngủ quên
                                                    "| overtake ; overtook ; overtaken ; " +                            //-đuổi bắt kịp
                                                    "| overthrow ; overthrew ; overthrown ; " +                         //-lật đổ
                                                    "| pay ; paid ; paid ; " +                                          //-trả (tiền)
                                                    "| prove ; proved ; proven , proved ; " +                           //-chứng minh (tỏ)
                                                    "| put ; put ; put ; đặt ;  " +                                     //-để
                                                    "| read ; read ; read ; " +                                         //-đọc
                                                    "| rebuild ; rebuilt ; rebuilt ; " +                                //-xây dựng lại
                                                    "| redo ; redid ; redone ; " +                                      //-làm lại
                                                    "| remake ; remade ; remade ; " +                                   //-làm lại ; chế tạo lại
                                                    "| rend ; rent ; rent ; " +                                         //-toạc ra ;  xé
                                                    "| repay ; repaid ; repaid ; " +                                    //-hoàn tiền lại
                                                    "| resell ; resold ; resold ; " +                                   //-bán lại
                                                    "| retake ; retook ; retaken ; " +                                  //-chiếm lại ;  tái chiếm
                                                    "| rewrite ; rewrote ; rewritten ; " +                              //-viết lại
                                                    "| rid ; rid ; rid ; " +                                            //-giải thoát
                                                    "| ride ; rode ; ridden ; " +                                       //-cưỡi
                                                    "| ring ; rang ; rung ; " +                                         //-rung chuông
                                                    "| rise ; rose ; risen ; " +                                        //-đứng dậy ;  mọc
                                                    "| run ; ran ; run ; " +                                            //-chạy
                                                    "| saw ; sawed ; sawn ; " +                                         //-cưa
                                                    "| say ; said ; said ; " +                                          //-nói
                                                    "| see ; saw ; seen ; " +                                           //-nhìn thấy
                                                    "| seek ; sought ; sought ; " +                                     //-tìm kiếm
                                                    "| sell ; sold ; sold ; " +                                         //-bán
                                                    "| send ; sent ; sent ; " +                                         //-gửi
                                                    "| sew ; sewed ; sewn , sewed ; " +                                 //-may
                                                    "| shake ; shook ; shaken ; " +                                     //-lay ;  lắc
                                                    "| shear ; sheared ; shorn ; " +                                    //-xén lông (Cừu)
                                                    "| shed ; shed ; shed ; rơi ;  " +                                  //-rụng
                                                    "| shine ; shone ; shone ; " +                                      //-chiếu sáng
                                                    "| shoot ; shot ; shot ; " +                                        //-bắn
                                                    "| show ; showed ; shown , showed ; " +                             //-cho xem
                                                    "| shrink ; shrank ; shrunk ; " +                                   //-co rút
                                                    "| shut ; shut ; shut ; " +                                         //-đóng lại
                                                    "| sing ; sang ; sung ; " +                                         //-ca hát
                                                    "| sink ; sank ; sunk ; " +                                         //-chìm ;  lặn
                                                    "| sit ; sat ; sat ; " +                                            //-ngồi
                                                    "| slay ; slew ; slain ; " +                                        //-sát hại ;  giết hại
                                                    "| sleep ; slept ; slept ; " +                                      //-ngủ
                                                    "| slide ; slid ; slid ; " +                                        //-trượt ;  lướt
                                                    "| sling ; slung ; slung ; " +                                      //-ném mạnh
                                                    "| slink ; slunk ; slunk ; " +                                      //-lẻn đi
                                                    "| smell ; smelt ; smelt ; " +                                      //-ngửi
                                                    "| smite ; smote ; smitten ; " +                                    //-đập mạnh
                                                    "| sow ; sowed ; sown , sewed ; " +                                 //-gieo ;  rải
                                                    "| speak ; spoke ; spoken ; " +                                     //-nói
                                                    "| speed ; sped , speeded ; sped , speeded ; " +                    //-chạy vụt
                                                    "| spell ; spelt , spelled ; spelt , spelled ; " +                  //-đánh vần
                                                    "| spend ; spent ; spent ; " +                                      //-tiêu sài
                                                    "| spill ; spilt , spilled ; spilt , spilled ; " +                  //-tràn ;  đổ ra
                                                    "| spin ; spun , span ; spun ; " +                                  //-quay sợi
                                                    "| spit ; spat ; spat ; " +                                         //-khạc nhổ
                                                    "| spoil ; spoilt , spoiled ; spoilt , spoiled ; " +                //-làm hỏng
                                                    "| spread ; spread ; spread ; " +                                   //-lan truyền
                                                    "| spring ; sprang ; sprung ; " +                                   //-nhảy
                                                    "| stand ; stood ; stood ; " +                                      //-đứng
                                                    "| stave ; stove , staved ; stove , staved ; " +                    //-đâm thủng
                                                    "| steal ; stole ; stolen ; " +                                     //-đánh cắp
                                                    "| stick ; stuck ; stuck ; " +                                      //-ghim vào ;  đính
                                                    "| sting ; stung ; stung ; " +                                      //-châm  ;  chích ;  đốt
                                                    "| stink ; stunk , stank ; stunk ; " +                              //-bốc mùi hôi
                                                    "| strew ; strewed ; strewn , strewed ; " +                         //-"rắc  ,  rải"
                                                    "| stride ; strode ; stridden ; " +                                 //-bước sải
                                                    "| strike ; struck ; struck ; " +                                   //-đánh đập
                                                    "| string ; strung ; strung ; " +                                   //-gắn dây vào
                                                    "| strive ; strove ; striven ; " +                                  //-cố sức
                                                    "| swear ; swore ; sworn ; " +                                      //-tuyên thệ
                                                    "| sweep ; swept ; swept ; " +                                      //-quét
                                                    "| swell ; swelled ; swollen , swelled ; " +                        //-phồng ;  sưng
                                                    "| swim ; swam ; swum ; " +                                         //-bơi lội
                                                    "| swing ; swung ; swung ; " +                                      //-đong đưa
                                                    "| take ; took ; taken ; " +                                        //-cầm ; lấy
                                                    "| teach ; taught ; taught ; " +                                    //-dạy ; giảng dạy
                                                    "| tear ; tore ; torn ; " +                                         //-xé ; rách
                                                    "| tell ; told ; told ; " +                                         //-kể ; bảo
                                                    "| think ; thought ; thought ; " +                                  //-suy nghĩ
                                                    "| throw ; threw ; thrown ; " +                                     //-ném  ;  liệng
                                                    "| thrust ; thrust ; thrust ; " +                                   //-thọc  ; nhấn
                                                    "| tread ; trod ; trodden , trod ; " +                              //-giẫm  ;  đạp
                                                    "| unbend ; unbent ; unbent ; " +                                   //-làm thẳng lại
                                                    "| undercut ; undercut ; undercut ; " +                             //-ra giá rẻ hơn
                                                    "| undergo ; underwent ; undergone ; " +                            //-kinh qua
                                                    "| underlie ; underlay ; underlain ; " +                            //-nằm dưới
                                                    "| underpay ; underpaid ; underpaid ; " +                           //-trả lương thấp
                                                    "| undersell ; undersold ; undersold ; " +                          //-bán rẻ hơn
                                                    "| understand ; understood ; understood ; " +                       //-hiểu
                                                    "| undertake ; undertook ; undertaken ; " +                         //-đảm nhận
                                                    "| underwrite ; underwrote ; underwritten ; " +                     //-bảo hiểm
                                                    "| undo ; undid ; undone ; " +                                      //-tháo ra
                                                    "| unfreeze ; unfroze ; unfrozen ; " +                              //-làm tan đông
                                                    "| unwind ; unwound ; unwound ; " +                                 //-tháo ra
                                                    "| uphold ; upheld ; upheld ; " +                                   //-ủng hộ
                                                    "| upset ; upset ; upset ; " +                                      //-đánh đổ ;  lật đổ
                                                    "| wake ; woke , waked ; woken , waked ; " +                        //-thức giấc
                                                    "| waylay ; waylaid ; waylaid ; " +                                 //-mai phục
                                                    "| wear ; wore ; worn ; " +                                         //-mặc
                                                    "| weave ; wove , weaved ; woven , weaved ; " +                     //-dệt
                                                    "| wed ; wed , wedded ; wed , wedded ; " +                          //-kết hôn
                                                    "| weep ; wept ; wept ; " +                                         //-khóc
                                                    "| wet ; wet , wetted ; wet , wetted ; " +                          //-làm ướt
                                                    "| win ; won ; won ; thắng  ;  " +                                  //-chiến thắng
                                                    "| wind ; wound ; wound ; " +                                       //-quấn
                                                    "| withdraw ; withdrew ; withdrawn ; " +                            //-rút lui
                                                    "| withhold ; withheld ; withheld ; " +                             //-từ khước
                                                    "| withstand ; withstood ; withstood ; " +                          //-cầm cự
                                                    "| work ; worked ;  worked ; " +                                    //-"rèn (sắt) ,  nhào nặng đất"
                                                    "| wring ; wrung ; wrung ; " +                                      //-vặn  ;  siết chặt
                                                    "| write ; wrote ; written ; ";                                    //-viết

        //public static Dictionary<string, SCRIPT[]> DicScript { get => DicScript1; set => DicScript1 = value; }
        //public static Dictionary<string, SCRIPT[]> DicScript1 { get => dicScript; set => dicScript = value; }
        #endregion

        const char heading_char = '#'; // ■ ≡ ¶ ■
        const string heading_text = "\r\n# ";
        static HttpClient web_word_pronunciation = new HttpClient();

        static ConcurrentDictionary<string, string> dicWord = null;
        static ConcurrentDictionary<string, string> dicWordPronunciation = null;
        static ConcurrentDictionary<string, string> dicWordLink = null;
        static ConcurrentDictionary<string, string> dicWordMeanVi = null;

        static ConcurrentDictionary<string, List<string>> dicWordSentence = null;
        static ConcurrentDictionary<string, List<string>> dicWordMp3 = null;

        static readonly string file_word_mean_vi = Path.Combine(path_data, "word-vi.bin");
        static readonly string file_word_pronunciation = Path.Combine(path_data, "word-pro.bin");

        public static void f_word_Init()
        {
            dicWord = new ConcurrentDictionary<string, string>();
            dicWordPronunciation = new ConcurrentDictionary<string, string>();
            dicWordLink = new ConcurrentDictionary<string, string>();
            dicWordMeanVi = new ConcurrentDictionary<string, string>();

            dicWordSentence = new ConcurrentDictionary<string, List<string>>();
            dicWordMp3 = new ConcurrentDictionary<string, List<string>>();

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "words");
            if (Directory.Exists(dir) == false) Directory.CreateDirectory(dir);
            else
            {
                string[] fs = Directory.GetFiles(dir, "*.txt"),
                wlink = new string[] { };
                string word = string.Empty,
                text = string.Empty,
                pronunciation = string.Empty,
                pronun_head = heading_char.ToString() + " REF:";

                for (int i = 0; i < fs.Length; i++)
                {
                    word = Path.GetFileName(fs[i]);
                    word = word.Substring(0, word.Length - 4).Trim().ToLower();
                    text = File.ReadAllText(fs[i]);
                    if (text.Contains('/'))
                        pronunciation = text.Split('/')[1];

                    if (text.Contains(pronun_head))
                    {
                        wlink = text
                            .Split(new string[] { pronun_head, Environment.NewLine })[1].Split(';')
                            .Select(x => x.Trim().ToLower())
                            .ToArray();
                        if (wlink.Length > 0)
                        {
                            for (int k = 0; k < wlink.Length; k++)
                                if (!dicWordLink.ContainsKey(wlink[k]))
                                    dicWordLink.TryAdd(wlink[k], word);
                        }
                    }

                    dicWord.TryAdd(word, text);
                    dicWordPronunciation.TryAdd(word, pronunciation);

                    if (text.Contains('{') && text.Contains('}'))
                    {
                        var mp3s = text
                            .Split(new char[] { '{', '}' })[1]
                            .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                            .Select(x => x.Trim())
                            .Where(x => x.Length > 0)
                            .ToList();
                        if (mp3s.Count > 0)
                        {
                            if (!dicWordMp3.ContainsKey(word))
                                dicWordMp3.TryAdd(word, mp3s);
                        }
                    }
                }
            }

            if (File.Exists(file_word_mean_vi))
                using (var file = File.OpenRead(file_word_mean_vi))
                    dicWordMeanVi = Serializer.Deserialize<ConcurrentDictionary<string, string>>(file);

            if (File.Exists(file_word_pronunciation))
                using (var file = File.OpenRead(file_word_pronunciation))
                    dicWordPronunciation = Serializer.Deserialize<ConcurrentDictionary<string, string>>(file);
        }

        private static void f_word_mean_vi_writeFile()
        {
            using (var file = File.Create(file_word_mean_vi))
                Serializer.Serialize<ConcurrentDictionary<string, string>>(file, dicWordMeanVi);
        }

        private static void f_word_pronunciation_writeFile()
        {
            using (var file = File.Create(file_word_pronunciation))
                Serializer.Serialize<ConcurrentDictionary<string, string>>(file, dicWordPronunciation);
        }

        private void f_word_MEDIA_KEY_WORD_TRANSLATER(msg m)
        {
            if (m.Input != null)
            {
                long mediaId = (long)m.Input;
                oWordCount[] a = f_media_getWords(mediaId);

                var akey = dicWordMeanVi.Keys.ToArray();

                string[] ws = a.OrderByDescending(x => x.count)
                        .Where(x => x.word.Length > 3)
                        .Select(x => x.word)
                        .ToArray();
                bool hasMultiPart = false;

                if (akey.Length > 0)
                    ws = ws.Where(x => !akey.Any(o => o == x)).ToArray();
                if (ws.Length > 0)
                {
                    if (ws.Length > 300)
                    {
                        ws = ws.Take(300).ToArray();
                        hasMultiPart = true;
                    }

                    string input = string.Join(Environment.NewLine, ws);

                    //string temp = HttpUtility.UrlEncode(input.Replace(" ", "---"));
                    string temp = HttpUtility.UrlEncode(input);
                    //temp = temp.Replace("-----", "%20");

                    string url = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}", temp, "en|vi");

                    string s = String.Empty;
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Encoding = Encoding.UTF7;
                        s = webClient.DownloadString(url);
                    }
                    string ht = HttpUtility.HtmlDecode(s);

                    string result = String.Empty;
                    int p = s.IndexOf("id=result_box");
                    if (p > 0)
                        s = "<span " + s.Substring(p, s.Length - p);
                    p = s.IndexOf("</div>");
                    if (p > 0)
                    {
                        s = s.Substring(0, p);
                        s = s.Replace("<br>", "¦");
                        s = HttpUtility.HtmlDecode(s);
                        result = Regex.Replace(s, @"<[^>]*>", String.Empty);
                    }
                    if (result.Length > 0)
                    {
                        string[] ms = result
                            .Replace("\r", string.Empty)
                            .Replace("\n", string.Empty)
                            .Split('¦');
                        if (ms.Length == ws.Length)
                        {
                            for (int i = 0; i < ms.Length; i++)
                            {
                                if (!dicWordMeanVi.ContainsKey(ws[i]))
                                {
                                    dicWordMeanVi.TryAdd(ws[i], ms[i]);
                                }
                            }

                            f_word_mean_vi_writeFile();

                            if (hasMultiPart)
                            {
                                f_word_MEDIA_KEY_WORD_TRANSLATER(m);
                            }
                            else
                            {
                                ///////////////////////////////////////////////////////////
                                ///////////////////////////////////////////////////////////
                                m.Output = new msgOutput()
                                {
                                    Ok = true,
                                };
                                m.Log = string.Format("Translate {0} words: {1}", ms.Length, "SUCCESSFULLY");
                                response_toMain(m);
                            }
                        }
                        else
                        {
                            m.Output = new msgOutput()
                            {
                                Ok = false,
                            };
                            m.Log = string.Format("Translate {0} words: {1}", ms.Length, "FAIL");
                            response_toMain(m);
                        }
                    }
                    else
                    {
                        m.Output = new msgOutput()
                        {
                            Ok = false,
                        };
                        m.Log = string.Format("Translate {0} words: {1}", ws.Length, "FAIL because cannot send request to API Google Translate");
                        response_toMain(m);
                    }
                }
                else
                {
                    m.Output = new msgOutput()
                    {
                        Ok = true,
                    };
                    m.Log = string.Format("All words exist in system. Translate successfully!");
                    response_toMain(m);
                }
            }
        }

        public static string f_word_meaning_Vi(string word_en)
        {
            if (dicWordMeanVi.ContainsKey(word_en))
            {
                string vi = string.Empty;
                if (dicWordMeanVi.TryGetValue(word_en, out vi))
                    return vi;
            }
            return string.Empty;
        }

        public static string f_word_speak_getURL(string word_en)
        {
            if (!string.IsNullOrEmpty(word_en))
            {
                if (word_en[word_en.Length - 1] == 's')
                    word_en = word_en.Substring(0, word_en.Length - 1);

                string url = string.Empty;
                //url = "https://s3.amazonaws.com/audio.oxforddictionaries.com/en/mp3/you_gb_1.mp3";
                //url = "https://ssl.gstatic.com/dictionary/static/sounds/oxford/you--_gb_1.mp3";
                //url = "https://ssl.gstatic.com/dictionary/static/sounds/20160317/you--_gb_1.mp3";

                url = string.Format("https://ssl.gstatic.com/dictionary/static/sounds/oxford/{0}--_gb_1.mp3", word_en);

                return url;
            }
            return string.Empty;
        }

        public static string[] f_word_speak_getURLs(string word_en)
        {
            if (!string.IsNullOrEmpty(word_en))
            {
                if (word_en[word_en.Length - 1] == 's')
                    word_en = word_en.Substring(0, word_en.Length - 1);
                List<string> urls = new List<string>();
                if (dicWordMp3.TryGetValue(word_en, out urls))
                {
                    return urls.ToArray();
                }
                else
                {
                    string url = string.Empty;
                    //url = "https://s3.amazonaws.com/audio.oxforddictionaries.com/en/mp3/you_gb_1.mp3";
                    //url = "https://ssl.gstatic.com/dictionary/static/sounds/oxford/you--_gb_1.mp3";
                    //url = "https://ssl.gstatic.com/dictionary/static/sounds/20160317/you--_gb_1.mp3"; 
                    url = string.Format("https://ssl.gstatic.com/dictionary/static/sounds/oxford/{0}--_gb_1.mp3", word_en);
                    return new string[] { url };
                }
            }
            return new string[] { };
        }

        private static string f_word_speak_getPronunciationFromCambridge(string word_en)
        {
            string requestUri = string.Format("https://dictionary.cambridge.org/dictionary/english/{0}", word_en);
            using (var response = web_word_pronunciation.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead).Result)
            {
                response.EnsureSuccessStatusCode();
                string htm = response.Content.ReadAsStringAsync().Result;
                if (htm.Contains(@"<span class=""ipa"">"))
                    return htm.Split(new string[] { @"<span class=""ipa"">" }, StringSplitOptions.None)[1].Split('<')[0].Trim();
            }

            return string.Empty;
        }

        private static string f_word_speak_getPronunciationFromOxford(string word_en, bool has_update_file_if_new)
        {
            if (word_en[word_en.Length - 1] == 's') word_en = word_en.Substring(0, word_en.Length - 1);
            string mean_en = string.Empty;

            string requestUri = string.Format("https://www.oxfordlearnersdictionaries.com/definition/english/{0}?q={0}", word_en);
            using (var response = web_word_pronunciation.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead).Result)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return string.Empty;

                response.EnsureSuccessStatusCode();
                string htm = response.Content.ReadAsStringAsync().Result,
                    pro = string.Empty, type = string.Empty;
                mean_en = word_en.ToUpper();

                HtmlNode nodes = f_word_speak_getPronunciationFromOxford_Nodes(htm);
                pro = nodes.QuerySelectorAll("span[class=\"phon\"]").Select(x => x.InnerText).Where(x => !string.IsNullOrEmpty(x)).Take(1).SingleOrDefault();
                type = nodes.QuerySelectorAll("span[class=\"pos\"]").Select(x => x.InnerText).Where(x => !string.IsNullOrEmpty(x)).Take(1).SingleOrDefault();
                string[] pro_s = nodes.QuerySelectorAll("span[class=\"vp-g\"]").Select(x => x.InnerText).Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => x.Replace(" BrE BrE", " = UK: ").Replace("; NAmE NAmE", "US: ").Replace("//", "/")).ToArray();
                string[] word_links = pro_s.Select(x => x.Split('=')[0].Trim()).ToArray();
                if (pro == null) pro = string.Empty;

                if (type != null && type.Length > 0)
                    mean_en += " (" + type + ")";

                if (!string.IsNullOrEmpty(pro))
                {
                    if (pro.StartsWith("BrE")) pro = pro.Substring(3).Trim();
                    pro = pro.Replace("//", "/");
                }

                List<string> ls_Verb_Group = new List<string>();
                var wgs = nodes.QuerySelectorAll("span[class=\"vp\"]").Select(x => x.InnerText_NewLine).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                foreach (string wi in wgs)
                {
                    string[] a = wi.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    ls_Verb_Group.Add(a[a.Length - 1]);
                }
                if (ls_Verb_Group.Count > 0)
                    mean_en += heading_text + "REF: " + string.Join("; ", ls_Verb_Group.ToArray());


                if (word_links.Length > 0)
                    mean_en += "\r\n" + string.Join(Environment.NewLine, word_links).Replace("-ing", "V-ing").Trim();

                string[] mp3 = nodes.QuerySelectorAll("div[data-src-mp3]")
                    .Select(x => x.GetAttributeValue("data-src-mp3", string.Empty))
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToArray();
                if (mp3.Length > 0)
                {
                    mean_en += "\r\n{\r\n" + string.Join(Environment.NewLine, mp3) + "\r\n}\r\n";

                    List<string> lss = new List<string>();
                    bool has = dicWordMp3.TryGetValue(word_en, out lss);
                    if (lss == null) lss = new List<string>();

                    lss.AddRange(mp3);
                    lss = lss.Distinct().ToList();
                    if (has)
                        dicWordMp3[word_en] = lss;
                    else
                        dicWordMp3.TryAdd(word_en, lss);
                }

                string[] uns = nodes.QuerySelectorAll("span[class=\"un\"]").Select(x => x.InnerText_NewLine).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                string[] idoms = nodes.QuerySelectorAll("span[class=\"idm-g\"]").Select(x => x.InnerText_NewLine).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                string[] defines = nodes.QuerySelectorAll("li[class=\"sn-g\"]").Select(x => x.InnerText_NewLine).Where(x => !string.IsNullOrEmpty(x)).ToArray();

                if (defines.Length > 0)
                {
                    mean_en += heading_text + "DEFINE:\r\n" +
                        string.Join(Environment.NewLine,
                            string.Join(Environment.NewLine, defines)
                                .Split(new char[] { '\r', '\n' })
                                .Select(x => x.Replace(".", ".\r\n").Trim())
                                .Where(x => x.Length > 0)
                                .ToArray())
                                .Replace("\r\n[", ". ")
                                .Replace("]", ":")

                                .Replace("1\r\n", "\r\n- ")
                                .Replace("2\r\n", "\r\n- ")
                                .Replace("3\r\n", "\r\n- ")
                                .Replace("4\r\n", "\r\n- ")
                                .Replace("5\r\n", "\r\n- ")
                                .Replace("6\r\n", "\r\n- ")
                                .Replace("7\r\n", "\r\n- ")
                                .Replace("8\r\n", "\r\n- ")
                                .Replace("9\r\n", "\r\n- ")

                                .Replace("1.", "\r\n+ ")
                                .Replace("2.", "\r\n+ ")
                                .Replace("3.", "\r\n+ ")
                                .Replace("4.", "\r\n+ ")
                                .Replace("5.", "\r\n+ ")
                                .Replace("6.", "\r\n+ ")
                                .Replace("7.", "\r\n+ ")
                                .Replace("8.", "\r\n+ ")
                                .Replace("9.", "\r\n+ ");
                }

                if (uns.Length > 0)
                    mean_en += heading_text + "NOTE:\r\n" + string.Join(Environment.NewLine, string.Join(Environment.NewLine, uns).Split(new char[] { '\r', '\n' }).Select(x => x.Replace(".", ".\r\n").Trim()).Where(x => x.Length > 0).ToArray());

                if (idoms.Length > 0)
                    mean_en += heading_text + "IDOM:\r\n" + string.Join(Environment.NewLine, string.Join(Environment.NewLine, idoms).Split(new char[] { '\r', '\n' }).Select(x => x.Replace(".", ".\r\n").Trim()).Where(x => x.Length > 0).ToArray());

                mean_en = Regex.Replace(mean_en, "[ ]{2,}", " ").Replace("\r\n’", "’");

                mean_en = string.Join(Environment.NewLine,
                    mean_en.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(x => x.Trim())
                    .Select(x => x.Length > 0 ?
                        (
                            (x[0] == '+' || x[0] == '-') ?
                                (x[0].ToString() + " " + x[2].ToString().ToUpper() + x.Substring(3))
                                    : (x[0].ToString().ToUpper() + x.Substring(1))
                        ) : x)
                    .ToArray());

                string[] sens = nodes.QuerySelectorAll("span[class=\"x\"]")
                    .Where(x => !string.IsNullOrEmpty(x.InnerText))
                    .Select(x => x.InnerText.Trim())
                    .Where(x => x.Length > 0)
                    .Select(x => "- " + x)
                    .ToArray();
                if (sens.Length > 0)
                {
                    string sen_text = string.Join(Environment.NewLine, sens);
                    mean_en += heading_text + "EXAMPLE:\r\n" + sen_text;
                }

                mean_en = mean_en.Replace("See full entry", string.Empty).Replace(Environment.NewLine, "|")
                    .Replace("’ ", @""" ").Replace(".’", @".""").Replace("’|", @"""|")
                    .Replace(" ‘", @" """)
                    .Replace("’", @"'");

                mean_en = Regex.Replace(mean_en, @"[^\x20-\x7E]", string.Empty);

                mean_en = mean_en.Replace("|", Environment.NewLine);
                //mean_en = Regex.Replace(mean_en, @"[^0-9a-zA-Z;,|{}():/'#+-._\r\n]+!\?", " ");
                mean_en = Regex.Replace(mean_en, "[ ]{2,}", " ");

                new Thread(new ParameterizedThreadStart((object obj) =>
                {
                    Tuple<string, string> it = (Tuple<string, string>)obj;
                    f_word_write_textFile(it.Item1, it.Item2);
                })).Start(new Tuple<string, string>(word_en, mean_en));

                return mean_en;
            }
        }

        private static HtmlNode f_word_speak_getPronunciationFromOxford_Nodes(string s)
        {
            s = Regex.Replace(s, @"<script[^>]*>[\s\S]*?</script>", string.Empty);
            s = Regex.Replace(s, @"<style[^>]*>[\s\S]*?</style>", string.Empty);
            s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            s = Regex.Replace(s, @"(?s)(?<=<!--).+?(?=-->)", string.Empty).Replace("<!---->", string.Empty);

            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|meta|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|meta|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Load the document using HTMLAgilityPack as normal
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(s);

            return doc.DocumentNode;//
        }

        public static string f_word_speak_getPronunciation(string word_en, bool has_update_file_if_new = false)
        {
            if (File.Exists("words/" + word_en + ".txt"))
            {
                return File.ReadAllText("words/" + word_en + ".txt");
            }

            string pro = string.Empty;
            if (dicWordPronunciation.ContainsKey(word_en))
            {
                if (dicWordPronunciation.TryGetValue(word_en, out pro))
                    return pro;
            }
            else
            {
                //pro = f_word_speak_getPronunciationFromCambridge(word_en, has_update_file_if_new);
                pro = f_word_speak_getPronunciationFromOxford(word_en, has_update_file_if_new);
                if (!string.IsNullOrEmpty(pro))
                {
                    dicWordPronunciation.TryAdd(word_en, pro);
                    if (has_update_file_if_new)
                        new Thread(new ThreadStart(() => { f_word_pronunciation_writeFile(); })).Start();
                }
            }
            return pro;
        }

        private static void f_word_write_textFile(string word_en, string content)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "words");
            if (Directory.Exists(dir) == false) Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, word_en + ".txt");

            using (var sw = new StreamWriter(File.Open(file, FileMode.OpenOrCreate), Encoding.ASCII))
                sw.WriteLine(content);
        }

        #endregion

        #region [ MEDIA ]

        static readonly string file_media = Path.Combine(path_data, "media.bin");
        static ConcurrentDictionary<long, oMedia> dicMediaStore = null;
        static ConcurrentDictionary<long, Bitmap> dicMediaImage = null;


        public static oMedia f_media_getInfo(long mediaId)
        {
            oMedia m = null;
            dicMediaStore.TryGetValue(mediaId, out m);
            return m;
        }

        public static bool f_media_Exist(long mediaId)
        {
            return dicMediaStore.ContainsKey(mediaId);
        }

        public static string f_media_getTitle(long mediaId)
        {
            string title = string.Empty;
            oMedia m = null;
            dicMediaStore.TryGetValue(mediaId, out m);
            if (m != null && !string.IsNullOrEmpty(m.Title)) title = m.Title;
            return title;
        }

        public static string f_media_getText(long mediaId)
        {
            string text = string.Empty;
            oMedia m = null;
            dicMediaStore.TryGetValue(mediaId, out m);
            if (m != null) text = m.Text;
            return text;
        }

        public static string[] f_media_getSentences(long mediaId)
        {
            oMedia m = null;
            dicMediaStore.TryGetValue(mediaId, out m);
            if (m != null)
                return m.SubtileEnglish_Sentence;
            return new string[] { };
        }

        public static string[] f_media_getSentencesByWord(long mediaId, string word)
        {
            oMedia m = null;
            dicMediaStore.TryGetValue(mediaId, out m);

            if (m != null)
                return m.SubtileEnglish_Sentence
                    .Where(x => x.Contains(word))
                    .ToArray();

            return new string[] { };
        }

        public static oWordCount[] f_media_getWords(long mediaId)
        {
            oMedia m = null;
            dicMediaStore.TryGetValue(mediaId, out m);
            if (m != null && !string.IsNullOrEmpty(m.Text))
                return m.Words.ToArray();
            return new oWordCount[] { };
        }

        public static string f_media_getYoutubeID(long mediaId)
        {
            string url = string.Empty;
            oMedia p = null;
            if (dicMediaStore.TryGetValue(mediaId, out p) && p != null && p.Paths != null)
                return p.Paths.YoutubeID;
            return string.Empty;
        }

        public static string f_media_fetchUriSource(long mediaId, MEDIA_TYPE type)
        {
            string url = string.Empty;
            oMedia p = null;
            if (dicMediaStore.TryGetValue(mediaId, out p) && p != null && p.Paths != null)
            {
                if (!string.IsNullOrEmpty(p.Paths.YoutubeID))
                {
                    switch (type)
                    {
                        case MEDIA_TYPE.MP4:
                            #region
                            if (!string.IsNullOrEmpty(p.Paths.PathMp4_Youtube))
                            {
                                url = p.Paths.PathMp4_Youtube;
                            }
                            else
                            {
                                var _client = new YoutubeClient();
                                //var Video = _client.GetVideoAsync(videoId);
                                //var Channel = _client.GetVideoAuthorChannelAsync(videoId);
                                var ms = _client.GetVideoMediaStreamInfosAsync(p.Paths.YoutubeID);
                                var ms_video = ms.Muxed.Where(x => x.Container == Container.Mp4).Take(1).SingleOrDefault();
                                var ms_audio = ms.Audio.Where(x => x.Container == Container.M4A).Take(1).SingleOrDefault();

                                if (ms_video != null)
                                {
                                    p.Paths.PathMp4_Youtube = ms_video.Url;
                                    url = p.Paths.PathMp4_Youtube;
                                }

                                if (ms_audio != null)
                                {
                                    p.Paths.PathMp3_Youtube = ms_audio.Url;
                                }
                            }
                            #endregion
                            break;
                        case MEDIA_TYPE.M4A:
                            #region
                            if (!string.IsNullOrEmpty(p.Paths.PathMp3_Youtube))
                            {
                                url = p.Paths.PathMp3_Youtube;
                            }
                            else
                            {
                                var _client = new YoutubeClient();
                                //var Video = _client.GetVideoAsync(videoId);
                                //var Channel = _client.GetVideoAuthorChannelAsync(videoId);
                                var ms = _client.GetVideoMediaStreamInfosAsync(p.Paths.YoutubeID);
                                var ms_video = ms.Muxed.Where(x => x.Container == Container.Mp4).Take(1).SingleOrDefault();
                                var ms_audio = ms.Audio.Where(x => x.Container == Container.M4A).Take(1).SingleOrDefault();

                                if (ms_video != null)
                                {
                                    p.Paths.PathMp4_Youtube = ms_video.Url;
                                }

                                if (ms_audio != null)
                                {
                                    p.Paths.PathMp3_Youtube = ms_audio.Url;
                                    url = p.Paths.PathMp3_Youtube;
                                }
                            }
                            #endregion
                            break;
                        case MEDIA_TYPE.MP3:
                            break;
                    }
                }
            }
            return url;
        }

        private void f_media_writeFile()
        {
            using (var file = File.OpenWrite(file_media))
            {
                Serializer.Serialize<ConcurrentDictionary<long, oMedia>>(file, dicMediaStore);
                file.Close();
            }
        }

        public static Bitmap f_image_getCache(long mediaId)
        {
            Bitmap bitmap = null;
            if (dicMediaImage.TryGetValue(mediaId, out bitmap) && bitmap != null)
            {
                return bitmap;
            }
            else
            {
                oMedia m = null;
                if (dicMediaStore.TryGetValue(mediaId, out m) && m != null)
                {
                    string imageUrl = string.Format("https://img.youtube.com/vi/{0}/default.jpg", m.Paths.YoutubeID);
                    try
                    {
                        WebClient client = new WebClient();
                        Stream stream = client.OpenRead(imageUrl);
                        bitmap = new Bitmap(stream);
                        //if (bitmap != null) bitmap.Save(filename, ImageFormat.Jpeg); 
                        //stream.Flush();
                        stream.Close();
                        client.Dispose();
                        dicMediaImage.TryAdd(mediaId, bitmap);
                    }
                    catch { }
                }
            }
            return bitmap;
        }

        #endregion

        #region [ SEARCH ]

        static ConcurrentDictionary<long, oMedia> dicMediaSearch = null;
        static bool stop_search = false;
        static AutoResetEvent wait_search = null;

        public static oMedia f_search_getInfo(long mediaId)
        {
            oMedia m = null;
            dicMediaSearch.TryGetValue(mediaId, out m);
            return m;
        }

        public static Bitmap f_search_getPhoto(long mediaId)
        {
            Bitmap m = null;
            dicMediaImage.TryGetValue(mediaId, out m);
            return m;
        }

        public static string f_search_getYoutubeID(long mediaId)
        {
            string url = string.Empty;
            oMedia p = null;
            if (dicMediaSearch.TryGetValue(mediaId, out p) && p != null)
                return p.Paths.YoutubeID;
            return string.Empty;
        }

        public static string f_search_fetchUriSource(long mediaId, MEDIA_TYPE type)
        {
            string url = string.Empty;
            oMedia p = null;
            if (dicMediaSearch.TryGetValue(mediaId, out p) && p != null && p.Paths != null)
            {
                if (!string.IsNullOrEmpty(p.Paths.YoutubeID))
                {
                    switch (type)
                    {
                        case MEDIA_TYPE.MP4:
                            #region
                            if (!string.IsNullOrEmpty(p.Paths.PathMp4_Youtube))
                            {
                                url = p.Paths.PathMp4_Youtube;
                            }
                            else
                            {
                                var _client = new YoutubeClient();
                                //var Video = _client.GetVideoAsync(videoId);
                                //var Channel = _client.GetVideoAuthorChannelAsync(videoId);
                                var ms = _client.GetVideoMediaStreamInfosAsync(p.Paths.YoutubeID);
                                var ms_video = ms.Muxed.Where(x => x.Container == Container.Mp4).Take(1).SingleOrDefault();
                                var ms_audio = ms.Audio.Where(x => x.Container == Container.M4A).Take(1).SingleOrDefault();

                                if (ms_video != null)
                                {
                                    p.Paths.PathMp4_Youtube = ms_video.Url;
                                    url = p.Paths.PathMp4_Youtube;
                                }

                                if (ms_audio != null)
                                {
                                    p.Paths.PathMp3_Youtube = ms_audio.Url;
                                }
                            }
                            #endregion
                            break;
                        case MEDIA_TYPE.M4A:
                            #region
                            if (!string.IsNullOrEmpty(p.Paths.PathMp3_Youtube))
                            {
                                url = p.Paths.PathMp3_Youtube;
                            }
                            else
                            {
                                var _client = new YoutubeClient();
                                //var Video = _client.GetVideoAsync(videoId);
                                //var Channel = _client.GetVideoAuthorChannelAsync(videoId);
                                var ms = _client.GetVideoMediaStreamInfosAsync(p.Paths.YoutubeID);
                                var ms_video = ms.Muxed.Where(x => x.Container == Container.Mp4).Take(1).SingleOrDefault();
                                var ms_audio = ms.Audio.Where(x => x.Container == Container.M4A).Take(1).SingleOrDefault();

                                if (ms_video != null)
                                {
                                    p.Paths.PathMp4_Youtube = ms_video.Url;
                                }

                                if (ms_audio != null)
                                {
                                    p.Paths.PathMp3_Youtube = ms_audio.Url;
                                    url = p.Paths.PathMp3_Youtube;
                                }
                            }
                            #endregion
                            break;
                        case MEDIA_TYPE.MP3:
                            break;
                    }
                }
            }
            return url;
        }

        static WebClient client_img = new WebClient();
        long[] f_media_searchYoutubeOnline(YoutubeClient _client, string query, int page)
        {
            string url = string.Format("https://www.youtube.com/search_ajax?style=json&search_query={0}&page={1}&hl=en", query, page);
            if (SearchMediaCaptionCC) url += "&sp=EgIoAQ%253D%253D";

            string raw = _client.GetStringAsync(url, false);

            // Get search results
            var searchResultsJson = JToken.Parse(raw);

            // Get videos
            var videosJson = searchResultsJson["video"].EmptyIfNull().ToArray();

            // Break if there are no videos
            if (!videosJson.Any()) return new long[] { };

            // Get all videos across pages
            var videos = new List<long>();

            // Parse videos
            foreach (var videoJson in videosJson)
            {
                // Basic info
                string videoId = videoJson["encrypted_id"].Value<string>();
                oMedia mi = new oMedia(videoId);
                mi.Paths.YoutubeID = videoId;

                if (!dicMediaStore.ContainsKey(mi.Id) && !dicMediaSearch.ContainsKey(mi.Id))
                {
                    List<ClosedCaptionTrackInfo> cap = new List<ClosedCaptionTrackInfo>();

                    if (SearchMediaCaptionCC)
                        cap = _client.GetVideoClosedCaptionTrackInfosAsync(videoId);

                    if (SearchMediaCaptionCC == false || (SearchMediaCaptionCC && cap.Count > 0))
                    {
                        ClosedCaptionTrackInfo cen = null;

                        if (SearchMediaCaptionCC)
                            cen = cap.Where(x => x.Language.Code == "en").Take(1).SingleOrDefault();

                        if (SearchMediaCaptionCC == false || (SearchMediaCaptionCC == true && cen != null))
                        {
                            mi.Tags.Add(query);

                            if (SearchMediaCaptionCC == true && cen != null)
                                mi.SubtileEnglish = _client.GetStringAsync(cen.Url);

                            var videoAuthor = videoJson["author"].Value<string>();
                            //var videoUploadDate = videoJson["added"].Value<string>().ParseDateTimeOffset("M/d/yy");
                            var videoUploadDate = videoJson["added"].Value<string>().ParseDateTimeOffset();
                            var videoTitle = videoJson["title"].Value<string>();
                            var videoDuration = TimeSpan.FromSeconds(videoJson["length_seconds"].Value<double>());
                            var videoDescription = videoJson["description"].Value<string>().HtmlDecode();

                            // Keywords
                            var videoKeywordsJoined = videoJson["keywords"].Value<string>();
                            var videoKeywords = Regex
                                .Matches(videoKeywordsJoined, @"(?<=(^|\s)(?<q>""?))([^""]|(""""))*?(?=\<q>(?=\s|$))")
                                .Cast<Match>()
                                .Select(m => m.Value)
                                .Where(s => s.IsNotBlank())
                                .ToList();

                            //// Statistics
                            //var videoViewCount = videoJson["views"].Value<string>().StripNonDigit().ParseLong();
                            //var videoLikeCount = videoJson["likes"].Value<long>();
                            //var videoDislikeCount = videoJson["dislikes"].Value<long>();
                            //var videoStatistics = new Statistics(videoViewCount, videoLikeCount, videoDislikeCount);

                            mi.ViewCount = videoJson["views"].Value<string>().StripNonDigit().ParseLong();

                            mi.DurationSecond = (int)videoDuration.TotalSeconds;
                            mi.Title = videoTitle;
                            mi.Description = videoDescription;
                            mi.Keywords = videoKeywords;
                            mi.Author = videoAuthor;
                            mi.UploadDate = int.Parse(videoUploadDate.ToString("yyMMdd"));

                            if (!dicMediaImage.ContainsKey(mi.Id))
                            {
                                using (Stream stream = client_img.OpenRead(string.Format("https://img.youtube.com/vi/{0}/default.jpg", videoId)))
                                {
                                    Bitmap bitmap = new Bitmap(stream);
                                    dicMediaImage.TryAdd(mi.Id, bitmap);
                                }
                            }

                            videos.Add(mi.Id);

                            dicMediaSearch.TryAdd(mi.Id, mi);
                        }
                    }
                }
            }

            return videos.ToArray();
        }

        #endregion

        #region [ SUBTITLE - CC ]

        public static List<oCaptionWord> f_analytic_wordFileXml(string file_xml)
        {
            XDocument xdoc = XDocument.Load(file_xml);
            List<oCaptionWord> listWord = new List<oCaptionWord>();
            foreach (var p in xdoc.Descendants("p"))
            {
                var its = p.Descendants("s").Select(x => new oCaptionWord(x)).ToArray();
                if (its.Length > 0)
                {
                    int tt = 0, dd = 0;
                    string t = p.Attribute("t").Value, d = p.Attribute("d").Value;
                    if (!string.IsNullOrEmpty(t)) int.TryParse(t, out tt);
                    if (!string.IsNullOrEmpty(d)) int.TryParse(d, out dd);
                    foreach (var it in its) it.TimeStart += tt;
                    listWord.AddRange(its);
                }
            }

            return listWord;
        }

        public static List<oCaptionSentence> f_render_Sentence(List<oCaptionWord> listWord)
        {
            List<oCaptionSentence> listSen = new List<oCaptionSentence>();
            oCaptionWord ci = null;
            oCaptionSentence si = new oCaptionSentence();
            string wi = string.Empty, wii = string.Empty;
            for (var i = 0; i < listWord.Count; i++)
            {
                ci = listWord[i];
                wi = ci.Word.Trim().ToLower();

                if (i == 0)
                {
                    si = new oCaptionSentence();
                    si.TimeStart = ci.TimeStart;
                    si.ListIndex.Add(i);
                    continue;
                }

                if (wi == "i" || wi == "we" || wi == "you" || wi == "they" || wi == "he" || wi == "she" || wi == "it"
                    || wi == "i'm" || wi == "we're" || wi == "you're" || wi == "they're" || wi == "he's" || wi == "she's" || wi == "it's"
                    || wi == "how" || wi == "where" || wi == "what" || wi == "whom" || wi == "who" || wi == "which")
                {
                    bool sub = false;
                    wii = listWord[i - 1].Word.ToLower();
                    if (i > 0 &&
                        (wii == "so" || wii == "and" || wii == "if" || wii == "when" || wii == "because"))
                    {
                        sub = true;
                        si.ListIndex.RemoveAt(si.ListIndex.Count - 1);
                    }

                    var ws = listWord.Where((x, id) => si.ListIndex.Any(y => y == id)).Select(x => x.Word).ToArray();
                    si.Words = string.Join(" ", ws);
                    listSen.Add(si);

                    si = new oCaptionSentence();
                    si.TimeStart = ci.TimeStart;
                    if (sub) si.ListIndex.Add(i - 1);
                    si.ListIndex.Add(i);
                }
                else
                {
                    si.ListIndex.Add(i);
                }
            }

            return listSen;
        }

        #endregion

        #region [ PROXY ]

        static int m_port = 0;
        static HttpListener m_listener;
        static bool m_running = true;

        void f_proxy_Start()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            m_port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();

            m_listener = new HttpListener();
            m_listener.Prefixes.Add("http://*:" + m_port + "/");
            m_listener.Start();
            //Console.WriteLine("Listening...");

            new Thread(new ParameterizedThreadStart((object lis) =>
            {
                HttpListener listener = (HttpListener)lis;
                while (m_running)
                {
                    try
                    {
                        var ctx = listener.GetContext();
                        string rawUrl = ctx.Request.RawUrl;
                        if (rawUrl == "/crossdomain.xml")
                        {
                            ctx.Response.ContentType = "text/xml";
                            string xml =
        @"<cross-domain-policy>
    <allow-access-from domain=""*.*"" headers=""SOAPAction""/>
    <allow-http-request-headers-from domain=""*.*"" headers=""SOAPAction""/> 
    <site-control permitted-cross-domain-policies=""master-only""/>
</cross-domain-policy>";
                            xml =
        @"<?xml version=""1.0""?>
<!DOCTYPE cross-domain-policy SYSTEM ""http://www.macromedia.com/xml/dtds/cross-domain-policy.dtd"">
<cross-domain-policy>
  <allow-access-from domain=""*"" />
</cross-domain-policy>";
                            byte[] bytes = Encoding.UTF8.GetBytes(xml);
                            ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                            ctx.Response.OutputStream.Flush();
                            ctx.Response.OutputStream.Close();
                            ctx.Response.Close();
                        }
                        else
                        {
                            if (rawUrl.Contains("?crawler_key=")) {
                                string crawler_key = ctx.Request.QueryString["crawler_key"];
                                if (!string.IsNullOrEmpty(crawler_key)) {
                                    crawler_key = HttpUtility.UrlDecode(crawler_key);
                                    string htm = fMain.f_brow_offline_getHTMLByUrl(crawler_key);
                                    byte[] bytes = Encoding.UTF8.GetBytes(htm);
                                    ctx.Response.ContentType = "text/html; charset=utf-8";
                                    //ctx.Response.ContentEncoding = Encoding.UTF8;
                                    ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                                    ctx.Response.OutputStream.Flush();
                                    ctx.Response.OutputStream.Close();
                                    ctx.Response.Close();
                                }
                            }
                            else
                            {
                                string key = ctx.Request.QueryString["key"];
                                if (string.IsNullOrEmpty(key))
                                {
                                    byte[] bytes = Encoding.UTF8.GetBytes(string.Format("Cannot find key: /?key=???"));
                                    ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                                    ctx.Response.OutputStream.Flush();
                                    ctx.Response.OutputStream.Close();
                                    ctx.Response.Close();
                                }
                                else
                                    new Thread(new Relay(ctx).ProcessRequest).Start();
                            }
                        }
                    }
                    catch { }
                }
            })).Start(m_listener);
        }

        string f_proxy_getUriProxy(long mediaId, MEDIA_TYPE type)
        {
            string key = string.Format("{0}{1}", type, mediaId);
            string url = string.Format("http://localhost:{0}/?key={1}", m_port, key);
            return url;
        }

        void proxy_Close()
        {
            m_listener.Stop();
            m_running = false;
            Thread.Sleep(10);
        }

        public static string f_proxy_getHost() {
            return string.Format("http://localhost:{0}/", m_port);
        }

        #endregion

        #region [ ARTICLE ]

        static readonly Dictionary<string, string> dicArticleAnalytic = new Dictionary<string, string>() {
            { "dictionary.cambridge.org", "article h1|article div.content" }
        };

        public static string f_article_analytic_HTML(string url, string htm)
        {
            //string s = File.ReadAllText("demo.html");
            Uri uri = new Uri(url);
            if (dicArticleAnalytic.ContainsKey(uri.Host))
            {
                string[] sels = dicArticleAnalytic[uri.Host].Split('|');
                if (sels.Length > 0)
                {
                    string rs = get_analytic_Content(htm, sels[0], sels[1]);
                    return rs;
                }
            }
            return string.Empty;
        }

        private static string get_analytic_Content(string s, string selector_H1, string selector_Content)
        {
            string si = string.Empty;
            s = Regex.Replace(s, @"<script[^>]*>[\s\S]*?</script>", string.Empty);
            s = Regex.Replace(s, @"<style[^>]*>[\s\S]*?</style>", string.Empty);
            s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            s = Regex.Replace(s, @"(?s)(?<=<!--).+?(?=-->)", string.Empty).Replace("<!---->", string.Empty);

            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            //s = Regex.Replace(s, @"<noscript[^>]*>[\s\S]*?</noscript>", string.Empty);
            s = Regex.Replace(s, @"</?(?i:embed|object|frameset|frame|iframe|meta|link)(.|\n|\s)*?>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(s);
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

            var nodes = doc.DocumentNode;
            var h1 = nodes.QuerySelector(selector_H1);
            var con = nodes.QuerySelector(selector_Content);
            if (h1 != null && con != null)
            {
                string tit = h1.InnerText, text = get_textEnglish(con);
                return "[*] " + tit + Environment.NewLine + Environment.NewLine + text;
            }

            //si = doc.DocumentNode.OuterHtml;
            ////string[] lines = si.Split(new char[] { '\r', '\n' }, StringSplitOptions.None).Where(x => x.Trim().Length > 0).ToArray();
            //string[] lines = si.Split(new char[] { '\r', '\n' }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
            //si = string.Join(Environment.NewLine, lines);
            //return si;

            return string.Empty;
        }

        private static string get_textEnglish(HtmlNode root)
        {
            if (root.HasChildNodes == false) return root.InnerText;

            string si = string.Empty, s = string.Empty, head_Heading = "\r\n[+] ", head_LI = "\r\n- ";

            foreach (HtmlNode node in root.ChildNodes)
            {
                si = string.Empty;
                if (!string.IsNullOrEmpty(node.InnerText) && node.InnerText.Trim().Length > 0)
                {
                    switch (node.Name)
                    {
                        case "div":
                            if (node.HasChildNodes)
                            {
                                foreach (HtmlNode node2 in node.ChildNodes)
                                {
                                    if (!string.IsNullOrEmpty(node2.InnerText) && node2.InnerText.Trim().Length > 0)
                                    {
                                        switch (node2.Name)
                                        {
                                            case "div":
                                                foreach (HtmlNode node3 in node2.ChildNodes)
                                                {
                                                    if (!string.IsNullOrEmpty(node3.InnerText) && node3.InnerText.Trim().Length > 0)
                                                    {
                                                        switch (node3.Name)
                                                        {
                                                            case "div":
                                                                foreach (HtmlNode node4 in node3.ChildNodes)
                                                                {
                                                                    if (!string.IsNullOrEmpty(node4.InnerText) && node4.InnerText.Trim().Length > 0)
                                                                    {
                                                                        switch (node4.Name)
                                                                        {
                                                                            case "div":
                                                                                foreach (HtmlNode node5 in node4.ChildNodes)
                                                                                {
                                                                                    if (!string.IsNullOrEmpty(node5.InnerText) && node5.InnerText.Trim().Length > 0)
                                                                                    {
                                                                                        switch (node5.Name)
                                                                                        {
                                                                                            case "div":
                                                                                                si += Environment.NewLine + Environment.NewLine + node5.InnerText.Trim() + Environment.NewLine;
                                                                                                break;
                                                                                            case "h1":
                                                                                            case "h2":
                                                                                            case "h3":
                                                                                            case "h4":
                                                                                            case "h5":
                                                                                            case "h6":
                                                                                                si += Environment.NewLine + head_Heading + node5.InnerText.Trim() + Environment.NewLine;
                                                                                                break;
                                                                                            case "p":
                                                                                                si += Environment.NewLine + node5.InnerText.Trim() + Environment.NewLine;
                                                                                                break;
                                                                                            case "ul":
                                                                                                si += string.Join(string.Empty, node5.ChildNodes.Select(x => head_LI + x.InnerText.Trim()).ToArray());
                                                                                                break;
                                                                                            default:
                                                                                                si += Environment.NewLine + Environment.NewLine + node5.InnerText.Trim();
                                                                                                break;
                                                                                        }
                                                                                        if (si.Length > 0 && si[si.Length - 1] == ':')
                                                                                            si += Environment.NewLine;
                                                                                    }
                                                                                }
                                                                                // tag DIV must be create new line
                                                                                si += Environment.NewLine;
                                                                                break;
                                                                            case "h1":
                                                                            case "h2":
                                                                            case "h3":
                                                                            case "h4":
                                                                            case "h5":
                                                                            case "h6":
                                                                                si += Environment.NewLine + head_Heading + node4.InnerText.Trim() + Environment.NewLine;
                                                                                break;
                                                                            case "p":
                                                                                si += Environment.NewLine + node4.InnerText.Trim() + Environment.NewLine;
                                                                                break;
                                                                            case "ul":
                                                                                si += string.Join(string.Empty, node4.ChildNodes.Select(x => head_LI + x.InnerText.Trim()).ToArray());
                                                                                break;
                                                                            default:
                                                                                si += Environment.NewLine + Environment.NewLine + node4.InnerText.Trim();
                                                                                break;
                                                                        }
                                                                        if (si.Length > 0 && si[si.Length - 1] == ':')
                                                                            si += Environment.NewLine;
                                                                    }
                                                                }
                                                                // tag DIV must be create new line
                                                                si += Environment.NewLine;
                                                                break;
                                                            case "h1":
                                                            case "h2":
                                                            case "h3":
                                                            case "h4":
                                                            case "h5":
                                                            case "h6":
                                                                si += Environment.NewLine + head_Heading + node3.InnerText.Trim() + Environment.NewLine;
                                                                break;
                                                            case "p":
                                                                si += Environment.NewLine + node3.InnerText.Trim() + Environment.NewLine;
                                                                break;
                                                            case "ul":
                                                                si += string.Join(string.Empty, node3.ChildNodes.Select(x => head_LI + x.InnerText.Trim()).ToArray());
                                                                break;
                                                            default:
                                                                si += Environment.NewLine + Environment.NewLine + node3.InnerText.Trim(); 
                                                                break;
                                                        }
                                                        if (si.Length > 0 && si[si.Length - 1] == ':')
                                                            si += Environment.NewLine;
                                                    }
                                                }
                                                // tag DIV must be create new line
                                                si += Environment.NewLine;
                                                break;
                                            case "h1":
                                            case "h2":
                                            case "h3":
                                            case "h4":
                                            case "h5":
                                            case "h6":
                                                si += Environment.NewLine + head_Heading + node2.InnerText.Trim() + Environment.NewLine;
                                                break;
                                            case "p":
                                                si += Environment.NewLine + node2.InnerText.Trim() + Environment.NewLine;
                                                break;
                                            case "ul":
                                                si += string.Join(string.Empty, node2.ChildNodes.Select(x => head_LI + x.InnerText.Trim()).ToArray());
                                                break;
                                            default:
                                                si += Environment.NewLine + Environment.NewLine + node2.InnerText.Trim(); 
                                                break;
                                        }
                                        if (si.Length > 0 && si[si.Length - 1] == ':')
                                            si += Environment.NewLine;
                                    }
                                }
                            }
                            else
                            {
                                si += Environment.NewLine + Environment.NewLine + node.InnerText.Trim();
                            }
                            // tag DIV must be create new line
                            si += Environment.NewLine;
                            break;
                        case "h1":
                        case "h2":
                        case "h3":
                        case "h4":
                        case "h5":
                        case "h6":
                            si += Environment.NewLine + head_Heading + node.InnerText.Trim() + Environment.NewLine;
                            break;
                        case "p":
                            si += Environment.NewLine + node.InnerText.Trim() + Environment.NewLine;
                            break;
                        case "ul":
                            si += string.Join(string.Empty, node.ChildNodes.Select(x => head_LI + x.InnerText.Trim()).ToArray());
                            break;
                        default:
                            si += Environment.NewLine + Environment.NewLine + node.InnerText.Trim();
                            break;
                    }
                    if (si.Length > 0 && si[si.Length - 1] == ':')
                        si += Environment.NewLine;
                    s += si;
                }
            }

            s = string.Join("." + Environment.NewLine, s.Split('.').Select(x => x.Trim()));
            s = string.Join("?" + Environment.NewLine, s.Split('?').Select(x => x.Trim()));

            s = s
                //.Replace(":", ":\r\n")
                //.Replace(".", ".\r\n")
                //.Replace("?", "?\r\n")
                .Replace("Not:", "\r\n[-] Not:")
                .Replace("Warning:", "\r\n[-] Warning:");

            s = Regex.Replace(s, @"[\r\n]{2,}", "\r\n");
            s = s.Replace(head_Heading, "\r\n" + head_Heading);

            return s;
        }

        #endregion
    }

    #region [ PROXY ]


    public class Relay
    {
        private readonly HttpListenerContext originalContext;

        public Relay(HttpListenerContext originalContext)
        {
            this.originalContext = originalContext;
        }

        public void ProcessRequest()
        {
            string rawUrl = originalContext.Request.RawUrl;
            string uri = string.Empty, key = string.Empty, type = "mp3";

            key = originalContext.Request.QueryString["key"];
            if (key.Length > 3)
            {
                type = key.Substring(0, 3).ToLower();
                key = key.Substring(3);
            }

            long mediaId = 0;
            long.TryParse(key, out mediaId);

            switch (type)
            {
                case "m4a":
                    uri = api_media.f_media_fetchUriSource(mediaId, MEDIA_TYPE.M4A);
                    break;
                case "mp4":
                    string online = originalContext.Request.QueryString["online"];
                    if (online == "true")
                        uri = api_media.f_search_fetchUriSource(mediaId, MEDIA_TYPE.MP4);
                    else
                        uri = api_media.f_media_fetchUriSource(mediaId, MEDIA_TYPE.MP4);
                    break;
                case "web": // webm
                    uri = api_media.f_media_fetchUriSource(mediaId, MEDIA_TYPE.WEB);
                    break;
                case "mp3":
                    uri = api_media.f_media_fetchUriSource(mediaId, MEDIA_TYPE.MP3);
                    break;
            }

            #region

            ////if (rawUrl == "/") rawUrl = "https://google.com.vn";  
            //if (rawUrl.Length > 3) type = rawUrl.Substring(rawUrl.Length - 3, 3).ToLower();
            //ConsoleUtilities.WriteRequest("Proxy receive a request for: " + rawUrl);

            //switch (type)
            //{
            //    case "m4a":
            //        uri = "https://r6---sn-jhjup-nbol.googlevideo.com/videoplayback?c=WEB&mn=sn-jhjup-nbol%2Csn-i3b7kn7d&mm=31%2C29&mv=m&mt=1526353247&signature=12D877BF10BDA8B8D9EB787F07C53C5E9B6BCDD2.CF50B6DE340882AC5453CBB0F10540B9D30F6E7B&ms=au%2Crdu&sparams=clen%2Cdur%2Cei%2Cgir%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Ckeepalive%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpcm2cms%2Cpl%2Crequiressl%2Csource%2Cexpire&ei=iU36WryBOpG-4gLMkY7YAw&ip=113.20.96.116&clen=3721767&keepalive=yes&id=o-AP0scLQEoW6xg_BDA4RnCp3bNCPg5y4hjvaLHhJnePWN&gir=yes&requiressl=yes&source=youtube&pcm2cms=yes&dur=234.289&pl=23&initcwndbps=463750&itag=140&ipbits=0&lmt=1510741503111392&expire=1526374890&key=yt6&mime=audio%2Fmp4&fvip=2";
            //        break;
            //    case "mp4":
            //        uri = "https://r6---sn-jhjup-nbol.googlevideo.com/videoplayback?key=yt6&signature=CD6655BD08EEDADA61255DE9638EADEBF9BC2DAB.640F4ED4573F543F7423F3C62699A7795A34C6AE&requiressl=yes&lmt=1510741625396835&source=youtube&dur=234.289&ipbits=0&c=WEB&initcwndbps=680000&mime=video%2Fmp4&pcm2cms=yes&sparams=dur%2Cei%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpcm2cms%2Cpl%2Cratebypass%2Crequiressl%2Csource%2Cexpire&id=o-AOpvYdf_hpR_jCGsytRQ4p_2uICpZxVqqewAyrpM1_U9&mm=31%2C29&mn=sn-jhjup-nbol%2Csn-i3beln7z&pl=23&ip=113.20.96.116&ei=mFD6WoeNDNOb4AK0squACA&ms=au%2Crdu&mt=1526354019&mv=m&ratebypass=yes&fvip=6&expire=1526375672&itag=22";
            //        break;
            //    case "ebm": // webm
            //        uri = "https://r6---sn-jhjup-nbol.googlevideo.com/videoplayback?gir=yes&key=yt6&signature=64933C7570840B48D0E3702A51200EF12DB71456.AA4398BD234730DA07841DAF7FDA6B7A2B341963&requiressl=yes&lmt=1510742527754463&source=youtube&dur=0.000&clen=15660856&ipbits=0&c=WEB&initcwndbps=680000&mime=video%2Fwebm&pcm2cms=yes&sparams=clen%2Cdur%2Cei%2Cgir%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpcm2cms%2Cpl%2Cratebypass%2Crequiressl%2Csource%2Cexpire&id=o-AOpvYdf_hpR_jCGsytRQ4p_2uICpZxVqqewAyrpM1_U9&mm=31%2C29&mn=sn-jhjup-nbol%2Csn-i3beln7z&pl=23&ip=113.20.96.116&ei=mFD6WoeNDNOb4AK0squACA&ms=au%2Crdu&mt=1526354019&mv=m&ratebypass=yes&fvip=6&expire=1526375672&itag=43";
            //        break;
            //    //case "mp3":
            //    default:
            //        uri = "https://drive.google.com/uc?export=download&id=1u2wJYTB-hVWeZOLLd9CxcA9KCLuEanYg";
            //        break;
            //}

            #endregion

            try
            {
                var relayRequest = (HttpWebRequest)WebRequest.Create(uri);
                relayRequest.KeepAlive = false;
                relayRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                relayRequest.UserAgent = this.originalContext.Request.UserAgent;
                var requestData = new RequestState(relayRequest, originalContext);

                switch (type)
                {
                    case "m4a":
                        requestData.context.Response.ContentType = "audio/x-m4a";
                        break;
                    case "mp4":
                        requestData.context.Response.ContentType = "video/mp4";
                        break;
                    case "web": // webm
                        requestData.context.Response.ContentType = "video/webm"; // audio/webm
                        break;
                    case "mp3":
                        requestData.context.Response.ContentType = "audio/mpeg";
                        break;
                }

                relayRequest.BeginGetResponse(ResponseCallBack, requestData);
            }
            catch { }
        }

        private static void ResponseCallBack(IAsyncResult asynchronousResult)
        {
            var requestData = (RequestState)asynchronousResult.AsyncState;
            ConsoleUtilities.WriteResponse("Proxy receive a response from " + requestData.context.Request.RawUrl);

            using (var responseFromWebSiteBeingRelayed = (HttpWebResponse)requestData.webRequest.EndGetResponse(asynchronousResult))
            {
                using (var responseStreamFromWebSiteBeingRelayed = responseFromWebSiteBeingRelayed.GetResponseStream())
                {
                    var originalResponse = requestData.context.Response;


                    if (responseFromWebSiteBeingRelayed.ContentType.Contains("text/html"))
                    {
                        var reader = new StreamReader(responseStreamFromWebSiteBeingRelayed);
                        string html = reader.ReadToEnd();
                        //Here can modify html
                        byte[] byteArray = System.Text.Encoding.Default.GetBytes(html);
                        var stream = new MemoryStream(byteArray);
                        stream.CopyTo(originalResponse.OutputStream);
                    }
                    else
                    {
                        try
                        {
                            responseStreamFromWebSiteBeingRelayed.CopyTo(originalResponse.OutputStream);
                        }
                        catch { }
                    }
                    originalResponse.OutputStream.Close();
                }
            }
        }
    }

    public static class ConsoleUtilities
    {
        public static void WriteRequest(string info)
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(info);
            Console.ResetColor();
        }
        public static void WriteResponse(string info)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(info);
            Console.ResetColor();
        }
    }

    public class RequestState
    {
        public readonly HttpWebRequest webRequest;
        public readonly HttpListenerContext context;

        public RequestState(HttpWebRequest request, HttpListenerContext context)
        {
            webRequest = request;
            this.context = context;
        }
    }

    #endregion

    public enum MEDIA_TYPE
    {
        AAC,
        M4A,
        MP3,
        MP4,
        WEB, // WEBM
    }

    public enum MEDIA_TAB
    {
        TAB_STORE = 1,
        TAB_SEARCH = 2,
    }
}
