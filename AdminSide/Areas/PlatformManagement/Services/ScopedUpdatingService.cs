using Microsoft.Extensions.Logging;
using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using Amazon.EC2;
using Amazon.EC2.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using Subnet = AdminSide.Areas.PlatformManagement.Models.Subnet;
using State = AdminSide.Areas.PlatformManagement.Models.State;
using System.Data.SqlClient;
using Amazon.CloudWatch;
using Amazon.CloudWatchEvents;
using Amazon.CloudWatchLogs;
using System.ComponentModel;

namespace AdminSide.Areas.PlatformManagement.Services
{
    internal class ScopedUpdatingService : IScopedUpdatingService
    {
        private readonly ILogger _logger;
        private readonly PlatformResourcesContext context;
        private readonly IAmazonEC2 ec2Client;
        private readonly IAmazonCloudWatch cwClient;
        private readonly IAmazonCloudWatchEvents cweClient;
        private readonly IAmazonCloudWatchLogs cwlClient;

        public ScopedUpdatingService(ILogger<ScopedUpdatingService> logger, PlatformResourcesContext context, IAmazonEC2 EC2Client, IAmazonCloudWatch cloudwatchClient, IAmazonCloudWatchEvents cloudwatcheventsClient, IAmazonCloudWatchLogs cloudwatchlogsClient)
        {
            _logger = logger;
            this.context = context;
            ec2Client = EC2Client;
            cwClient = cloudwatchClient;
            cweClient = cloudwatcheventsClient;
            cwlClient = cloudwatchlogsClient;
        }

