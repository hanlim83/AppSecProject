using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using ASPJ_MVC.Models;
using CodeHollow.FeedReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    [Authorize]
    public class HomeController : Controller
    {

        private readonly PlatformResourcesContext _context;

        private readonly IAmazonCloudWatch CWClient;

        public HomeController(PlatformResourcesContext context, IAmazonCloudWatch CWClient)
        {
            _context = context;
            this.CWClient = CWClient;
        }

        public async Task<IActionResult> Index()
        {
            var CloudwatchFeed = await FeedReader.ReadAsync("https://status.aws.amazon.com/rss/cloudwatch-ap-southeast-1.rss");
            var CloudwatchFeed1 = CloudwatchFeed.Items.ElementAt(0);
            if (CloudwatchFeed1.Title.StartsWith("Service is operating normally"))
            {
                ViewData["CWStatus"] = "OK";
            }
            else if (CloudwatchFeed1.Title.StartsWith("Informational message") || CloudwatchFeed1.Title.StartsWith("Performance issues"))
            {
                ViewData["CWStatus"] = "WARNING";
            }
            else if (CloudwatchFeed1.Title.StartsWith("Service disruption"))
            {
                ViewData["CWStatus"] = "CRITICAL";
            }

            var EC2Feed = await FeedReader.ReadAsync("https://status.aws.amazon.com/rss/ec2-ap-southeast-1.rss");
            var EC2Feed1 = EC2Feed.Items.ElementAt(0);
            if (EC2Feed1.Title.StartsWith("Service is operating normally"))
            {
                ViewData["EC2Status"] = "OK";
            }
            else if (EC2Feed1.Title.StartsWith("Informational message") || EC2Feed1.Title.StartsWith("Performance issues"))
            {
                ViewData["EC2Status"] = "WARNING";
            }
            else if (EC2Feed1.Title.StartsWith("Service disruption"))
            {
                ViewData["EC2Status"] = "CRITICAL";
            }

            var EBSFeed = await FeedReader.ReadAsync("https://status.aws.amazon.com/rss/elasticbeanstalk-ap-southeast-1.rss");
            var EBSFeed1 = EBSFeed.Items.ElementAt(0);
            if (EBSFeed1.Title.StartsWith("Service is operating normally"))
            {
                ViewData["EBSStatus"] = "OK";
            }
            else if (EBSFeed1.Title.StartsWith("Informational message") || EBSFeed1.Title.StartsWith("Performance issues"))
            {
                ViewData["EBSStatus"] = "WARNING";
            }
            else if (EBSFeed1.Title.StartsWith("Service disruption"))
            {
                ViewData["EBSStatus"] = "CRITICAL";
            }

            var ELBFeed = await FeedReader.ReadAsync("https://status.aws.amazon.com/rss/elb-ap-southeast-1.rss");
            if (ELBFeed.Items.Count != 0)
            {
                var ELBFeed1 = ELBFeed.Items.ElementAt(0);
                if (ELBFeed1.Title.StartsWith("Service is operating normally"))
                {
                    ViewData["ELBStatus"] = "OK";
                }
                else if (ELBFeed1.Title.StartsWith("Informational message") || ELBFeed1.Title.StartsWith("Performance issues"))
                {
                    ViewData["ELBStatus"] = "WARNING";
                }
                else if (ELBFeed1.Title.StartsWith("Service disruption"))
                {
                    ViewData["ELBStatus"] = "CRITICAL";
                }
            }
            else
            {
                ViewData["ELBStatus"] = "OK";
            }

            var NATFeed = await FeedReader.ReadAsync("https://status.aws.amazon.com/rss/natgateway-ap-southeast-1.rss");
            if (NATFeed.Items.Count != 0)
            {
                var NATFeed1 = NATFeed.Items.ElementAt(0);
                if (NATFeed1.Title.StartsWith("Service is operating normally"))
                {
                    ViewData["NATStatus"] = "OK";
                }
                else if (NATFeed1.Title.StartsWith("Informational message") || NATFeed1.Title.StartsWith("Performance issues"))
                {
                    ViewData["NATStatus"] = "WARNING";
                }
                else if (NATFeed1.Title.StartsWith("Service disruption"))
                {
                    ViewData["NATStatus"] = "CRITICAL";
                }
            }
            else
            {
                ViewData["NATStatus"] = "OK";
            }

            var RDSFeed = await FeedReader.ReadAsync("https://status.aws.amazon.com/rss/rds-ap-southeast-1.rss");
            var RDSFeed1 = RDSFeed.Items.ElementAt(0);
            if (RDSFeed1.Title.StartsWith("Service is operating normally"))
            {
                ViewData["RDSStatus"] = "OK";
            }
            else if (RDSFeed1.Title.StartsWith("Informational message") || RDSFeed1.Title.StartsWith("Performance issues"))
            {
                ViewData["RDSStatus"] = "WARNING";
            }
            else if (RDSFeed1.Title.StartsWith("Service disruption"))
            {
                ViewData["RDSStatus"] = "CRITICAL";
            }

            var R53Feed = await FeedReader.ReadAsync("https://status.aws.amazon.com/rss/route53.rss");
            var R53Feed1 = R53Feed.Items.ElementAt(0);
            if (R53Feed1.Title.StartsWith("Service is operating normally") || R53Feed1.Title.Contains("[RESOLVED]"))
            {
                ViewData["R53Status"] = "OK";
            }
            else if (R53Feed1.Title.StartsWith("Informational message") || R53Feed1.Title.StartsWith("Performance issues"))
            {
                ViewData["R53Status"] = "WARNING";
            }
            else if (R53Feed1.Title.StartsWith("Service disruption"))
            {
                ViewData["R53Status"] = "CRITICAL";
            }

            var SGFeed = await FeedReader.ReadAsync("https://status.aws.amazon.com/rss/storagegateway-ap-southeast-1.rss");
            var SGFeed1 = SGFeed.Items.ElementAt(0);
            if (SGFeed1.Title.StartsWith("Service is operating normally"))
            {
                ViewData["SGStatus"] = "OK";
            }
            else if (SGFeed1.Title.StartsWith("Informational message") || SGFeed1.Title.StartsWith("Performance issues"))
            {
                ViewData["SGStatus"] = "WARNING";
            }
            else if (SGFeed1.Title.StartsWith("Service disruption"))
            {
                ViewData["SGStatus"] = "CRITICAL";
            }

            var S3Feed = await FeedReader.ReadAsync("https://status.aws.amazon.com/rss/s3-ap-southeast-1.rss");
            var S3Feed1 = S3Feed.Items.ElementAt(0);
            if (S3Feed1.Title.StartsWith("Service is operating normally"))
            {
                ViewData["S3Status"] = "OK";
            }
            else if (S3Feed1.Title.StartsWith("Informational message") || S3Feed1.Title.StartsWith("Performance issues"))
            {
                ViewData["S3Status"] = "WARNING";
            }
            else if (S3Feed1.Title.StartsWith("Service disruption"))
            {
                ViewData["S3Status"] = "CRITICAL";
            }

            var VPCFeed = await FeedReader.ReadAsync("https://status.aws.amazon.com/rss/vpc-ap-southeast-1.rss");
            var VPCFeed1 = VPCFeed.Items.ElementAt(0);
            if (VPCFeed1.Title.StartsWith("Service is operating normally"))
            {
                ViewData["VPCStatus"] = "OK";
            }
            else if (VPCFeed1.Title.StartsWith("Informational message") || VPCFeed1.Title.StartsWith("Performance issues"))
            {
                ViewData["VPCStatus"] = "WARNING";
            }
            else if (VPCFeed1.Title.StartsWith("Service disruption"))
            {
                ViewData["VPCStatus"] = "CRITICAL";
            }

            DescribeAlarmsResponse response = await CWClient.DescribeAlarmsAsync();
            Boolean Flag = false;
            foreach (MetricAlarm a in response.MetricAlarms)
            {
                if (a.AlarmName.Contains("awsec2") && a.StateValue == StateValue.ALARM)
                {
                    Flag = true;
                    break;
                }
            }
            if (Flag == true)
                ViewData["CWPlatformState"] = "ALARM";
            else
                ViewData["CWPlatformState"] = "OK";
            Flag = false;
            foreach (MetricAlarm a in response.MetricAlarms)
            {
                if (a.AlarmName.Contains("awsapplicationelb-app-") && a.StateValue == StateValue.ALARM)
                {
                    Flag = true;
                    break;
                }
            }
            if (Flag == true)
                ViewData["CWELBState"] = "ALARM";
            else
                ViewData["CWELBState"] = "OK";
            Flag = false;
            foreach (MetricAlarm a in response.MetricAlarms)
            {
                if (a.AlarmName.Contains("-eCTFVM-") && a.StateValue == StateValue.ALARM)
                {
                    Flag = true;
                    break;
                }
            }
            if (Flag == true)
                ViewData["CWCSState"] = "ALARM";
            else
                ViewData["CWCSState"] = "OK";
            if (_context.VPCs.ToList().Count == 0)
            {
                if (ViewData["MissingVPC"].Equals("FIXING"))
                {
                    ViewData["MissingVPC"] = "FIXING";
                }
                else
                {
                    ViewData["MissingVPC"] = "YES";
                }
            }
            else
            {
                ViewData["MissingVPC"] = "NO";
                List<Server> allServers = await _context.Servers.ToListAsync();
                ViewData["ServerTotalCount"] = allServers.Count();
                List<Server> errorServers = await _context.Servers.FromSql("SELECT * FROM dbo.Servers WHERE State = 4").ToListAsync();
                ViewData["ServerErrorCount"] = errorServers.Count();
                List<Server> runningServers = await _context.Servers.FromSql("SELECT * FROM dbo.Servers WHERE State = 1").ToListAsync();
                ViewData["ServerRunningCount"] = runningServers.Count();
                List<Subnet> allSubnets = await _context.Subnets.ToListAsync();
                ViewData["SubnetTotalCount"] = allSubnets.Count();
                if (errorServers.Count() == 0)
                {
                    ViewData["ServerHealth"] = "OK";
                }
                else
                {
                    ViewData["ServerHealth"] = "NOT OK";
                }

                List<Subnet> intranetSubnets = await _context.Subnets.FromSql("SELECT * FROM dbo.Subnets WHERE Type = 2").ToListAsync();
                ViewData["SubnetIntranetCount"] = intranetSubnets.Count();
                List<Subnet> extranetSubnets = await _context.Subnets.FromSql("SELECT * FROM dbo.Subnets WHERE Type = 1").ToListAsync();
                ViewData["SubnetExtranetCount"] = extranetSubnets.Count();
                List<Subnet> internetSubnets = await _context.Subnets.FromSql("SELECT * FROM dbo.Subnets WHERE Type = 0").ToListAsync();
                ViewData["SubnetInternetCount"] = internetSubnets.Count();
                List<Route> problemRoutes = await _context.Routes.FromSql("SELECT * FROM dbo.Routes WHERE Status = 1").ToListAsync();
                if (problemRoutes.Count() == 0)
                {
                    ViewData["RouteHealth"] = "OK";
                }
                else
                {
                    ViewData["RouteHealth"] = "NOT OK";
                }
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}