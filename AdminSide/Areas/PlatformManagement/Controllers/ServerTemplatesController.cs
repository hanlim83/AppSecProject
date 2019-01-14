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
using State = AdminSide.Areas.PlatformManagement.Models.State;
using Subnet = AdminSide.Areas.PlatformManagement.Models.Subnet;
using Tenancy = AdminSide.Areas.PlatformManagement.Models.Tenancy;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    public class ServerTemplatesController : Controller
    {
        private readonly PlatformResourcesContext _context;

        IAmazonEC2 EC2Client { get; set; }
        private ChallengeServersCreationFormModel creationReference;

        public ServerTemplatesController(PlatformResourcesContext context, IAmazonEC2 ec2Client)
        {
            this._context = context;
            this.EC2Client = ec2Client;
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
                } else
                {
                    ViewData["Result"] = "Delete Failed!";
                    return RedirectToAction("");
                }
            } else
            {
                return NotFound();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //Imported From Challenge Servers

        public IActionResult SpecifySettings()
        {
            return RedirectToAction("");
        }

        public IActionResult VerifySettings()
        {
            return RedirectToAction("");
        }

        [HttpPost]

        public async Task<IActionResult> SpecifySettings(String selectedTemplate)
        {
            if (selectedTemplate != null)
            {
                TempData["selectedTemplate"] = selectedTemplate;
                return View(await _context.Subnets.ToListAsync());
            }
            else
                return NotFound();
        }

        [HttpPost]

        public async Task<IActionResult> VerifySettings(String ServerName, String ServerWorkload, String SelectedTemplate, String ServerStorage, String ServerTenancy, String ServerSubnet)
        {
            creationReference = new ChallengeServersCreationFormModel
            {
                ServerName = ServerName,
                ServerWorkload = ServerWorkload,
                selectedTemplate = await _context.Templates.FindAsync(Int32.Parse(SelectedTemplate)),
                ServerStorage = Int32.Parse(ServerStorage),
                ServerTenancy = ServerTenancy,
                ServerSubnet = await _context.Subnets.FindAsync(Int32.Parse(ServerSubnet))
            };
            TempData["TemplateID"] = creationReference.selectedTemplate.ID;
            TempData["SubnetID"] = creationReference.ServerSubnet.ID;
            return View(creationReference);
        }

        [HttpPost]

        public async Task<IActionResult> Submission([Bind("TemplateName,OperatingSystem,ServerName,ServerWorkload,ServerStorage,ServerTenancy,SubnetCIDR,TemplateID,SubnetID")] ChallengeServersCreationFormModel Model2)
        {
            Template selectedT = await _context.Templates.FindAsync(Int32.Parse(Model2.TemplateID));
            Subnet selectedS = await _context.Subnets.FindAsync(Int32.Parse(Model2.SubnetID));
            RunInstancesRequest request = new RunInstancesRequest
            {
                BlockDeviceMappings = new List<BlockDeviceMapping> {
                        new BlockDeviceMapping {
                            DeviceName = "/dev/xvda",
                            Ebs = new EbsBlockDevice { VolumeSize = Model2.ServerStorage }
                        }
                    },
                ImageId = selectedT.AWSAMIReference,
                KeyName = "ASPJ Instances Master Key Pair",
                MaxCount = 1,
                MinCount = 1,
                SubnetId = selectedS.AWSVPCSubnetReference
            };
            if (Model2.ServerWorkload.Equals("Low"))
            {
                request.InstanceType = "t2.micro";
                if (Model2.ServerTenancy.Equals("Dedicated Instance"))
                {
                    request.Placement = new Placement
                    {
                        Tenancy = Amazon.EC2.Tenancy.Dedicated
                    };
                }
                else if (Model2.ServerTenancy.Equals("Dedicated Hardware"))
                {
                    request.Placement = new Placement
                    {
                        Tenancy = Amazon.EC2.Tenancy.Host
                    };
                }
            }
            else if (Model2.ServerWorkload.Equals("Medium"))
            {
                request.InstanceType = "t2.medium";
                if (Model2.ServerTenancy.Equals("Dedicated Instance"))
                {
                    request.Placement = new Placement
                    {
                        Tenancy = Amazon.EC2.Tenancy.Dedicated
                    };
                }
                else if (Model2.ServerTenancy.Equals("Dedicated Hardware"))
                {
                    request.Placement = new Placement
                    {
                        Tenancy = Amazon.EC2.Tenancy.Host
                    };
                }
            }
            else if (Model2.ServerWorkload.Equals("Large"))
            {
                request.InstanceType = "t2.xlarge";
                if (Model2.ServerTenancy.Equals("Dedicated Instance"))
                {
                    request.Placement = new Placement
                    {
                        Tenancy = Amazon.EC2.Tenancy.Dedicated
                    };
                }
                else if (Model2.ServerTenancy.Equals("Dedicated Hardware"))
                {
                    request.Placement = new Placement
                    {
                        Tenancy = Amazon.EC2.Tenancy.Host
                    };
                }
            }
            try
            {
                RunInstancesResponse response = await EC2Client.RunInstancesAsync(request);
                if (response.HttpStatusCode.Equals(HttpStatusCode.OK))
                {
                    await EC2Client.CreateTagsAsync(new CreateTagsRequest
                    {
                        Resources = new List<string>
                        {
                            response.Reservation.Instances[0].InstanceId
                        },
                        Tags = new List<Tag>
                        {
                            new Tag("Name",Model2.ServerName)
                        }
                    });
                    Server newlyCreated = new Server
                    {
                        Name = Model2.ServerName,
                        OperatingSystem = selectedT.OperatingSystem,
                        StorageAssigned = Model2.ServerStorage,
                        DateCreated = DateTime.Now,
                        AWSEC2Reference = response.Reservation.Instances[0].InstanceId,
                        AWSSecurityGroupReference = response.Reservation.Instances[0].SecurityGroups[0].GroupId,
                        LinkedSubnet = selectedS,
                        SubnetID = selectedS.ID
                    };
                    if (Model2.ServerWorkload.Equals("Low"))
                        newlyCreated.Workload = Workload.Low;
                    else if (Model2.ServerWorkload.Equals("Medium"))
                        newlyCreated.Workload = Workload.Medium;
                    else if (Model2.ServerWorkload.Equals("Large"))
                        newlyCreated.Workload = Workload.Large;
                    if (selectedS.Type == SubnetType.Internet)
                    {
                        newlyCreated.IPAddress = response.Reservation.Instances[0].PublicIpAddress;
                        newlyCreated.DNSHostname = response.Reservation.Instances[0].PublicDnsName;
                        newlyCreated.Visibility = Visibility.Internet;
                    }
                    else
                    {
                        newlyCreated.IPAddress = response.Reservation.Instances[0].PrivateIpAddress;
                        newlyCreated.DNSHostname = response.Reservation.Instances[0].PrivateDnsName;
                        if (selectedS.Type == SubnetType.Extranet)
                            newlyCreated.Visibility = Visibility.Extranet;
                        else
                            newlyCreated.Visibility = Visibility.Intranet;
                    }
                    if (response.Reservation.Instances[0].State.Code == 0)
                        newlyCreated.State = State.Starting;
                    else if (response.Reservation.Instances[0].State.Code == 16)
                        newlyCreated.State = State.Running;
                    if (Model2.ServerTenancy.Equals("Shared"))
                        newlyCreated.Tenancy = Tenancy.Shared;
                    else if (Model2.ServerTenancy.Equals("Dedicated Instance"))
                        newlyCreated.Tenancy = Tenancy.DedicatedInstance;
                    else if (Model2.ServerTenancy.Equals("Dedicated Hardware"))
                        newlyCreated.Tenancy = Tenancy.DedicatedHardware;
                    _context.Servers.Add(newlyCreated);
                    _context.SaveChanges();
                    ViewData["ServerName"] = newlyCreated.Name;
                    return View(response.Reservation.Instances[0]);
                }
                else
                {
                    ViewData["Exception"] = "An error occured!";
                    return View(new Instance());
                }
            }
            catch (AmazonEC2Exception e)
            {
                ViewData["Exception"] = e.Message;
                return View(new Instance());
            }
        }
    }
}