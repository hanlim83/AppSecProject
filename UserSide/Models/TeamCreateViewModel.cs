using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class TeamCreateViewModel
    {
        public Competition Competition { get; set; }
        public Team Team { get; set; }
        public TeamUser TeamUser { get; set; }
    }
}
