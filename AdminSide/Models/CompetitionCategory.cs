using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public class CompetitionCategory
    {
        public int ID { get; set; }
        [Required]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }
        
        //public ICollection<Competition> Competitions { get; set; }
    }
}
