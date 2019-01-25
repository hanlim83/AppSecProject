using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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

        //[ForeignKey("CompetitionID")]
        public int CompetitionID { get; set; }
        [Display(Name = "Competition Category")]
        //[ForeignKey("CompetitionCategoryID")]
        public int CompetitionCategoryID { get; set; }
    }
}
