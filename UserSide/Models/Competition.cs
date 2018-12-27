using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Competition
    {
        public int ID { get; set; }
        [Display(Name = "Competition Name")]
        public string CompetitionName { get; set; }
        public string Status { get; set; }

        //public ICollection<Competition> Competitions { get; set; }
    }
}
