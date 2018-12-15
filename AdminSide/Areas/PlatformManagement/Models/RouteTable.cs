using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class RouteTable
    {
        public int ID { get; set; }
        public string AWSVPCRouteTableReference { get; set; }
        public ICollection<Route>LinkedRoutes { get; set; }
    }
}
