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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace UserSide.Controllers
{
    [Authorize]
    //the line above makes a page protected and will redirect user back to login
    public class CompetitionsController : Controller
    {
        private readonly CompetitionContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CompetitionsController(CompetitionContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Competitions
        public async Task<IActionResult> Index()
        {
            var competition = await _context.Competitions
                .Include(c => c.Teams)
                .ThenInclude(t => t.TeamUsers)
                .ToListAsync();

            var teamUsers = _context.TeamUsers
                .ToList();

            if (competition == null)
            {
                return NotFound();
            }

            //CompetitionIndexViewModel vm = new CompetitionIndexViewModel();
            //vm.Competition = competition;
            //vm.TeamUsers = teamUsers;

            return View(competition);
            //return View(await _context.Competitions.ToListAsync());
        }

        // GET: Competitions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competition = await _context.Competitions
                .Include(c => c.Teams)
                .ThenInclude(t => t.TeamUsers)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (competition == null)
            {
                return NotFound();
            }

            return View(competition);
        }
        
        public async Task<IActionResult> SignUp(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competition = await _context.Competitions
                .Include(c => c.Teams)
                .ThenInclude(t => t.TeamUsers)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (competition == null)
            {
                return NotFound();
            }

            ViewData["CompetitionID"] = id;

            TeamCreateViewModel teamCreateViewModel = new TeamCreateViewModel();
            teamCreateViewModel.Competition = competition;

            //Need to get user.Id
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            foreach (var Team in competition.Teams)
            {
                foreach (var TeamUser in Team.TeamUsers)
                {
                    if (TeamUser.UserId.Equals(userId))
                    {
                        return RedirectToAction("Index", "Competitions");
                    }
                    else
                    {
                        return View();
                    }
                }
            }
            return View();
            //return View();
        }

        // POST: Competitions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("TeamName, Password, CompetitionID")]Team team)
        {
            if (ModelState.IsValid)
            {
                _context.Add(team);
                //get userId
                var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                TeamUser teamUser = new TeamUser();
                teamUser.UserId = userId;

                teamUser.TeamId = team.TeamID;
                _context.Add(teamUser);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Competitions");
        }

        private bool CompetitionExists(int id)
        {
            return _context.Competitions.Any(e => e.ID == id);
        }
    }
}