        public async Task DoWorkAsync()
        {
            _logger.LogInformation("Update Background Service is running.");
            try
            {
                VPC vpc = await context.VPCs.FindAsync(1);
                //if (context.Subnets.Any())
                //{
                //    _logger.LogInformation("Update Background Service is checking Subnets");
                //    DescribeSubnetsResponse response = await ec2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                //    {
                //        Filters = new List<Filter>
                //{
                //     new Filter {Name = "vpc-id", Values = new List<string> {vpc.AWSVPCReference}}
                //}
                //    });
                //}
                //else
                //{
                //    DescribeSubnetsResponse response = await ec2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                //    {
                //        Filters = new List<Filter>
                //{
                //     new Filter {Name = "vpc-id", Values = new List<string> {vpc.AWSVPCReference}}
                //}
                //    });
                //    int incrementer = 4;
                //    context.Database.OpenConnection();
                //    context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Subnet ON");
                //    foreach (var subnet in response.Subnets)
                //    {
                //        try
                //        {
                //            Subnet newSubnet = new Subnet();
                //            if (subnet.CidrBlock.Equals("172.30.0.0/24"))
                //            {
                //                newSubnet.ID = 1;
                //                newSubnet.Name = "Default Internet Subnet";
                //                newSubnet.IPv4CIDR = subnet.CidrBlock;
                //                newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                //                newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                //                newSubnet.Type = SubnetType.Internet;
                //                newSubnet.SubnetSize = "254";
                //                context.Subnets.Add(newSubnet);
                //                await context.SaveChangesAsync();
                //            }
                //            else if (subnet.CidrBlock.Equals("172.30.1.0/24"))
                //            {
                //                newSubnet.ID = 2;
                //                newSubnet.Name = "Default Extranet Subnet";
                //                newSubnet.IPv4CIDR = subnet.CidrBlock;
                //                newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                //                newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                //                newSubnet.Type = SubnetType.Extranet;
                //                newSubnet.SubnetSize = "254";
                //                context.Subnets.Add(newSubnet);
                //                await context.SaveChangesAsync();
                //            }
                //            else if (subnet.CidrBlock.Equals("172.30.2.0/24"))
                //            {
                //                newSubnet.ID = 3;
                //                newSubnet.Name = "Default Intranet Subnet";
                //                newSubnet.IPv4CIDR = subnet.CidrBlock;
                //                newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                //                newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                //                newSubnet.Type = SubnetType.Intranet;
                //                newSubnet.SubnetSize = "254";
                //                context.Subnets.Add(newSubnet);
                //                await context.SaveChangesAsync();
                //            }
                //            else
                //            {
                //                newSubnet.ID = incrementer;
                //                newSubnet.Name = subnet.SubnetId;
                //                newSubnet.IPv4CIDR = subnet.CidrBlock;
                //                newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                //                newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                //                newSubnet.Type = SubnetType.Extranet;
                //                string subnetPrefix = subnet.CidrBlock.Substring(subnet.CidrBlock.Length - 3);
                //                switch (subnetPrefix)
                //                {
                //                    case "/17":
                //                        newSubnet.SubnetSize = Convert.ToString(32766);
                //                        break;
                //                    case "/18":
                //                        newSubnet.SubnetSize = Convert.ToString(16382);
                //                        break;
                //                    case "/19":
                //                        newSubnet.SubnetSize = Convert.ToString(8190);
                //                        break;
                //                    case "/20":
                //                        newSubnet.SubnetSize = Convert.ToString(4094);
                //                        break;
                //                    case "/21":
                //                        newSubnet.SubnetSize = Convert.ToString(2046);
                //                        break;
                //                    case "/22":
                //                        newSubnet.SubnetSize = Convert.ToString(1022);
                //                        break;
                //                    case "/23":
                //                        newSubnet.SubnetSize = Convert.ToString(510);
                //                        break;
                //                    case "/24":
                //                        newSubnet.SubnetSize = Convert.ToString(254);
                //                        break;
                //                    case "/25":
                //                        newSubnet.SubnetSize = Convert.ToString(126);
                //                        break;
                //                    case "/26":
                //                        newSubnet.SubnetSize = Convert.ToString(62);
                //                        break;
                //                    case "/27":
                //                        newSubnet.SubnetSize = Convert.ToString(30);
                //                        break;
                //                    case "/28":
                //                        newSubnet.SubnetSize = Convert.ToString(14);
                //                        break;
                //                    case "/29":
                //                        newSubnet.SubnetSize = Convert.ToString(6);
                //                        break;
                //                    case "/30":
                //                        newSubnet.SubnetSize = Convert.ToString(2);
                //                        break;
                //                    default:
                //                        break;
                //                }
                //                context.Subnets.Add(newSubnet);
                //                await context.SaveChangesAsync();
                //                ++incrementer;
                //            }
                //        }
                //        catch (Exception)
                //        {
                //            context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Subnet OFF");
                //            if (context.Subnets.Any())
                //            {
                //                foreach (Subnet failed in context.Subnets)
                //                {
                //                    context.Subnets.Remove(failed);
                //                }
                //                context.SaveChanges();
                //            }
                //            context.Database.CloseConnection();
                //        }
                //    }
                //    context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Subnet OFF");
                //    context.Database.CloseConnection();
                //}
                if (context.Servers.Any())
                {
                    _logger.LogInformation("Update Background Service is checking servers");
                    List<Server> servers = await context.Servers.ToListAsync();
                    DescribeInstancesResponse response = await ec2Client.DescribeInstancesAsync(new DescribeInstancesRequest
                    {
                        Filters = new List<Filter>
                {
                     new Filter {Name = "vpc-id", Values = new List<string> {vpc.AWSVPCReference}}
                }
                    });
                    foreach (var server in servers)
                    {
                        foreach (var reservation in response.Reservations)
                        {
                            foreach (var instance in reservation.Instances)
                            {
                                bool Flag = false;
                                if (server.AWSEC2Reference.Equals(instance.InstanceId))
                                {
                                    if (instance.State.Code == 0 && server.State != State.Starting)
                                    {
                                        server.State = State.Starting;
                                        Flag = true;
                                    }
                                    else if (instance.State.Code == 16 && server.State != State.Running)
                                    {
                                        server.State = State.Running;
                                        Flag = true;
                                    }
                                    else if (instance.State.Code == 64 && server.State != State.Stopping)
                                    {
                                        server.State = State.Stopping;
                                        Flag = true;
                                    }
                                    else if (instance.State.Code == 80 && server.State != State.Stopped)
                                    {
                                        server.State = State.Stopped;
                                        Flag = true;
                                    }
                                    if (server.Visibility == Visibility.Internet && (server.IPAddress != instance.PublicIpAddress || server.DNSHostname != instance.PublicDnsName) && server.State != State.Stopped)
                                    {
                                        server.IPAddress = instance.PublicIpAddress;
                                        server.DNSHostname = instance.PublicDnsName;
                                        Flag = true;
                                    } else if (server.Visibility == Visibility.Internet && (server.IPAddress != instance.PublicIpAddress || server.DNSHostname != instance.PublicDnsName))
                                    {
                                        server.IPAddress = "Public IP Address is not available when server is stopped";
                                        server.DNSHostname = "Public DNS Hostname is not available when server is stopped";
                                        Flag = true;
                                    }
                                    else if ((server.Visibility == Visibility.Extranet || server.Visibility == Visibility.Intranet) && (server.IPAddress != instance.PrivateIpAddress || server.DNSHostname != instance.PrivateDnsName))
                                    {
                                        server.IPAddress = instance.PrivateIpAddress;
                                        server.DNSHostname = instance.PrivateDnsName;
                                        Flag = true;
                                    }
                                    if (Flag == true)
                                        context.Servers.Update(server);
                                    break;
                                }
                            }
                        }
                    }
                    context.SaveChanges();
                }
                else
                {
                    DescribeInstancesResponse response = await ec2Client.DescribeInstancesAsync(new DescribeInstancesRequest
                    {
                        Filters = new List<Filter>
                {
                     new Filter {Name = "vpc-id", Values = new List<string> {vpc.AWSVPCReference}}
                }
                    });
                }
                //_logger.LogInformation("Update Background Service is getting RSS Feeds");
                //var feed = await FeedReader.ReadAsync("https://hnrss.org/newcomments");
                //foreach (FeedItem var in feed.Items)
                //{
                //    RSSFeed Feed = new RSSFeed
                //    {
                //        Title = var.Title,
                //        Link = var.Link,
                //        Description = var.Description,
                //        PubDate = var.PublishingDateString,
                //        main = false
                //    };
                //    NewsFeedcontext.Feeds.Add(Feed);
                //}
                //await NewsFeedcontext.SaveChangesAsync();
                _logger.LogInformation("Update Background Service has completed!");
            } catch (SqlException e)
            {
                _logger.LogInformation("Update Background Service faced an SQL exception! "+e.Message+" | "+e.Source);
                return;
            } catch (Exception e)
            {
                _logger.LogInformation("Update Background Service faced an exception! " + e.Message + " | " + e.Source);
                return;
            } 
        }
    }
}