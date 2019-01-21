using AdminSide.Areas.PlatformManagement.Data;
using Amazon.CloudWatch;
using Amazon.CloudWatchEvents;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using ASPJ_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    [Authorize]
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

        public async Task<IActionResult> Index(string Component)
        {
            if (Component == null)
                return View();
            else
            {
                if (Component.Equals("RDS"))
                {
                    GetLogEventsResponse response = await CloudwatchLogsClient.GetLogEventsAsync(new GetLogEventsRequest
                    {
                        LogGroupName = "RDSOSMetrics",
                        LogStreamName = "db-74DSOXWDBQWHTVNTY7RFXWRZYE"
                    });
                    foreach (OutputLogEvent e in response.Events)
                    {
                        e.Message = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(e.Message),Formatting.Indented);
                    }
                    return View(response);
                } else if (Component.Equals("PlatformVPCELBA"))
                {
                    GetLogEventsResponse response = await CloudwatchLogsClient.GetLogEventsAsync(new GetLogEventsRequest
                    {
                        LogGroupName = "PlatformVPCLogs",
                        LogStreamName = "eni-06a6a263548d0f56f-all"
                    });
                    return View(response);
                }
                else if (Component.Equals("PlatformVPCELBB"))
                {
                    GetLogEventsResponse response = await CloudwatchLogsClient.GetLogEventsAsync(new GetLogEventsRequest
                    {
                        LogGroupName = "PlatformVPCLogs",
                        LogStreamName = "eni-06f01cd8e3ac74e45-all"
                    });
                    return View(response);
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