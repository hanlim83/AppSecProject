using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminSide.Areas.PlatformManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Amazon.EC2;
using Amazon.EC2.Model;
using AdminSide.Areas.PlatformManagement.Models;
using System.Diagnostics;
using ASPJ_MVC.Models;
using Microsoft.EntityFrameworkCore;
using Subnet = AdminSide.Areas.PlatformManagement.Models.Subnet;
using System.Net;
using State = AdminSide.Areas.PlatformManagement.Models.State;
using Tenancy = AdminSide.Areas.PlatformManagement.Models.Tenancy;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    public class ChallengeServersController : Controller
    {
        private readonly PlatformResourcesContext _context;

        private IAmazonEC2 EC2Client { get; set; }

        private ChallengeServersCreationFormModel creationReference;

        public ChallengeServersController(PlatformResourcesContext context, IAmazonEC2 ec2Client)
        {
            this._context = context;
            this.EC2Client = ec2Client;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Servers.ToListAsync());
        }

        public async Task<IActionResult> SelectTemplate()
        {
            return View(await _context.Templates.ToListAsync());
        }

        public IActionResult SpecifySettings()
        {
            return RedirectToAction("SelectTemplate");
        }

        public IActionResult VerifySettings()
        {
            if (creationReference == null)
                return RedirectToAction("SelectTemplate");
            else
                return View(creationReference);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SpecifySettings(String selectedTemplate)
        {
            /*
            if (!_context.Subnets.Any())
            {
                DescribeSubnetsResponse response = await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                {
                    Filters = new List<Filter>
                {
                     new Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                }
                });
                int incrementer = 4;
                _context.Database.OpenConnection();
                _context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Subnet ON");
                foreach (var subnet in response.Subnets)
                {
                    try
                    {
                        Subnet newSubnet = new Subnet();
                        if (subnet.CidrBlock.Equals("172.30.0.0/24"))
                        {
                            newSubnet.ID = 1;
                            newSubnet.Name = "Default Internet Subnet";
                            newSubnet.IPv4CIDR = subnet.CidrBlock;
                            newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                            newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                            newSubnet.Type = SubnetType.Internet;
                            newSubnet.SubnetSize = "254";
                            _context.Subnets.Add(newSubnet);
                            await _context.SaveChangesAsync();
                        }
                        else if (subnet.CidrBlock.Equals("172.30.1.0/24"))
                        {
                            newSubnet.ID = 2;
                            newSubnet.Name = "Default Extranet Subnet";
                            newSubnet.IPv4CIDR = subnet.CidrBlock;
                            newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                            newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                            newSubnet.Type = SubnetType.Extranet;
                            newSubnet.SubnetSize = "254";
                            _context.Subnets.Add(newSubnet);
                            await _context.SaveChangesAsync();
                        }
                        else if (subnet.CidrBlock.Equals("172.30.2.0/24"))
                        {
                            newSubnet.ID = 3;
                            newSubnet.Name = "Default Intranet Subnet";
                            newSubnet.IPv4CIDR = subnet.CidrBlock;
                            newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                            newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                            newSubnet.Type = SubnetType.Intranet;
                            newSubnet.SubnetSize = "254";
                            _context.Subnets.Add(newSubnet);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            newSubnet.ID = incrementer;
                            newSubnet.Name = subnet.SubnetId;
                            newSubnet.IPv4CIDR = subnet.CidrBlock;
                            newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                            newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                            newSubnet.Type = SubnetType.Extranet;
                            string subnetPrefix = subnet.CidrBlock.Substring(subnet.CidrBlock.Length - 3);
                            switch (subnetPrefix)
                            {
                                case "/17":
                                    newSubnet.SubnetSize = Convert.ToString(32766);
                                    break;
                                case "/18":
                                    newSubnet.SubnetSize = Convert.ToString(16382);
                                    break;
                                case "/19":
                                    newSubnet.SubnetSize = Convert.ToString(8190);
                                    break;
                                case "/20":
                                    newSubnet.SubnetSize = Convert.ToString(4094);
                                    break;
                                case "/21":
                                    newSubnet.SubnetSize = Convert.ToString(2046);
                                    break;
                                case "/22":
                                    newSubnet.SubnetSize = Convert.ToString(1022);
                                    break;
                                case "/23":
                                    newSubnet.SubnetSize = Convert.ToString(510);
                                    break;
                                case "/24":
                                    newSubnet.SubnetSize = Convert.ToString(254);
                                    break;
                                case "/25":
                                    newSubnet.SubnetSize = Convert.ToString(126);
                                    break;
                                case "/26":
                                    newSubnet.SubnetSize = Convert.ToString(62);
                                    break;
                                case "/27":
                                    newSubnet.SubnetSize = Convert.ToString(30);
                                    break;
                                case "/28":
                                    newSubnet.SubnetSize = Convert.ToString(14);
                                    break;
                                case "/29":
                                    newSubnet.SubnetSize = Convert.ToString(6);
                                    break;
                                case "/30":
                                    newSubnet.SubnetSize = Convert.ToString(2);
                                    break;
                                default:
                                    break;
                            }
                            _context.Subnets.Add(newSubnet);
                            await _context.SaveChangesAsync();
                            ++incrementer;
                        }
                    }
                    catch (Exception)
                    {
                        _context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Subnet OFF");
                        if (_context.Subnets.Any())
                        {
                            foreach (Subnet failed in _context.Subnets)
                            {
                                _context.Subnets.Remove(failed);
                            }
                            _context.SaveChanges();
                        }
                        _context.Database.CloseConnection();
                    }
                }
                _context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Subnet OFF");
                _context.Database.CloseConnection();
            } */
            if (selectedTemplate != null)
            {
                TempData["selectedTemplate"] = selectedTemplate;
                return View(await _context.Subnets.ToListAsync());
            }
            else
                return RedirectToAction("SelectTemplate");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteServer(String serverID)
        {
            Server deleted = await _context.Servers.FindAsync(Int32.Parse(serverID));
            if (deleted != null)
            {
                TerminateInstancesRequest request = new TerminateInstancesRequest
                {
                    InstanceIds = new List<string>
                    {
                        deleted.AWSEC2Reference
                    }
                };
                try
                {
                    TerminateInstancesResponse response = await EC2Client.TerminateInstancesAsync(request);
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        _context.Servers.Remove(deleted);
                        _context.SaveChanges();
                        ViewData["Result"] = "Successfully Deleted!";
                        return RedirectToAction("");
                    }
                    else
                    {
                        ViewData["Result"] = "Deletion Failed!";
                        return RedirectToAction("");
                    }
                }
                catch (AmazonEC2Exception e)
                {
                    ViewData["Result"] = e.Message;
                    return RedirectToAction("");
                }
            }
            else
                return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyServer(String serverID)
        {
            Server selected = await _context.Servers.FindAsync(Int32.Parse(serverID));
            StopInstancesResponse response = await EC2Client.StopInstancesAsync(new StopInstancesRequest
            {
                InstanceIds = new List<string> {
                    selected.AWSEC2Reference
            }
            });
            if (response.HttpStatusCode == HttpStatusCode.OK)
                return View(selected);
            else
                return StatusCode(500);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modification([Bind("ID,ServerName,ServerWorkload,ServerStorage,ServerTenancy")] ChallengeServersModificationFormModel Modified)
        {
            Server retrieved = await _context.Servers.FindAsync(Int32.Parse(Modified.ID));
            if (retrieved == null)
            {
                return RedirectToAction("");
            }
            else
            {
                if (!retrieved.Name.Equals(Modified.ServerName))
                {
                    retrieved.Name = Modified.ServerName;
                }
                DescribeInstanceAttributeResponse Aresponse = await EC2Client.DescribeInstanceAttributeAsync(new DescribeInstanceAttributeRequest
                {
                    Attribute = InstanceAttributeName.BlockDeviceMapping,
                    InstanceId = retrieved.AWSEC2Reference
                });
                ModifyInstanceAttributeRequest Irequest = new ModifyInstanceAttributeRequest
                {
                    InstanceId = retrieved.AWSEC2Reference
                };
                ModifyVolumeRequest Vrequest = new ModifyVolumeRequest
                {
                    VolumeId = Aresponse.InstanceAttribute.BlockDeviceMappings[0].Ebs.VolumeId
                };
                ModifyInstancePlacementRequest Prequest = new ModifyInstancePlacementRequest
                {
                    InstanceId = retrieved.AWSEC2Reference
                };
                if (Modified.ServerWorkload.Equals("Low") && retrieved.Workload != Workload.Low)
                {
                    Irequest.InstanceType = "t2.micro";
                    retrieved.Workload = Workload.Low;
                }
                else if (Modified.ServerWorkload.Equals("Medium") && retrieved.Workload != Workload.Medium)
                {
                    Irequest.InstanceType = "t2.medium";
                    retrieved.Workload = Workload.Medium;
                }
                else if (Modified.ServerWorkload.Equals("Large") && retrieved.Workload != Workload.Large)
                {
                    Irequest.InstanceType = "t2.xlarge";
                    retrieved.Workload = Workload.Large;
                }
                if (retrieved.StorageAssigned != Modified.ServerStorage && Modified.ServerStorage > retrieved.StorageAssigned)
                {
                    Vrequest.Size = Modified.ServerStorage;
                    retrieved.StorageAssigned = Modified.ServerStorage;
                }
                else
                {

                }
                Tenancy previous = retrieved.Tenancy;
                if (Modified.ServerTenancy.Equals("Dedicated Instance") && retrieved.Tenancy != Tenancy.DedicatedInstance)
                {
                    Prequest.Tenancy = HostTenancy.Dedicated;
                    retrieved.Tenancy = Tenancy.DedicatedInstance;
                }
                else if (Modified.ServerTenancy.Equals("Dedicated Hardware") && retrieved.Tenancy != Tenancy.DedicatedHardware)
                {
                    Prequest.Tenancy = HostTenancy.Host;
                    retrieved.Tenancy = Tenancy.DedicatedHardware;
                }
                else
                {

                }
                if (Irequest.InstanceType != null)
                {
                    try
                    {
                        ModifyInstanceAttributeResponse response = await EC2Client.ModifyInstanceAttributeAsync(Irequest);
                    }
                    catch (AmazonEC2Exception e)
                    {
                        return RedirectToAction("");
                    }
                }
                if (Vrequest.Size != 0)
                {
                    try
                    {
                        ModifyVolumeResponse response = await EC2Client.ModifyVolumeAsync(Vrequest);
                    }
                    catch (AmazonEC2Exception e)
                    {
                        return RedirectToAction("");
                    }
                }
                try
                {
                    _context.Servers.Update(retrieved);
                    await _context.SaveChangesAsync();
                    if (Prequest.Tenancy != null)
                    {
                        ModifyInstancePlacementResponse response = await EC2Client.ModifyInstancePlacementAsync(Prequest);
                    }
                    return RedirectToAction("");
                }
                catch (DbUpdateConcurrencyException)
                {
                    retrieved = await _context.Servers.FindAsync(Int32.Parse(Modified.ID));
                    if (Irequest.InstanceType != null)
                    {
                        if (Modified.ServerWorkload.Equals("Low"))
                        {
                            Irequest.InstanceType = "t2.micro";
                            retrieved.Workload = Workload.Low;
                        }
                        else if (Modified.ServerWorkload.Equals("Medium"))
                        {
                            Irequest.InstanceType = "t2.medium";
                            retrieved.Workload = Workload.Medium;
                        }
                        else if (Modified.ServerWorkload.Equals("Large"))
                        {
                            Irequest.InstanceType = "t2.xlarge";
                            retrieved.Workload = Workload.Large;
                        }
                        ModifyInstanceAttributeResponse response = await EC2Client.ModifyInstanceAttributeAsync(Irequest);
                    }
                    if (Vrequest.Size != 0)
                    {
                        Vrequest.Size = retrieved.StorageAssigned;
                        ModifyVolumeResponse response = await EC2Client.ModifyVolumeAsync(Vrequest);
                    }
                    return RedirectToAction("");
                }
                catch (AmazonEC2Exception e)
                {
                    retrieved = await _context.Servers.FindAsync(Int32.Parse(Modified.ID));
                    retrieved.Tenancy = previous;
                    _context.Servers.Update(retrieved);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("");
                }
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}