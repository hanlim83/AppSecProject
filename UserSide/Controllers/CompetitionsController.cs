using System.Collections.Generic;
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
using Microsoft.AspNetCore.Http;

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
        public async Task<IActionResult> Index(bool? check)
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

            if (check == null)
            {
                ViewData["ShowWrongDirectory"] = false;
            }
            else
            {
                ViewData["ShowWrongDirectory"] = true;
            }
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
            
            var user = await _userManager.GetUserAsync(HttpContext.User);
            foreach (var Team in competition.Teams)
            {
                foreach (var TeamUser in Team.TeamUsers)
                {
                    if (TeamUser.UserId.Equals(user.Id))
                    {
                        ViewData["ShowWrongDirectory"] = "true";
                        return RedirectToAction("Index", "Competitions", new { check = true });
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
                //BCryptPasswordHash bCryptPasswordHash = new BCryptPasswordHash();
                var salt = BCryptPasswordHash.GetRandomSalt();
                var hashPassword = BCryptPasswordHash.HashPassword(team.Password, salt);
                team.Password = hashPassword;
                team.Salt = salt;
                _context.Add(team);
                //get userId
                //var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                //Migrating to new way to get user object
                var user = await _userManager.GetUserAsync(HttpContext.User);

                TeamUser teamUser = new TeamUser();
                teamUser.UserId = user.Id;
                teamUser.UserName = user.UserName;

                teamUser.TeamId = team.TeamID;
                _context.Add(teamUser);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Competitions");
            }
            ViewData["CompetitionID"] = team.CompetitionID;
            return View();
        }

        public async Task<IActionResult> Join(int? id, bool? check)
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

            //var user = await _userManager.GetUserAsync(HttpContext.User);
            //var username = user.UserName;
            var dictionary = new Dictionary<int, string>
            {

            };

            foreach (var Team in competition.Teams)
            {
                dictionary.Add(Team.TeamID, Team.TeamName);
                foreach (var TeamUser in Team.TeamUsers)
                {
                    if (TeamUser.UserId.Equals(userId))
                    {
                        return RedirectToAction("Index", "Competitions", new { check = true });
                    }
                }
            }
            ViewBag.SelectList = new SelectList(dictionary, "Key", "Value");
            if (check == null)
            {
                ViewData["Show"] = false;
            }
            else
            {
                ViewData["Show"] = true;
            }
            return View();
            //return View();
        }

        //Just add user to UserTeam will do
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join([Bind("TeamID, Password, CompetitionID")]Team team)
        {
            var localvarTeam = await _context.Teams
                .Include(t => t.TeamUsers)
                .FirstOrDefaultAsync(m => m.TeamID == team.TeamID);

            var ProvidedPasswordhash = BCryptPasswordHash.HashPassword(team.Password, localvarTeam.Salt);

            if (localvarTeam.Password.Equals(ProvidedPasswordhash))
            //if (BCryptPasswordHash.ValidatePassword(ProvidedPasswordhash, (localvarTeam.Password)))
            {
                //if (ModelState.IsValid)
                //{
                //get userId
                //var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                //Migrate to get user object
                var user = await _userManager.GetUserAsync(HttpContext.User);
                TeamUser teamUser = new TeamUser();
                teamUser.UserId = user.Id;
                teamUser.UserName = user.UserName;

                teamUser.TeamId = team.TeamID;
                _context.Add(teamUser);
                await _context.SaveChangesAsync();
                //}
                return RedirectToAction("Index", "Competitions");
            }
            else
            {
                @ViewData["Show"] = true;
                return RedirectToAction("Join", "Competitions", new { id = team.CompetitionID, check = true });
            }
        }

        private bool CompetitionExists(int id)
        {
            return _context.Competitions.Any(e => e.ID == id);
        }
    }
}
