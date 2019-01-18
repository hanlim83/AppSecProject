using Microsoft.EntityFrameworkCore;
using UserSide.Models;

namespace UserSide.Data
{
    public class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions<ChatContext> options) : base(options)
        {
        }

        public DbSet<Chats> Chats { get; set; }
        public DbSet<UserChat> UserChats { get; set; }
        public DbSet<GroupChat> GroupChats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chats>().ToTable("Chats");
            modelBuilder.Entity<UserChat>().ToTable("UserChats");
            modelBuilder.Entity<GroupChat>().ToTable("GroupChats");
        }
    }
}
