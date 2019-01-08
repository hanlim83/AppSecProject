using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class ForumCategory
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public ICollection<Forum> Forums { get; set; }
        public ICollection<Topic> Topics { get; set; }
    }
}
