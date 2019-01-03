using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using AdminSide.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Data
{
    public class DbInitializer
    {

        public static void InitializePlatformResources(PlatformResourcesContext context)
        {
            context.Database.EnsureCreated();
            if (!context.RouteTables.Any() && !context.Routes.Any())
            {
                var defaultRouteTables = new RouteTable[]
                {
                    new RouteTable
                    {
                        AWSVPCRouteTableReference = "rtb-087bb7bba0f0c4eee"
                    },
                    new RouteTable
                    {
                        AWSVPCRouteTableReference = "rtb-0fe88d50b748fd6e9"
                    },
                    new RouteTable
                    {
                        AWSVPCRouteTableReference = "rtb-0f051204240351aee"
                    }
                };
                foreach (RouteTable rt in defaultRouteTables)
                {
                    context.RouteTables.Add(rt);
                }
                var defaultRoutes = new Route[]
                {
                    new Route
                    {
                        RouteTableID = defaultRouteTables.Single(rt => rt.AWSVPCRouteTableReference == "rtb-087bb7bba0f0c4eee").ID,
                        IPCIDR = "172.30.0.0/16",
                        Destination = "Challenge Network",
                        Description = "Route to Challenge Network (IPv4)",
                        Status = Status.OK,
                        RouteType = RouteType.Mandatory
                    },
                    new Route
                    {
                        RouteTableID = defaultRouteTables.Single(rt => rt.AWSVPCRouteTableReference == "rtb-087bb7bba0f0c4eee").ID,
                        IPCIDR = "2406:da18:456:f300::/56",
                        Destination = "Challenge Network",
                        Description = "Route to Challenge Network (IPv6)",
                        Status = Status.OK,
                        RouteType = RouteType.Mandatory
                    },
                    new Route
                    {
                        RouteTableID = defaultRouteTables.Single(rt => rt.AWSVPCRouteTableReference == "rtb-087bb7bba0f0c4eee").ID,
                        IPCIDR = "0.0.0.0/0",
                        Destination = "Internet Gateway",
                        Description = "Route to the Internet (IPv4)",
                        Status = Status.OK,
                        RouteType = RouteType.Mandatory
                    },
                    new Route
                    {
                        RouteTableID = defaultRouteTables.Single(rt => rt.AWSVPCRouteTableReference == "rtb-087bb7bba0f0c4eee").ID,
                        IPCIDR = "::/80",
                        Destination = "Internet Gateway",
                        Description = "Route to the Internet (IPv6)",
                        Status = Status.OK,
                        RouteType = RouteType.Mandatory
                    },
                    new Route
                    {
                        RouteTableID = defaultRouteTables.Single(rt => rt.AWSVPCRouteTableReference == "rtb-0fe88d50b748fd6e9").ID,
                        IPCIDR = "172.30.0.0/16",
                        Destination = "Challenge Network",
                        Description = "Route to Challenge Network (IPv4)",
                        Status = Status.OK,
                        RouteType = RouteType.Mandatory
                    },
                    new Route
                    {
                        RouteTableID = defaultRouteTables.Single(rt => rt.AWSVPCRouteTableReference == "rtb-0fe88d50b748fd6e9").ID,
                        IPCIDR = "2406:da18:456:f300::/56",
                        Destination = "Challenge Network",
                        Description = "Route to Challenge Network (IPv6)",
                        Status = Status.OK,
                        RouteType = RouteType.Mandatory
                    },
                    new Route
                    {
                        RouteTableID = defaultRouteTables.Single(rt => rt.AWSVPCRouteTableReference == "rtb-0fe88d50b748fd6e9").ID,
                        IPCIDR = "0.0.0.0/0",
                        Destination = "NAT Gateway",
                        Description = "Route to the Internet (IPv4)",
                        Status = Status.OK,
                        RouteType = RouteType.Mandatory
                    },
                    new Route
                    {
                        RouteTableID = defaultRouteTables.Single(rt => rt.AWSVPCRouteTableReference == "rtb-0fe88d50b748fd6e9").ID,
                        IPCIDR = "::/80",
                        Destination = "One-Way Internet Gateway",
                        Description = "Route to the Internet (IPv6)",
                        Status = Status.OK,
                        RouteType = RouteType.Mandatory
                    },
                    new Route
                    {
                        RouteTableID = defaultRouteTables.Single(rt => rt.AWSVPCRouteTableReference == "rtb-0f051204240351aee").ID,
                        IPCIDR = "172.30.0.0/16",
                        Destination = "Challenge Network",
                        Description = "Route to Challenge Network (IPv4)",
                        Status = Status.OK,
                        RouteType = RouteType.Mandatory
                    },
                    new Route
                    {
                        RouteTableID = defaultRouteTables.Single(rt => rt.AWSVPCRouteTableReference == "rtb-0f051204240351aee").ID,
                        IPCIDR = "2406:da18:456:f300::/56",
                        Destination = "Challenge Network",
                        Description = "Route to Challenge Network (IPv6)",
                        Status = Status.OK,
                        RouteType = RouteType.Mandatory
                    }
                };
                foreach (Route r in defaultRoutes)
                {
                    context.Routes.Add(r);
                }
                context.SaveChanges();
            }
        }

        public static void InitializeCompetitions (CompetitionContext context)
        {
            context.Database.EnsureCreated();

            if (context.Competitions.Any())
            {
                return;   // DB has been seeded
            }

            var competition = new Competition[]
            {
            new Competition{CompetitionName="NYP Global CTF", Status="Active", BucketName="NYP Global CTF"}
            };

            foreach (Competition c in competition)
            {
                context.Competitions.Add(c);
            }
            context.SaveChanges();

            var competitionCategory = new CompetitionCategory[]
            {
            new CompetitionCategory{ CategoryName="Web", CompetitionID=1 },
            new CompetitionCategory{ CategoryName="Crypto", CompetitionID=1 },
            new CompetitionCategory{ CategoryName="Forensics", CompetitionID=1 },
            new CompetitionCategory{ CategoryName="Misc", CompetitionID=1 }
            };

            foreach (CompetitionCategory cc in competitionCategory)
            {
                context.CompetitionCategories.Add(cc);
            }
            context.SaveChanges();
        }
    }
}
