using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Chats
    {
        [Key]
        public string ChatID { get; set; }

        public string UserID { get; set; }
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string Messsage { get; set; }
    }
}
