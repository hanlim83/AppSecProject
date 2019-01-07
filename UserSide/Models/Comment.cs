using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Comment
    {
        public string userName { get; set; }

        public int topicID { get; set; }

        public string Content { get; set; }

        public DateTime DT { get; set; }

        public Topic linkedTopic { get; set; }
    }
}
