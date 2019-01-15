using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserSide.Data;
using UserSide.Models;

namespace UserSide.Controllers
{
    public class ChatController : Controller
    {
        private readonly ChatContext _context;

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult TalkView()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }


        // POST: UserChats/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserID,UserName,GroupID")] GroupChat groupChat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(groupChat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(groupChat);
        }
    }
}