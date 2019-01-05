using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public enum RouteType
    {
        Mandatory,Optional
    }

    public enum Status
    {
        OK,Blackhole
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
        public Status Status { get; set; }
        [Display(Name = "IP CIDR")]
        public string IPCIDR { get; set; }

        public int RouteTableID { get; set; }
        public RouteTable LinkedRouteTable { get; set; }

        public ICollection<Subnet> LinkedSubnets { get; set; }
    }
}
