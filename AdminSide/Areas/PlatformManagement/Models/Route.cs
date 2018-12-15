using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public enum RouteType
    {
        Mandatory,Optional
    }
    public class Route
    {
        public int ID { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [Display(Name = "Route Type")]
        public RouteType RouteType { get; set; }
        public string Destination { get; set; }
        public string Status { get; set; }
        [Display(Name = "IP CIDR")]
        public string IPCIDR { get; set; }
        public string RouteTableID { get; set; }

        public ICollection<Subnet> LinkedSubnets { get; set; }
    }
}
