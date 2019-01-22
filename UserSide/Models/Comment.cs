using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Comment
    {
        [Key]
        public int CommentID { get; set; }

        public string UserName { get; set; }

        public string Content { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy} | {0:hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime DT { get; set; }

        [ForeignKey("PostID")]
        public int PostID { get; set; }

        public Post LinkedPost { get; set; }

        public string Secret { get; set; }
    }
}
