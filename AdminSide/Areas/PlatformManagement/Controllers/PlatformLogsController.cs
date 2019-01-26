using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using Amazon.CloudWatch;
using Amazon.CloudWatchEvents;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using ASPJ_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    [Authorize]
    public class PlatformLogsController : Controller
    {
        private readonly PlatformResourcesContext _context;

        private readonly UserManager<IdentityUser> _userManager;

        IAmazonCloudWatch CloudwatchClient { get; set; }

        IAmazonCloudWatchEvents CloudwatchEventsClient { get; set; }

        IAmazonCloudWatchLogs CloudwatchLogsClient { get; set; }

        public PlatformLogsController(PlatformResourcesContext context, UserManager<IdentityUser> userManager, IAmazonCloudWatch cloudwatchClient, IAmazonCloudWatchEvents cloudwatcheventsClient, IAmazonCloudWatchLogs cloudwatchLogsClient)
        {
            this._context = context;
            this.CloudwatchClient = cloudwatchClient;
            this.CloudwatchEventsClient = cloudwatcheventsClient;
            this.CloudwatchLogsClient = cloudwatchLogsClient;
            this._userManager = userManager;
        }

        public IActionResult Index(string OverrideMode)
        {
            if (string.IsNullOrEmpty(OverrideMode))
            {
                ViewData["Mode"] = "Automatic";
                PlatformLogsParentViewModel model = new PlatformLogsParentViewModel
                {
                    Response = null,
                    Streams = _context.CloudWatchLogStreams.ToList(),
                    SelectedValue = 0
                };
                return View(model);
            } else if (OverrideMode.Equals("true"))
            {
                ViewData["Mode"] = "Automatic";
                PlatformLogsParentViewModel model = new PlatformLogsParentViewModel
                {
                    Response = null,
                    Streams = _context.CloudWatchLogStreams.ToList(),
                    SelectedValue = 0
                };
                return View(model);
            }
            else
            {
                ViewData["Mode"] = "Generic";
                PlatformLogsParentViewModel model = new PlatformLogsParentViewModel
                {
                    Response = null,
                    Streams = _context.CloudWatchLogStreams.ToList(),
                    SelectedValue = 0

                };
                return View(model);
            }
        }

        [HttpPost]

        public async Task<IActionResult> Index(string StreamID, string ViewMode)
        {
            if (StreamID == null)
            {
                PlatformLogsParentViewModel model = new PlatformLogsParentViewModel
                {
                    Response = null,
                    Streams = _context.CloudWatchLogStreams.ToList(),
                    SelectedValue = 0
                };
                TempData["Exception"] = "Invaild Selection of Component!";
                return View(model);
            }
            else if (StreamID.Equals("0"))
            {
                List<RDSSQLLog> Logs = _context.GetRDSLogs().Result.ToList();
                PlatformLogsParentViewModel model = new PlatformLogsParentViewModel
                {
                    Response = null,
                    Streams = _context.CloudWatchLogStreams.ToList(),
                    SelectedValue = int.Parse(StreamID),
                    SQLlogs = Logs
                };
                if(ViewMode.Equals("Automatic"))
                    ViewData["Mode"] = "Automatic";
                else if (ViewMode.Equals("Generic"))
                    ViewData["Mode"] = "Automatic";
                return View(model);
            }
            else
            {
                CloudWatchLogStream selectedStream = await _context.CloudWatchLogStreams.FindAsync(int.Parse(StreamID));
                if (selectedStream != null)
                {
                    GetLogEventsResponse response = await CloudwatchLogsClient.GetLogEventsAsync(new GetLogEventsRequest
                    {
                        LogGroupName = selectedStream.LinkedGroup.Name.Replace("@", "/"),
                        LogStreamName = selectedStream.Name
                    });
                    foreach(OutputLogEvent e in response.Events)
                    {
                        e.Timestamp = e.Timestamp.ToLocalTime();
                    }
                    PlatformLogsParentViewModel model = new PlatformLogsParentViewModel
                    {
                        Response = response,
                        Streams = _context.CloudWatchLogStreams.ToList(),
                        SelectedValue = int.Parse(StreamID)
                    };
                    ViewData["Mode"] = "Generic";
                    return View(model);
                }
                else
                    return NotFound();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}