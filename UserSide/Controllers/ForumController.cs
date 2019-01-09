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

namespace UserSide.Controllers
{
    public class ForumController : Controller
    {

        private readonly ForumContext context1;

        public ForumController(ForumContext con)
        {
            context1 = con;
        }

        // GET: Forum Post
        public async Task<IActionResult> Index()
        {
            return View(await context1.Forums.ToListAsync());
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult NewTopicF()
        {
            return View();
        }
    }
}