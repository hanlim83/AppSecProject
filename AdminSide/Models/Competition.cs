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
        [Key]
        public int ID { get; set; }
        [Required]
        [Display(Name = "Competition Name")]
        public string CompetitionName { get; set; }
        [Required]
        public string Status { get; set; }
        //[Required]
        //Add regex here
        [Display(Name = "Bucket Name")]
        public string BucketName { get; set; }

        //Take in start time/end time, may deprecate status

        public ICollection<CompetitionCategory> CompetitionCategories { get; set; }
        public ICollection<Challenge> Challenges { get; set; }
        public ICollection<Team> Teams { get; set; }
    }
}
