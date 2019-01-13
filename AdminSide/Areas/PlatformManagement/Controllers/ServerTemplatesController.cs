using Microsoft.AspNetCore.Mvc;
using AdminSide.Areas.PlatformManagement.Data;
using Amazon.EC2;
using ASPJ_MVC.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    public class ServerTemplatesController : Controller
    {
        private readonly PlatformResourcesContext _context;

        IAmazonEC2 EC2Client { get; set; }

        public ServerTemplatesController(PlatformResourcesContext context, IAmazonEC2 ec2Client)
        {
            this._context = context;
            this.EC2Client = ec2Client;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Templates.ToListAsync());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}