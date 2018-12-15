using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public enum SubnetType
    {
        Internet, Extranet, Intranet
    }

    public class Subnet
    {

        public int ID { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "A Subnet must have a name")]
        [Display(Name = "Subnet Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Subnet Type")]
        public SubnetType Type { get; set; }
        [Display(Name = "IPv4 CIDR")]
        [Required]
        [RegularExpression(@"172.30.[0-9]{1,3}.[0-9]{1,3}", ErrorMessage = "Please Enter IP CIDR Correctly")]
        public string IPv4CIDR { get; set; }
        [Display(Name = "Subnet Size")]
        public string SubnetSize { get; set; }
        [Display(Name = "IPv6 CIDR")]
        public string IPv6CIDR { get; set; }
        public string AWSVPCSubnetReference { get; set; }

        public ICollection<Server>LinkedServers { get; set; }
    }
}
