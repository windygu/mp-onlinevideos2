﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Web;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Net;

namespace OnlineVideos.Sites
{
    public class SwefilmerSiteUtil : SiteUtilBase
    {
        public enum VideoSort { Date, Views, Length, Alphabet };

        //Configuration properties and default values
        [Category("OnlineVideosConfiguration"), Description("Url to post to for login")]
        protected string loginPostUrl = @"http://www.swefilmer.com/login.php";
        [Category("OnlineVideosConfiguration"), Description("Postdata used in login post, use {0} for username and {1} for password")]
        protected string loginPostDataFormatString = @"username={0}&pass={1}&remember=1&ref=&Login=Logga+in";
        //User configuration
        [Category("OnlineVideosUserConfiguration"), LocalizableDisplayName("Sort videos by"), Description("Sort videos by")]
        VideoSort videoSort = VideoSort.Date;
        [Category("OnlineVideosUserConfiguration"), LocalizableDisplayName("Username"), Description("Swefilmer username")]
        protected string username = null;
        [Category("OnlineVideosUserConfiguration"), LocalizableDisplayName("Password"), Description("Swefilmer password")]
        protected string password = null;

        private string nextPageUrl = "";
        private CookieContainer cc = null;

        private bool isLoggedIn()
        {
            bool anyCookieExpired = false;
            bool hasUserCookie = false;
            if (cc != null)
            {
                string cookieDomain = new Uri("http://www.swefilmer.com").GetLeftPart(UriPartial.Authority); 
                //Check if cookies are valid/expired and if the cookie container contains an user cookie 
                foreach (Cookie cookie in cc.GetCookies(new Uri(cookieDomain)))
                {
                    //Some cookies do not expire, check with MinValue
                    anyCookieExpired |= !(cookie.Expires == DateTime.MinValue || cookie.Expires > DateTime.Now);
                    // When logged in a cookie is set where the cookie value contains the username.
                    hasUserCookie |= cookie.Value.Contains(username);
                }
            }
            //If no cookie expired and if an "user cookie" exists
            return !anyCookieExpired && hasUserCookie;
        }

        private CookieContainer GetCookie()
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
            {
                cc = null;
            }
            else if (!isLoggedIn())
            {
                //User not logged in, but would like to be...
                cc = new CookieContainer();
                //log in, use this.cc to get the log in response cookies
                GetWebDataFromPost(loginPostUrl, string.Format(loginPostDataFormatString, HttpUtility.UrlEncode(username), HttpUtility.UrlEncode(password)), cc);
                if (!isLoggedIn())
                {
                    //Failed to log in, use a new cookie container next time
                    cc = null;
                    // Throw; Show message to user
                    throw new OnlineVideosException("Wrong username or password, unable to log in.");
                }
            }
            return cc;
        }

        public override int DiscoverDynamicCategories()
        {
            Settings.Categories.Clear();
            HtmlDocument doc = GetWebData<HtmlDocument>("http://www.swefilmer.com", GetCookie());
            var ul = doc.DocumentNode.SelectSingleNode("//ul[@id = 'ul_categories']");
            Category tv = new Category() { Name = "TV-Serier", HasSubCategories = true, SubCategories = new List<Category>() };
            foreach (var a in ul.SelectNodes("li/ul/li/a"))
            {
                tv.SubCategories.Add(new RssLink() {ParentCategory = tv, Name = HttpUtility.HtmlDecode(a.InnerText.Trim()), Url = a.GetAttributeValue("href", ""), HasSubCategories = false });
            }
            tv.SubCategoriesDiscovered = tv.SubCategories.Count() > 0;
            Settings.Categories.Add(tv);

            foreach (var a in ul.SelectNodes("li/a").Where(n => !n.InnerText.Contains("TV-Serier")))
            {
                Settings.Categories.Add(new RssLink() { Name = HttpUtility.HtmlDecode(a.InnerText.Trim()), Url = a.GetAttributeValue("href", ""), HasSubCategories = false });
            }
            Settings.DynamicCategoriesDiscovered = Settings.Categories.Count > 0;

            return Settings.Categories.Count;
        }

