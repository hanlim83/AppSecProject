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
        public static void InitializeForum(ForumContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}
