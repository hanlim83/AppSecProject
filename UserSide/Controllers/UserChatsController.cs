using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UserSide.Data;
using UserSide.Models;

namespace UserSide.Controllers
{
    public class UserChatsController : Controller
    {
        private readonly ChatContext _context;

        public UserChatsController(ChatContext context)
        {
            _context = context;
        }

        // GET: UserChats
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserChats.ToListAsync());
        }

        // GET: UserChats/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userChat = await _context.UserChats
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (userChat == null)
            {
                return NotFound();
            }

            return View(userChat);
        }

        // GET: UserChats/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserChats/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserID,UserName,GroupID")] UserChat userChat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userChat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userChat);
        }

        // GET: UserChats/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userChat = await _context.UserChats.FindAsync(id);
            if (userChat == null)
            {
                return NotFound();
            }
            return View(userChat);
        }

        // POST: UserChats/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("UserID,UserName,GroupID")] UserChat userChat)
        {
            if (id != userChat.UserID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userChat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserChatExists(userChat.UserID))
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
            return View(userChat);
        }

        // GET: UserChats/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userChat = await _context.UserChats
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (userChat == null)
            {
                return NotFound();
            }

            return View(userChat);
        }

        // POST: UserChats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userChat = await _context.UserChats.FindAsync(id);
            _context.UserChats.Remove(userChat);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserChatExists(string id)
        {
            return _context.UserChats.Any(e => e.UserID == id);
        }
    }
}
