using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class LiveChat
    {
        [Key]
        public int ChatID { get; set; }

        public DateTime dateTime { get; set; }
        public string Username { get; set; }
        public string UserID { get; set; }



    }
}
