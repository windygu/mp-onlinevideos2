﻿using System;
using System.Text.RegularExpressions;

namespace OnlineVideos.Hoster
{
    public class MovShare : MyHosterBase
    {
        public override string GetHosterUrl()
        {
            return "Movshare.net";
        }

        public override string GetVideoUrl(string url)
        {
            string page = WebCache.Instance.GetWebData(url);
            if (!string.IsNullOrEmpty(page))
            {
                return ParseData(page);
            }
            return String.Empty;
        }

        protected string ParseData(string data)
        {
            string step1 = WiseCrack(data);
            if (String.IsNullOrEmpty(step1))
                step1 = data;
            Match m = Regex.Match(step1, @"\}\('(?<p>.+?)[^\\]',(?<a>[^,]*),(?<c>[^,]*),[^']*'(?<k>\|[^']*)'.split\('\|'\),(?<e>[^,]*),(?<d>[^\)]*)\)");
            if (m.Success)
            {
                data = Helpers.StringUtils.Unpack(m.Groups["p"].Value, Int32.Parse(m.Groups["a"].Value), Int32.Parse(m.Groups["c"].Value), m.Groups["k"].Value.Split('|'),
                    Int32.Parse(m.Groups["e"].Value), m.Groups["d"].Value);
            }


            m = Regex.Match(data, @"flashvars\.file=""(?<fileid>[^""]*)"";\s*flashvars\.filekey=(?<filekey>[^;]*);");
            if (m.Success)
            {
                string fileKey = m.Groups["filekey"].Value;
                string fileId = m.Groups["fileid"].Value;
                while (m.Success && !fileKey.Contains("."))
                {
                    m = Regex.Match(data, String.Format(@"var\s{0}=(?<newval>[^;]*);", fileKey));
                    if (m.Success)
                        fileKey = m.Groups["newval"].Value;
                }
                fileKey = fileKey.Trim('"').Replace(".", "%2E").Replace("-", "%2D");
                data = WebCache.Instance.GetWebData(
                    String.Format("http://www.movshare.net/api/player.api.php?pass=undefined&codes=undefined&user=undefined&file={0}&key={1}",
                    fileId, fileKey));
                m = Regex.Match(data, @"url=(?<url>[^&]*)&");
                if (m.Success)
                    return m.Groups["url"].Value;
            }
            return String.Empty;
        }

    }
}
