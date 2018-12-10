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

namespace ASPJ_MVC.Controllers
{
    public class AWSTestingController : Controller
    {
        IAmazonS3 S3Client { get; set; }

        IAmazonEC2 EC2Client { get; set; }

        IAmazonCloudWatchLogs SentryClient { get; set; }

        IAmazonSimpleNotificationService SNSClient { get; set; }

        public AWSTestingController (IAmazonS3 s3Client, IAmazonEC2 ec2Client, IAmazonCloudWatchLogs cloudWatchLogsClient, IAmazonSimpleNotificationService snsClient)
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
                    } else
                    {
                        return RedirectToAction("S3List");
                    }

                } catch (AmazonS3Exception e)
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
        public async Task<IActionResult> SNSSend([Bind("DisplayID","Number","Message")] AWSTestingSNSFormModel data)
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
