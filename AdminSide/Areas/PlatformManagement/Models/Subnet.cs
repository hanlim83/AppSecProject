﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        [RegularExpression(@"^[A-Za-z0-9 _-]*[A-Za-z0-9][A-Za-z0-9 _ -]*$",ErrorMessage = "Please use only alphanumeric characters, dashes and underscores only")]
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
        public bool editable { get; set; }

        public virtual RouteTable LinkedRT { get; set; }
        public int? RouteTableID { get; set; }
        public string AWSVPCRouteTableAssoicationID { get; set; }

        public int VPCID { get; set; }
        public virtual VPC LinkedVPC { get; set; }

        public virtual ICollection<Server>LinkedServers { get; set; }
    }
}
