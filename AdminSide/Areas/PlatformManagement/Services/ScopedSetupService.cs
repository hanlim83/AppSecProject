﻿using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using Amazon.CloudWatch;
using Amazon.CloudWatchEvents;
using Amazon.CloudWatchLogs;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.RDS;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Route = AdminSide.Areas.PlatformManagement.Models.Route;
using RouteTable = AdminSide.Areas.PlatformManagement.Models.RouteTable;
using Status = AdminSide.Areas.PlatformManagement.Models.Status;
using Subnet = AdminSide.Areas.PlatformManagement.Models.Subnet;

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
        private readonly IAmazonRDS rdsClient;
        private const string IPv4CIDR = "172.30.0.0/16";
        private static string IPv6CIDR;
        private static Boolean Flag;

        public ScopedSetupService(ILogger<ScopedSetupService> logger, PlatformResourcesContext Context, IAmazonEC2 EC2Client, IAmazonCloudWatch cloudwatchClient, IAmazonCloudWatchEvents cloudwatcheventsClient, IAmazonCloudWatchLogs cloudwatchlogsClient, IAmazonRDS relationaldatabaseserviceClient)
        {
            _logger = logger;
            context = Context;
            ec2Client = EC2Client;
            cwClient = cloudwatchClient;
            cweClient = cloudwatcheventsClient;
            cwlClient = cloudwatchlogsClient;
            rdsClient = relationaldatabaseserviceClient;
        }

        public async Task DoWorkAsync()
        {
            _logger.LogInformation("Setup Background Service is running.");
            _logger.LogInformation("Setup Background Service is getting IP Address for RDS.");
            string externalip = new WebClient().DownloadString("https://checkip.amazonaws.com");
            try
            {
                DescribeSecurityGroupsResponse responseDescribeSecurityGroups = await ec2Client.DescribeSecurityGroupsAsync(new DescribeSecurityGroupsRequest
                {
                    GroupIds = new List<string>
                {
                    "sg-0c3eb90f56531f376"
                }
                });
                if (responseDescribeSecurityGroups.SecurityGroups[0] != null)
                {
                    SecurityGroup securityGroup = responseDescribeSecurityGroups.SecurityGroups[0];
                    Flag = false;
                    bool legacy = false;
                    foreach (IpPermission ip in securityGroup.IpPermissions)
                    {
                        foreach (string ir in ip.IpRanges)
                        {
                            if (ir.Equals(externalip + "/32"))
                            {
                                Flag = true;
                                legacy = true;
                                break;
                            }
                        }
                        foreach (IpRange ir in ip.Ipv4Ranges)
                        {
                            if (ir.CidrIp.Equals(externalip + "/32"))
                            {
                                Flag = true;
                                break;
                            }
                        }
                        if (Flag == true)
                            break;
                    }
                    if (Flag == true && legacy == false)
                    {
                        AuthorizeSecurityGroupIngressResponse responseAuthorizeSecurityGroupIngress = await ec2Client.AuthorizeSecurityGroupIngressAsync(new AuthorizeSecurityGroupIngressRequest
                        {
                            IpPermissions = new List<IpPermission>
                            {
                                new IpPermission
                                {
                                    Ipv4Ranges = new List<IpRange>
                                    {
                                        new IpRange
                                        {
                                            CidrIp = externalip + "/32"
                                        }
                                    }
                                }
                            },
                            GroupId = securityGroup.GroupId
                        });
                        if (responseAuthorizeSecurityGroupIngress.HttpStatusCode == HttpStatusCode.OK)
                            _logger.LogInformation("Setup Background Service has completed RDS authroization");
                    }
                    else if (Flag == true && legacy == true)
                    {
                        RevokeSecurityGroupIngressResponse responseRevokeSecurityGroupIngress = await ec2Client.RevokeSecurityGroupIngressAsync(new RevokeSecurityGroupIngressRequest
                        {
                            IpPermissions = new List<IpPermission>
                            {
                                new IpPermission
                                {
                                    IpRanges = new List<string>
                                    {
                                        externalip + "/32"
                                    }
                                }
                            },
                            GroupId = securityGroup.GroupId
                        });
                        AuthorizeSecurityGroupIngressResponse responseAuthorizeSecurityGroupIngress = await ec2Client.AuthorizeSecurityGroupIngressAsync(new AuthorizeSecurityGroupIngressRequest
                        {
                            IpPermissions = new List<IpPermission>
                            {
                                new IpPermission
                                {
                                    Ipv4Ranges = new List<IpRange>
                                    {
                                        new IpRange
                                        {
                                            CidrIp = externalip + "/32"
                                        }
                                    }
                                }
                            },
                            GroupId = securityGroup.GroupId
                        });
                        if (responseRevokeSecurityGroupIngress.HttpStatusCode == HttpStatusCode.OK && responseAuthorizeSecurityGroupIngress.HttpStatusCode == HttpStatusCode.OK)
                            _logger.LogInformation("Setup Background Service has completed RDS authroization");
                        else
                            _logger.LogInformation("Setup Background Service has failed RDS authroization");
                    }
                }
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
                    DescribeSecurityGroupsResponse responseSecurityGroups = await ec2Client.DescribeSecurityGroupsAsync(new DescribeSecurityGroupsRequest
                    {
                        Filters = new List<Filter>
                        {
                            new Filter
                            {
                                Name ="vpc-id",
                                Values = new List<string>
                                {
                                    responseCreateVPC.Vpc.VpcId
                                }
                            }
                        }
                    });
                    VPC newlyCreatedVPC = new VPC
                    {
                        AWSVPCReference = responseCreateVPC.Vpc.VpcId,
                        AWSVPCDefaultSecurityGroup = responseSecurityGroups.SecurityGroups[0].GroupId
                    };
                    context.VPCs.Add(newlyCreatedVPC);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("VPC created!");
                }
                else
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
                        CreateSubnetResponse responseCreateSubnet = await ec2Client.CreateSubnetAsync(new CreateSubnetRequest
                        {
                            CidrBlock = IPv4CIDR.Substring(0, 7) + i + ".0/24",
                            Ipv6CidrBlock = ipv6CIDRStr[0] + ":" + ipv6CIDRStr[1] + ":" + ipv6CIDRStr[2] + ":" + ipv6CIDRStr[3].Substring(0, 3) + i + "::/64",
                            VpcId = vpc.AWSVPCReference
                        });
                        Subnet newSubnet = new Subnet
                        {
                            IPv4CIDR = responseCreateSubnet.Subnet.CidrBlock,
                            IPv6CIDR = responseCreateSubnet.Subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock,
                            SubnetSize = "254",
                            AWSVPCSubnetReference = responseCreateSubnet.Subnet.SubnetId,
                            VPCID = vpc.ID
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
                            newSubnet.Type = SubnetType.Intranet;
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
                            await ec2Client.ModifySubnetAttributeAsync(new ModifySubnetAttributeRequest
                            {
                                SubnetId = responseCreateSubnet.Subnet.SubnetId,
                                MapPublicIpOnLaunch = true
                            });
                            await ec2Client.ModifySubnetAttributeAsync(new ModifySubnetAttributeRequest
                            {
                                SubnetId = responseCreateSubnet.Subnet.SubnetId,
                                AssignIpv6AddressOnCreation = true
                            });
                            newSubnet.Name = "Internet Subnet - Autocreated";
                            newSubnet.Type = SubnetType.Internet;
                        }
                        context.Subnets.Add(newSubnet);
                    }
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Subnets created!");
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
                        AWSVPCRouteTableReference = AWSIntranet.RouteTableId,
                        VPCID = vpc.ID
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
                    AssociateRouteTableResponse responseAssociateRouteTable = await ec2Client.AssociateRouteTableAsync(new AssociateRouteTableRequest
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
                        AWSVPCRouteTableReference = responseCreateRouteTable.RouteTable.RouteTableId,
                        VPCID = vpc.ID
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
                        AWSVPCRouteTableReference = responseCreateRouteTable.RouteTable.RouteTableId,
                        VPCID = vpc.ID
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
                    _logger.LogInformation("Setup Background Service Sleeping while waiting for NAT Gateway to be ready");
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
                    _logger.LogInformation("Route Tables Created!");
                    _logger.LogInformation("Linking Subnets to Route Tables (SQL)");
                    Subnet subnet = await context.Subnets.FindAsync(1);
                    RouteTable routeTable = await context.RouteTables.FindAsync(1);
                    subnet.RouteTableID = routeTable.ID;
                    context.Subnets.Update(subnet);
                    subnet = await context.Subnets.FindAsync(2);
                    routeTable = await context.RouteTables.FindAsync(3);
                    subnet.RouteTableID = routeTable.ID;
                    context.Subnets.Update(subnet);
                    subnet = await context.Subnets.FindAsync(3);
                    routeTable = await context.RouteTables.FindAsync(2);
                    subnet.RouteTableID = routeTable.ID;
                    context.Subnets.Update(subnet);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Linking Subnets to Route Tables (SQL) Completed!");
                    _logger.LogInformation("Setup Background Service Completed!");
                }
                else
                    _logger.LogInformation("Route Tables Found!");
            }
            catch (SqlException)
            {
                _logger.LogInformation("Incorrect database schema used! Exiting...");
                return;
            }
            catch (AmazonEC2Exception e)
            {
                _logger.LogInformation("Setup Background Service has encounted an error!\n" + e.Source + "\n" + e.Message);
                return;
            }
            catch (Exception e)
            {
                _logger.LogInformation("Setup Background Service has encounted an error!\n" + e.Source + "\n" + e.Message);
            }
        }
    }
}