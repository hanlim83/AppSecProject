using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UserSide.Models
{
    public class TeamUser
    {
        [Key]
        public int TeamUserID { get; set; }

        public string UserName { get; set; }

        [ForeignKey("TeamId")]
        public int TeamId { get; set; }
        //public Team Team { get; set; }
        public string UserId { get; set; }
    }
}
