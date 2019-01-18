using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AdminSide.Models
{
    public class RSSFeed
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public String Description { get; set; }
        public string PubDate { get; set; }

        public Boolean main { get; set; }
  
    }
}
