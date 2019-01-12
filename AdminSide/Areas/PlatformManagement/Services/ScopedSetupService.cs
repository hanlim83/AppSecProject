using Microsoft.Extensions.Logging;
using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using Amazon.EC2;
using Amazon.EC2.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Subnet = AdminSide.Areas.PlatformManagement.Models.Subnet;
using System.Data.SqlClient;
using Amazon.CloudWatch;
using Amazon.CloudWatchEvents;
using Amazon.CloudWatchLogs;
using RouteTable = AdminSide.Areas.PlatformManagement.Models.RouteTable;
using Route = AdminSide.Areas.PlatformManagement.Models.Route;
using Status = AdminSide.Areas.PlatformManagement.Models.Status;
using System.Threading;

namespace AdminSide.Areas.PlatformManagement.Services
{
    internal class ScopedSetupService : IScopedSetupService
    {
        private readonly ILogger _logger;
        private readonly PlatformResourcesContext context;
        private readonly IAmazonEC2 ec2Client;
        private readonly IAmazonCloudWatch cwClient;
        private readonly IAmazonCloudWatchEvents cweClient;
        private readonly IAmazonCloudWatchLogs cwlClient;
        private const string IPv4CIDR = "172.30.0.0/16";
        private static string IPv6CIDR;
        private static Boolean Flag;
        private static string MainAssociationID;

        public ScopedSetupService(ILogger<ScopedSetupService> logger, PlatformResourcesContext Context, IAmazonEC2 EC2Client, IAmazonCloudWatch cloudwatchClient, IAmazonCloudWatchEvents cloudwatcheventsClient, IAmazonCloudWatchLogs cloudwatchlogsClient)
        {
            _logger = logger;
            context = Context;
            ec2Client = EC2Client;
            cwClient = cloudwatchClient;
            cweClient = cloudwatcheventsClient;
            cwlClient = cloudwatchlogsClient;
        }

