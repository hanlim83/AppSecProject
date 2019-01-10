using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminSide.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminSide.Data
{
    public class CompetitionContext : DbContext
    {
        public CompetitionContext(DbContextOptions<CompetitionContext> options) : base(options)
        {
            
        }

        public DbSet<Competition> Competitions { get; set; }
        public DbSet<CompetitionCategory> CompetitionCategories { get; set; }
        public DbSet<CategoryDefault> CategoryDefault { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Team> Teams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Competition>().ToTable("Competition");
            //modelBuilder.Entity<Competition>()
            //    .HasOne<CompetitionCategory>(c => c.ID)
            //    .WithMany(g => g.CompetitionID);
            modelBuilder.Entity<CompetitionCategory>().ToTable("CompetitionCategory");
            modelBuilder.Entity<CategoryDefault>().ToTable("CategoryDefault");
            modelBuilder.Entity<Challenge>().ToTable("Challenges");
            modelBuilder.Entity<Team>().ToTable("Teams");
        }
    }
}
