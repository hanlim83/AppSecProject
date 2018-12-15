using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
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

        IAmazonEC2 EC2Client { get; set; }

        public ChallengeNetworkSubnetsController(PlatformResourcesContext context, IAmazonEC2 ec2Client)
        {
            this._context = context;
            this.EC2Client = ec2Client;
        }

        public async Task<IActionResult> Index()
        {
            if (!_context.Subnets.Any())
            {
                DescribeSubnetsResponse response = await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                {
                    Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {"vpc-09cd2d2019d9ac437"}}
                }
                });
                foreach (var subnet in response.Subnets)
                {
                    Subnet newSubnet = new Subnet();
                    newSubnet.Name = subnet.SubnetId;
                    newSubnet.IPv4CIDR = subnet.CidrBlock;
                    newSubnet.IPv6CIDR = subnet.Ipv6CidrBlockAssociationSet[0].Ipv6CidrBlock;
                    newSubnet.AWSVPCSubnetReference = subnet.SubnetId;
                    _context.Add(newSubnet);
                    await _context.SaveChangesAsync();
                }
                return View(await _context.Subnets.ToListAsync());
            }
            else
            {
                return View(await _context.Subnets.ToListAsync());
            }
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
                        return StatusCode(500);
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
            else if (action.Equals("Modify"))
            {
                return View(await _context.Subnets.ToListAsync());
            }
            else
            {
                return View(await _context.Subnets.ToListAsync());
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}