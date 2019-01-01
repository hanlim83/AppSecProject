using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminSide.Areas.PlatformManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Amazon.EC2;
using Amazon.EC2.Model;
using AdminSide.Areas.PlatformManagement.Models;
using System.Diagnostics;
using ASPJ_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    public class ChallengeServersController : Controller
    {
        private readonly PlatformResourcesContext _context;

        IAmazonEC2 EC2Client { get; set; }

        public ChallengeServersController(PlatformResourcesContext context, IAmazonEC2 ec2Client)
        {
            this._context = context;
            this.EC2Client = ec2Client;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Servers.ToListAsync());
        }

        public IActionResult SelectTemplate()
        {
            return View();
        }

        public IActionResult SpecifySettings()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}