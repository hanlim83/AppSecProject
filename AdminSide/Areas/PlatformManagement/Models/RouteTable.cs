using System.Collections.Generic;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class RouteTable
    {
        public int ID { get; set; }
        public string AWSVPCRouteTableReference { get; set; }
        public virtual ICollection<Subnet>LinkedSubnets { get; set; }
        public virtual ICollection<Route>LinkedRoutes { get; set; }
        public int VPCID { get; set; }
        public virtual VPC LinkedVPC { get; set; }
    }
}
