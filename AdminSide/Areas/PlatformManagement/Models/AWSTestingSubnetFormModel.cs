using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class AWSTestingSubnetFormModel
    {
        [DisplayName("IP CIDR")]
        [Required]
        public string IPCIDR { get; set; }
    }
}
