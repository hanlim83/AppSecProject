using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminSide.Data;
using AdminSide.Models;

namespace AdminSide.Controllers
{
    public class ForumController : Controller
    {

        private readonly ForumContext context1;

        public ForumController(ForumContext con)
        {
            context1 = con;
        }

        // GET: Forum Post
        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            var posts = from p in context1.Posts
                        select p;

            switch (sortOrder)
            {
                case "name_desc":
                    posts = posts.OrderByDescending(s => s.UserName);
                    break;
                case "Date":
                    posts = posts.OrderBy(p => p.DT);
                    break;
                case "date_desc":
                    posts = posts.OrderByDescending(p => p.DT);
                    break;
                default:
                    break;
            }

            var category = await context1.ForumCategories
                .Include(p => p.Posts)
                .AsNoTracking()
                .ToListAsync();
            //.FirstOrDefaultAsync(m => m.CategoryID == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);

            //return View(await context1.Posts.AsNoTracking().ToListAsync());
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}

        // GET: Topic/Create
        public IActionResult NewTopicF()
        {
            return View();
        }

        // POST: Topic/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewTopicF([Bind("Title,Content,UserName, DT, CategoryID")] Post post)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    context1.Add(post);
                    await context1.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Please Try Again.");
            }
            return View(post);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await context1.Posts
                //.Include(c => c.CategoryID)
                //    .ThenInclude(e => e.Course)
                //.AsNoTracking()
                .SingleOrDefaultAsync(m => m.PostID == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await context1.Posts
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.PostID == id);
            if (post == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again.";
            }

            return View(post);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await context1.Posts.FindAsync(id);
            context1.Posts.Remove(post);
            await context1.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return context1.Posts.Any(e => e.PostID == id);
        }
    }
}
