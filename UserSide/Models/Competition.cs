using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Competition
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [Display(Name = "Competition Name")]
        public string CompetitionName { get; set; }
        [Required]
        public string Status { get; set; }
        [Display(Name = "Bucket Name")]
        public string BucketName { get; set; }

        public ICollection<CompetitionCategory> CompetitionCategories { get; set; }
        //public ICollection<Challenge> Challenges { get; set; }
        public ICollection<Team> Teams { get; set; }
    }
}
