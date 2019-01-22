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
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupMember { get; set; }

        public ICollection<Chat> Chats { get; set; }
    }
}
