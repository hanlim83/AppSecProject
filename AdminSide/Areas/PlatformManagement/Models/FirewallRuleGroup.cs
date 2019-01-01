using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class FirewallRuleGroup
    {
        [Key]
        public int ServerID { get; set; }

        
        public Server Server { get; set; }
    }
}
