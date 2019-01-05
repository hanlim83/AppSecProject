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
        public string Name { get; set; }
        //public string Category { get; set; }
        //To check if this^ is needed as it can be dynamically retrieved to the binded category
        public string Description { get; set; }
        public int Value { get; set; }
        public string Flag { get; set; }

        [ForeignKey("CompetitionID")]
        public int CompetitionID { get; set; }
        [ForeignKey("CompetitionCategoryID")]
        public int CompetitionCategoryID { get; set; }
    }
}
