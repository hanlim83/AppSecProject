using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserSide.Data;
using UserSide.Models;

namespace UserSide.Controllers
{
    [Authorize]
    //the line above makes a page protected and will redirect user back to login
    public class ChallengesController : Controller
    {
        IAmazonS3 S3Client { get; set; }

        private readonly CompetitionContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ChallengesController(CompetitionContext context, IAmazonS3 s3Client, UserManager<IdentityUser> userManager)
        {
            _context = context;
            this.S3Client = s3Client;
            _userManager = userManager;
        }

        // GET: Challenges
        public async Task<IActionResult> Index(int id)
        {
            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .ThenInclude(cc => cc.Challenges)
                .Include(c => c.Teams)
                .ThenInclude(t => t.TeamUsers)
                .Include(c => c.Teams)
                .ThenInclude(t => t.TeamChallenges)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (competition == null)
            {
                return NotFound();
            }

            if (competition.Status.Equals("Upcoming"))
            {
                return RedirectToAction("Index", "Competitions", new { check = true });
            }
            else if (competition.Status.Equals("Active"))
            {
                if (ValidateUserJoined(id).Result == true)
                {
                    if (competition == null)
                    {
                        return NotFound();
                    }
                    //Optimize this next time
                    var competition2 = await _context.Competitions
                    .Include(c => c.Teams)
                    .ThenInclude(t => t.TeamUsers)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.ID == id);

                    var user = await _userManager.GetUserAsync(HttpContext.User);

                    foreach (var Team in competition2.Teams)
                    {
                        foreach (var TeamUser in Team.TeamUsers)
                        {
                            if (TeamUser.UserId.Equals(user.Id))
                            {
                                ViewData["TeamID"] = Team.TeamID;
                            }
                        }
                    }
                    ViewData["Archived"] = false;
                    return View(competition);
                }
                else
                {
                    return RedirectToAction("Index", "Competitions", new { check = true });
                }
            }
            else
            {
                ViewData["Archived"] = true;
                return View(competition);
            }
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

