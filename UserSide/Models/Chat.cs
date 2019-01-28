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
        [Display(Name ="Message not read")]
        public int MsgCount { get; set; }

        [Display(Name ="Chat with")]
        public string UserOne { get; set; }
        public string UserTwo { get; set; }
        public ICollection<Message> Messages { get; set; }
        //public GroupChat GroupChat { get; set; }
        //public int GroupId { get; set; }

    }
}
