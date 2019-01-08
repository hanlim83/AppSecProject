using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public class ChallengesViewModelIEnumerable
    {
        public Competition Competition { get; set; }
        public CompetitionCategory CompetitionCategory { get; set; }
        public Challenge Challenge { get; set; }

        public List<SelectListItem> CategoriesList { get; } = new List<SelectListItem>
        {

        };
    }
}
