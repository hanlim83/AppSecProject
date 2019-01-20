﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public class TeamChallenge
    {
        [Key]
        public int TeamChallengeID { get; set; }

        [ForeignKey("TeamId")]
        public int TeamId { get; set; }
        [ForeignKey("ChallengeId")]
        public int ChallengeId { get; set; }
    }
}
