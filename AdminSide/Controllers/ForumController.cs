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
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace AdminSide.Controllers
{
    public class ForumController : Controller
    {

        private readonly ForumContext context1;
        private readonly UserManager<IdentityUser> _userManager;

        public ForumController(ForumContext con, UserManager<IdentityUser> userManager)
        {
            context1 = con;
            _userManager = userManager;
        }

        // GET: Forum Post
        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? page)
        {

            //Take in user object
            var user = await _userManager.GetUserAsync(HttpContext.User);
            ////For username (can use it inside method also)
            var username = user;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var posts = from p in context1.Posts
                        select p;
            if (!String.IsNullOrEmpty(searchString))
            {
                posts = posts.Where(p => p.UserName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    posts = posts.OrderByDescending(p => p.UserName);
                    break;
                case "Date":
                    posts = posts.OrderBy(p => p.DT);
                    break;
                case "date_desc":
                    posts = posts.OrderByDescending(p => p.DT);
                    break;
                default:
                    posts = posts.OrderBy(p => p.UserName);
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

            //PostViewModel postViewModel = new PostViewModel();
            //postViewModel.ForumCategory = category;

            //foreach(var Category in category.Post)
            //{

            //}
            //int pageSize = 3;

            return View(category);
            //return View(await PaginatedList<Post>.CreateAsync(posts.AsNoTracking(), page ?? 1, pageSize));
        }


        // GET: Forum/Details
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

        // GET: Topic/Create
        public IActionResult NewTopicF()
        {
            PopulateCategoryDropDownList();
            return View();
        }

        // GET: Forum/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await context1.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            PopulateCategoryDropDownList();
            return View(post);
        }

        // GET: Forum/Delete
        public async Task<IActionResult> Delete(int? id)
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

            return View(post);
        }

        // GET: Category/New
        public IActionResult NewCategory()
        {
            return View();
        }

        // POST: Topic/New
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewTopicF([Bind("Title,Content,UserName, DT, CategoryID")] Post post)
        {
            if (ModelState.IsValid)
            {
                //Take in user object
                var user = await _userManager.GetUserAsync(HttpContext.User);
                ////For username (can use it inside method also)
                var username = user;
                post.UserName = user.UserName;
                post.DT = DateTime.Now;

                context1.Add(post);
                await context1.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // POST: Forum/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Content,UserName, DT, CategoryID")] Post post)
        {
            if (id != post.PostID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    post.DT = DateTime.Now;
                    // GET THE ID
                    var userID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    post.UserName = userID;
                    context1.Update(post);
                    await context1.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // POST: Forum/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await context1.Posts.FindAsync(id);
            context1.Posts.Remove(post);
            await context1.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Topic/New
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewCategory([Bind("CategoryName")] ForumCategory category)
        {
            if (ModelState.IsValid)
            {
                context1.Add(category);
                await context1.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Checking if Post Exist
        private bool PostExists(int id)
        {
            return context1.Posts.Any(e => e.PostID == id);
        }

        // Dropdown List for Category 
        private void PopulateCategoryDropDownList(object selectCategory = null)
        {
            var categoryQuery = from c in context1.ForumCategories
                                orderby c.CategoryName
                                select c;
            ViewBag.CategoryID = new SelectList(categoryQuery.AsNoTracking(), "CategoryID", "CategoryName", selectCategory);
        }
    }
}