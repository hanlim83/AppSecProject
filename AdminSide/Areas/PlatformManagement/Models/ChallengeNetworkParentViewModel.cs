using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class ChallengeNetworkParentViewModel
    {
        public List<Subnet>RetrievedSubnets { get; set; }
        public List<Route>RetrievedRoutes { get; set; }
    }
}