            if (ValidateUserJoined(challenge.CompetitionID).Result == true)
            {
                ViewData["Invalid"] = false;
                ViewData["WrongFlag"] = false;
                return View(challenge);
            }
            else
            {
                return RedirectToAction("Index", "Competitions");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details([Bind("ID, Flag, CompetitionID")] Challenge challenge, int? id)
        {
            if (challenge.Flag == null)
            {
                var temp_challenge1 = await _context.Challenges
                    .FirstOrDefaultAsync(m => m.ID == challenge.ID);

                var competition1 = await _context.Competitions.FindAsync(temp_challenge1.CompetitionID);
                string bucketName = competition1.BucketName;
                var category = await _context.CompetitionCategories.FindAsync(temp_challenge1.CompetitionCategoryID);
                string folderName = category.CategoryName;
                if (temp_challenge1.FileName != null)
                {
                    string fileName = temp_challenge1.FileName;
                    Regex pattern = new Regex("[+]");
                    string tempFileName = pattern.Replace(fileName, "%2B");
                    tempFileName.Replace(' ', '+');
                    ViewData["FileLink"] = "https://s3-ap-southeast-1.amazonaws.com/" + bucketName + "/" + folderName + "/" + tempFileName;
                }
                ViewData["CompetitionID"] = temp_challenge1.CompetitionID;
                ViewData["ChallengeID"] = temp_challenge1.ID;

                ViewData["Invalid"] = true;
                ViewData["WrongFlag"] = false;
                return View(temp_challenge1);
            }

            Team team = null;

            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .ThenInclude(cc => cc.Challenges)
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

            foreach (var teamChallenges in teamChallengesList.TeamChallenges)
            {
                if (teamChallenges.ChallengeId == challenge.ID)
                {
                    if (teamChallenges.Solved == true)
                    {
                        return RedirectToAction("Details", "Challenges", new { id });
                    }
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

            //if (challenge.CompetitionID == null)
            //{
            //    return NotFound();
            //}

            var temp_challenge = await _context.Challenges
                .FirstOrDefaultAsync(m => m.ID == challenge.ID);
            if (temp_challenge == null)
            {
                return NotFound();
            }
            // Flag is correct
            if (challenge.Flag.Equals(temp_challenge.Flag))
            {
                //Add entry to TeamChallenge
                TeamChallenge teamChallenge = new TeamChallenge();
                teamChallenge.ChallengeId = localvarchallenge.ID;
                teamChallenge.TeamId = team.TeamID;
                teamChallenge.Solved = true;
                _context.Add(teamChallenge);
                await _context.SaveChangesAsync();

                //Add entry to chain
                //Block and block data
                Block block = new Block();
                block.TimeStamp = DateTime.Now;
                block.CompetitionID = challenge.CompetitionID;
                block.TeamID = team.TeamID;
                block.ChallengeID = localvarchallenge.ID;
                block.TeamChallengeID = teamChallenge.TeamChallengeID;
                block.Score = localvarchallenge.Value;
                //Previous Hash
                Blockchain blockchain = new Blockchain(_context);
                Block latestBlock = await blockchain.GetLatestBlock();
                block.PreviousHash = latestBlock.Hash;
                //Current Hash
                string data = block.TimeStamp + ";" + block.CompetitionID + ";" + block.TeamID + ";" + block.ChallengeID + ";" + block.TeamChallengeID + ";" + block.Score + ";" + block.PreviousHash;
                block.Hash = GenerateSHA512String(data);

                _context.Add(block);
                await _context.SaveChangesAsync();

                //Add points to team score
                team.Score += localvarchallenge.Value;
                _context.Update(team);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Challenges", new { id = challenge.CompetitionID });
            }
            else
            {
                //Wrong flag
                TeamChallenge teamChallenge = new TeamChallenge();
                teamChallenge.ChallengeId = localvarchallenge.ID;
                teamChallenge.TeamId = team.TeamID;
                teamChallenge.Solved = false;
                _context.Add(teamChallenge);
                await _context.SaveChangesAsync();

                var Challenge = await _context.Challenges
                    .FirstOrDefaultAsync(m => m.ID == id);
                if (Challenge == null)
                {
                    return NotFound();
                }
                //Stop field from being populated at View
                Challenge.Flag = null;

                var Competition = await _context.Competitions.FindAsync(Challenge.CompetitionID);
                string bucketName = Competition.BucketName;
                var category = await _context.CompetitionCategories.FindAsync(Challenge.CompetitionCategoryID);
                string folderName = category.CategoryName;
                if (Challenge.FileName != null)
                {
                    string fileName = Challenge.FileName;
                    Regex pattern = new Regex("[+]");
                    string tempFileName = pattern.Replace(fileName, "%2B");
                    tempFileName.Replace(' ', '+');
                    ViewData["FileLink"] = "https://s3-ap-southeast-1.amazonaws.com/" + bucketName + "/" + folderName + "/" + tempFileName;
                }
                ViewData["CompetitionID"] = Challenge.CompetitionID;
                ViewData["ChallengeID"] = Challenge.ID;

                //if (ValidateUserJoined(Challenge.CompetitionID).Result == true)
                //{
                ViewData["Invalid"] = false;
                ViewData["WrongFlag"] = true;
                return View(Challenge);
                //}
            }
        }

        private static string GenerateSHA512String(string inputString)
        {
            SHA512 sha512 = SHA512.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private async Task<bool> ValidateUserJoined(int id)
        {
            var competition = await _context.Competitions
                .Include(c => c.Teams)
                .ThenInclude(t => t.TeamUsers)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            var user = await _userManager.GetUserAsync(HttpContext.User);

            foreach (var Team in competition.Teams)
            {
                foreach (var TeamUser in Team.TeamUsers)
                {
                    if (TeamUser.UserId.Equals(user.Id))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ChallengeExists(int id)
        {
            return _context.Challenges.Any(e => e.ID == id);
        }
    }
}
