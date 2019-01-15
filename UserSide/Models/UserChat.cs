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
        public string UserID { get; set; }
            
        public string UserName { get; set; }
        public int GroupID { get; set; }

        public ICollection<Chats> Chats { get; set; }
    }
}
