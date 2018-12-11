using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace UserSide.Controllers
{
    public class Chat : Controller
    {
        public IActionResult Index()
        {
            ViewData["Message"] = "Lets talk";
            return View();
        }
    }
}