        public async Task DoWorkAsync()
        {
            _logger.LogInformation("Setup Background Service is running.");
            try
            {
                DescribeVpcsResponse responseDescribeVPC = await ec2Client.DescribeVpcsAsync();
                Flag = false;
                foreach (var VPC in responseDescribeVPC.Vpcs)
                {
                    if (VPC.CidrBlock.Contains(IPv4CIDR))
                        Flag = true;
                    if (Flag == true)
                        break;
                }
                if (Flag == false)
                {
                    _logger.LogInformation("VPC not found! Creating...");
                    CreateVpcResponse responseCreateVPC = await ec2Client.CreateVpcAsync(new CreateVpcRequest
                    {
                        CidrBlock = IPv4CIDR,
                        AmazonProvidedIpv6CidrBlock = true
                    });
                    await ec2Client.CreateTagsAsync(new CreateTagsRequest
                    {
                        Resources = new List<string>
                        {
                            responseCreateVPC.Vpc.VpcId
                        },
                        Tags = new List<Tag>
                        {
                            new Tag("Name","ASPJ VM VPC - Autocreated")
                        }
                    });
                    VPC newlyCreatedVPC = new VPC
                    {
                        AWSVPCReference = responseCreateVPC.Vpc.VpcId
                    };
                    context.VPCs.Add(newlyCreatedVPC);
                    await context.SaveChangesAsync();
                } else
                    _logger.LogInformation("VPC already created!");
                VPC vpc = await context.VPCs.FindAsync(1);
                responseDescribeVPC = await ec2Client.DescribeVpcsAsync(new DescribeVpcsRequest
                {
                    Filters = new List<Filter>
                        {
                            new Filter
                            {
                                Name = "vpc-id",
                                Values = new List<string>
                                {
                                    vpc.AWSVPCReference
                                }
                            }
                        }
                });
                Vpc AWSvpc = responseDescribeVPC.Vpcs[0];
                VpcIpv6CidrBlockAssociation set = AWSvpc.Ipv6CidrBlockAssociationSet[0];
                IPv6CIDR = set.Ipv6CidrBlock;
                DescribeSubnetsResponse responseDescribeSubnets = await ec2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                {
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Name = "vpc-id",
                            Values = new List<string>
                            {
                                vpc.AWSVPCReference
                            }
                        }
                    }
                });
                if (responseDescribeSubnets.Subnets.Count == 0)
                {
                    _logger.LogInformation("Default Subnets not found! Creating...");
                    string[] ipv6CIDRStr = IPv6CIDR.Split(":");
                    for (int i = 0; i < 3; i++)
                    {
                        String testing = ipv6CIDRStr[0] + ":" + ipv6CIDRStr[1] + ":" + ipv6CIDRStr[2] + ":" + ipv6CIDRStr[3].Substring(0, 3) + i + "::/64";
                        if (testing.Equals("2406:da18:b5d:dc00::/64"))
                            _logger.LogInformation("True");
                        else
                            _logger.LogInformation("False");
                        CreateSubnetResponse responseCreateSubnet = await ec2Client.CreateSubnetAsync(new CreateSubnetRequest
                        {
                            CidrBlock = IPv4CIDR.Substring(0, 7)+i+ ".0/24",
                            Ipv6CidrBlock = ipv6CIDRStr[0] + ":" + ipv6CIDRStr[1] + ":" + ipv6CIDRStr[2] + ":" + ipv6CIDRStr[3].Substring(0, 3) + i + "::/64",
                            VpcId = vpc.AWSVPCReference
                        });
                        Subnet newSubnet = new Subnet
                        {
                            IPv4CIDR = responseCreateSubnet.Subnet.CidrBlock,
                            IPv6CIDR = responseCreateSubnet.Subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock,
                            SubnetSize = "254",
                            AWSVPCSubnetReference = responseCreateSubnet.Subnet.SubnetId
                        };
                        if (i == 0)
                        {
                            await ec2Client.CreateTagsAsync(new CreateTagsRequest
                            {
                                Resources = new List<string>
                        {
                            responseCreateSubnet.Subnet.SubnetId
                        },
                                Tags = new List<Tag>
                        {
                            new Tag("Name","Intranet Subnet - Autocreated")
                        }
                            });
                            newSubnet.Name = "Intranet Subnet - Autocreated";
                            newSubnet.Type = SubnetType.Internet;
                        }
                        else if (i == 1)
                        {
                            await ec2Client.CreateTagsAsync(new CreateTagsRequest
                            {
                                Resources = new List<string>
                        {
                            responseCreateSubnet.Subnet.SubnetId
                        },
                                Tags = new List<Tag>
                        {
                            new Tag("Name","Extranet Subnet - Autocreated")
                        }
                            });
                            newSubnet.Name = "Extranet Subnet - Autocreated";
                            newSubnet.Type = SubnetType.Extranet;
                        }
                        else if (i == 2)
                        {
                            await ec2Client.CreateTagsAsync(new CreateTagsRequest
                            {
                                Resources = new List<string>
                        {
                            responseCreateSubnet.Subnet.SubnetId
                        },
                                Tags = new List<Tag>
                        {
                            new Tag("Name","Internet Subnet - Autocreated")
                        }
                            });
                            newSubnet.Name = "Internet Subnet - Autocreated";
                            newSubnet.Type = SubnetType.Intranet;
                        }
                        context.Subnets.Add(newSubnet);
                    }
                    await context.SaveChangesAsync();
                }
                else
                    _logger.LogInformation("Subnets already created!");
                DescribeRouteTablesResponse responseDescribeRouteTable = await ec2Client.DescribeRouteTablesAsync(new DescribeRouteTablesRequest
                {
                    Filters = new List<Filter>
                {
                     new Filter {Name = "vpc-id", Values = new List<string> {vpc.AWSVPCReference}}
                }
                });
                if (responseDescribeRouteTable.RouteTables.Count < 3)
                {
                    _logger.LogInformation("Default Route Tables not created! Creating...");
                    Amazon.EC2.Model.RouteTable AWSIntranet = responseDescribeRouteTable.RouteTables[0];
                    RouteTable RTIntranet = new RouteTable
                    {
                        AWSVPCRouteTableReference = AWSIntranet.RouteTableId
                    };
                    await ec2Client.CreateTagsAsync(new CreateTagsRequest
                    {
                        Resources = new List<string>
                        {
                            AWSIntranet.RouteTableId
                        },
                        Tags = new List<Tag>
                        {
                            new Tag("Name","Intranet Route Table - Autocreated")
                        }
                    });
                    Subnet Intranet = await context.Subnets.FindAsync(1);
                    AssociateRouteTableResponse responseAssociateRouteTable =  await ec2Client.AssociateRouteTableAsync(new AssociateRouteTableRequest
                    {
                        RouteTableId = AWSIntranet.RouteTableId,
                        SubnetId = Intranet.AWSVPCSubnetReference
                    });
                    Intranet.AWSVPCRouteTableAssoicationID = responseAssociateRouteTable.AssociationId;
                    context.Subnets.Update(Intranet);
                    context.RouteTables.Add(RTIntranet);
                    await context.SaveChangesAsync();
                    RTIntranet = await context.RouteTables.FindAsync(1);
                    foreach (var route in AWSIntranet.Routes)
                    {
                        if (route.DestinationIpv6CidrBlock == null)
                        {
                            Route r = new Route
                            {
                                Description = "Route to Challenge Network (IPv4, Pre-Created)",
                                RouteType = RouteType.Mandatory,
                                Status = Status.OK,
                                IPCIDR = route.DestinationCidrBlock,
                                applicability = Applicability.All,
                                LinkedRouteTable = RTIntranet,
                                RouteTableID = RTIntranet.ID,
                                Destination = "Challenge Network"
                            };
                            context.Routes.Add(r);
                        }
                        else
                        {
                            Route r = new Route
                            {
                                Description = "Route to Challenge Network (IPv6, Pre-Created)",
                                RouteType = RouteType.Mandatory,
                                Status = Status.OK,
                                IPCIDR = route.DestinationIpv6CidrBlock,
                                applicability = Applicability.All,
                                LinkedRouteTable = RTIntranet,
                                RouteTableID = RTIntranet.ID,
                                Destination = "Challenge Network"
                            };
                            context.Routes.Add(r);
                        }
                    }
                    Subnet SInternet = await context.Subnets.FindAsync(3);
                    CreateRouteTableResponse responseCreateRouteTable = await ec2Client.CreateRouteTableAsync(new CreateRouteTableRequest
                    {
                        VpcId = vpc.AWSVPCReference
                    });
                    RouteTable RTInternet = new RouteTable
                    {
                        AWSVPCRouteTableReference = responseCreateRouteTable.RouteTable.RouteTableId
                    };
                    context.RouteTables.Add(RTInternet);
                    await context.SaveChangesAsync();
                    await ec2Client.CreateTagsAsync(new CreateTagsRequest
                    {
                        Resources = new List<string>
                        {
                            responseCreateRouteTable.RouteTable.RouteTableId
                        },
                        Tags = new List<Tag>
                        {
                            new Tag
                            {
                                Key= "Name",
                                Value= "Internet Route Table - Autocreated"
                            }
                        }
                    });
                    responseAssociateRouteTable = await ec2Client.AssociateRouteTableAsync(new AssociateRouteTableRequest
                    {
                        RouteTableId = responseCreateRouteTable.RouteTable.RouteTableId,
                        SubnetId = SInternet.AWSVPCSubnetReference
                    });
                    SInternet.AWSVPCRouteTableAssoicationID = responseAssociateRouteTable.AssociationId;
                    context.Subnets.Update(SInternet);
                    CreateInternetGatewayResponse responseCreateInternetGateway = await ec2Client.CreateInternetGatewayAsync();
                    await ec2Client.CreateTagsAsync(new CreateTagsRequest
                    {
                        Resources = new List<string>
                        {
                            responseCreateInternetGateway.InternetGateway.InternetGatewayId
                        },
                        Tags = new List<Tag>
                        {
                            new Tag
                            {
                                Key= "Name",
                                Value= "ASPJ Internet Gateway - Autocreated"
                            }
                        }
                    });
                    AttachInternetGatewayResponse responseAttachInternetGateway = await ec2Client.AttachInternetGatewayAsync(new AttachInternetGatewayRequest
                    {
                        InternetGatewayId = responseCreateInternetGateway.InternetGateway.InternetGatewayId,
                        VpcId = vpc.AWSVPCReference
                    });
                    RTInternet = await context.RouteTables.FindAsync(2);
                    Route Internetv4 = new Route
                    {
                        Description = "Route to Internet(IPv4, Pre-Created)",
                        RouteType = RouteType.Mandatory,
                        Status = Status.OK,
                        IPCIDR = "0.0.0.0/0",
                        applicability = Applicability.Internet,
                        LinkedRouteTable = RTInternet,
                        RouteTableID = RTInternet.ID,
                        Destination = "Internet Gateway"
                    };
                    CreateRouteResponse responseCreateRoute = await ec2Client.CreateRouteAsync(new CreateRouteRequest
                    {
                        DestinationCidrBlock = Internetv4.IPCIDR,
                        GatewayId = responseCreateInternetGateway.InternetGateway.InternetGatewayId,
                        RouteTableId = responseCreateRouteTable.RouteTable.RouteTableId
                    });
                    Route Internetv6 = new Route
                    {
                        Description = "Route to Internet(IPv6, Pre-Created)",
                        RouteType = RouteType.Mandatory,
                        Status = Status.OK,
                        IPCIDR = "::/80",
                        applicability = Applicability.Internet,
                        LinkedRouteTable = RTInternet,
                        RouteTableID = RTInternet.ID,
                        Destination = "Internet Gateway"
                    };
                    responseCreateRoute = await ec2Client.CreateRouteAsync(new CreateRouteRequest
                    {
                        DestinationIpv6CidrBlock = Internetv6.IPCIDR,
                        GatewayId = responseCreateInternetGateway.InternetGateway.InternetGatewayId,
                        RouteTableId = responseCreateRouteTable.RouteTable.RouteTableId
                    });
                    context.Routes.Add(Internetv4);
                    context.Routes.Add(Internetv6);
                    Subnet SExtranet = await context.Subnets.FindAsync(2);
                    responseCreateRouteTable = await ec2Client.CreateRouteTableAsync(new CreateRouteTableRequest
                    {
                        VpcId = vpc.AWSVPCReference
                    });
                    RouteTable RTExtranet = new RouteTable
                    {
                        AWSVPCRouteTableReference = responseCreateRouteTable.RouteTable.RouteTableId
                    };
                    context.RouteTables.Add(RTExtranet);
                    AllocateAddressResponse responseAllocateAddress = await ec2Client.AllocateAddressAsync(new AllocateAddressRequest
                    {
                        Domain = "vpc"
                    });
                    CreateNatGatewayResponse responseCreateNATGateway = await ec2Client.CreateNatGatewayAsync(new CreateNatGatewayRequest
                    {
                        AllocationId = responseAllocateAddress.AllocationId,
                        SubnetId = SInternet.AWSVPCSubnetReference
                    });
                    await context.SaveChangesAsync();
                    await ec2Client.CreateTagsAsync(new CreateTagsRequest
                    {
                        Resources = new List<string>
                        {
                            responseCreateRouteTable.RouteTable.RouteTableId
                        },
                        Tags = new List<Tag>
                        {
                            new Tag
                            {
                                Key= "Name",
                                Value= "Extranet Route Table - Autocreated"
                            }
                        }
                    });
                    responseAssociateRouteTable = await ec2Client.AssociateRouteTableAsync(new AssociateRouteTableRequest
                    {
                        RouteTableId = responseCreateRouteTable.RouteTable.RouteTableId,
                        SubnetId = SExtranet.AWSVPCSubnetReference
                    });
                    SExtranet.AWSVPCRouteTableAssoicationID = responseAssociateRouteTable.AssociationId;
                    context.Subnets.Update(SExtranet);                   
                    CreateEgressOnlyInternetGatewayResponse responseCreateEgressOnlyInternetGatway = await ec2Client.CreateEgressOnlyInternetGatewayAsync(new CreateEgressOnlyInternetGatewayRequest
                    {
                        VpcId = vpc.AWSVPCReference
                    });                   
                    RTExtranet = await context.RouteTables.FindAsync(3);
                    Route Extranetv4 = new Route
                    {
                        Description = "Route to Internet(IPv4, Pre-Created)",
                        RouteType = RouteType.Mandatory,
                        Status = Status.OK,
                        IPCIDR = "0.0.0.0/0",
                        applicability = Applicability.Extranet,
                        LinkedRouteTable = RTExtranet,
                        RouteTableID = RTExtranet.ID,
                        Destination = "NAT Gateway"
                    };
                    Route Extranetv6 = new Route
                    {
                        Description = "Route to Internet(IPv6, Pre-Created)",
                        RouteType = RouteType.Mandatory,
                        Status = Status.OK,
                        IPCIDR = "::/80",
                        applicability = Applicability.Extranet,
                        LinkedRouteTable = RTExtranet,
                        RouteTableID = RTExtranet.ID,
                        Destination = "Internet-Only Gateway"
                    };
                    context.Routes.Add(Extranetv4);
                    context.Routes.Add(Extranetv6);
                    await context.SaveChangesAsync();
                    Thread.Sleep(new TimeSpan(0, 1, 0));
                    responseCreateRoute = await ec2Client.CreateRouteAsync(new CreateRouteRequest
                    {
                        DestinationCidrBlock = Extranetv4.IPCIDR,
                        NatGatewayId = responseCreateNATGateway.NatGateway.NatGatewayId,
                        RouteTableId = responseCreateRouteTable.RouteTable.RouteTableId
                    });
                    responseCreateRoute = await ec2Client.CreateRouteAsync(new CreateRouteRequest
                    {
                        DestinationIpv6CidrBlock = Extranetv6.IPCIDR,
                        EgressOnlyInternetGatewayId = responseCreateEgressOnlyInternetGatway.EgressOnlyInternetGateway.EgressOnlyInternetGatewayId,
                        RouteTableId = responseCreateRouteTable.RouteTable.RouteTableId
                    });
                    await ec2Client.CreateTagsAsync(new CreateTagsRequest
                    {
                        Resources = new List<string>
                        {
                            responseCreateNATGateway.NatGateway.NatGatewayId
                        },
                        Tags = new List<Tag>
                        {
                            new Tag
                            {
                                Key= "Name",
                                Value= "ASPJ Internet-Indirect Gateway - Autocreated"
                            }
                        }
                    });
                } else
                    _logger.LogInformation("Route Tables Found!");
                //if (context.Subnets.Any())
                //{
                //    List<Subnet> existingSubnets = await context.Subnets.ToListAsync();
                //    foreach (var Rsubnet in responseDescribeSubnets.Subnets)
                //    {
                //        Subnet subnetToBeDeleted = null;
                //        Flag = false;
                //        foreach (var Esubent in existingSubnets)
                //        {
                //            if (Rsubnet.SubnetId.Equals(Esubent.AWSVPCSubnetReference))
                //            {
                //                Flag = true;
                //                break;
                //            }
                //            else
                //            {
                //                Flag = false;
                //                subnetToBeDeleted = Esubent;
                //            }
                //        }
                //        if (Flag == false && subnetToBeDeleted != null)
                //            context.Subnets.Remove(subnetToBeDeleted);
                //    }
                //    foreach (var Rsubnet in responseDescribeSubnets.Subnets)
                //    {
                //        Flag = true;
                //        foreach (var Esubent in existingSubnets)
                //        {
                //            if (Rsubnet.SubnetId.Equals(Esubent.AWSVPCSubnetReference))
                //            {
                //                Flag = false;
                //                break;
                //            }
                //            else
                //                Flag = true;
                //        }
                //    }
                //else
                //{
                //        int incrementer = 4;
                //        context.Database.OpenConnection();
                //        context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Subnet ON");
                //        foreach (var subnet in response.Subnets)
                //        {
                //            try
                //            {
                //                Subnet newSubnet = new Subnet();
                //                if (subnet.CidrBlock.Equals("172.30.0.0/24"))
                //                {
                //                    newSubnet.ID = 1;
                //                    newSubnet.Name = "Default Internet Subnet";
                //                    newSubnet.IPv4CIDR = subnet.CidrBlock;
                //                    newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                //                    newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                //                    newSubnet.Type = SubnetType.Internet;
                //                    newSubnet.SubnetSize = "254";
                //                    context.Subnets.Add(newSubnet);
                //                    await context.SaveChangesAsync();
                //                }
                //                else if (subnet.CidrBlock.Equals("172.30.1.0/24"))
                //                {
                //                    newSubnet.ID = 2;
                //                    newSubnet.Name = "Default Extranet Subnet";
                //                    newSubnet.IPv4CIDR = subnet.CidrBlock;
                //                    newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                //                    newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                //                    newSubnet.Type = SubnetType.Extranet;
                //                    newSubnet.SubnetSize = "254";
                //                    context.Subnets.Add(newSubnet);
                //                    await context.SaveChangesAsync();
                //                }
                //                else if (subnet.CidrBlock.Equals("172.30.2.0/24"))
                //                {
                //                    newSubnet.ID = 3;
                //                    newSubnet.Name = "Default Intranet Subnet";
                //                    newSubnet.IPv4CIDR = subnet.CidrBlock;
                //                    newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                //                    newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                //                    newSubnet.Type = SubnetType.Intranet;
                //                    newSubnet.SubnetSize = "254";
                //                    context.Subnets.Add(newSubnet);
                //                    await context.SaveChangesAsync();
                //                }
                //                else
                //                {
                //                    newSubnet.ID = incrementer;
                //                    newSubnet.Name = subnet.SubnetId;
                //                    newSubnet.IPv4CIDR = subnet.CidrBlock;
                //                    newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                //                    newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                //                    newSubnet.Type = SubnetType.Extranet;
                //                    string subnetPrefix = subnet.CidrBlock.Substring(subnet.CidrBlock.Length - 3);
                //                    switch (subnetPrefix)
                //                    {
                //                        case "/17":
                //                            newSubnet.SubnetSize = Convert.ToString(32766);
                //                            break;
                //                        case "/18":
                //                            newSubnet.SubnetSize = Convert.ToString(16382);
                //                            break;
                //                        case "/19":
                //                            newSubnet.SubnetSize = Convert.ToString(8190);
                //                            break;
                //                        case "/20":
                //                            newSubnet.SubnetSize = Convert.ToString(4094);
                //                            break;
                //                        case "/21":
                //                            newSubnet.SubnetSize = Convert.ToString(2046);
                //                            break;
                //                        case "/22":
                //                            newSubnet.SubnetSize = Convert.ToString(1022);
                //                            break;
                //                        case "/23":
                //                            newSubnet.SubnetSize = Convert.ToString(510);
                //                            break;
                //                        case "/24":
                //                            newSubnet.SubnetSize = Convert.ToString(254);
                //                            break;
                //                        case "/25":
                //                            newSubnet.SubnetSize = Convert.ToString(126);
                //                            break;
                //                        case "/26":
                //                            newSubnet.SubnetSize = Convert.ToString(62);
                //                            break;
                //                        case "/27":
                //                            newSubnet.SubnetSize = Convert.ToString(30);
                //                            break;
                //                        case "/28":
                //                            newSubnet.SubnetSize = Convert.ToString(14);
                //                            break;
                //                        case "/29":
                //                            newSubnet.SubnetSize = Convert.ToString(6);
                //                            break;
                //                        case "/30":
                //                            newSubnet.SubnetSize = Convert.ToString(2);
                //                            break;
                //                        default:
                //                            break;
                //                    }
                //                    context.Subnets.Add(newSubnet);
                //                    await context.SaveChangesAsync();
                //                    ++incrementer;
                //                }
                //            }
                //            catch (Exception)
                //            {
                //                context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Subnet OFF");
                //                if (context.Subnets.Any())
                //                {
                //                    foreach (Subnet failed in context.Subnets)
                //                    {
                //                        context.Subnets.Remove(failed);
                //                    }
                //                    context.SaveChanges();
                //                }
                //                context.Database.CloseConnection();
                //            }
                //        }
                //        context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Subnet OFF");
                //        context.Database.CloseConnection();
                //    }
                //if (context.Servers.Any())
                //{
                //    List<Server> servers = await context.Servers.ToListAsync();
                //    DescribeInstancesResponse response = await ec2Client.DescribeInstancesAsync(new DescribeInstancesRequest
                //    {
                //        Filters = new List<Filter>
                //{
                //     new Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                //}
                //    });
                //    foreach (var server in servers)
                //    {
                //        foreach (var reservation in response.Reservations)
                //        {
                //            foreach (var instance in reservation.Instances)
                //            {
                //                bool Flag = false;
                //                if (server.AWSEC2Reference.Equals(instance.InstanceId))
                //                {
                //                    if (instance.State.Code == 0 && server.State != State.Starting)
                //                    {
                //                        server.State = State.Starting;
                //                        Flag = true;
                //                    }
                //                    else if (instance.State.Code == 16 && server.State != State.Running)
                //                    {
                //                        server.State = State.Running;
                //                        Flag = true;
                //                    }
                //                    else if (instance.State.Code == 64 && server.State != State.Stopping)
                //                    {
                //                        server.State = State.Stopping;
                //                        Flag = true;
                //                    }
                //                    else if (instance.State.Code == 80 && server.State != State.Stopped)
                //                    {
                //                        server.State = State.Stopped;
                //                        Flag = true;
                //                    }
                //                    if (server.Visibility == Visibility.Internet && (server.IPAddress != instance.PublicIpAddress || server.DNSHostname != instance.PublicDnsName))
                //                    {
                //                        server.IPAddress = instance.PublicIpAddress;
                //                        server.DNSHostname = instance.PublicDnsName;
                //                        Flag = true;
                //                    }
                //                    else if ((server.Visibility == Visibility.Extranet || server.Visibility == Visibility.Intranet) && (server.IPAddress != instance.PrivateIpAddress || server.DNSHostname != instance.PrivateDnsName))
                //                    {
                //                        server.IPAddress = instance.PrivateIpAddress;
                //                        server.DNSHostname = instance.PrivateDnsName;
                //                        Flag = true;
                //                    }
                //                    if (Flag == true)
                //                        context.Servers.Update(server);
                //                    break;
                //                }
                //            }
                //        }
                //    }
                //    context.SaveChanges();
                //}
                //else
                //{
                //    DescribeInstancesResponse response = await ec2Client.DescribeInstancesAsync(new DescribeInstancesRequest
                //    {
                //        Filters = new List<Filter>
                //{
                //     new Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                //}
                //    });
                //}
            }
            catch (SqlException)
            {
                _logger.LogInformation("Incorrect database schema used! Exiting...");
                return;
            }
            catch(AmazonEC2Exception e)
            {
                _logger.LogInformation("Setup Background Service has encounted an error!\n"+e.Source+"\n"+e.Message);
                return;
            }
            catch (Exception e)
            {
                _logger.LogInformation("Setup Background Service has encounted an error!\n" + e.Source + "\n" + e.Message);
            }
        }
        public async Task AWSNATDelayedTask()
        {

        }
    }
}