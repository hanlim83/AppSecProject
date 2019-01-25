using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class UserChat
    {
        [Key]
        public string UserId { get; set; }

        public string UserOne{ get; set; }

        public string UserTwo { get; set; }

        public ICollection<Chat> Chats { get; set; }
    }
}
