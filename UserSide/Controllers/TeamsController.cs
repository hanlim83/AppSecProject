using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UserSide.Data;
using UserSide.Models;

namespace UserSide.Controllers
{
    [Authorize]
    //the line above makes a page protected and will redirect user back to login
    public class TeamsController : Controller
    {
        private readonly CompetitionContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TeamsController(CompetitionContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Teams
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //Prob need to do some sorting by score here before returning
            var competition = await _context.Competitions
                .Include(c => c.Teams)
                .ThenInclude(t => t.TeamUsers)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (competition == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);

            foreach (var Team in competition.Teams)
            {
                foreach (var TeamUser in Team.TeamUsers)
                {
                    if (TeamUser.UserId.Equals(user.Id))
                    {
                        return View(competition);
                    }
                }
            }
            return RedirectToAction("Index", "Competitions");
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .FirstOrDefaultAsync(m => m.TeamID == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.TeamID == id);
        }
    }
}
