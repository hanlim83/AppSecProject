using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdminSide.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<IdentityUser> AspNetUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            //IdentityUser identityUser = new IdentityUser
            //{
            //    //UserName = "tester",
            //    Email = "hugochiaxyz@gmail.com",
            //    //NormalizedEmail = "tester@test.com".ToUpper(),
            //    //NormalizedUserName = "tester".ToUpper(),
            //    //TwoFactorEnabled = false,
            //    //EmailConfirmed = true,
            //    //PhoneNumber = "123456789",
            //    //PhoneNumberConfirmed = false
            //};

            //PasswordHasher<IdentityUser> ph = new PasswordHasher<IdentityUser>();
            //identityUser.PasswordHash = ph.HashPassword(identityUser, "Pass123!");

            modelBuilder.Entity<IdentityRole>().ToTable("ASPNETIdentityAdmin");
        }
    }
}
