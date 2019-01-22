using UserSide.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserSide.Data;
using Microsoft.AspNetCore.Identity;

namespace UserSide.Data
{
    public class DbInitializer
    {
        public static void InitializePlatformResources(CompetitionContext context)
        {
            context.Database.EnsureCreated();

            if (context.Competitions.Any())
            {
                return;   // DB has been seeded
            }

            var competition = new Competition[]
            {
            new Competition{CompetitionName="NYP Global CTF", Status="Active"}
            };

            foreach (Competition c in competition)
            {
                context.Competitions.Add(c);
            }
            context.SaveChanges();

            var competitionCategory = new CompetitionCategory[]
            {
            new CompetitionCategory{ CategoryName="Web" },
            new CompetitionCategory{ CategoryName="Crypto" },
            new CompetitionCategory{ CategoryName="Forensics" },
            new CompetitionCategory{ CategoryName="Misc" }
            };

            foreach (CompetitionCategory cc in competitionCategory)
            {
                context.CompetitionCategories.Add(cc);
            }
            context.SaveChanges();
        }

        public static void InitializeForum(ForumContext context)
        {
            context.Database.EnsureCreated();

            if (context.Posts.Any())
            {
                return;   // DB has been seeded
            }

            var category = new ForumCategory[]
            {
            new ForumCategory{CategoryName="General"},
            new ForumCategory{CategoryName="Crypto"}
            };

            foreach (ForumCategory c in category)
            {
                context.ForumCategories.Add(c);
            }
            context.SaveChanges();

            var post = new Post[]
            {
            new Post{ Title="Errors", Content="How To Fix", UserName="Elxxwy", CategoryID=1 },
            new Post{ Title="General", Content="How To Do", UserName="Eevee", CategoryID=2 },
            new Post{ Title="Errors", Content="How To UnFix", UserName="EVELYN", CategoryID=1 },
            new Post{ Title="General", Content="How To Undo", UserName="Elxxwy", CategoryID=2 },
            };

            foreach (Post p in post)
            {
                context.Posts.Add(p);
            }
            context.SaveChanges();

        }

        public static void InitializeChat(ChatContext context)
        {
            context.Database.EnsureCreated();

            if (context.UserChats.Any())
            {
                return;   // DB has been seeded
            }

            var Chat = new UserChat[]
            {
                new UserChat{UserID="100",UserName="yolo",},
                new UserChat{UserID="101",UserName="Tamny"}
            };

            foreach (UserChat c in Chat)
            {
                context.UserChats.Add(c);
            }
            context.SaveChanges();

            var Group = new GroupChat[]
            {
                new GroupChat{GroupID=888,GroupName="PowerMan"},
                new GroupChat{GroupID=999,GroupName="good try"}
            };

            foreach (GroupChat c in Group)
            {
                context.GroupChats.Add(c);
            }
            context.SaveChanges();

        }

        public static void InitializeIdentity(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            //if (context.AspNetUsers.Any())
            //{
            //    return;   // DB has been seeded
            //}

            //var user = new IdentityUser
            //{
            //    UserName = "hugochiaxyz@gmail.com",
            //    Email = "hugochiaxyz@gmail.com",
            //    NormalizedEmail = "hugochiaxyz@gmail.com".ToUpper(),
            //    NormalizedUserName = "hugochiaxyz@gmail.com".ToUpper(),
            //};

            //PasswordHasher<IdentityUser> ph = new PasswordHasher<IdentityUser>();
            //user.PasswordHash = ph.HashPassword(user, "Pass123!");

            //context.AspNetUsers.Add(user);

            //context.SaveChanges();
        }
    }
}
