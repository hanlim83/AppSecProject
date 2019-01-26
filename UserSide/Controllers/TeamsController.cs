using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
            //dataPoints.Add(new DataPoint(1420050600000, 33000));
            //dataPoints.Add(new DataPoint(1422729000000, 35960));
            //dataPoints.Add(new DataPoint(1425148200000, 42160));
            //dataPoints.Add(new DataPoint(1427826600000, 42240));
            //dataPoints.Add(new DataPoint(1430418600000, 43200));
            //dataPoints.Add(new DataPoint(1433097000000, 40600));
            //dataPoints.Add(new DataPoint(1435689000000, 42560));
            //dataPoints.Add(new DataPoint(1438367400000, 44280));
            //dataPoints.Add(new DataPoint(1441045800000, 44800));
            //dataPoints.Add(new DataPoint(1443637800000, 48720));
            //dataPoints.Add(new DataPoint(1446316200000, 50840));
            //dataPoints.Add(new DataPoint(1448908200000, 51600));
            //dataPoints.Add(new DataPoint(solve, fail));

            //ViewBag.NewVisitors = JsonConvert.SerializeObject(dataPoints);

            //dataPoints = new List<DataPoint>();
            //dataPoints.Add(new DataPoint(1420050600000, 22000));
            //dataPoints.Add(new DataPoint(1422729000000, 26040));
            //dataPoints.Add(new DataPoint(1425148200000, 25840));
            //dataPoints.Add(new DataPoint(1427826600000, 23760));
            //dataPoints.Add(new DataPoint(1430418600000, 28800));
            //dataPoints.Add(new DataPoint(1433097000000, 29400));
            //dataPoints.Add(new DataPoint(1435689000000, 33440));
            //dataPoints.Add(new DataPoint(1438367400000, 37720));
            //dataPoints.Add(new DataPoint(1441045800000, 35200));
            //dataPoints.Add(new DataPoint(1443637800000, 35280));
            //dataPoints.Add(new DataPoint(1446316200000, 31160));
            //dataPoints.Add(new DataPoint(1448908200000, 34400));
            //dataPoints.Add(new DataPoint(solve, fail));

            //ViewBag.ReturningVisitors = JsonConvert.SerializeObject(dataPoints);

            ViewData["Total"] = team.TeamChallenges.Count();

            return View(team);
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.TeamID == id);
        }
    }
}
