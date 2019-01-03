using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using AdminSide.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            if (!context.Templates.Any())
            {
                var defaultTemplates = new Template[]
                {
                    new Template
                    {
                        Name = "Amazon Linux 2 LTS Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Amazon Linux 2 LTS",
                        AWSAMIReference = "ami-0b84d2c53ad5250c2"
                    },
                    new Template
                    {
                        Name = "Amazon Linux 2018.3 Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Amazon Linux 2018.3",
                        AWSAMIReference = "ami-05b3bcf7f311194b3"
                    },
                    new Template
                    {
                        Name = "Red Hat Enterprise Linux Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Red Hat Enterprise Linux 7.5",
                        AWSAMIReference = "ami-76144b0a"
                    },
                    new Template
                    {
                        Name = "SUSE Linux Enterprise Server Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "SUSE Linux Enterprise Server 15",
                        AWSAMIReference = "ami-0920c364049458e86"
                    },
                    new Template
                    {
                        Name = "Ubuntu Server 18.04 LTS Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Ubuntu Server 18.04 LTS",
                        AWSAMIReference = "ami-0c5199d385b432989"
                    },
                    new Template
                    {
                        Name = "Ubuntu Server 14.04 LTS Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Ubuntu Server 14.04 LTS",
                        AWSAMIReference = "ami-039950f07c4a0d878"
                    },
                    new Template
                    {
                        Name = "Microsoft Windows Server 2016 Base Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Microsoft Windows Server 2016 Base",
                        AWSAMIReference = "ami-04385f3f533c85af7"
                    },
                    new Template
                    {
                        Name = "Microsoft Windows Server 2012 R2 Base Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Microsoft Windows Server 2012 R2 Base",
                        AWSAMIReference = "ami-059b411d0e166914e"
                    },
                    new Template
                    {
                        Name = "Microsoft Windows Server 2012 Base Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Microsoft Windows Server 2012 Base",
                        AWSAMIReference = "ami-0caa1a0b06b16e0c0"
                    },
                    new Template
                    {
                        Name = "Microsoft Windows Server 2008 R2 Base Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Microsoft Windows Server 2008 R2 Base",
                        AWSAMIReference = "ami-0428d47da132d9763"
                    },
                    new Template
                    {
                        Name = "Microsoft Windows Server 2008 SP2 Base Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Microsoft Windows Server 2008 SP2 Base",
                        AWSAMIReference = "ami-07bae34c7bd0d31d0"
                    },
                    new Template
                    {
                        Name = "Ubuntu Server 16.04 LTS Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Ubuntu Server 16.04 LTS",
                        AWSAMIReference = "ami-0eb1f21bbd66347fe"
                    },
                    new Template
                    {
                        Name = "Microsoft Windows Server 2003 R2 Base Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Microsoft Windows Server 2003 R2 Base",
                        AWSAMIReference = "ami-0407dbd3ff0dca87c"
                    },
                    new Template
                    {
                        Name = "Kali Linux Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Kali Linux 2018.3",
                        AWSAMIReference = "ami-0db99f1dca7fe5ee2"
                    }
                };
                foreach (Template t in defaultTemplates)
                {
                    context.Templates.Add(t);
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
