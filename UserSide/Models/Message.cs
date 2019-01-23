using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Message
    {
        [Key]
        public int MsgId { get; set; }
    
        public string Receiver { get; set; }

        public string Messsage { get; set; }

        public int Count { get; set; }

        public string Sender { get; set; }

        public Chat Chat { get; set; }
        public int ChatId { get; set; }
    }
}
