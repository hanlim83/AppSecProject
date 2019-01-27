using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdminSide.Models
{
    public class Challenge
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        //[Range(0, int.MaxValue, ErrorMessage = "Please enter valid integer Number")]
        [Range(1, 9999, ErrorMessage = "Please enter valid score between 1 - 9999")]
        public int Value { get; set; }
        [Required]
        public string Flag { get; set; }
        public string FileName { get; set; }

        public int CompetitionID { get; set; }
        [Display(Name = "Competition Category")]
        public int CompetitionCategoryID { get; set; }

        //public ICollection<TeamChallenge> TeamChallenges { get; set; }
    }
}
