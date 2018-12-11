using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminSide.Models;
using Amazon.S3;
using Amazon.EC2;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using ASPJ_MVC.Models;
using Amazon.S3.Model;
using System.Net;
using Microsoft.AspNetCore.Http;
using Amazon.EC2.Model;

namespace ASPJ_MVC.Controllers
{
    public class AWSTestingController : Controller
    {
        IAmazonS3 S3Client { get; set; }

        IAmazonEC2 EC2Client { get; set; }

        IAmazonCloudWatchLogs SentryClient { get; set; }

        IAmazonSimpleNotificationService SNSClient { get; set; }

        public AWSTestingController(IAmazonS3 s3Client, IAmazonEC2 ec2Client, IAmazonCloudWatchLogs cloudWatchLogsClient, IAmazonSimpleNotificationService snsClient)
        {
            this.S3Client = s3Client;
            this.EC2Client = ec2Client;
            this.SentryClient = cloudWatchLogsClient;
            this.SNSClient = snsClient;
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
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-097a8c101382a6b0f"}}
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
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-097a8c101382a6b0f"}}
                }
            });
            Boolean flag = false;
            for (int i = 0; i < response.Subnets.Count; i++)
            {
                Subnet subnet = response.Subnets[i];
                String retrievedID = subnet.SubnetId;
                if (subnetId.Equals(retrievedID))
                {
                    flag = true;
                    break;
                }
            }
            if (flag == false)
            {
                return View();
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
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-097a8c101382a6b0f"}}
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
                CreateSubnetRequest request = new CreateSubnetRequest("vpc-097a8c101382a6b0f", data.IPCIDR+"/24");
                DescribeSubnetsResponse response = await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                {
                    Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-097a8c101382a6b0f"}}
                }
                });
                List<int> ipv6CIDR = new List<int>();
                List<Subnet> subnets = response.Subnets;
                int ipv6Subnet = 0;
                string[] ipv6CIDRstr = new string[6];
                for (int i = 0; i < subnets.Count; i++)
                {
                    List<SubnetIpv6CidrBlockAssociation> ipv6 = subnets[i].Ipv6CidrBlockAssociationSet;
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
                    for (int j = 0; j < ipv6CIDR.Count; j++)
                    {
                        
                        if (ipv6Subnet == ipv6CIDR[j])
                            passed = false;
                        else
                            passed = true;
                    }
                    if (passed == true)
                        flag = true;
                }
                if (ipv6Subnet < 9)
                    request.Ipv6CidrBlock = ipv6CIDRstr[0] + ":" + ipv6CIDRstr[1] + ":" + ipv6CIDRstr[2] + ":" + ipv6CIDRstr[3].Substring(0,2) + "0" + ipv6Subnet.ToString() + "::"+ ipv6CIDRstr[5];
                else
                    request.Ipv6CidrBlock = ipv6CIDRstr[0] + ":" + ipv6CIDRstr[1] + ":" + ipv6CIDRstr[2] + ":" + ipv6CIDRstr[3].Substring(0, 2) + Convert.ToInt32(ipv6Subnet).ToString() + "::" + ipv6CIDRstr[5];
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
                } catch (Amazon.EC2.AmazonEC2Exception e)
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
            return View(await SentryClient.GetLogEventsAsync(Crequest));
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
