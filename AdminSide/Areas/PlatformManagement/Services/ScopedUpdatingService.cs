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