        private List<VideoInfo> Videos(string url)
        {
            HtmlDocument doc = GetWebData<HtmlDocument>(url, GetCookie());
            var divs = doc.DocumentNode.SelectNodes("//div[@class = 'moviefilm']");
            List<VideoInfo> videos = new List<VideoInfo>();
            foreach (var div in divs)
            {
                var a = div.SelectSingleNode("a");
                var img = a.SelectSingleNode("img");
                videos.Add(new VideoInfo() { Title = img.GetAttributeValue("alt", ""), ImageUrl = img.GetAttributeValue("src", ""), VideoUrl = a.GetAttributeValue("href", "") });
            }
            var fastphp = doc.DocumentNode.SelectSingleNode("//div[@class = 'fastphp']");
            HtmlNode next = null;
            if (fastphp != null)
                next = fastphp.Descendants("a").FirstOrDefault(a => HttpUtility.HtmlDecode(a.InnerText).Contains("nästa"));
            HasNextPage = next != null;
            if (HasNextPage) nextPageUrl = "http://www.swefilmer.com/" + next.GetAttributeValue("href", "");
            return videos;
        }

        public override List<VideoInfo> getVideoList(Category category)
        {
            var url = (category as RssLink).Url;
            url = url.Substring(0, url.LastIndexOf("-") + 1);
            switch (videoSort)
            {
                case VideoSort.Alphabet:
                    url += "artist.html";
                    break;
                case VideoSort.Length:
                    url += "length.html";
                    break;
                case VideoSort.Views:
                    url += "views.html";
                    break;
                case VideoSort.Date:
                default:
                    url += "date.html";
                    break;
            }
            return Videos(url);
        }

        public override List<VideoInfo> getNextPageVideos()
        {
            return Videos(nextPageUrl);
        }

        public override bool CanSearch
        {
            get
            {
                return true;
            }
        }

        public override List<ISearchResultItem> DoSearch(string query)
        {
            List<ISearchResultItem> results = new List<ISearchResultItem>();
            Videos("http://www.swefilmer.com/search.php?keywords=" + HttpUtility.UrlEncode(query)).ForEach(v => results.Add(v));
            return results;
        }
        public override List<string> getMultipleVideoUrls(VideoInfo video, bool inPlaylist = false)
        {
            string decodedBase64 = "";
            string bestUrl = "";
            video.PlaybackOptions = new Dictionary<string, string>();
            HtmlDocument doc = GetWebData<HtmlDocument>(video.VideoUrl, GetCookie());
            var playerholder = doc.DocumentNode.SelectSingleNode("//div[@id = 'Playerholder']");
            if (playerholder == null)
                throw new OnlineVideosException("You have to logged in to be able to watch this video");
            var script = playerholder.SelectSingleNode("script").InnerText;
            Regex rgx = new Regex(@"'(.*)'");
            Match m = rgx.Match(script);
            if (m.Success)
            {
                var base64 = m.Groups[1].Value;
                decodedBase64 = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(base64));
            }
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(decodedBase64);
            var iframe = doc.DocumentNode.SelectSingleNode("//iframe");
            string videoUrl = "";
            if (iframe != null)
            {
                videoUrl = iframe.GetAttributeValue("src", "");
            }
            else
            {
                var obj = doc.DocumentNode.SelectSingleNode("//object");
                if (obj != null)
                {
                    var flashVars = obj.SelectSingleNode("//param[@name = 'FlashVars']").GetAttributeValue("value", "");
                    rgx = new Regex(@"proxy.link=(.*)");
                    m = rgx.Match(flashVars);
                    if (m.Success)
                    {
                        videoUrl = HttpUtility.UrlDecode(m.Groups[1].Value);
                    }
                }
            }

            if (!string.IsNullOrEmpty(videoUrl))
            {
                string frameData = GetWebData(videoUrl) ?? "";
                rgx = new Regex(@"var vars = (.*)");
                m = rgx.Match(frameData);
                if (m.Success)
                {
                    var json = JObject.Parse(m.Groups[1].Value);
                    rgx = new Regex(@"url([0-9]+)");
                    int res;
                    int max = 0;
                    string url;
                    foreach (JToken token in json.Descendants())
                    {
                        JProperty property = token as JProperty;
                        if (property != null)
                        {
                            m = rgx.Match(property.Name);
                            if (m.Success)
                            {
                                res = int.Parse(m.Groups[1].Value);
                                url = ((string)json[string.Format("url{0}", res)]).Replace("https://","http://");
                                video.PlaybackOptions.Add(string.Format("{0}p", res), url);
                                if (max < res)
                                {
                                    max = res;
                                    bestUrl = url;
                                }
                            }
                        }
                    }
                }
            }
            if (inPlaylist) video.PlaybackOptions.Clear();
            return new List<string>() { bestUrl };
        }
    }
}