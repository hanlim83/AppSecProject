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

namespace UserSide.Controllers
{
    [Authorize]
    //the line above makes a page protected and will redirect user back to login
    public class CompetitionsController : Controller
    {
        private readonly CompetitionContext _context;

        public CompetitionsController(CompetitionContext context)
        {
            _context = context;
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
                .FirstOrDefaultAsync(m => m.ID == id);
            if (competition == null)
            {
                return NotFound();
            }

            return View();
            //return View();
        }

        private bool CompetitionExists(int id)
        {
            return _context.Competitions.Any(e => e.ID == id);
        }
    }
}
