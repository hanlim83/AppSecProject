
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Forum
    {

        public int ForumID { get; set; }

        public string UserName { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }

        public string Category { get; set; }

        public string Content { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy} | {0:hh:mm tt}", ApplyFormatInEditMode = true)]
        [Display(Name = "Posted On")]
        public DateTime DT { get; set; }

        public ICollection<Topic> Topics { get; set; }

        public ForumCategory LinkedCategory { get; set; }


    }
}
