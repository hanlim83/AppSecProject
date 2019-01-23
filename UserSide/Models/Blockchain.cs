using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserSide.Data;

namespace UserSide.Models
{
    public class Blockchain
    {
        private readonly CompetitionContext _context;

        public IList<Block> Chain { set; get; }
        
        public Blockchain(CompetitionContext context)
        {
            _context = context;
        }

        public async Task<Block> GetLatestBlock()
        {
            Chain = await _context.Blockchain.ToListAsync();
            return Chain[Chain.Count - 1];
        }

        public async Task<IList<Block>> GetChain()
        {
            return await _context.Blockchain.ToListAsync();
        }
    }
}
