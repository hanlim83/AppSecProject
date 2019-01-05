using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public class CategoriesViewModelIEnumerable
    {
        public Competition Competition { get; set; }
        public CompetitionCategory CompetitionCategory { get; set; }
        public CategoryDefault CategoryDefault { get; set; }

        //From Microsoft

        public IEnumerable<string> SelectedCategories { get; set; }
        
        public List<SelectListItem> CategoriesList { get; } = new List<SelectListItem>
        {
            
         };

        //From Microsoft
    }
}
