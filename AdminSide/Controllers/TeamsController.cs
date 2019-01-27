using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminSide.Data;
using AdminSide.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace AdminSide.Controllers
{
    [Authorize]
    //the line above makes a page protected and will redirect user back to login
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

            var competition = await _context.Competitions
                .Include(c => c.Teams)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (competition == null)
            {
                return NotFound();
            }

            return View(competition);
            //return View(await _context.Teams.ToListAsync());
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.TeamUsers)
                .Include(t => t.TeamChallenges)
                .FirstOrDefaultAsync(m => m.TeamID == id);
            if (team == null)
            {
                return NotFound();
            }

            int solve = 0;
            int fail = 0;

            foreach (var item in team.TeamChallenges)
            {
                if (item.Solved == true)
                {
                    solve++;
                }
                else
                {
                    fail++;
                }
            }

            List<DataPoint> dataPoints = new List<DataPoint>();

            dataPoints.Add(new DataPoint(solve, "Solve", "#00FF08"));
            //dataPoints.Add(new DataPoint(363040, "Fails", "#546BC1"));
            dataPoints.Add(new DataPoint(fail, "Fails", "#ff0000"));


            ViewBag.SovlePercentage = JsonConvert.SerializeObject(dataPoints);

            dataPoints = new List<DataPoint>();

            ViewData["Total"] = team.TeamChallenges.Count();

            //var competition = await _context.Competitions
            //    .Include(c => c.CompetitionCategories)
            //    .ThenInclude(cc => cc.Challenges)
            //    .ThenInclude(ch => ch.TeamChallenges)
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(m => m.ID == team.CompetitionID);

            //List<DataPoint> categoryDataPoints = new List<DataPoint>();

            //int TotalSolved = 0;

            //List<string> colorList = new List<string>()
            //{
            //    "#FF9E00",
            //    "#009EFF",
            //    "#783DF2",
            //    "#3DE3F2",
            //    "#52D406",
            //    "#F2F03D"
            //};

            //int colorCounter = 0;
            //foreach (var category in competition.CompetitionCategories)
            //{
            //    int categorySolvedCounter = 0;
            //    foreach (var challenge in category.Challenges)
            //    {
            //        foreach (var teamChallenge in challenge.TeamChallenges)
            //        {
            //            if (teamChallenge.Solved == true && teamChallenge.TeamId == team.TeamID)
            //            {
            //                //categoryDataPoints.Add(new DataPoint(1, category.CategoryName, "#00FF08"));
            //                categorySolvedCounter++;
            //                TotalSolved++;
            //            }
            //        }
            //    }
            //    categoryDataPoints.Add(new DataPoint(categorySolvedCounter, category.CategoryName, colorList[colorCounter]));
            //    colorCounter++;
            //}

            //ViewBag.CategoryBreakdown = JsonConvert.SerializeObject(categoryDataPoints);

            //ViewData["TotalSolved"] = TotalSolved;

            return View(team);
        }

        // GET: Teams/Create
        public IActionResult Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Team team = new Team();
            team.CompetitionID = (int)id;
            return View(team);
        }

        // POST: Teams/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamID,TeamName,Password,CompetitionID")] Team team)
        {
            if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Index", "Teams", new { id = team.CompetitionID });
            }
            return View(team);
            //return RedirectToAction("Index", "Teams", new { id = team.CompetitionID });
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
        public async Task<IActionResult> Edit(int id, [Bind("TeamID,TeamName,Password,CompetitionID")] Team team)
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

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams
                .Include(t => t.TeamUsers)
                .FirstOrDefaultAsync(m => m.TeamID == id);
            foreach (var teamUser in team.TeamUsers)
            {
                _context.TeamUsers.Remove(teamUser);
            }
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Teams", new { id = team.CompetitionID });
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.TeamID == id);
        }
    }
}
