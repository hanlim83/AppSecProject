using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Chat
    {
        [Key]
        public int ChatID { get; set; }

        public string SendRec { get; set; }
       
        public string Messsage { get; set; }

        public int Count { get; set; }
    }
}
