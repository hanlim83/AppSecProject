using System.Collections.Generic;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class RouteTable
    {
        public int ID { get; set; }
        public string AWSVPCRouteTableReference { get; set; }
        public ICollection<Subnet>LinkedSubnets { get; set; }
        public ICollection<Route>LinkedRoutes { get; set; }
    }
}
