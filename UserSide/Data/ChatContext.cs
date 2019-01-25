using Microsoft.EntityFrameworkCore;
using UserSide.Models;

namespace UserSide.Data
{
    public class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions<ChatContext> options) : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<UserChat> UserChats { get; set; }
       // public DbSet<GroupChat> GroupChats { get; set; }
        public DbSet<Chat> Chats { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>().ToTable("Messages");
           // modelBuilder.Entity<GroupChat>().ToTable("GroupChats");
            modelBuilder.Entity<UserChat>().ToTable("UserChats");
            modelBuilder.Entity<Chat>().ToTable("Chats");
        }
    }
}
