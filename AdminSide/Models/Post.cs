using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public class Post
    {
        [Key]
        public int PostID { get; set; }

        [Display(Name = "Posted By")]
        public string UserName { get; set; }

        [StringLength(50, MinimumLength = 5)]
        public string Title { get; set; }

        public string Content { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy} | {0:hh:mm tt}", ApplyFormatInEditMode = true)]
        [Display(Name = "Posted On")]
        public DateTime DT { get; set; }

        [ForeignKey("CategoryID")]
        public int CategoryID { get; set; }
        //public string CategoryName { get; set; }

        public ICollection<Comment> Comments { get; set; }

        //public ForumCategory LinkedCategory { get; set; }

        public string Secret { get; set; }

    }
}
