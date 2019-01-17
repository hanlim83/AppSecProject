using AdminSide.Areas.PlatformManagement.Models;
using AdminSide.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Data
{
    public class NewsFeedContext :DbContext
    {
        public NewsFeedContext(DbContextOptions<NewsFeedContext> options) : base(options)
        {

        }

        public DbSet<RSSFeed>Feeds { get; set; }
        public DbSet<FeedSource> FeedSources { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RSSFeed>().ToTable("Feeds");
            modelBuilder.Entity<FeedSource>().ToTable("FeedSource");
        }

    }
}
