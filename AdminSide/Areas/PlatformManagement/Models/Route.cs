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
    public enum DestinationType
    {
        VPC,Subnet
    }
    public class Route
    {
        public int ID { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public RouteType RouteType { get; set; }
        [Required]
        public DestinationType DestinationType { get; set; }
        [Required]
        public string AWSVPCRouteReference { get; set; }

        public ICollection<Subnet> LinkedSubnets { get; set; }
    }
}
