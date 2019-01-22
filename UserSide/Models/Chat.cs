using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class Chat
    {
        [Key]
        public int ChatID { get; set; }
        public int MsgCount { get; set; }

        public UserChat UserChat {get; set;}
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        //public GroupChat GroupChat { get; set; }
        //public int GroupId { get; set; }

    }
}
