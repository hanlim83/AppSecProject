using System;
using System.Collections.Generic;
using System.Linq;
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
            }
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
                        Tenancy = Tenancy.Dedicated
                    };
                }
                else if (Model2.ServerTenancy.Equals("Dedicated Hardware"))
                {
                    request.Placement = new Placement
                    {
                        Tenancy = Tenancy.Host
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
                        Tenancy = Tenancy.Dedicated
                    };
                }
                else if (Model2.ServerTenancy.Equals("Dedicated Hardware"))
                {
                    request.Placement = new Placement
                    {
                        Tenancy = Tenancy.Host
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
                        Tenancy = Tenancy.Dedicated
                    };
                }
                else if (Model2.ServerTenancy.Equals("Dedicated Hardware"))
                {
                    request.Placement = new Placement
                    {
                        Tenancy = Tenancy.Host
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
                    } else
                    {
                        ViewData["Result"] = "Deletion Failed!";
                        return RedirectToAction("");
                    }
                } catch (AmazonEC2Exception e)
                {
                    ViewData["Result"] = e.Message;
                    return RedirectToAction("");
                }
            }
            else
                return NotFound();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}