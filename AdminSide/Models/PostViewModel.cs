using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public class PostViewModel
    {
        public IEnumerable<ForumCategory> ForumCategory { get; set; }
        public Post post { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
