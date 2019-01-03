using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public class CategoriesViewModelIEnumerable
    {
        public Competition competition { get; set; }
        public CompetitionCategory competitionCategory { get; set; }

        //From Microsoft

        public IEnumerable<string> SelectedCategories { get; set; }

        public List<SelectListItem> CategoriesList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "volvo", Text = "Volvo" },
            new SelectListItem { Value = "saab", Text = "Saab" },
            new SelectListItem { Value = "fiat", Text = "Fiat" },
            new SelectListItem { Value = "audi", Text = "Audi" },
         };

        //From Microsoft
    }
}
