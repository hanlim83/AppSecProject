﻿using System;
using System.ComponentModel.DataAnnotations;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public enum TemplateType
    {
        Default,Custom
    }
    public class Template
    {
        public int ID { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "A template must have a name")]
        [Display(Name = "Template Name")]
        [RegularExpression(@"^[A-Za-z0-9 _-]*[A-Za-z0-9][A-Za-z0-9 _ -]*$", ErrorMessage = "Please use only alphanumeric characters, dashes and underscores only")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Template Type")]
        public TemplateType Type { get; set; }
        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateCreated { get; set; }
        [Display(Name = "Operating System")]
        public string OperatingSystem { get; set; }
        public string AWSAMIReference { get; set; }
        [Display(Name = "Template Description")]
        [RegularExpression(@"^[A-Za-z0-9 _-]*[A-Za-z0-9][A-Za-z0-9 _ -]*$", ErrorMessage = "Please use only alphanumeric characters, dashes and underscores only")]
        public string TemplateDescription { get; set; }
        public bool SpecificMinimumSize { get; set; }
        public int MinimumStorage { get; set; }
        public string AWSSnapshotReference { get; set; }
    }
}
