using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserSide.Models;
using Microsoft.EntityFrameworkCore;

namespace UserSide.Data
{
    public class CompetitionContext : DbContext
    {
        public CompetitionContext(DbContextOptions<CompetitionContext> options) : base(options)
        {

        }

        public DbSet<Competition> Competitions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Competition>().ToTable("Competition");
        }
    }
}
