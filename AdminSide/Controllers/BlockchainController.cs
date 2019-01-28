using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AdminSide.Data;
using AdminSide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminSide.Controllers
{
    [Authorize]
    //the line above makes a page protected and will redirect user back to login
    public class BlockchainController : Controller
    {
        private readonly CompetitionContext _context;

        public class Results
        {
            public string TeamName { get; set; }
            [Display(Name = "Blockchain Score")]
            public int BlockchainScore { get; set; }
            [Display(Name = "Team Score")]
            public int TeamScore { get; set; }
        }

        public BlockchainController(CompetitionContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competition = await _context.Competitions
                .Include(c => c.CompetitionCategories)
                .ThenInclude(cc => cc.Challenges)
                //.Include(c1 => c1.Challenges)
                .Include(c => c.Teams)
                .ThenInclude(t => t.TeamUsers)
                .Include(c => c.Teams)
                .ThenInclude(t => t.TeamChallenges)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            Blockchain blockchain = new Blockchain(_context);
            Block latestBlock = await blockchain.GetLatestBlock();
            ViewData["LastHash"] = latestBlock.Hash;
            var IsValid = blockchain.IsValid();
            ViewData["ChainValid"] = IsValid;

            var chain = blockchain.GetChain().Result;

            List<Results> IncorrectTeams = new List<Results>();

            foreach (var Team in competition.Teams)
            {
                var score = 0;
                //get all transactions for particular team
                foreach (var block in chain)
                {
                    if (block.TeamID == Team.TeamID)
                    {
                        score += block.Score;
                    }
                }
                if (Team.Score != score)
                {
                    //false
                    Results result = new Results();
                    result.TeamName = Team.TeamName;
                    result.BlockchainScore = score;
                    result.TeamScore = Team.Score;
                    IncorrectTeams.Add(result);
                }
                else
                {
                    //true
                }
            }
            ViewBag.ResultList = IncorrectTeams;
            ViewData["CompetitionID"] = id;
            return View(blockchain);
        }

        //Testing edit for "Attack"
        // GET: Competitions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var block = await _context.Blockchain.FindAsync(id);

            return View(block);
        }


        // POST: Competitions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ID, TimeStamp, CompetitionID, TeamID, ChallengeID, TeamChallengeID, Score, PreviousHash, Hash")] Block block)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(block);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlockExists(block.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Blockchain", new { id = block.CompetitionID });
            }
            return RedirectToAction("Index", "Blockchain", new { id = block.CompetitionID });
        }

        private bool BlockExists(int id)
        {
            return _context.Blockchain.Any(e => e.ID == id);
        }
    }
}