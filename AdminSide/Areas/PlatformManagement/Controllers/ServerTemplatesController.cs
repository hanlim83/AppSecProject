using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using Amazon.EC2;
using Amazon.EC2.Model;
using ASPJ_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    public class ServerTemplatesController : Controller
    {
        private readonly PlatformResourcesContext _context;

        IAmazonEC2 EC2Client { get; set; }

        public ServerTemplatesController(PlatformResourcesContext context, IAmazonEC2 ec2Client)
        {
            this._context = context;
            this.EC2Client = ec2Client;
        }

        public IActionResult CreateTemplate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTemplate([Bind("serverID", "Name", "TemplateDescription")]ServerTemplateCreationFormModel submission)
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
                    return StatusCode(500);
            }
            else
                return StatusCode(500);
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Templates.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string TemplateID)
        {
            Template found = await _context.Templates.FindAsync(int.Parse(TemplateID));
            if (found != null)
                return View(found);
            else
                return NotFound();
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
                    ViewData["Result"] = "Sucessfullly Modified!";
                    return RedirectToAction("");
                }
                catch (DbUpdateConcurrencyException)
                {
                    found = await _context.Templates.FindAsync(modified.ID);
                    if (found == null)
                    {
                        ViewData["Result"] = "Template Not Found! May have been deleted by another user";
                        return RedirectToAction("");
                    }
                    else
                    {
                        ViewData["Result"] = "Template has been ediited by another user, your changes were not saved";
                        return RedirectToAction("");
                    }
                }
            }
            else
                return NotFound();
        }

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
                    _context.Templates.Remove(deleted);
                    await _context.SaveChangesAsync();
                    ViewData["Result"] = "Deleted Sucessfully!";
                    return RedirectToAction("");
                }
                else
                {
                    ViewData["Result"] = "Delete Failed!";
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