using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminSide.Areas.PlatformManagement.Data;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.CloudWatchEvents;
using Amazon.CloudWatchEvents.Model;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using ASPJ_MVC.Models;
using System.Diagnostics;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    public class PlatformLogsController : Controller
    {
        private readonly PlatformResourcesContext _context;

        IAmazonCloudWatch CloudwatchClient { get; set; }

        IAmazonCloudWatchEvents CloudwatchEventsClient { get; set; }

        IAmazonCloudWatchLogs CloudwatchLogsClient { get; set; }

        public PlatformLogsController(PlatformResourcesContext context, IAmazonCloudWatch cloudwatchClient, IAmazonCloudWatchEvents cloudwatcheventsClient, IAmazonCloudWatchLogs cloudwatchLogsClient)
        {
            this._context = context;
            this.CloudwatchClient = cloudwatchClient;
            this.CloudwatchEventsClient = cloudwatcheventsClient;
            this.CloudwatchLogsClient = cloudwatchLogsClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string Component)
        {
            if (Component == null)
                return View();
            else
            {
                if (Component.Equals("RDS"))
                {
                    return View(await CloudwatchLogsClient.GetLogEventsAsync(new GetLogEventsRequest {
                        LogGroupName= "RDSOSMetrics",
                        LogStreamName= "db-74DSOXWDBQWHTVNTY7RFXWRZYE"
                    }));
                }
                else
                    return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}