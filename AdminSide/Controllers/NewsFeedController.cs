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
using AdminSide.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminSide.Controllers
{
    public class NewsFeedController : Controller
    {

        private readonly NewsFeedContext _context;

        public NewsFeedController(NewsFeedContext context)
        {
            this._context = context;
        }

        //For the feed when it just loaded
        public async Task <IActionResult> Index(bool? check)
        {
            List<FeedSource> AllSources = await _context.FeedSources.ToListAsync();
            List<RSSFeed> AllFeeds = new List<RSSFeed>();
            foreach (FeedSource var in AllSources)
            {
                var feed = await FeedReader.ReadAsync(var.sourceURL);
                foreach (FeedItem item in feed.Items)
                {
                    RSSFeed Feed2 = new RSSFeed
                    {
                        Title = item.Title,
                        Link = item.Link,
                        Description = item.Description,
                        sourceCat = var.sourceName
                    };
                    AllFeeds.Add(Feed2);
                }

            }
            //    check = true;
            //if (check == null)
            //{
            //    ViewData["ShowWrongDirectory"] = false;
            //}
            //else
            //{
            //    ViewData["ShowWrongDirectory"] = true;
            //    //return RedirectToAction("Index", "NewsFeed", new { check = true });
            //}

            //return View();
            return View(AllFeeds);
            //return RedirectToAction("Index", "NewsFeed");

        }
        // for feed when searched
        [HttpPost]
        public async Task<IActionResult> Index(string SearchQuery, string Filter, bool? check)
        {
            List<FeedSource> AllSources = await _context.FeedSources.ToListAsync();
            List<RSSFeed> AllFeeds = new List<RSSFeed>();
            foreach (FeedSource var in AllSources)
            {
                var feed = await FeedReader.ReadAsync(var.sourceURL);
                foreach (FeedItem item in feed.Items)
                {
                    RSSFeed Feed2 = new RSSFeed
                    {
                        Title = item.Title,
                        Link = item.Link,
                        Description = item.Description,
                        sourceCat = var.sourceName
                    };
                    AllFeeds.Add(Feed2);
                }

            }
        
            List<RSSFeed> searchFeeds = new List<RSSFeed>();
            foreach(RSSFeed feed in AllFeeds)
            {
                if (Filter == "All")
                {
                    if (feed.Title.Contains(SearchQuery))
                        searchFeeds.Add(feed);
                    if (feed.sourceCat.Contains(SearchQuery))
                        searchFeeds.Add(feed);
                    if (feed.Description.Contains(SearchQuery))
                        searchFeeds.Add(feed);
                }

                if (Filter == "Title")
                {
                    if (feed.Title.Contains(SearchQuery))
                        searchFeeds.Add(feed);
                }

                if (Filter =="Category")
                {
                    if (feed.sourceCat.Contains(SearchQuery))
                        searchFeeds.Add(feed);
                }

                if (Filter == "Description")
                {
                    if (feed.Description.Contains(SearchQuery))
                        searchFeeds.Add(feed);
                }
            }

            //if (searchFeeds.Count == 0)
            //{
            //    return RedirectToAction("Index", "NewsFeed", new { check = true });
            //}
            return View(searchFeeds);
        }

        

        // GET: ListSource
        public async Task<IActionResult> ListSource()
        {
            return View(await _context.FeedSources.ToListAsync());
        }

        // GET: FeedSources/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedSource = await _context.FeedSources
                .FirstOrDefaultAsync(m => m.ID == id);
            if (feedSource == null)
            {
                return NotFound();
            }

            return View(feedSource);
        }

        // GET: FeedSources/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FeedSources/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,sourceName,sourceURL")] FeedSource feedSource)
        {
            if (ModelState.IsValid)
            {
                _context.Add(feedSource);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListSource));
                
            }
            return View(feedSource);
        }

        // GET: FeedSources/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedSource = await _context.FeedSources.FindAsync(id);
            if (feedSource == null)
            {
                return NotFound();
            }
            return View(feedSource);
        }

        // POST: FeedSources/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,sourceName,sourceURL")] FeedSource feedSource)
        {
            if (id != feedSource.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(feedSource);
                    await _context.SaveChangesAsync();
                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedSourceExists(feedSource.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ListSource));
            }
            return View(feedSource);
        }

        // GET: FeedSources/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedSource = await _context.FeedSources
                .FirstOrDefaultAsync(m => m.ID == id);
            if (feedSource == null)
            {
                return NotFound();
            }

            return View(feedSource);
        }

        // POST: FeedSources/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feedSource = await _context.FeedSources.FindAsync(id);
            _context.FeedSources.Remove(feedSource);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListSource));
        }

        private bool FeedSourceExists(int id)
        {
            return _context.FeedSources.Any(e => e.ID == id);
        }
    }
}