using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class ForumViewModel
    {
        public List<ForumCategory> ForumCategory { get; set; }
        public List<Post> Posts { get; set; }

    }
}
