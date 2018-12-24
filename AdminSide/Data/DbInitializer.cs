using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Data
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
            new Competition{ID=1, CompetitionName="NYP Global CTF", Status="Active"}
            };

            foreach (Competition c in competition)
            {
                context.Competitions.Add(c);
            }
            context.SaveChanges();
        }
    }
}
