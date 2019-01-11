using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminSide.Areas.PlatformManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Amazon.EC2;
using Amazon.EC2.Model;
using ASPJ_MVC.Models;
using System.Diagnostics;
using Subnet = AdminSide.Areas.PlatformManagement.Models.Subnet;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    public class ChallengeNetworkSubnetsController : Controller
    {
        private readonly PlatformResourcesContext _context;

        private IAmazonEC2 EC2Client { get; set; }

        public ChallengeNetworkSubnetsController(PlatformResourcesContext context, IAmazonEC2 ec2Client)
        {
            this._context = context;
            this.EC2Client = ec2Client;
        }

        public async Task<IActionResult> Index()
        {
            /*
            if (!_context.Subnets.Any())
            {
                DescribeSubnetsResponse response = await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                {
                    Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
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
                            foreach(Subnet failed in _context.Subnets)
                            {
                                _context.Subnets.Remove(failed);
                            }
                            _context.SaveChanges();
                        }
                        _context.Database.CloseConnection();
                        ViewData["Result"] = "An error occured when quering AWS for Subnet information!";
                        return View(await _context.Subnets.ToListAsync());
                    }
                }
                _context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Subnet OFF");
                _context.Database.CloseConnection();
                return View(await _context.Subnets.ToListAsync());
            }
            else
            {
            */
                return View(await _context.Subnets.ToListAsync());
            //}
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string action, string subnetID)
        {
            if (action.Equals("Delete") && !String.IsNullOrEmpty(subnetID))
            {
                var Deletesubnet = await _context.Subnets.FindAsync(Int32.Parse(subnetID));
                if (Deletesubnet == null)
                {
                    ViewData["Result"] = "Invaild Subnet!";
                    return View(await _context.Subnets.ToListAsync());
                }
                else if (subnetID == "1" || subnetID == "2" || subnetID == "3")
                {
                    ViewData["Result"] = "You cannot delete a default subnet!";
                    return View(await _context.Subnets.ToListAsync());
                }
                else
                {
                    DescribeSubnetsResponse response = await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                    {
                        Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                }
                    });
                    Boolean flag = false;
                    for (int i = 0; i < response.Subnets.Count; i++)
                    {
                        Amazon.EC2.Model.Subnet subnet = response.Subnets[i];
                        String retrievedID = subnet.SubnetId;
                        if (Deletesubnet.AWSVPCSubnetReference.Equals(retrievedID))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag == false)
                    {
                        ViewData["Result"] = "Subnet not found! The subnet may have been modified by another user";
                        return View(await _context.Subnets.ToListAsync());
                    }
                    else
                    {
                        try
                        {
                            DeleteSubnetRequest request = new DeleteSubnetRequest(Deletesubnet.AWSVPCSubnetReference);
                            DeleteSubnetResponse responseEC2 = await EC2Client.DeleteSubnetAsync(request);
                            if (responseEC2.HttpStatusCode == HttpStatusCode.OK)
                            {
                                _context.Subnets.Remove(Deletesubnet);
                                await _context.SaveChangesAsync();
                                ViewData["Result"] = "Successfully Deleted!";
                                return View(await _context.Subnets.ToListAsync());
                            }
                            else
                            {
                                ViewData["Result"] = "Failed!";
                                return View(await _context.Subnets.ToListAsync());
                            }
                        }
                        catch (AmazonEC2Exception e)
                        {
                            ViewData["Result"] = e.Message;
                            return View(await _context.Subnets.ToListAsync());
                        }
                    }
                }
            }
            else if (action.Equals("Modify") && !String.IsNullOrEmpty(subnetID))
            {
                if (subnetID == "1" || subnetID == "2" || subnetID == "3")
                {
                    ViewData["Result"] = "You cannot modify a default subnet!";
                    return View(await _context.Subnets.ToListAsync());
                }
                return RedirectToAction("Edit", new { id = subnetID });
            }
            else
            {
                return View(await _context.Subnets.ToListAsync());
            }
        }

        public async Task<IActionResult> Edit(int ID)
        {
            if (_context.Subnets.Any())
            {
                var subnet = await _context.Subnets.FindAsync(ID);
                if (subnet == null)
                {
                    return NotFound();
                }
                ViewData["SubnetID"] = ID;
                return View(subnet);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int ID, [Bind("ID,Name,Type,IPv4CIDR,IPv6CIDR")] Subnet Newsubnet)
        {
            var subnet = await _context.Subnets.FindAsync(ID);
            if (subnet == null)
            {
                return NotFound();
            }
            else if (!subnet.ID.Equals(Newsubnet.ID) || !subnet.IPv4CIDR.Equals(Newsubnet.IPv4CIDR) || !subnet.IPv6CIDR.Equals(Newsubnet.IPv6CIDR))
            {
                return StatusCode(500);
            }
            else if (subnet.ID == 1 || subnet.ID == 2 || subnet.ID == 3)
            {
                ViewData["Exception"] = "You cannot edit a default subnet!";
                return View(subnet);
            }
            else
            {
                subnet.Name = Newsubnet.Name;
                subnet.Type = Newsubnet.Type;
                try
                {
                    _context.Update(subnet);
                    await _context.SaveChangesAsync();
                    TempData["Result"] = "Successfully Modified!";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _context.Subnets.FindAsync(subnet.ID) == null)
                    {
                        TempData["Result"] = "Subnet is gone! The subnet may have been modifed by another user";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["Result"] = "Subnet has been updated since you last clicked! No modification has been made";
                        return RedirectToAction("Index");
                    }
                }
            }
        }

        public async Task<IActionResult> Create()
        {
            DescribeVpcsResponse response = await EC2Client.DescribeVpcsAsync(new DescribeVpcsRequest
            {
                Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                }
            });
            String[] IPBlocks = response.Vpcs[0].CidrBlock.Split(".");
            ViewData["IPCIDR"] = IPBlocks[0] + "." + IPBlocks[1];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Type,IPv4CIDR,SubnetSize")] Subnet subnet)
        {
            if (ModelState.IsValid)
            {
                switch (Int32.Parse(subnet.SubnetSize))
                {
                    case 32766:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/17";
                        break;
                    case 16382:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/18";
                        break;
                    case 8190:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/19";
                        break;
                    case 4094:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/20";
                        break;
                    case 2046:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/21";
                        break;
                    case 1022:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/22";
                        break;
                    case 510:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/23";
                        break;
                    case 254:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/24";
                        break;
                    case 126:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/25";
                        break;
                    case 62:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/26";
                        break;
                    case 30:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/27";
                        break;
                    case 14:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/28";
                        break;
                    case 6:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/29";
                        break;
                    case 2:
                        subnet.IPv4CIDR = subnet.IPv4CIDR + "/30";
                        break;
                    default:
                        ViewData["Exception"] = "Input Invaild!";
                        return View();
                }
                DescribeSubnetsResponse response = await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                {
                    Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                }
                });
                List<int> ipv6CIDR = new List<int>();
                List<Amazon.EC2.Model.Subnet> subnets = response.Subnets;
                int ipv6Subnet = 0;
                string[] ipv6CIDRstr = new string[6];
                if (subnets.Count == 0)
                {
                    DescribeVpcsResponse responseV = await EC2Client.DescribeVpcsAsync(new DescribeVpcsRequest
                    {
                        Filters = new List<Amazon.EC2.Model.Filter>
                        {
                            new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                        }
                    });
                    Vpc vpc = responseV.Vpcs[0];
                    VpcIpv6CidrBlockAssociation ipv6CidrBlockAssociation = vpc.Ipv6CidrBlockAssociationSet[0];
                    ipv6CIDRstr = ipv6CidrBlockAssociation.Ipv6CidrBlock.Split(":");
                }
                else
                {
                    foreach (Amazon.EC2.Model.Subnet s in subnets)
                    {
                        List<SubnetIpv6CidrBlockAssociation> ipv6 = s.Ipv6CidrBlockAssociationSet;
                        ipv6CIDRstr = ipv6[0].Ipv6CidrBlock.Split(":");
                        ipv6Subnet = int.Parse(ipv6CIDRstr[3].Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        ipv6CIDR.Add(ipv6Subnet);
                    }
                    Boolean flag = false;
                    while (flag != true)
                    {
                        Console.WriteLine("Doing while loop");
                        Console.WriteLine(ipv6Subnet);
                        Boolean passed = false;
                        ++ipv6Subnet;
                        foreach (int i in ipv6CIDR)
                        {

                            if (ipv6Subnet == ipv6CIDR[i])
                            {
                                passed = false;
                                break;
                            }
                            else
                                passed = true;
                        }
                        if (passed == true)
                            flag = true;
                    }
                }
                if (ipv6CIDRstr[5] == "/56")
                {
                    if (ipv6Subnet < 9)
                        subnet.IPv6CIDR = ipv6CIDRstr[0] + ":" + ipv6CIDRstr[1] + ":" + ipv6CIDRstr[2] + ":" + ipv6CIDRstr[3].Substring(0, 2) + "0" + ipv6Subnet.ToString() + "::/64";
                    else
                        subnet.IPv6CIDR = ipv6CIDRstr[0] + ":" + ipv6CIDRstr[1] + ":" + ipv6CIDRstr[2] + ":" + ipv6CIDRstr[3].Substring(0, 2) + Convert.ToInt32(ipv6Subnet).ToString() + "::/64";
                }
                else
                {
                    if (ipv6Subnet < 9)
                        subnet.IPv6CIDR = ipv6CIDRstr[0] + ":" + ipv6CIDRstr[1] + ":" + ipv6CIDRstr[2] + ":" + ipv6CIDRstr[3].Substring(0, 2) + "0" + ipv6Subnet.ToString() + "::" + ipv6CIDRstr[5];
                    else
                        subnet.IPv6CIDR = ipv6CIDRstr[0] + ":" + ipv6CIDRstr[1] + ":" + ipv6CIDRstr[2] + ":" + ipv6CIDRstr[3].Substring(0, 2) + Convert.ToInt32(ipv6Subnet).ToString() + "::" + ipv6CIDRstr[5];
                }
                CreateSubnetRequest request = new CreateSubnetRequest("vpc-09cd2d2019d9ac437", subnet.IPv4CIDR);
                request.Ipv6CidrBlock = subnet.IPv6CIDR;
                try
                {
                    CreateSubnetResponse responseS = await EC2Client.CreateSubnetAsync(request);
                    if (responseS.HttpStatusCode == HttpStatusCode.OK)
                    {
                        subnet.AWSVPCSubnetReference = responseS.Subnet.SubnetId;
                        _context.Subnets.Add(subnet);
                        await _context.SaveChangesAsync();
                        TempData["Result"] = "Successfully Created!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewData["Exception"] = "Failed to Create!";
                        return View();
                    }
                }
                catch (Amazon.EC2.AmazonEC2Exception e)
                {
                    ViewData["Exception"] = e.Message;
                    return View();
                }
            }
            else
            {
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