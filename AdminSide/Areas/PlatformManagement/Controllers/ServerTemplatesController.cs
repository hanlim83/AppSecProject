using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using AdminSide.Areas.PlatformManagement.Services;
using Amazon.EC2;
using Amazon.EC2.Model;
using ASPJ_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    [Authorize]
    public class ServerTemplatesController : Controller
    {
        private readonly PlatformResourcesContext _context;

        IAmazonEC2 EC2Client { get; set; }

        private readonly IApplicationLifetime _appLifetime;
        private readonly ILogger _logger;

        public IBackgroundTaskQueue Backgroundqueue { get; }

        public ServerTemplatesController(PlatformResourcesContext context, IAmazonEC2 ec2Client, IBackgroundTaskQueue Queue, IApplicationLifetime appLifetime, ILogger<ChallengeServersController> logger)
        {
            this._context = context;
            this.EC2Client = ec2Client;
            Backgroundqueue = Queue;
            _appLifetime = appLifetime;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateTemplate(string ServerID)
        {
            ViewData["serverID"] = ServerID;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitCreateTemplate([Bind("serverID", "Name", "TemplateDescription")]ServerTemplateCreationFormModel submission)
        {
            Server given = await _context.Servers.FindAsync(int.Parse(submission.serverID));
            if (given != null)
            {
                Template newlyCreated = new Template
                {
                    Name = submission.Name,
                    TemplateDescription = submission.TemplateDescription,
                    DateCreated = DateTime.Now,
                    OperatingSystem = given.OperatingSystem,
                    SpecificMinimumSize = true,
                    MinimumStorage = given.StorageAssigned,
                    Type = TemplateType.Custom
                };
                CreateImageResponse response = await EC2Client.CreateImageAsync(new CreateImageRequest
                {
                    BlockDeviceMappings = new List<BlockDeviceMapping> {
                        new BlockDeviceMapping {
                            DeviceName = "/dev/xvda",
                            Ebs = new EbsBlockDevice { VolumeSize = given.StorageAssigned }
                        }
                    },
                    Name = submission.Name + " - Created on Platform",
                    Description = submission.TemplateDescription,
                    NoReboot = false,
                    InstanceId = given.AWSEC2Reference
                });
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    newlyCreated.AWSAMIReference = response.ImageId;
                    _context.Templates.Add(newlyCreated);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("");
                }
                else
                {
                    return StatusCode(500);
                }
            }
            else
            {
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> Index()
        {
            if (_context.VPCs.ToList().Count == 0)
            {
                return RedirectToAction("", "Home", "");
            }
            return View(await _context.Templates.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string TemplateID)
        {
            Template found = await _context.Templates.FindAsync(int.Parse(TemplateID));
            if (found != null)
            {
                return View(found);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitEdit([Bind("ID,Name,TemplateDescription")]Template modified)
        {
            Template found = await _context.Templates.FindAsync(modified.ID);
            if (found != null)
            {
                found.Name = modified.Name;
                found.TemplateDescription = modified.TemplateDescription;
                try
                {
                    _context.Templates.Update(found);
                    await _context.SaveChangesAsync();
                    TempData["Result"] = "Sucessfullly Modified!";
                    return RedirectToAction("");
                }
                catch (DbUpdateConcurrencyException)
                {
                    found = await _context.Templates.FindAsync(modified.ID);
                    if (found == null)
                    {
                        TempData["Result"] = "Template Not Found! May have been deleted by another user";
                        return RedirectToAction("");
                    }
                    else
                    {
                        TempData["Result"] = "Template has been ediited by another user, your changes were not saved";
                        return RedirectToAction("");
                    }
                }
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(String TemplateID)
        {
            Template deleted = await _context.Templates.FindAsync(int.Parse(TemplateID));
            if (deleted != null)
            {
                DeregisterImageResponse response = await EC2Client.DeregisterImageAsync(new DeregisterImageRequest
                {
                    ImageId = deleted.AWSAMIReference
                });
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    if (!String.IsNullOrEmpty(deleted.AWSSnapshotReference))
                    {
                        Backgroundqueue.QueueBackgroundWorkItem(async token =>
                        {
                            _logger.LogInformation("Deletion of snapshot scheduled");
                            await Task.Delay(TimeSpan.FromMinutes(3), token);
                            await EC2Client.DeleteSnapshotAsync(new DeleteSnapshotRequest(deleted.AWSSnapshotReference));
                            _logger.LogInformation("Deletion of snapshot done!");
                        });
                    }
                    _context.Templates.Remove(deleted);
                    await _context.SaveChangesAsync();
                    TempData["Result"] = "Deleted Sucessfully!";
                    return RedirectToAction("");
                }
                else
                {
                    TempData["Result"] = "Delete Failed!";
                    return RedirectToAction("");
                }
            }
            else
            {
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