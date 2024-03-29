﻿using System;
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
        
        [DisplayName("Team Name")]
        public string TeamName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Salt { get; set; }
        public int Score { get; set; }

        [ForeignKey("CompetitionID")]
        public int CompetitionID { get; set; }

        public ICollection<TeamUser> TeamUsers { get; set; }
        public ICollection<TeamChallenge> TeamChallenges { get; set; }
    }
}
