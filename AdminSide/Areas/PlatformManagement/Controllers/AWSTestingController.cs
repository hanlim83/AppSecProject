﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminSide.Areas.PlatformManagement.Models;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.CloudWatchEvents;
using Amazon.CloudWatchEvents.Model;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.RDS;
using Amazon.RDS.Model;
using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using Amazon.ElasticBeanstalk;
using Amazon.ElasticBeanstalk.Model;
using System.Net;
using Microsoft.AspNetCore.Http;
using ASPJ_MVC.Models;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    public class AWSTestingController : Controller
    {
        IAmazonS3 S3Client { get; set; }

        IAmazonEC2 EC2Client { get; set; }

        IAmazonCloudWatch CloudwatchClient { get; set; }

        IAmazonCloudWatchEvents CloudwatchEventsClient { get; set; }

        IAmazonCloudWatchLogs CloudwatchLogsClient { get; set; }

        IAmazonSimpleNotificationService SNSClient { get; set; }

        IAmazonRDS RDSClient { get; set; }

        IAmazonElasticLoadBalancingV2 ELBClient { get; set; }

        IAmazonElasticBeanstalk EBSClient { get; set; }

        public AWSTestingController(IAmazonS3 s3Client, IAmazonEC2 ec2Client, IAmazonCloudWatch cloudwatchClient, IAmazonCloudWatchEvents cloudwatcheventsClient, IAmazonCloudWatchLogs cloudwatchLogsClient, IAmazonSimpleNotificationService snsClient, IAmazonRDS rdsClient, IAmazonElasticLoadBalancingV2 elbClient, IAmazonElasticBeanstalk ebsClient)
        {
            this.S3Client = s3Client;
            this.EC2Client = ec2Client;
            this.CloudwatchClient = cloudwatchClient;
            this.CloudwatchEventsClient = cloudwatcheventsClient;
            this.CloudwatchLogsClient = cloudwatchLogsClient;
            this.SNSClient = snsClient;
            this.RDSClient = rdsClient;
            this.ELBClient = elbClient;
            this.EBSClient = ebsClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> S3List()
        {
            return View(await S3Client.ListBucketsAsync());
        }

        public IActionResult S3Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> S3Create([Bind("BucketName")] AWSTestingS3FormModel data)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    PutBucketResponse response = await S3Client.PutBucketAsync(data.BucketName);
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        return RedirectToAction("S3List");
                    }
                    else
                    {
                        return RedirectToAction("S3List");
                    }

                }
                catch (AmazonS3Exception e)
                {
                    ViewData["Exception"] = e.Message;
                    return View();
                }
            }
            else
            {
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> EC2List()
        {
            return View(await EC2Client.DescribeInstancesAsync(new DescribeInstancesRequest
            {
                Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                }
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EC2List(string instanceID)
        {
            DescribeInstancesResponse response = await EC2Client.DescribeInstancesAsync(new DescribeInstancesRequest
            {
                Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                }
            });
            Boolean flag = false;
            foreach(Amazon.EC2.Model.Reservation r in response.Reservations)
            {
                foreach(Amazon.EC2.Model.Instance i in r.Instances)
                {
                    if (instanceID.Equals(i.InstanceId))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag == true)
                    break;
            }
            if (flag == true)
            {
                TerminateInstancesRequest request = new TerminateInstancesRequest
                {
                    InstanceIds = new List<string>
                    {
                        instanceID
                    }
                };
                try
                {
                    TerminateInstancesResponse responseT = await EC2Client.TerminateInstancesAsync(request);
                    if (responseT.HttpStatusCode == HttpStatusCode.OK)
                    {
                        ViewData["Result"] = "Successfully Terminated!";
                        return View();
                    }
                    else
                    {
                        ViewData["Result"] = "Something happened";
                        return View();
                    }
                } catch (AmazonEC2Exception e)
                {
                    ViewData["Result"] = e.Message;
                    return View();
                }
            } else
            {
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> EC2Create()
        {
            return View(await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
            {
                Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                }
            }));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EC2Create(string imageID, string subnetID)
        {
            DescribeSecurityGroupsResponse response = await EC2Client.DescribeSecurityGroupsAsync(new DescribeSecurityGroupsRequest
            {
                Filters = new List<Amazon.EC2.Model.Filter>
                        {
                            new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                        }
            });
            SecurityGroup securityGroup = response.SecurityGroups[0];
            RunInstancesRequest request = new RunInstancesRequest
            {
                ImageId = imageID,
                InstanceType = InstanceType.T1Micro,
                MinCount = 1,
                MaxCount = 1,
                KeyName = "ASPJ Instances Master Key Pair",
                SubnetId = subnetID
            };
            try
            {
                RunInstancesResponse responseI = await EC2Client.RunInstancesAsync(request);
                if (responseI.HttpStatusCode == HttpStatusCode.OK)
                {
                    ViewData["Result"] = "Successfully Created and Running!";
                    return RedirectToAction("EC2List");
                }
                else
                {
                    ViewData["Exception"] = "Somthing went wrong";
                    return View();
                }
            }
            catch (AmazonEC2Exception e)
            {
                ViewData["Exception"] = e.Message;
                return View();
            }

        }

        public async Task<IActionResult> VPCList()
        {
            return View(await EC2Client.DescribeVpcsAsync());
        }

        public async Task<IActionResult> SubnetList()
        {
            return View(await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
            {
                Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                }
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubnetList(string subnetId)
        {
            DescribeSubnetsResponse response = await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
            {
                Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                }
            });
            Boolean flag = false;
            for (int i = 0; i < response.Subnets.Count; i++)
            {
                Amazon.EC2.Model.Subnet subnet = response.Subnets[i];
                String retrievedID = subnet.SubnetId;
                if (subnetId.Equals(retrievedID))
                {
                    flag = true;
                    break;
                }
            }
            if (flag == false)
            {
                return StatusCode(500);
            }
            else
            {
                try
                {
                    DeleteSubnetRequest request = new DeleteSubnetRequest(subnetId);
                    DeleteSubnetResponse responseEC2 = await EC2Client.DeleteSubnetAsync(request);
                    if (responseEC2.HttpStatusCode == HttpStatusCode.OK)
                    {
                        ViewData["Result"] = "Successfully Deleted!";
                        return View(await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                        {
                            Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                }
                        }));
                    }
                    else
                    {
                        ViewData["Result"] = "Failed!";
                        return View();
                    }
                }
                catch (AmazonEC2Exception e)
                {
                    ViewData["Result"] = e.Message;
                    return View();
                }
            }
        }

        public IActionResult SubnetCreate()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubnetCreate([Bind("IPCIDR")] AWSTestingSubnetFormModel data)
        {
            if (ModelState.IsValid)
            {
                CreateSubnetRequest request = new CreateSubnetRequest("vpc-09cd2d2019d9ac437", data.IPCIDR + "/24");
                DescribeSubnetsResponse response = await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                {
                    Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                }
                });
                List<int> ipv6CIDR = new List<int>();
                List<Amazon.EC2.Model.Subnet> subnets = response.Subnets;
                int ipv6Subnet = 0;
                string[] ipv6CIDRstr = new string[6];
                if (subnets.Count == 0)
                {
                    DescribeVpcsResponse responseV = await EC2Client.DescribeVpcsAsync(new DescribeVpcsRequest
                    {
                        Filters = new List<Amazon.EC2.Model.Filter>
                        {
                            new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                        }
                    });
                    Vpc vpc = responseV.Vpcs[0];
                    VpcIpv6CidrBlockAssociation ipv6CidrBlockAssociation = vpc.Ipv6CidrBlockAssociationSet[0];
                    ipv6CIDRstr = ipv6CidrBlockAssociation.Ipv6CidrBlock.Split(":");
                }
                else
                {
                    foreach (Amazon.EC2.Model.Subnet s in subnets)
                    {
                        List<SubnetIpv6CidrBlockAssociation> ipv6 = s.Ipv6CidrBlockAssociationSet;
                        ipv6CIDRstr = ipv6[0].Ipv6CidrBlock.Split(":");
                        ipv6Subnet = int.Parse(ipv6CIDRstr[3].Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        ipv6CIDR.Add(ipv6Subnet);
                    }
                    Boolean flag = false;
                    while (flag != true)
                    {
                        Console.WriteLine("Doing while loop");
                        Console.WriteLine(ipv6Subnet);
                        Boolean passed = false;
                        ++ipv6Subnet;
                        foreach (int i in ipv6CIDR)
                        {

                            if (ipv6Subnet == ipv6CIDR[i])
                                passed = false;
                            else
                                passed = true;
                        }
                        if (passed == true)
                            flag = true;
                    }
                }
                if (ipv6CIDRstr[5] == "/56")
                {
                    if (ipv6Subnet < 9)
                        request.Ipv6CidrBlock = ipv6CIDRstr[0] + ":" + ipv6CIDRstr[1] + ":" + ipv6CIDRstr[2] + ":" + ipv6CIDRstr[3].Substring(0, 2) + "0" + ipv6Subnet.ToString() + "::/64";
                    else
                        request.Ipv6CidrBlock = ipv6CIDRstr[0] + ":" + ipv6CIDRstr[1] + ":" + ipv6CIDRstr[2] + ":" + ipv6CIDRstr[3].Substring(0, 2) + Convert.ToInt32(ipv6Subnet).ToString() + "::/64";
                }
                else
                {
                    if (ipv6Subnet < 9)
                        request.Ipv6CidrBlock = ipv6CIDRstr[0] + ":" + ipv6CIDRstr[1] + ":" + ipv6CIDRstr[2] + ":" + ipv6CIDRstr[3].Substring(0, 2) + "0" + ipv6Subnet.ToString() + "::" + ipv6CIDRstr[5];
                    else
                        request.Ipv6CidrBlock = ipv6CIDRstr[0] + ":" + ipv6CIDRstr[1] + ":" + ipv6CIDRstr[2] + ":" + ipv6CIDRstr[3].Substring(0, 2) + Convert.ToInt32(ipv6Subnet).ToString() + "::" + ipv6CIDRstr[5];
                }
                try
                {
                    CreateSubnetResponse responseS = await EC2Client.CreateSubnetAsync(request);
                    if (responseS.HttpStatusCode == HttpStatusCode.OK)
                    {
                        ViewData["Result"] = "Successfully Created!";
                        return RedirectToAction("SubnetList");
                    }
                    else
                    {
                        ViewData["Exception"] = "Failed to Create!";
                        return View();
                    }
                }
                catch (Amazon.EC2.AmazonEC2Exception e)
                {
                    ViewData["Exception"] = e.Message;
                    return View();
                }
            }
            else
            {
                return StatusCode(500);
            }
        }


        public async Task<IActionResult> CloudwatchView()
        {
            GetLogEventsRequest Crequest = new GetLogEventsRequest("RDSOSMetrics", "db-74DSOXWDBQWHTVNTY7RFXWRZYE")
            {
                Limit = 15
            };
            return View(await CloudwatchLogsClient.GetLogEventsAsync(Crequest));
        }

        public IActionResult SNSSend()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SNSSend([Bind("DisplayID", "Number", "Message")] AWSTestingSNSFormModel data)
        {
            if (ModelState.IsValid)
            {
                PublishRequest Srequest = new PublishRequest();
                Srequest.MessageAttributes["AWS.SNS.SMS.SenderID"] = new MessageAttributeValue { StringValue = data.DisplayID, DataType = "String" };
                Srequest.MessageAttributes["AWS.SNS.SMS.SMSType"] = new MessageAttributeValue { StringValue = "Transactional", DataType = "String" };
                Srequest.PhoneNumber = data.Number;
                Srequest.Message = data.Message;
                try
                {
                    PublishResponse response = await SNSClient.PublishAsync(Srequest);
                    if (string.IsNullOrEmpty(response.MessageId))
                    {
                        ViewData["Result"] = "An Error has occured!";
                        return View();
                    }

                    else
                    {
                        ViewData["Result"] = "Successfully Sent!";
                        return View();
                    }
                }
                catch (AmazonSimpleNotificationServiceException e)
                {
                    ViewData["Result"] = e.Message;
                    return View();
                }
            }
            else
            {
                return StatusCode(500);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}