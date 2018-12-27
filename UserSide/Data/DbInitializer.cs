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
        public static void InitializePlatformResources (CompetitionContext context)
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
    }
}
