﻿using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using AdminSide.Data;
using AdminSide.Models;
using System;
using System.Linq;

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
                        AWSAMIReference = "ami-0b84d2c53ad5250c2",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "Amazon Linux 2018.3 Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Amazon Linux 2018.3",
                        AWSAMIReference = "ami-05b3bcf7f311194b3",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "Red Hat Enterprise Linux Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Red Hat Enterprise Linux 7.5",
                        AWSAMIReference = "ami-76144b0a",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "SUSE Linux Enterprise Server Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "SUSE Linux Enterprise Server 15",
                        AWSAMIReference = "ami-0920c364049458e86",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "Ubuntu Server 18.04 LTS Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Ubuntu Server 18.04 LTS",
                        AWSAMIReference = "ami-0c5199d385b432989",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "Ubuntu Server 14.04 LTS Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Ubuntu Server 14.04 LTS",
                        AWSAMIReference = "ami-039950f07c4a0d878",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "Microsoft Windows Server 2016 Base Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Microsoft Windows Server 2016 Base",
                        AWSAMIReference = "ami-04385f3f533c85af7",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "Microsoft Windows Server 2012 R2 Base Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Microsoft Windows Server 2012 R2 Base",
                        AWSAMIReference = "ami-059b411d0e166914e",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "Microsoft Windows Server 2012 Base Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Microsoft Windows Server 2012 Base",
                        AWSAMIReference = "ami-0caa1a0b06b16e0c0",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "Microsoft Windows Server 2008 R2 Base Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Microsoft Windows Server 2008 R2 Base",
                        AWSAMIReference = "ami-0428d47da132d9763",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "Microsoft Windows Server 2008 SP2 Base Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Microsoft Windows Server 2008 SP2 Base",
                        AWSAMIReference = "ami-07bae34c7bd0d31d0",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "Ubuntu Server 16.04 LTS Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Ubuntu Server 16.04 LTS",
                        AWSAMIReference = "ami-0eb1f21bbd66347fe",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "Microsoft Windows Server 2003 R2 Base Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Microsoft Windows Server 2003 R2 Base",
                        AWSAMIReference = "ami-0407dbd3ff0dca87c",
                        SpecificMinimumSize = false
                    },
                    new Template
                    {
                        Name = "Kali Linux Template",
                        Type = TemplateType.Default,
                        DateCreated = DateTime.Parse("1/1/2019"),
                        OperatingSystem = "Kali Linux 2018.3",
                        AWSAMIReference = "ami-0db99f1dca7fe5ee2",
                        SpecificMinimumSize = true,
                        MinimumStorage = 25
                    },
                    new Template
                    {
                        Name = "Splunk Enterprise Template",
                        Type = TemplateType.Custom,
                        DateCreated = DateTime.Parse("5/1/2019"),
                        OperatingSystem = "Amazon Linux 2 LTS",
                        AWSAMIReference = "ami-04c4105e8f4bf70eb",
                        SpecificMinimumSize = true,
                        MinimumStorage = 15,
                        TemplateDescription = "Default Amazon 2 LTS Template with Splunk Enterprise 7 Pre-loaded \nUser Name: eCTFAdmin \nPassword:eCTFP@ss"
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

            var categoryDefault = new CategoryDefault[]
            {
            new CategoryDefault{ CategoryName="Web" },
            new CategoryDefault{ CategoryName="Crypto" },
            new CategoryDefault{ CategoryName="Forensics" },
            new CategoryDefault{ CategoryName="Misc" }
            };

            foreach (CategoryDefault cd in categoryDefault)
            {
                context.CategoryDefault.Add(cd);
            }
            context.SaveChanges();

            var challenges = new Challenge[]
            {
            new Challenge{ Name="Challenge 1", Description="Testing 1", Value=100, Flag="aaa", FileName="TestingOnly", CompetitionID=1, CompetitionCategoryID=1 },
            new Challenge{ Name="Challenge 2", Description="Testing 2", Value=200, Flag="aab", FileName="TestingOnly", CompetitionID=1, CompetitionCategoryID=1 },
            new Challenge{ Name="Challenge 3", Description="Testing 3", Value=300, Flag="aac", FileName="TestingOnly", CompetitionID=1, CompetitionCategoryID=1 },
            new Challenge{ Name="Challenge 4", Description="Testing 4", Value=400, Flag="aad", FileName="TestingOnly", CompetitionID=1, CompetitionCategoryID=1 },
            };

            foreach (Challenge ch in challenges)
            {
                context.Challenges.Add(ch);
            }
            context.SaveChanges();

            var teams = new Team[]
            {
            new Team{ TeamName="T0X1C V4P04", Password="Pass123!", Score=100, CompetitionID=1},
            new Team{ TeamName="Team 1", Password="Pass123!", Score=80, CompetitionID=1},
            new Team{ TeamName="Team 2", Password="Pass123!", Score=50, CompetitionID=1},
            new Team{ TeamName="Team 3", Password="Pass123!", Score=20, CompetitionID=1},
            };

            foreach (Team t in teams)
            {
                context.Teams.Add(t);
            }
            context.SaveChanges();
        }

        public static void InitializeForum(ForumContext context)
        {
            context.Database.EnsureCreated();

            if (context.Posts.Any())
            {
                return;   // DB has been seeded
            }

            var category = new ForumCategory[]
            {
            new ForumCategory{CategoryName="General"},
            new ForumCategory{CategoryName="Crypto"}
            };

            foreach (ForumCategory c in category)
            {
                context.ForumCategories.Add(c);
            }
            context.SaveChanges();

            var post = new Post[]
            {
            new Post{ Title="Errors", Content="How To Fix", UserName="Elxxwy", CategoryID=1 },
            new Post{ Title="General", Content="How To Do", UserName="Eevee", CategoryID=2 },
            new Post{ Title="Errors", Content="How To UnFix", UserName="EVELYN", CategoryID=1 },
            new Post{ Title="General", Content="How To Undo", UserName="Elxxwy", CategoryID=2 },
            };

            foreach (Post p in post)
            {
                context.Posts.Add(p);
            }
            context.SaveChanges();

        }
    }
}
