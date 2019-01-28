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

            if (context.ForumCategories.Any())
            {
                return;   // DB has been seeded
            }
        }

        public static void InitializeChat(ChatContext context)
        {
            context.Database.EnsureCreated();

            if (context.Chats.Any())
            {
                return;   // DB has been seeded
            }

            //var Chat = new UserChat[]
            //{
            //    new UserChat{UserId="1",UserName="Stephen Curry"},
            //    new UserChat{UserId="2",UserName="Lebron James"}
            //};

            //foreach (UserChat c in Chat)
            //{
            //    context.UserChats.Add(c);
            //}
            //context.SaveChanges();

            //var Group = new GroupChat[]
            //{
            //    new GroupChat{GroupId=1,GroupName="Golden State Warriors",GroupMember="Brother"},
            //    new GroupChat{GroupId=2,GroupName="Los Angeles Laker",GroupMember="sister"}
            //};

            //foreach (GroupChat g in Group)
            //{
            //    context.GroupChats.Add(g);
            //}
            //context.SaveChanges();
            var text = new Chat[]
           {
                new Chat{UserOne="Stephen",UserTwo="James",MsgCount=12},
                new Chat{UserOne="James",UserTwo="Stephen",MsgCount=2}
           };

            foreach (Chat t in text)
            {
                context.Chats.Add(t);
            }

            var Talk = new Message[]
            {
                new Message{Sender="James",Receiver="Stephen",Messsage="Hello ",ChatId=1},
                new Message{Sender="Stephen",Receiver="James",Messsage="I AM THE GOAT",ChatId=2}
            };

            foreach (Message m in Talk)
            {
                context.Messages.Add(m);
            }
            context.SaveChanges();

           
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
