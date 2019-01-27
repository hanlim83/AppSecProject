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

        public IActionResult Index(string Override)
        {
            if (string.IsNullOrEmpty(Override))
            {
                ViewData["Mode"] = "Automatic";
                PlatformLogsParentViewModel model = new PlatformLogsParentViewModel
                {
                    Response = null,
                    Streams = _context.CloudWatchLogStreams.ToList(),
                    SelectedValue = -1
                };
                return View(model);
            } else if (Override.Equals("true"))
            {
                ViewData["Mode"] = "Generic";
                PlatformLogsParentViewModel model = new PlatformLogsParentViewModel
                {
                    Response = null,
                    Streams = _context.CloudWatchLogStreams.ToList(),
                    SelectedValue = -1
                };
                return View(model);
            }
            else
            {
                ViewData["Mode"] = "Automatic";
                PlatformLogsParentViewModel model = new PlatformLogsParentViewModel
                {
                    Response = null,
                    Streams = _context.CloudWatchLogStreams.ToList(),
                    SelectedValue = -1

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
            } else if (ViewMode.Equals("Automatic"))
            {
                CloudWatchLogStream selectedStream = await _context.CloudWatchLogStreams.FindAsync(int.Parse(StreamID));
                if (selectedStream != null)
                {
                    GetLogEventsResponse response = await CloudwatchLogsClient.GetLogEventsAsync(new GetLogEventsRequest
                    {
                        LogGroupName = selectedStream.LinkedGroup.Name.Replace("@", "/"),
                        LogStreamName = selectedStream.Name
                    });
                    PlatformLogsParentViewModel model = new PlatformLogsParentViewModel
                    {
                        Streams = _context.CloudWatchLogStreams.ToList(),
                        SelectedValue = int.Parse(StreamID)
                    };
                    if (selectedStream.DisplayName.Contains("IIS"))
                    {
                        List<IISLog> logs = new List<IISLog>();
                        foreach (OutputLogEvent e in response.Events)
                        {
                            if (!e.Message.StartsWith('#'))
                            {
                                string[] spiltedRecord = e.Message.Split();
                                IISLog l = new IISLog
                                {
                                    TimeStamp = e.Timestamp.ToLocalTime(),
                                ServerIP = spiltedRecord[0],
                                    HTTPMethod = spiltedRecord[1],
                                    Path = spiltedRecord[2],
                                    Query = spiltedRecord[3],
                                    DestPort = int.Parse(spiltedRecord[4]),
                                    AuthenticatedUser = spiltedRecord[5],
                                    SourceIP = spiltedRecord[6],
                                    SourceUserAgent = spiltedRecord[7],
                                    Referer = spiltedRecord[8],
                                    HTTPStatusCode = int.Parse(spiltedRecord[9]),
                                    SubstatusCode = int.Parse(spiltedRecord[10]),
                                    Win32StatusCode = int.Parse(spiltedRecord[11]),
                                    Duration = int.Parse(spiltedRecord[12]),
                                };
                                logs.Add(l);
                            }
                        }
                        model.IISLogs = logs;
                    } else if (selectedStream.DisplayName.Contains("eni"))
                    {
                        List<ENILog> logs = new List<ENILog>();
                        foreach (OutputLogEvent e in response.Events)
                        {
                            if (!e.Message.Contains("NODATA"))
                            {
                                string[] spiltedRecord = e.Message.Split();
                                ENILog l = new ENILog
                                {
                                    TimeStamp = e.Timestamp.ToLocalTime(),
                                    Version = int.Parse(spiltedRecord[0]),
                                    AccountID = long.Parse(spiltedRecord[1]),
                                    Interface = spiltedRecord[2],
                                    SourceAddress = spiltedRecord[3],
                                    DestinationAddress = spiltedRecord[4],
                                    SourcePort = spiltedRecord[5],
                                    DestinationPort = spiltedRecord[6],
                                    Packets = int.Parse(spiltedRecord[8]),
                                    Bytes = int.Parse(spiltedRecord[9]),
                                    StartTime = long.Parse(spiltedRecord[10]),
                                    EndTime = long.Parse(spiltedRecord[11]),
                                    Action = spiltedRecord[12],
                                    LogAction = spiltedRecord[13]
                                };
                                if (spiltedRecord[7].Equals("1"))
                                    l.Protocol = "ICMP";
                                else if (spiltedRecord[7].Equals("4"))
                                    l.Protocol = "IPv4";
                                else if (spiltedRecord[7].Equals("6"))
                                    l.Protocol = "TCP";
                                else if (spiltedRecord[7].Equals("17"))
                                    l.Protocol = "UDP";
                                else if (spiltedRecord[7].Equals("41"))
                                    l.Protocol = "IPv6";
                                logs.Add(l);
                            }
                        }
                        model.ENILogs = logs;
                    }
                    else
                    {
                        foreach (OutputLogEvent e in response.Events)
                        {
                            e.Timestamp = e.Timestamp.ToLocalTime();
                        }
                        model.Response = response;
                    }
                    ViewData["Mode"] = "Automatic";
                    return View(model);
                }
                else
                    return NotFound();
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