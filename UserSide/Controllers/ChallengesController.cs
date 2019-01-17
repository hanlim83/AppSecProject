using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UserSide.Data;
using UserSide.Models;

namespace UserSide.Controllers
{
    public class ChallengesController : Controller
    {
        IAmazonS3 S3Client { get; set; }

        private readonly CompetitionContext _context;

        public ChallengesController(CompetitionContext context, IAmazonS3 s3Client)
        {
            _context = context;
            this.S3Client = s3Client;
        }

        // GET: Challenges
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .Include(c1 => c1.Challenges)
                .Include(c => c.Teams)
                .ThenInclude(t => t.TeamUsers)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (competition == null)
            {
                return NotFound();
            }

            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            foreach (var Team in competition.Teams)
            {
                foreach (var TeamUser in Team.TeamUsers)
                {
                    if (TeamUser.UserId.Equals(userId))
                    {
                        return View(competition);
                    }
                }
            }
            return RedirectToAction("Index", "Competitions");
        }

        // GET: Challenges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challenge = await _context.Challenges
                .FirstOrDefaultAsync(m => m.ID == id);
            if (challenge == null)
            {
                return NotFound();
            }
            //Stop field from being populated at View
            challenge.Flag = null;

            var competition = await _context.Competitions.FindAsync(challenge.CompetitionID);
            string bucketName = competition.BucketName;
            var category = await _context.CompetitionCategories.FindAsync(challenge.CompetitionCategoryID);
            string folderName = category.CategoryName;
            if (challenge.FileName != null)
            {
                string fileName = challenge.FileName;
                Regex pattern = new Regex("[+]");
                string tempFileName = pattern.Replace(fileName, "%2B");
                tempFileName.Replace(' ', '+');
                ViewData["FileLink"] = "https://s3-ap-southeast-1.amazonaws.com/" + bucketName + "/" + folderName + "/" + tempFileName;
            }
            ViewData["CompetitionID"] = challenge.CompetitionID;
            ViewData["ChallengeID"] = challenge.ID;


            return View(challenge);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details([Bind("ID, Flag, CompetitionID")] Challenge challenge, int? id)
        {
            Team team = null;

            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .Include(c => c.Challenges)
                .Include(c => c.Teams)
                .ThenInclude(t => t.TeamUsers)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == challenge.CompetitionID);

            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            foreach (var Team in competition.Teams)
            {
                foreach (var TeamUser in Team.TeamUsers)
                {
                    if (TeamUser.UserId.Equals(userId))
                    {
                        team = Team;
                        break;
                    }
                }
            }

            //Get all challenges this team has solved
            var teamChallengesList = await _context.Teams
                .Include(t => t.TeamChallenges)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.TeamID == team.TeamID);

            foreach(var teamChallenges in teamChallengesList.TeamChallenges)
            {
                if (teamChallenges.ChallengeId == challenge.ID)
                {
                    return RedirectToAction("Details", "Challenges", new { id });
                }
            }

            var localvarchallenge = await _context.Challenges
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == challenge.ID);

            //if (ModelState.IsValid)
            //{
            //    _context.Add(challenge);
            //    await _context.SaveChangesAsync();
            //    return RedirectToAction(nameof(Index));
            //}
            if (challenge.CompetitionID == null)
            {
                return NotFound();
            }

            var temp_challenge = await _context.Challenges
                .FirstOrDefaultAsync(m => m.ID == challenge.ID);
            if (temp_challenge == null)
            {
                return NotFound();
            }

            if (challenge.Flag.Equals(temp_challenge.Flag))
            {
                //Flag is correct
                //Add entry to TeamChallenge
                TeamChallenge teamChallenge = new TeamChallenge();
                teamChallenge.ChallengeId = localvarchallenge.ID;
                teamChallenge.TeamId = team.TeamID;
                _context.Add(teamChallenge);
                await _context.SaveChangesAsync();

                //Add points to team score
                team.Score += localvarchallenge.Value;
                //team.TeamChallenges = new Collection<TeamChallenge>();
                //team.TeamChallenges.Add(teamChallenge);
                _context.Update(team);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Challenges", new { id = challenge.CompetitionID });
            }
            //Wrong flag
            return RedirectToAction("Details", "Challenges", new { id });
        }

        private bool ChallengeExists(int id)
        {
            return _context.Challenges.Any(e => e.ID == id);
        }
    }
}
