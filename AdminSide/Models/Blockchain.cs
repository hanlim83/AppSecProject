using AdminSide.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AdminSide.Models
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

        public bool IsValid()
        {
            //Chain = await _context.Blockchain.ToListAsync();
            for (int i = 1; i < Chain.Count; i++)
            {
                Block currentBlock = Chain[i];
                Block previousBlock = Chain[i - 1];

                string data = currentBlock.TimeStamp + ";" + currentBlock.CompetitionID + ";" + currentBlock.TeamID
                    + ";" + currentBlock.ChallengeID + ";" + currentBlock.TeamChallengeID + ";" + currentBlock.Score
                    + ";" + currentBlock.PreviousHash;
                if (currentBlock.Hash != GenerateSHA512String(data))
                {
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }
            }
            return true;
        }

        private static string GenerateSHA512String(string inputString)
        {
            SHA512 sha512 = SHA512.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
