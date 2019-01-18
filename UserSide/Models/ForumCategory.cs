using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class ForumCategory
    {
        [Key]
        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public ICollection<Post> Posts { get; set; }

        public string Secret { get; set; }

    }
}
