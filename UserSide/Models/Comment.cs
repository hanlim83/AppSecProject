using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Comment
    {
        public int CommentID { get; set; }

        public string UserName { get; set; }

        public int TopicID { get; set; }

        public string Content { get; set; }

        public DateTime DT { get; set; }

        public Topic LinkedTopic { get; set; }
    }
}
