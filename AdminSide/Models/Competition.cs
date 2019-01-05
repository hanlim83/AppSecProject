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
        //May change BucketName to be dynamic next time
        //To automate bucket naming creation
        //[Required]
        //Add regex here
        [Display(Name = "Bucket Name")]
        public string BucketName { get; set; }

        public ICollection<CompetitionCategory> CompetitionCategories { get; set; }
        public ICollection<Challenge> Challenges { get; set; }
    }
}
