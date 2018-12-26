using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    /*public enum StatusType
    {
        Starting, Active, Inactive
    }*/

    public class Competition
    {
        public int ID { get; set; }
        [Display(Name = "Competition Name")]
        public string CompetitionName { get; set; }
        public string Status { get; set; }

        public ICollection<CompetitionCategory> CompetitionCategories { get; set; }
    }
}
