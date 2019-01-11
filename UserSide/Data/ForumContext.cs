using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserSide.Models;
using Microsoft.EntityFrameworkCore;

namespace UserSide.Data
{
    public class ForumContext : DbContext
    {
        public ForumContext(DbContextOptions<ForumContext> options) : base(options)
        {

        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ForumCategory> ForumCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>().ToTable("Post");
            modelBuilder.Entity<Comment>().ToTable("Comment");
            modelBuilder.Entity<ForumCategory>().ToTable("ForumCategory");
        }
    }
}
