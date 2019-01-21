using Microsoft.EntityFrameworkCore;
using UserSide.Models;

namespace UserSide.Data
{
    public class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions<ChatContext> options) : base(options)
        {
        }

        public DbSet<Chat> Chats { get; set; }
        public DbSet<UserChat> UserChats { get; set; }
        public DbSet<GroupChat> GroupChats { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chat>().ToTable("Chats");
            modelBuilder.Entity<GroupChat>().ToTable("GroupChats");
            modelBuilder.Entity<UserChat>().ToTable("UserChats");
        }
    }
}
