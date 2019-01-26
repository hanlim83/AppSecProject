using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using Amazon.CloudWatch;
using Amazon.CloudWatchEvents;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.RDS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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
        private const string IPv4VMCIDR = "172.30.0.0/16";
        private static string VMVPCID;
        private static Boolean Flag;
        private const string IPv4PlatformCIDR = "172.29.0.0/16";
        private static string PlatformVPCID;

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
            string tempt = new WebClient().DownloadString("https://checkip.amazonaws.com");
            string externalip = tempt.Substring(0, tempt.Length - 2);
            try
            {
                DescribeSecurityGroupsResponse responseDescribeSecurityGroups = await ec2Client.DescribeSecurityGroupsAsync(new DescribeSecurityGroupsRequest
                {
                    GroupIds = new List<string>
                {
                    "sg-05613fe8f8f7a54c4"
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
                    if (Flag == false && legacy == false)
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
                                    },
                                    IpProtocol = "tcp",
                                    FromPort = 1433,
                                    ToPort = 1433
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
                                    },
                                    IpProtocol = "tcp",
                                    FromPort = 1433,
                                    ToPort = 1433
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
                                    },
                                    IpProtocol = "tcp",
                                    FromPort = 1433,
                                    ToPort = 1433
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
                    if (VPC.CidrBlock.Contains(IPv4VMCIDR))
                    {
                        VMVPCID = VPC.VpcId;
                        Flag = true;
                    }
                    else if (VPC.CidrBlock.Contains(IPv4PlatformCIDR))
                        PlatformVPCID = VPC.VpcId;
                    if (Flag == true)
                        break;
                }
                if (Flag == false)
                {
                    _logger.LogInformation("VPC not found! Creating...");
                    CreateVpcResponse responseCreateVPC = await ec2Client.CreateVpcAsync(new CreateVpcRequest
                    {
                        CidrBlock = IPv4VMCIDR,
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
                    await ec2Client.ModifyVpcAttributeAsync(new ModifyVpcAttributeRequest
                    {
                        EnableDnsSupport = true,
                        VpcId = responseCreateVPC.Vpc.VpcId
                    });
                    await ec2Client.ModifyVpcAttributeAsync(new ModifyVpcAttributeRequest
                    {
                        EnableDnsHostnames = true,
                        VpcId = responseCreateVPC.Vpc.VpcId
                    });
                    await ec2Client.CreateFlowLogsAsync(new CreateFlowLogsRequest
                    {
                        DeliverLogsPermissionArn = "arn:aws:iam::188363912800:role/VPC-Flow-Logs",
                        LogDestinationType = "cloud-watch-logs",
                        LogGroupName = "VMVPCLogs",
                        ResourceIds = new List<string>
                        {
                            responseCreateVPC.Vpc.VpcId
                        },
                        ResourceType = FlowLogsResourceType.VPC,
                        TrafficType = TrafficType.ALL
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
                    _logger.LogInformation("Setup Background Service Sleeping while IPv6 Assignment");
                    responseDescribeVPC = await ec2Client.DescribeVpcsAsync(new DescribeVpcsRequest
                    {
                        VpcIds = new List<string>
                        {
                            responseCreateVPC.Vpc.VpcId
                        }
                    });
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    VPC newlyCreatedVPC = new VPC
                    {
                        AWSVPCReference = responseCreateVPC.Vpc.VpcId,
                        AWSVPCDefaultSecurityGroup = responseSecurityGroups.SecurityGroups[0].GroupId,
                        type = VPCType.DefaultVM,
                        BaseIPv4CIDR = responseCreateVPC.Vpc.CidrBlock,
                        BaseIPv6CIDR = responseDescribeVPC.Vpcs[0].Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock
                    };
                    context.VPCs.Add(newlyCreatedVPC);
                    _logger.LogInformation("VPC created!");
                    if (context.VPCs.FromSql("SELECT * FROM dbo.VPCs WHERE AWSVPCReference = '" + PlatformVPCID + "'").ToList().Count() == 0)
                    {
                        _logger.LogInformation("Platform VPC not inside SQL Database!");
                        responseDescribeVPC = await ec2Client.DescribeVpcsAsync(new DescribeVpcsRequest
                        {
                            Filters = new List<Filter>
                            {
                                new Filter
                                {
                                    Name = "vpc-id",
                                    Values = new List<string>
                                    {
                                        PlatformVPCID
                                    }
                                }
                            }
                        });
                        responseSecurityGroups = await ec2Client.DescribeSecurityGroupsAsync(new DescribeSecurityGroupsRequest
                        {
                            Filters = new List<Filter>
                        {
                            new Filter
                            {
                                Name ="vpc-id",
                                Values = new List<string>
                                {
                                    PlatformVPCID
                                }
                            }
                        }
                        });
                        VPC newlyCreatedVPC2 = new VPC
                        {
                            AWSVPCReference = responseDescribeVPC.Vpcs[0].VpcId,
                            AWSVPCDefaultSecurityGroup = responseSecurityGroups.SecurityGroups[0].GroupId,
                            type = VPCType.Platform,
                            BaseIPv4CIDR = responseDescribeVPC.Vpcs[0].CidrBlock,
                            BaseIPv6CIDR = responseDescribeVPC.Vpcs[0].Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock
                        };
                        context.VPCs.Add(newlyCreatedVPC2);
                    }
                    await context.SaveChangesAsync();
                }
                else
                {
                    if (!context.VPCs.Any() || context.VPCs.FromSql("SELECT * FROM dbo.VPCs WHERE AWSVPCReference = '" + VMVPCID + "'").ToList().Count() == 0)
                    {
                        _logger.LogInformation("VM VPC already created but not inside SQL Database!");
                        responseDescribeVPC = await ec2Client.DescribeVpcsAsync(new DescribeVpcsRequest
                        {
                            Filters = new List<Filter>
                            {
                                new Filter
                                {
                                    Name = "vpc-id",
                                    Values = new List<string>
                                    {
                                        VMVPCID
                                    }
                                }
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
                                    VMVPCID
                                }
                            }
                        }
                        });                      
                        VPC newlyCreatedVPC = new VPC
                        {
                            AWSVPCReference = responseDescribeVPC.Vpcs[0].VpcId,
                            AWSVPCDefaultSecurityGroup = responseSecurityGroups.SecurityGroups[0].GroupId,
                            type = VPCType.DefaultVM,
                            BaseIPv4CIDR = responseDescribeVPC.Vpcs[0].CidrBlock,
                            BaseIPv6CIDR = responseDescribeVPC.Vpcs[0].Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock
                        };
                        context.VPCs.Add(newlyCreatedVPC);
                        context.SaveChanges();
                    }
                    else
                        _logger.LogInformation("VPC already created!");
                    if (context.VPCs.FromSql("SELECT * FROM dbo.VPCs WHERE AWSVPCReference = '" + PlatformVPCID + "'").ToList().Count() == 0)
                    {
                        _logger.LogInformation("Platform VPC not inside SQL Database!");
                        responseDescribeVPC = await ec2Client.DescribeVpcsAsync(new DescribeVpcsRequest
                        {
                            Filters = new List<Filter>
                            {
                                new Filter
                                {
                                    Name = "vpc-id",
                                    Values = new List<string>
                                    {
                                        PlatformVPCID
                                    }
                                }
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
                                    PlatformVPCID
                                }
                            }
                        }
                        });
                        VPC newlyCreatedVPC = new VPC
                        {
                            AWSVPCReference = responseDescribeVPC.Vpcs[0].VpcId,
                            AWSVPCDefaultSecurityGroup = responseSecurityGroups.SecurityGroups[0].GroupId,
                            type = VPCType.Platform,
                            BaseIPv4CIDR = responseDescribeVPC.Vpcs[0].CidrBlock,
                            BaseIPv6CIDR = responseDescribeVPC.Vpcs[0].Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock
                        };
                        context.VPCs.Add(newlyCreatedVPC);
                        context.SaveChanges();
                    }
                }
                VPC vpc = await context.VPCs.FindAsync(1);
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
                    string[] ipv6CIDRStr = vpc.BaseIPv6CIDR.Split(":");
                    for (int i = 0; i < 3; i++)
                    {
                        CreateSubnetResponse responseCreateSubnet = await ec2Client.CreateSubnetAsync(new CreateSubnetRequest
                        {
                            CidrBlock = vpc.BaseIPv4CIDR.Substring(0, 7) + i + ".0/24",
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
                else if (responseDescribeSubnets.Subnets.Count >= 3)
                {
                    if (!context.Subnets.Any())
                    {
                        _logger.LogInformation("Subnets already created but not inside SQL Database!");
                        Flag = true;
                    }
                    else
                        _logger.LogInformation("Subnets already created!");
                }
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
                }
                else
                {
                    if (!context.RouteTables.Any() && Flag == true)
                    {
                        _logger.LogInformation("Route Tables already created but not inside SQL database!");
                        Flag = false;
                        foreach (Amazon.EC2.Model.RouteTable RT in responseDescribeRouteTable.RouteTables)
                        {
                            Boolean isInternet = false;
                            Boolean isExtranet = false;
                            Boolean isIntranet = false;
                            int IntranetCounter = 0;
                            RouteTable newRT = new RouteTable
                            {
                                VPCID = vpc.ID,
                                AWSVPCRouteTableReference = RT.RouteTableId
                            };
                            context.RouteTables.Add(newRT);
                            context.SaveChanges();
                            List<RouteTable> queryResult = context.RouteTables.FromSql("SELECT * FROM dbo.RouteTables WHERE AWSVPCRouteTableReference = '" + RT.RouteTableId + "'").ToList();
                            if (queryResult.Count() == 1)
                                newRT = queryResult[0];
                            foreach (Amazon.EC2.Model.Route R in RT.Routes)
                            {
                                if (!String.IsNullOrEmpty(R.GatewayId) && R.GatewayId.Contains("igw"))
                                {
                                    isInternet = true;
                                    break;
                                }
                                else if (String.IsNullOrEmpty(R.GatewayId) && (!String.IsNullOrEmpty(R.NatGatewayId) || !String.IsNullOrEmpty(R.EgressOnlyInternetGatewayId)))
                                {
                                    isExtranet = true;
                                    break;
                                }
                                else if (!String.IsNullOrEmpty(R.GatewayId) && R.GatewayId.Contains("local"))
                                {
                                    ++IntranetCounter;
                                }
                            }
                            if (IntranetCounter == 2 && (isInternet == false || isExtranet == false))
                                isIntranet = true;
                            if (isInternet == true)
                            {
                                foreach (Amazon.EC2.Model.Route r in RT.Routes)
                                {
                                    Route newRoute = new Route();
                                    if (!string.IsNullOrEmpty(r.DestinationCidrBlock) && !string.IsNullOrEmpty(r.GatewayId))
                                    {
                                        if (r.GatewayId.Contains("igw"))
                                        {
                                            newRoute.Description = "Route To Internet (IPv4)";
                                            newRoute.RouteType = RouteType.Mandatory;
                                            newRoute.Destination = "Internet Gateway";
                                            if (r.State == Amazon.EC2.RouteState.Active)
                                                newRoute.Status = Status.OK;
                                            else if (r.State == Amazon.EC2.RouteState.Blackhole)
                                                newRoute.Status = Status.Blackhole;
                                            newRoute.IPCIDR = r.DestinationCidrBlock;
                                            newRoute.applicability = Applicability.Internet;
                                            newRoute.RouteTableID = newRT.ID;
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(r.DestinationIpv6CidrBlock) && !string.IsNullOrEmpty(r.GatewayId))
                                    {
                                        if (r.GatewayId.Contains("igw"))
                                        {
                                            newRoute.Description = "Route To Internet (IPv6)";
                                            newRoute.RouteType = RouteType.Mandatory;
                                            newRoute.Destination = "Internet Gateway";
                                            if (r.State == Amazon.EC2.RouteState.Active)
                                                newRoute.Status = Status.OK;
                                            else if (r.State == Amazon.EC2.RouteState.Blackhole)
                                                newRoute.Status = Status.Blackhole;
                                            newRoute.IPCIDR = r.DestinationIpv6CidrBlock;
                                            newRoute.applicability = Applicability.Internet;
                                            newRoute.RouteTableID = newRT.ID;
                                        }
                                    }
                                    if (newRoute.Description != null)
                                        context.Routes.Add(newRoute);
                                }
                                foreach (RouteTableAssociation a in RT.Associations)
                                {
                                    if (!string.IsNullOrEmpty(a.SubnetId))
                                    {
                                        responseDescribeSubnets = await ec2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                                        {
                                            Filters = new List<Filter>
                                            {
                                                new Filter("subnet-id",new List<string>
                                                {
                                                    a.SubnetId
                                                })
                                            }
                                        });
                                        DescribeTagsResponse responseDescribeTag = await ec2Client.DescribeTagsAsync(new DescribeTagsRequest(new List<Filter>
                                            {
                                                new Filter("key",new List<string>{
                                                    "Name"
                                                }),
                                                new Filter("resource-id",new List<string>{
                                                    a.SubnetId
                                                })
                                            }));
                                        if (responseDescribeSubnets.HttpStatusCode == HttpStatusCode.OK && responseDescribeTag.HttpStatusCode == HttpStatusCode.OK)
                                        {
                                            Subnet newSubnet = new Subnet
                                            {
                                                Name = responseDescribeTag.Tags[0].Value,
                                                Type = SubnetType.Internet,
                                                IPv4CIDR = responseDescribeSubnets.Subnets[0].CidrBlock,
                                                IPv6CIDR = responseDescribeSubnets.Subnets[0].Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock,
                                                AWSVPCSubnetReference = responseDescribeSubnets.Subnets[0].SubnetId,
                                                AWSVPCRouteTableAssoicationID = a.RouteTableAssociationId,
                                                RouteTableID = newRT.ID,
                                                VPCID = vpc.ID
                                            };
                                            switch (responseDescribeSubnets.Subnets[0].CidrBlock.Substring(responseDescribeSubnets.Subnets[0].CidrBlock.Length - 3, 3))
                                            {
                                                case "/17":
                                                    newSubnet.SubnetSize = Convert.ToString(32766);
                                                    break;
                                                case "/18":
                                                    newSubnet.SubnetSize = Convert.ToString(16382);
                                                    break;
                                                case "/19":
                                                    newSubnet.SubnetSize = Convert.ToString(8190);
                                                    break;
                                                case "/20":
                                                    newSubnet.SubnetSize = Convert.ToString(4094);
                                                    break;
                                                case "/21":
                                                    newSubnet.SubnetSize = Convert.ToString(2046);
                                                    break;
                                                case "/22":
                                                    newSubnet.SubnetSize = Convert.ToString(1022);
                                                    break;
                                                case "/23":
                                                    newSubnet.SubnetSize = Convert.ToString(510);
                                                    break;
                                                case "/24":
                                                    newSubnet.SubnetSize = Convert.ToString(254);
                                                    break;
                                                case "/25":
                                                    newSubnet.SubnetSize = Convert.ToString(126);
                                                    break;
                                                case "/26":
                                                    newSubnet.SubnetSize = Convert.ToString(62);
                                                    break;
                                                case "/27":
                                                    newSubnet.SubnetSize = Convert.ToString(30);
                                                    break;
                                                case "/28":
                                                    newSubnet.SubnetSize = Convert.ToString(14);
                                                    break;
                                                case "/29":
                                                    newSubnet.SubnetSize = Convert.ToString(6);
                                                    break;
                                                case "/30":
                                                    newSubnet.SubnetSize = Convert.ToString(2);
                                                    break;
                                                default:
                                                    break;
                                            }
                                            context.Subnets.Add(newSubnet);
                                        }
                                    }
                                }
                            }
                            else if (isExtranet == true)
                            {
                                foreach (Amazon.EC2.Model.Route r in RT.Routes)
                                {
                                    Route newRoute = new Route();
                                    if (!string.IsNullOrEmpty(r.DestinationCidrBlock) && !string.IsNullOrEmpty(r.NatGatewayId))
                                    {
                                        if (r.NatGatewayId.Contains("nat"))
                                        {
                                            newRoute.Description = "Route To Internet (IPv4)";
                                            newRoute.RouteType = RouteType.Mandatory;
                                            newRoute.Destination = "NAT Gateway";
                                            if (r.State == Amazon.EC2.RouteState.Active)
                                                newRoute.Status = Status.OK;
                                            else if (r.State == Amazon.EC2.RouteState.Blackhole)
                                                newRoute.Status = Status.Blackhole;
                                            newRoute.IPCIDR = r.DestinationCidrBlock;
                                            newRoute.applicability = Applicability.Internet;
                                            newRoute.RouteTableID = newRT.ID;
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(r.DestinationIpv6CidrBlock) && !string.IsNullOrEmpty(r.EgressOnlyInternetGatewayId))
                                    {
                                        if (r.EgressOnlyInternetGatewayId.Contains("eigw"))
                                        {
                                            newRoute.Description = "Route To Internet (IPv6)";
                                            newRoute.RouteType = RouteType.Mandatory;
                                            newRoute.Destination = "Internet-Only Gateway";
                                            if (r.State == Amazon.EC2.RouteState.Active)
                                                newRoute.Status = Status.OK;
                                            else if (r.State == Amazon.EC2.RouteState.Blackhole)
                                                newRoute.Status = Status.Blackhole;
                                            newRoute.IPCIDR = r.DestinationIpv6CidrBlock;
                                            newRoute.applicability = Applicability.Internet;
                                            newRoute.RouteTableID = newRT.ID;
                                        }
                                    }
                                    if (newRoute.Description != null)
                                        context.Routes.Add(newRoute);
                                }
                                foreach (RouteTableAssociation a in RT.Associations)
                                {
                                    if (!string.IsNullOrEmpty(a.SubnetId))
                                    {
                                        responseDescribeSubnets = await ec2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                                        {
                                            Filters = new List<Filter>
                                            {
                                                new Filter("subnet-id",new List<string>
                                                {
                                                    a.SubnetId
                                                })
                                            }
                                        });
                                        DescribeTagsResponse responseDescribeTag = await ec2Client.DescribeTagsAsync(new DescribeTagsRequest(new List<Filter>
                                            {
                                                new Filter("key",new List<string>{
                                                    "Name"
                                                }),
                                                new Filter("resource-id",new List<string>{
                                                    a.SubnetId
                                                })
                                            }));
                                        if (responseDescribeSubnets.HttpStatusCode == HttpStatusCode.OK && responseDescribeTag.HttpStatusCode == HttpStatusCode.OK)
                                        {
                                            Subnet newSubnet = new Subnet
                                            {
                                                Name = responseDescribeTag.Tags[0].Value,
                                                Type = SubnetType.Extranet,
                                                IPv4CIDR = responseDescribeSubnets.Subnets[0].CidrBlock,
                                                IPv6CIDR = responseDescribeSubnets.Subnets[0].Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock,
                                                AWSVPCSubnetReference = responseDescribeSubnets.Subnets[0].SubnetId,
                                                AWSVPCRouteTableAssoicationID = a.RouteTableAssociationId,
                                                RouteTableID = newRT.ID,
                                                VPCID = vpc.ID
                                            };
                                            switch (responseDescribeSubnets.Subnets[0].CidrBlock.Substring(responseDescribeSubnets.Subnets[0].CidrBlock.Length - 3, 3))
                                            {
                                                case "/17":
                                                    newSubnet.SubnetSize = Convert.ToString(32766);
                                                    break;
                                                case "/18":
                                                    newSubnet.SubnetSize = Convert.ToString(16382);
                                                    break;
                                                case "/19":
                                                    newSubnet.SubnetSize = Convert.ToString(8190);
                                                    break;
                                                case "/20":
                                                    newSubnet.SubnetSize = Convert.ToString(4094);
                                                    break;
                                                case "/21":
                                                    newSubnet.SubnetSize = Convert.ToString(2046);
                                                    break;
                                                case "/22":
                                                    newSubnet.SubnetSize = Convert.ToString(1022);
                                                    break;
                                                case "/23":
                                                    newSubnet.SubnetSize = Convert.ToString(510);
                                                    break;
                                                case "/24":
                                                    newSubnet.SubnetSize = Convert.ToString(254);
                                                    break;
                                                case "/25":
                                                    newSubnet.SubnetSize = Convert.ToString(126);
                                                    break;
                                                case "/26":
                                                    newSubnet.SubnetSize = Convert.ToString(62);
                                                    break;
                                                case "/27":
                                                    newSubnet.SubnetSize = Convert.ToString(30);
                                                    break;
                                                case "/28":
                                                    newSubnet.SubnetSize = Convert.ToString(14);
                                                    break;
                                                case "/29":
                                                    newSubnet.SubnetSize = Convert.ToString(6);
                                                    break;
                                                case "/30":
                                                    newSubnet.SubnetSize = Convert.ToString(2);
                                                    break;
                                                default:
                                                    break;
                                            }
                                            context.Subnets.Add(newSubnet);
                                        }
                                    }
                                }
                            }
                            else if (isIntranet == true)
                            {
                                foreach (Amazon.EC2.Model.Route r in RT.Routes)
                                {
                                    Route newRoute = new Route();
                                    if (!string.IsNullOrEmpty(r.DestinationCidrBlock) && !string.IsNullOrEmpty(r.GatewayId))
                                    {
                                        if (r.GatewayId.Equals("local"))
                                        {
                                            newRoute.Description = "Route To Challenge Network (IPv4)";
                                            newRoute.RouteType = RouteType.Mandatory;
                                            newRoute.Destination = "Challenge Network";
                                            if (r.State == Amazon.EC2.RouteState.Active)
                                                newRoute.Status = Status.OK;
                                            else if (r.State == Amazon.EC2.RouteState.Blackhole)
                                                newRoute.Status = Status.Blackhole;
                                            newRoute.IPCIDR = r.DestinationCidrBlock;
                                            newRoute.applicability = Applicability.All;
                                            newRoute.RouteTableID = newRT.ID;
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(r.DestinationIpv6CidrBlock) && !string.IsNullOrEmpty(r.GatewayId))
                                    {
                                        if (r.GatewayId.Equals("local"))
                                        {
                                            newRoute.Description = "Route To Challenge Network (IPv6)";
                                            newRoute.RouteType = RouteType.Mandatory;
                                            newRoute.Destination = "Challenge Network";
                                            if (r.State == Amazon.EC2.RouteState.Active)
                                                newRoute.Status = Status.OK;
                                            else if (r.State == Amazon.EC2.RouteState.Blackhole)
                                                newRoute.Status = Status.Blackhole;
                                            newRoute.IPCIDR = r.DestinationIpv6CidrBlock;
                                            newRoute.applicability = Applicability.All;
                                            newRoute.RouteTableID = newRT.ID;
                                        }
                                    }
                                    if (newRoute.Description != null)
                                        context.Routes.Add(newRoute);
                                }
                                foreach (RouteTableAssociation a in RT.Associations)
                                {
                                    if (!string.IsNullOrEmpty(a.SubnetId))
                                    {
                                        responseDescribeSubnets = await ec2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                                        {
                                            Filters = new List<Filter>
                                            {
                                                new Filter("subnet-id",new List<string>
                                                {
                                                    a.SubnetId
                                                })
                                            }
                                        });
                                        DescribeTagsResponse responseDescribeTag = await ec2Client.DescribeTagsAsync(new DescribeTagsRequest(new List<Filter>
                                            {
                                                new Filter("key",new List<string>{
                                                    "Name"
                                                }),
                                                new Filter("resource-id",new List<string>{
                                                    a.SubnetId
                                                })
                                            }));
                                        if (responseDescribeSubnets.HttpStatusCode == HttpStatusCode.OK && responseDescribeTag.HttpStatusCode == HttpStatusCode.OK)
                                        {
                                            Subnet newSubnet = new Subnet
                                            {
                                                Name = responseDescribeTag.Tags[0].Value,
                                                Type = SubnetType.Intranet,
                                                IPv4CIDR = responseDescribeSubnets.Subnets[0].CidrBlock,
                                                IPv6CIDR = responseDescribeSubnets.Subnets[0].Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock,
                                                AWSVPCSubnetReference = responseDescribeSubnets.Subnets[0].SubnetId,
                                                AWSVPCRouteTableAssoicationID = a.RouteTableAssociationId,
                                                RouteTableID = newRT.ID,
                                                VPCID = vpc.ID
                                            };
                                            switch (responseDescribeSubnets.Subnets[0].CidrBlock.Substring(responseDescribeSubnets.Subnets[0].CidrBlock.Length - 3, 3))
                                            {
                                                case "/17":
                                                    newSubnet.SubnetSize = Convert.ToString(32766);
                                                    break;
                                                case "/18":
                                                    newSubnet.SubnetSize = Convert.ToString(16382);
                                                    break;
                                                case "/19":
                                                    newSubnet.SubnetSize = Convert.ToString(8190);
                                                    break;
                                                case "/20":
                                                    newSubnet.SubnetSize = Convert.ToString(4094);
                                                    break;
                                                case "/21":
                                                    newSubnet.SubnetSize = Convert.ToString(2046);
                                                    break;
                                                case "/22":
                                                    newSubnet.SubnetSize = Convert.ToString(1022);
                                                    break;
                                                case "/23":
                                                    newSubnet.SubnetSize = Convert.ToString(510);
                                                    break;
                                                case "/24":
                                                    newSubnet.SubnetSize = Convert.ToString(254);
                                                    break;
                                                case "/25":
                                                    newSubnet.SubnetSize = Convert.ToString(126);
                                                    break;
                                                case "/26":
                                                    newSubnet.SubnetSize = Convert.ToString(62);
                                                    break;
                                                case "/27":
                                                    newSubnet.SubnetSize = Convert.ToString(30);
                                                    break;
                                                case "/28":
                                                    newSubnet.SubnetSize = Convert.ToString(14);
                                                    break;
                                                case "/29":
                                                    newSubnet.SubnetSize = Convert.ToString(6);
                                                    break;
                                                case "/30":
                                                    newSubnet.SubnetSize = Convert.ToString(2);
                                                    break;
                                                default:
                                                    break;
                                            }
                                            context.Subnets.Add(newSubnet);
                                        }
                                    }
                                }
                            }
                            context.SaveChanges();
                        }
                    }
                    else
                        _logger.LogInformation("Route Tables already created!");
                }
                if (!context.CloudWatchLogGroups.Any() && !context.CloudWatchLogStreams.Any())
                {
                    _logger.LogInformation("Importing CloudWatch Data...");
                    DescribeLogGroupsResponse responseDescribeLogGroups = await cwlClient.DescribeLogGroupsAsync(new DescribeLogGroupsRequest());
                    foreach (LogGroup lg in responseDescribeLogGroups.LogGroups)
                    {
                        CloudWatchLogGroup newG = new CloudWatchLogGroup
                        {
                            ARN = lg.Arn,
                            CreationTime = lg.CreationTime
                        };
                        if (lg.LogGroupName.Contains("/"))
                            newG.Name = lg.LogGroupName.Replace("/", "@");
                        else
                            newG.Name = lg.LogGroupName;
                        if (lg.RetentionInDays != null)
                            lg.RetentionInDays = (int)lg.RetentionInDays;
                        context.CloudWatchLogGroups.Add(newG);
                        context.SaveChanges();
                        List<CloudWatchLogGroup> gQuery = context.CloudWatchLogGroups.FromSql("SELECT * FROM dbo.CloudWatchLogGroups WHERE ARN = {0}",lg.Arn).ToList();
                        if (gQuery.Count() == 1)
                            newG = gQuery[0];
                        DescribeLogStreamsResponse responseDescribeLogStreams = await cwlClient.DescribeLogStreamsAsync(new DescribeLogStreamsRequest
                        {
                            LogGroupName = lg.LogGroupName
                        });
                        foreach (LogStream ls in responseDescribeLogStreams.LogStreams)
                        {
                            CloudWatchLogStream newS = new CloudWatchLogStream
                            {
                                ARN = ls.Arn,
                                CreationTime = ls.CreationTime,
                                FirstEventTime = ls.FirstEventTimestamp,
                                LastEventTime = ls.LastEventTimestamp,
                                Name = ls.LogStreamName,
                                LinkedGroupID = newG.ID
                            };
                            if (newG.Name.Equals("VMVPCLogs"))
                                newS.DisplayName = "Network Flow Log For Challenge Network Interface (" + newS.Name.Substring(0, newS.Name.Length - 4) + ")";
                            else if (newG.Name.Equals("PlatformVPCLogs"))
                                newS.DisplayName = "Network Flow Log For Platform Network Interface (" + newS.Name.Substring(0, newS.Name.Length - 4) + ")";
                            else if (newG.Name.Equals("RDSOSMetrics"))
                            {
                                if (!newS.Name.Equals("db-74DSOXWDBQWHTVNTY7RFXWRZYE"))
                                    newS.DisplayName = "SQL Database CPU Usage";
                            }
                            else if (newG.Name.Equals("@aws@elasticbeanstalk@User-Side@IIS-Log"))
                                newS.DisplayName = "IIS Logs for User Side Web Server";
                            else if (newG.Name.Equals("@aws@elasticbeanstalk@Admin-Side@IIS-Log"))
                                newS.DisplayName = "IIS Logs for Admin Side Web Server";
                            else if (newG.Name.Equals("@aws@elasticbeanstalk@User-Side@EBDeploy-Log"))
                                newS.DisplayName = "Elastic Beanstalk Deployment Tool Logs for User Side";
                            else if (newG.Name.Equals("@aws@elasticbeanstalk@Admin-Side@EBDeploy-Log"))
                                newS.DisplayName = "Elastic Beanstalk Deployment Tool Logs for Admin Side";
                            else if (newG.Name.Equals("@aws@elasticbeanstalk@User-Side@EBHooks-Log"))
                                newS.DisplayName = "Elastic Beanstalk Deployment Hook Logs for User Side";
                            else if (newG.Name.Equals("@aws@elasticbeanstalk@Admin-Side@EBHooks-Log"))
                                newS.DisplayName = "Elastic Beanstalk Deployment Hook Logs for Admin Side";
                            else
                                newS.DisplayName = newS.Name;
                            if (!newS.Name.Equals("db-74DSOXWDBQWHTVNTY7RFXWRZYE"))
                                context.CloudWatchLogStreams.Add(newS);
                        }
                        await context.SaveChangesAsync();
                    }
                }
                _logger.LogInformation("Setup Background Service Completed!");
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