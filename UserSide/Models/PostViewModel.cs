using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class PostViewModel
    {
        public Post Post { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
