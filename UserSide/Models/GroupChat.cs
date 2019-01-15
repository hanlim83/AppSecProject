using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class GroupChat
    {
        [Key]
        public int GroupID { get; set; }
        
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string GroupName { get; set; }

        public ICollection<Chats> Chats { get; set; }
        public ICollection<UserChat> userChats { get; set; }
    }
}
