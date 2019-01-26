using System;
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
            return View();
        }
    }
}