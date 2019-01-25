using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class CompetitionCategory
    {
        public int ID { get; set; }
        [Required]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [ForeignKey("CompetitionID")]
        public int CompetitionID { get; set; }

        public ICollection<Challenge> Challenges { get; set; }
    }
}
