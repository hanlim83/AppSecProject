using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using Amazon.CloudWatch;
using Amazon.CloudWatchEvents;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.EC2;
using Amazon.EC2.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using State = AdminSide.Areas.PlatformManagement.Models.State;

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
            _logger.LogInformation("Update Background Service is running");
            try
            {
                _logger.LogInformation("Update Background Service is checking VPCs");
                if (context.VPCs.Any())
                {
                    DescribeVpcsResponse responseDescribeVPC = await ec2Client.DescribeVpcsAsync();
                    List<VPC> vpcs = await context.VPCs.ToListAsync();
                    foreach (VPC vpc in vpcs)
                    {
                        Boolean Flag = false;

                        foreach (Vpc AWSVPC in responseDescribeVPC.Vpcs)
                        {
                            if (vpc.AWSVPCReference.Equals(AWSVPC.VpcId))
                            {
                                Flag = true;
                                break;
                            }
                        }
                        if (Flag == false)
                        {
                            context.VPCs.Remove(vpc);
                        }
                    }
                    context.SaveChanges();
                    _logger.LogInformation("Update Background Service completed checking of VPCs");
                }
                if (context.VPCs.Any())
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
                                        }
                                        else if (server.Visibility == Visibility.Internet && (server.IPAddress != instance.PublicIpAddress || server.DNSHostname != instance.PublicDnsName))
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
                                        {
                                            context.Servers.Update(server);
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                        context.SaveChanges();
                    }
                    if (context.CloudWatchLogGroups.Any() && context.CloudWatchLogStreams.Any())
                    {
                        _logger.LogInformation("Update Background Service is checking cloudwatch groups");
                        List<CloudWatchLogGroup> allStreams = await context.CloudWatchLogGroups.ToListAsync();
                        foreach (CloudWatchLogGroup g in allStreams)
                        {
                            DescribeLogStreamsResponse response = await cwlClient.DescribeLogStreamsAsync(new DescribeLogStreamsRequest
                            {
                                LogGroupName = g.Name.Replace("@", "/")
                            });
                            foreach (LogStream ls in response.LogStreams)
                            {
                                Boolean Flag = false;
                                foreach (CloudWatchLogStream CWLS in g.LogStreams)
                                {
                                    if (ls.Arn.Equals(CWLS.ARN))
                                    {
                                        Flag = true;
                                        break;
                                    }
                                }
                                if (Flag == false)
                                {
                                    CloudWatchLogStream newS = new CloudWatchLogStream
                                    {
                                        ARN = ls.Arn,
                                        CreationTime = ls.CreationTime,
                                        FirstEventTime = ls.FirstEventTimestamp,
                                        LastEventTime = ls.LastEventTimestamp,
                                        Name = ls.LogStreamName,
                                        LinkedGroupID = g.ID
                                    };
                                    if (g.Name.Equals("VMVPCLogs"))
                                    {
                                        newS.DisplayName = "Network Flow Log For Challenge Network Interface (" + newS.Name.Substring(0, newS.Name.Length - 4) + ")";
                                    }
                                    else if (g.Name.Equals("PlatformVPCLogs"))
                                    {
                                        newS.DisplayName = "Network Flow Log For Platform Network Interface (" + newS.Name.Substring(0, newS.Name.Length - 4) + ")";
                                    }
                                    else if (g.Name.Equals("RDSOSMetrics"))
                                    {
                                        if (!newS.Name.Equals("db-74DSOXWDBQWHTVNTY7RFXWRZYE"))
                                        {
                                            newS.DisplayName = "SQL Database CPU Usage";
                                        }
                                    }
                                    else if (g.Name.Equals("@aws@elasticbeanstalk@User-Side@IIS-Log"))
                                    {
                                        newS.DisplayName = "IIS Logs for User Side Web Server";
                                    }
                                    else if (g.Name.Equals("@aws@elasticbeanstalk@Admin-Side@IIS-Log"))
                                    {
                                        newS.DisplayName = "IIS Logs for Admin Side Web Server";
                                    }
                                    else if (g.Name.Equals("@aws@elasticbeanstalk@User-Side@EBDeploy-Log"))
                                    {
                                        newS.DisplayName = "Elastic Beanstalk Deployment Tool Logs for User Side";
                                    }
                                    else if (g.Name.Equals("@aws@elasticbeanstalk@Admin-Side@EBDeploy-Log"))
                                    {
                                        newS.DisplayName = "Elastic Beanstalk Deployment Tool Logs for Admin Side";
                                    }
                                    else if (g.Name.Equals("@aws@elasticbeanstalk@User-Side@EBHooks-Log"))
                                    {
                                        newS.DisplayName = "Elastic Beanstalk Deployment Hook Logs for User Side";
                                    }
                                    else if (g.Name.Equals("@aws@elasticbeanstalk@Admin-Side@EBHooks-Log"))
                                    {
                                        newS.DisplayName = "Elastic Beanstalk Deployment Hook Logs for Admin Side";
                                    }
                                    else
                                    {
                                        newS.DisplayName = newS.Name;
                                    }

                                    if (!newS.Name.Equals("db-74DSOXWDBQWHTVNTY7RFXWRZYE"))
                                    {
                                        context.CloudWatchLogStreams.Add(newS);
                                    }
                                }
                            }
                            foreach (CloudWatchLogStream CWLS in g.LogStreams)
                            {
                                Boolean Flag = false;
                                foreach (LogStream ls in response.LogStreams)
                                {
                                    if (CWLS.ARN.Equals(ls.Arn))
                                    {
                                        Flag = true;
                                        break;
                                    }
                                }
                                if (Flag == false)
                                {
                                    context.CloudWatchLogStreams.Remove(CWLS);
                                }
                            }
                        }
                        await context.SaveChangesAsync();
                    }
                    if (context.Templates.Any())
                    {
                        DescribeSnapshotsResponse response = await ec2Client.DescribeSnapshotsAsync(new DescribeSnapshotsRequest());
                        List<Template> templates = await context.Templates.FromSql("SELECT * FROM dbo.Templates WHERE AWSSnapshotReference = NULL").ToListAsync();
                        foreach (Template t in templates)
                        {
                            foreach (Snapshot s in response.Snapshots)
                            {
                                if (s.Description.Contains(t.AWSAMIReference))
                                {
                                    t.AWSSnapshotReference = s.SnapshotId;
                                    break;
                                }
                            }
                        }
                        await context.SaveChangesAsync();
                    }
                }
                _logger.LogInformation("Update Background Service has completed!");
            }
            catch (SqlException e)
            {
                _logger.LogInformation("Update Background Service faced an SQL exception! " + e.Message + " | " + e.Source);
                return;
            }
            catch (Exception e)
            {
                _logger.LogInformation("Update Background Service faced an exception! " + e.Message + " | " + e.Source);
                return;
            }
        }
    }
}