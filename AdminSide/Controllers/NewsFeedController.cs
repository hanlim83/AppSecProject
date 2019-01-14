using AdminSide.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CodeHollow.FeedReader;

namespace AdminSide.Controllers
{
    public class NewsFeedController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SearchPage()
        {
            return View();
        }

        // RSS FEED Codes
        /*[HttpPost]
        public ActionResult Index (string RSSURL)
        {
            WebClient wclient = new WebClient();
            string RSSDate = wclient.DownloadString(RSSURL);

            XDocument xml = XDocument.Parse(RSSDate);
            var RSSFeedData = (from x in xml.Descendants("item") select new RSSFeed
            {
                Title = ((string)x.Element("title")),
                Link = ((string)x.Element("link")),
                Description = ((string)x.Element("description")),
                PubDate = ((string)x.Element("pubDate"))
            });
            ViewBag.RSSFeed = RSSFeedData;
            ViewBag.URL = RSSURL;
            return View();
        }*/
        [HttpPost]
        public async Task<IActionResult> Index (string RSSURL)
        {
            
            var feed = await FeedReader.ReadAsync(RSSURL); //await will wait for the feed
            RSSFeed rssfeed = new RSSFeed
            {
                Title = feed.Title,
                Link = feed.Link,
                Description = feed.Description,
               // PubDate = feed.LastUpdatedDateString
            };

            List<FeedItem> feedlist = new List<FeedItem>();
            foreach (var item in feed.Items)
            {
                feedlist.Add(item);
            }
            ViewBag.RSSFeed = feedlist;

            ViewBag.URL = "https://hnrss.org/newcomments";
            return View();
        }
    }
}