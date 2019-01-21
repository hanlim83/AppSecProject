using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class ChallengeNetworkParentModel
    {
        public List<Subnet>retrievedSubnets { get; set; }
        public List<Route>retrievedRoutes { get; set; }
    }
}
