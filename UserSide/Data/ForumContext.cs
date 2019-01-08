using Microsoft.EntityFrameworkCore;
using UserSide.Models;

namespace UserSide.Data
{
    public class ForumContext : DbContext
    {
        public ForumContext(DbContextOptions<ForumContext> options) : base(options)
        {

        }

        public DbSet<Forum> Forums { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Forum>().ToTable("Forum");
            modelBuilder.Entity<Topic>().ToTable("Topic");
            modelBuilder.Entity<Comment>().ToTable("Comment");
        }
    }
}
