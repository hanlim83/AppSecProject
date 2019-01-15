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
    public class TeamsController : Controller
    {
        private readonly CompetitionContext _context;

        public TeamsController(CompetitionContext context)
        {
            _context = context;
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
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (competition == null)
            {
                return NotFound();
            }

            return View(competition);
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

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamID,TeamName,Password,Score,CompetitionID")] Team team)
        {
            if (id != team.TeamID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.TeamID))
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
            return View(team);
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.TeamID == id);
        }
    }
}
