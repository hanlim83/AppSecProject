﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    //public enum Status
    //{
    //    Upcoming, Active, Inactive
    //}

    public class Competition
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [Display(Name = "Competition Name")]
        public string CompetitionName { get; set; }
        public string Status { get; set; }
        //[Required]
        //Add regex here
        [Display(Name = "Bucket Name")]
        public string BucketName { get; set; }
        
        public ICollection<CompetitionCategory> CompetitionCategories { get; set; }
        public ICollection<Team> Teams { get; set; }
    }
}
