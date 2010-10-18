using System.Text.RegularExpressions;
using System.Web;

namespace OnlineVideos.Hoster
{
	public class LiveVideo : Hoster.Base.HosterBase
	{
        public override string getHosterUrl()
        {
            return "livevideo.com";
        }

        public override string getVideoUrls(string url)
        {
            string lsHtml = HttpUtility.UrlDecode(Sites.SiteUtilBase.GetRedirectedUrl(url));
            Match loMatch = Regex.Match(lsHtml, "video=([^\"]+)");
            if (loMatch.Success)
            {
                string lsUrl = loMatch.Groups[1].Value;
                string url_hash = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(lsUrl + "&f=flash" + "undefined" + "LVX*7x8yzwe", "MD5").ToLower();
                lsUrl += "&f=flash" + "undefined" + "&h=" + url_hash;

                string str = Sites.SiteUtilBase.GetWebData(lsUrl);
                Match loMatch2 = Regex.Match(str, @"video_id=([^&]+)");
                if (loMatch2.Success)
                {
                    lsUrl = HttpUtility.UrlDecode(loMatch2.Groups[1].Value);
                    return lsUrl;
                }
            }
            // getting here means some error occured
            return "";
        }
    }
}