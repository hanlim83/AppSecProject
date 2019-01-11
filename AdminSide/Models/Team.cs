using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public class Team
    {
        [Key]
        public int TeamID { get; set; }

        [Required]
        [DisplayName("Team Name")]
        public string TeamName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        //[Display(Name = "Remember me?")]
        //public bool RememberMe { get; set; }

        [ForeignKey("CompetitionID")]
        public int CompetitionID { get; set; }
    }
}
