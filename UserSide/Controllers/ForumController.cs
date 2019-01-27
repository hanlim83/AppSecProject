using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UserSide.Data;
using UserSide.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Web;

namespace UserSide.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> Index()
        {

            //Take in user object
            var user = await _userManager.GetUserAsync(HttpContext.User);
            ////For username (can use it inside method also)
            var username = user;

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
            List<Comment> comments = context1.Comments.FromSql("SELECT * FROM dbo.Comment WHERE PostID = " + post.PostID).ToList();
            PostViewModel model = new PostViewModel
            {
                Post = post,
                Comments = comments
            };

            if (post == null)
            {
                return NotFound();
            }

            return View(model);
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

            var post = await context1.Posts.SingleOrDefaultAsync(m => m.PostID == id);

            if (post == null)
            {
                return NotFound();
            }

            // OWASP:Authorize
            //Take in user object
            var user = await _userManager.GetUserAsync(HttpContext.User);
            ////For username (can use it inside method also)
            var username = user;

            if (!user.UserName.Equals(post.UserName))
            {
                ViewData["ShowWrongDirectory"] = "true";
                return RedirectToAction("Index", "Forum", new { check = true });
            }
            else if (user.UserName.Equals(post.UserName))
            {
                ViewData["ShowWrongDirectory"] = "false";
            }

            PopulateCategoryDropDownList();
            return View(post);
        }

        // POST: Topic/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewTopicF([Bind("Title,Content, UserName, DT, CategoryID")] Post post)
        {
            if (ValidateCheck(post.Content) == true)
            {
                ViewData["Alert"] = "Please Remove The Special Characters.";
            }
            else
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
            }

            PopulateCategoryDropDownList();
            return View(post);
        }

        // POST: Forum/Edit
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postToUpdate = await context1.Posts.SingleOrDefaultAsync(p => p.PostID == id);
            if (await TryUpdateModelAsync<Post>(
                postToUpdate,
                "",
                p => p.Title, p => p.Content, p => p.CategoryID))
            {
                try
                {
                    await context1.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    // Log the errors  
                    ModelState.AddModelError("", "Unable to save changes. ");
                }
            }
            return View(postToUpdate);
        }

        //if (ValidateCheck(post.Content) == true)
        //{
        //    ViewData["Alert"] = "Please Remove The Special Characters.";
        //}
        //else
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            context1.Update(post);
        //            await context1.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!PostExists(post.PostID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //}
        //return View(post);
        //}

        // POST: Forum/PostReply
        [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostReply(Comment comment, String PostID)
    {
        //Take in user object
        var user = await _userManager.GetUserAsync(HttpContext.User);
        //For username (can use it inside method also)
        var username = user;

        if (user == null)
        {
            return NotFound();
        }

        if (ValidateCheck(comment.Content) == true)
        {
            ViewData["Alert"] = "Please Remove The Special Characters.";
        }
        else
        {
            Comment c = new Comment();
            c.UserName = user.UserName;
            c.Content = comment.Content;
            c.DT = DateTime.Now;
            c.CommentID = comment.CommentID;
            c.PostID = int.Parse(PostID);

            context1.Add(c);
            await context1.SaveChangesAsync();
        }
        return RedirectToAction("Details", new { id = PostID });
    }

    // Checking if Post Exists
    private bool PostExists(int id)
    {
        return context1.Posts.Any(e => e.PostID == id);
    }

    // Drop down List for Category
    private void PopulateCategoryDropDownList(object selectCategory = null)
    {
        var categoryQuery = from c in context1.ForumCategories
                            orderby c.CategoryName
                            select c;
        ViewBag.CategoryID = new SelectList(categoryQuery.AsNoTracking(), "CategoryID", "CategoryName", selectCategory);
    }

    // Validate Input for Special Characters
    public Boolean ValidateCheck(String input)
    {
        if (input.Contains("||") || input.Contains("-") || input.Contains("/") || input.Contains("<") || input.Contains(">") || input.Contains("<") || input.Contains(">") || input.Contains("=") || input.Contains("<=") || input.Contains(">=") || input.Contains("~=") || input.Contains("!=") || input.Contains("^=") || input.Contains("(") || input.Contains(")"))
        {
            return true;
        }
        else
            return false;
    }
}
}