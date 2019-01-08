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
        public DbSet<CompetitionCategory> CompetitionCategories { get; set; }
        public DbSet<Challenge> Challenges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Competition>().ToTable("Competition");
            modelBuilder.Entity<CompetitionCategory>().ToTable("CompetitionCategory");
            modelBuilder.Entity<Challenge>().ToTable("Challenges");
        }

        public DbSet<UserSide.Models.Challenge> Challenge { get; set; }
    }
}
