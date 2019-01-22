using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminSide.Data;
using AdminSide.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminSide.Controllers
{
    public class BlockchainController : Controller
    {
        private readonly CompetitionContext _context;

        public BlockchainController(CompetitionContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            Blockchain blockchain = new Blockchain(_context);
            Block latestBlock = await blockchain.GetLatestBlock();
            ViewData["LastHash"] = latestBlock.Hash;
            var IsValid = blockchain.IsValid();
            ViewData["ChainValid"] = IsValid;
            return View();
        }
    }
}