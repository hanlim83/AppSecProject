using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public class TeamUser
    {
        [Key]
        public int TeamUserID { get; set; }

        public int TeamId { get; set; }
        public string UserId { get; set; }
    }
}
