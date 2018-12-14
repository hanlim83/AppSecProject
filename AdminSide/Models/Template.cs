﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public class Template
    {
        public int ID { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "A template must have a name")]
        [Display(Name = "Template Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateCreated { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Operating System")]
        public string OperatingSystem { get; set; }
        [Required]
        public string AWSAMIReference { get; set; }
    }
}
