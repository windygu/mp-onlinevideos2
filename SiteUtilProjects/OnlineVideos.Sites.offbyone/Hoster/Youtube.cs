﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OnlineVideos.Hoster.Base;
using OnlineVideos.Sites;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace OnlineVideos.Hoster
{
    public class Youtube : HosterBase
    {
        public override string getHosterUrl()
        {
            return "Youtube.com";
        }

        static readonly int[] fmtOptionsQualitySorted = new int[] { 37, 22, 35, 18, 34, 5, 0, 17, 13 };
        static Regex swfJsonArgs = new Regex(@"(?:var\s)?(?:swfArgs|'SWF_ARGS')\s*(?:=|\:)\s(?<json>\{.+\})|(?:\<param\sname=\\""flashvars\\""\svalue=\\""(?<params>[^""]+)\\""\>)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public override Dictionary<string, string> getPlaybackOptions(string url)
        {
            Dictionary<string, string> PlaybackOptions = null;

            string videoId = url;
            if (videoId.ToLower().Contains("youtube.com"))
            {
                // get an Id from the Url
                int p = videoId.LastIndexOf("watch?v="); // for http://www.youtube.com/watch?v=jAgBeAFZVgI
                if (p >= 0)
                    p += +8;
                else
                    p = videoId.LastIndexOf('/') + 1;
                int q = videoId.IndexOf('?', p);
                if (q < 0) q = videoId.IndexOf('&', p);
                if (q < 0) q = videoId.Length;
                videoId = videoId.Substring(p, q - p);
            }

            NameValueCollection Items = new NameValueCollection();
            try
            {
                string contents = Sites.SiteUtilBase.GetWebData(string.Format("http://youtube.com/get_video_info?video_id={0}", videoId));
                Items = System.Web.HttpUtility.ParseQueryString(contents);
                if (Items["status"] == "fail")
                {
                    contents = Sites.SiteUtilBase.GetWebData(string.Format("http://www.youtube.com/watch?v={0}", videoId));
                    Match m = swfJsonArgs.Match(contents);
                    if (m.Success)
                    {
                        if (m.Groups["params"].Success)
                        {
                            Items = System.Web.HttpUtility.ParseQueryString(m.Groups["params"].Value);
                        }
                        else if (m.Groups["json"].Success)
                        {
                            Items.Clear();
                            foreach (var z in Newtonsoft.Json.Linq.JObject.Parse(m.Groups["json"].Value))
                            {
                                Items.Add(z.Key, z.Value.Value<string>(z.Key));
                            }
                        }
                    }
                }
            }
            catch { }

            string[] FmtMap = null;
            if (Items.Get("fmt_url_map") != "")
            {
                FmtMap = Items["fmt_url_map"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Array.Sort(FmtMap, new Comparison<string>(delegate(string a, string b)
                {
                    int a_i = int.Parse(a.Substring(0, a.IndexOf("|")));
                    int b_i = int.Parse(b.Substring(0, b.IndexOf("|")));
                    int index_a = Array.IndexOf(fmtOptionsQualitySorted, a_i);
                    int index_b = Array.IndexOf(fmtOptionsQualitySorted, b_i);
                    return index_b.CompareTo(index_a);
                }));
                PlaybackOptions = new Dictionary<string, string>();
                foreach (string fmtValue in FmtMap)
                {
                    int fmtValueInt = int.Parse(fmtValue.Substring(0, fmtValue.IndexOf("|")));
                    switch (fmtValueInt)
                    {
                        case 0:
                        case 5:
                        case 34:
                            PlaybackOptions.Add("320x240 | (" + fmtValueInt.ToString().PadLeft(2, ' ') + ") | .flv", string.Format("{0}&ext=.{1}", fmtValue.Substring(fmtValue.IndexOf("|") + 1), "flv")); break;
                        case 13:
                        case 17:
                            PlaybackOptions.Add("176x144 | (" + fmtValueInt.ToString().PadLeft(2, ' ') + ") | .mp4", string.Format("{0}&ext=.{1}", fmtValue.Substring(fmtValue.IndexOf("|") + 1), "mp4")); break;
                        case 18:
                            PlaybackOptions.Add("480x360 | (" + fmtValueInt.ToString().PadLeft(2, ' ') + ") | .mp4", string.Format("{0}&ext=.{1}", fmtValue.Substring(fmtValue.IndexOf("|") + 1), "mp4")); break;
                        case 35:
                            PlaybackOptions.Add("640x480 | (" + fmtValueInt.ToString().PadLeft(2, ' ') + ") | .flv", string.Format("{0}&ext=.{1}", fmtValue.Substring(fmtValue.IndexOf("|") + 1), "flv")); break;
                        case 22:
                            PlaybackOptions.Add("1280x720 | (" + fmtValueInt.ToString().PadLeft(2, ' ') + ") | .mp4", string.Format("{0}&ext=.{1}", fmtValue.Substring(fmtValue.IndexOf("|") + 1), "mp4")); break;
                        case 37:
                            PlaybackOptions.Add("1920x1080 | (" + fmtValueInt.ToString().PadLeft(2, ' ') + ") | .mp4", string.Format("{0}&ext=.{1}", fmtValue.Substring(fmtValue.IndexOf("|") + 1), "mp4")); break;
                    }
                }
            }

            return PlaybackOptions;
        }

        public override string getVideoUrls(string url)
        {
            // return highest quality by default
            var result = getPlaybackOptions(url);
            if (result != null && result.Count > 0) return result.Last().Value;
            else return String.Empty;
        }
    }
}