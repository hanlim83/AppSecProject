﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Forum
    {
        public int forumID { get; set; }

        public string userName { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }

        public string Category { get; set; }

        public string Content { get; set; }

        public DateTime DT { get; set; }

        public ICollection<Topic> Topics { get; set; }
    }
}