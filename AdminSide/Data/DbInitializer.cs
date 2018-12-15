using AdminSide.Areas.PlatformManagement.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Data
{
    public class DbInitializer
    {
        public static void InitializePlatformResources (PlatformResourcesContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}
