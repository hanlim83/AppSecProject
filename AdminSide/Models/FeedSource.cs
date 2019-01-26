using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public class FeedSource
    {
        public int ID { get; set; }

        [Display(Name = "Source Name")]
        public string sourceName { get; set; }

        [Display(Name = "Source URL")]
        public string sourceURL { get; set; }
    }
}
