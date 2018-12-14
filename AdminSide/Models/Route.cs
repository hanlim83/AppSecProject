using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Models
{
    public enum RouteType
    {
        Mandatory,Optional
    }
    public class Route
    {
        public int ID { get; set; }

        public string Description { get; set; }

        public string AWSVPCRouteReference { get; set; }

        public ICollection<Subnet> LinkedSubnets { get; set; }
    }
}
