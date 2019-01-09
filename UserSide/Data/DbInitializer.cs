using UserSide.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserSide.Data;

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

            if (context.Forums.Any())
            {
                return;   // DB has been seeded
            }

            var category = new ForumCategory[]
            {
            new ForumCategory{CategoryID=1, CategoryName="General"},
            new ForumCategory{CategoryID=1, CategoryName="Crypto"}
            };

            foreach (ForumCategory c in category)
            {
                context.ForumCategories.Add(c);
            }
            context.SaveChanges();

            var forum = new Forum[]
            {
            new Forum{ Title="Errors", Content="How To Fix", UserName="Elxxwy", CategoryID=1 },
            new Forum{ Title="General", Content="How To Do", UserName="Eevee", CategoryID=2 },
            new Forum{ Title="Errors", Content="How To UnFix", UserName="Eevee", CategoryID=1 },
            new Forum{ Title="General", Content="How To Undo", UserName="Elxxwy", CategoryID=2 },
            };

            foreach (Forum f in forum)
            {
                context.Forums.Add(f);
            }
            context.SaveChanges();
        }
    }
}
