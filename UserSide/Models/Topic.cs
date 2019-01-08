using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Topic
    {
        public int TopicID { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }

        public string Category { get; set; }

        public string Content { get; set; }

        public int ForumID { get; set; }

        public Forum LinkedForum { get; set; }

        public ForumCategory LinkedCategory { get; set; }

        public ICollection<Comment> Comments { get; set; }

    }
}
