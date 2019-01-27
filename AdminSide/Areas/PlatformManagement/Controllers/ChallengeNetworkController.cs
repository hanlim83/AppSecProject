using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using Amazon.EC2;
using Amazon.EC2.Model;
using ASPJ_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteTable = AdminSide.Areas.PlatformManagement.Models.RouteTable;
using Subnet = AdminSide.Areas.PlatformManagement.Models.Subnet;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    [Authorize]
    public class ChallengeNetworkController : Controller
    {
        private readonly PlatformResourcesContext _context;

        IAmazonEC2 EC2Client { get; set; }

        public ChallengeNetworkController(PlatformResourcesContext context, IAmazonEC2 ec2Client)
        {
            this._context = context;
            this.EC2Client = ec2Client;
        }

        public async Task<IActionResult> Index()
        {
            ChallengeNetworkParentViewModel model = new ChallengeNetworkParentViewModel
            {
                RetrievedSubnets = await _context.Subnets.ToListAsync(),
                RetrievedRoutes = await _context.Routes.ToListAsync()
            };
            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> Index(string action, string subnetID)
        {
            if (action.Equals("Delete") && !String.IsNullOrEmpty(subnetID))
            {
                Subnet Deletesubnet = await _context.Subnets.FindAsync(Int32.Parse(subnetID));
                if (Deletesubnet == null)
                {
                    TempData["Result"] = "Invaild Subnet!";
                    ChallengeNetworkParentViewModel model = new ChallengeNetworkParentViewModel
                    {
                        RetrievedSubnets = await _context.Subnets.ToListAsync(),
                        RetrievedRoutes = await _context.Routes.ToListAsync()
                    };
                    return View(model);
                }
                else if (Deletesubnet.editable == false)
                {
                    TempData["Result"] = "You cannot delete a default subnet!";
                    ChallengeNetworkParentViewModel model = new ChallengeNetworkParentViewModel
                    {
                        RetrievedSubnets = await _context.Subnets.ToListAsync(),
                        RetrievedRoutes = await _context.Routes.ToListAsync()
                    };
                    return View(model);
                }
                else
                {
                    DescribeSubnetsResponse response = await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                    {
                        Filters = new List<Amazon.EC2.Model.Filter>
                        {
                            new Filter("vpc-id",new List<string>
                            {
                                Deletesubnet.LinkedVPC.AWSVPCReference
                            })
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
                        ChallengeNetworkParentViewModel model = new ChallengeNetworkParentViewModel
                        {
                            RetrievedSubnets = await _context.Subnets.ToListAsync(),
                            RetrievedRoutes = await _context.Routes.ToListAsync()
                        };
                        return View(model);
                    }
                    else
                    {
                        try
                        {
                            List<RouteTable> RTs = await _context.RouteTables.ToListAsync();
                            DeleteSubnetRequest request = new DeleteSubnetRequest(Deletesubnet.AWSVPCSubnetReference);
                            DeleteSubnetResponse responseEC2 = await EC2Client.DeleteSubnetAsync(request);
                            if (responseEC2.HttpStatusCode == HttpStatusCode.OK)
                            {
                                _context.Subnets.Remove(Deletesubnet);
                                await _context.SaveChangesAsync();
                                TempData["Result"] = "Successfully Deleted!";
                                ChallengeNetworkParentViewModel model = new ChallengeNetworkParentViewModel
                                {
                                    RetrievedSubnets = await _context.Subnets.ToListAsync(),
                                    RetrievedRoutes = await _context.Routes.ToListAsync()
                                };
                                return View(model);
                            }
                            else
                            {
                                TempData["Result"] = "Failed!";
                                ChallengeNetworkParentViewModel model = new ChallengeNetworkParentViewModel
                                {
                                    RetrievedSubnets = await _context.Subnets.ToListAsync(),
                                    RetrievedRoutes = await _context.Routes.ToListAsync()
                                };
                                return View(model);
                            }
                        }
                        catch (AmazonEC2Exception e)
                        {
                            TempData["Result"] = e.Message;
                            ChallengeNetworkParentViewModel model = new ChallengeNetworkParentViewModel
                            {
                                RetrievedSubnets = await _context.Subnets.ToListAsync(),
                                RetrievedRoutes = await _context.Routes.ToListAsync()
                            };
                            return View(model);
                        }
                    }
                }
            }
            else if (action.Equals("Modify") && !String.IsNullOrEmpty(subnetID))
            {
                var Modifysubnet = await _context.Subnets.FindAsync(Int32.Parse(subnetID));
                if (Modifysubnet.editable == false)
                {
                    ViewData["Result"] = "You cannot modify a default subnet!";
                    ChallengeNetworkParentViewModel model = new ChallengeNetworkParentViewModel
                    {
                        RetrievedSubnets = await _context.Subnets.ToListAsync(),
                        RetrievedRoutes = await _context.Routes.ToListAsync()
                    };
                    return View(model);
                }
                return RedirectToAction("Edit", new { id = subnetID });
            }
            else
            {
                ChallengeNetworkParentViewModel model = new ChallengeNetworkParentViewModel
                {
                    RetrievedSubnets = await _context.Subnets.ToListAsync(),
                    RetrievedRoutes = await _context.Routes.ToListAsync()
                };
                return View(model);
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
            else if (subnet.editable == false)
            {
                ViewData["Exception"] = "You cannot edit a default subnet!";
                return View(subnet);
            }
            else
            {
                if (!subnet.Name.Equals(Newsubnet.Name))
                {
                    await EC2Client.DeleteTagsAsync(new DeleteTagsRequest
                    {
                        Resources = new List<string>
                        {
                            subnet.AWSVPCSubnetReference
                        },
                        Tags = new List<Tag>
                        {
                            new Tag("Name")
                        }
                    });
                    await EC2Client.CreateTagsAsync(new CreateTagsRequest
                    {
                        Resources = new List<string>
                        {
                            subnet.AWSVPCSubnetReference
                        },
                        Tags = new List<Tag>
                        {
                            new Tag("Name",Newsubnet.Name)
                        }
                    });
                    subnet.Name = Newsubnet.Name;
                }
                if (subnet.Type != Newsubnet.Type)
                {
                    try
                    {
                        DisassociateRouteTableResponse responseD = await EC2Client.DisassociateRouteTableAsync(new DisassociateRouteTableRequest
                        {
                            AssociationId = subnet.AWSVPCRouteTableAssoicationID
                        });
                        if (responseD.HttpStatusCode == HttpStatusCode.OK)
                        {
                            AssociateRouteTableRequest requestA = new AssociateRouteTableRequest
                            {
                                SubnetId = subnet.AWSVPCSubnetReference
                            };
                            if (Newsubnet.Type == Models.SubnetType.Internet)
                            {
                                RouteTable Internet = await _context.RouteTables.FindAsync(1);
                                requestA.RouteTableId = Internet.AWSVPCRouteTableReference;
                            }
                            else if (Newsubnet.Type == Models.SubnetType.Extranet)
                            {
                                RouteTable Extranet = await _context.RouteTables.FindAsync(2);
                                requestA.RouteTableId = Extranet.AWSVPCRouteTableReference;
                            }
                            else if (Newsubnet.Type == Models.SubnetType.Intranet)
                            {
                                RouteTable Intranet = await _context.RouteTables.FindAsync(3);
                                requestA.RouteTableId = Intranet.AWSVPCRouteTableReference;
                            }
                            AssociateRouteTableResponse responseA = await EC2Client.AssociateRouteTableAsync(requestA);
                            if (responseA.HttpStatusCode == HttpStatusCode.OK)
                            {
                                subnet.Type = Newsubnet.Type;
                            }
                            else
                            {
                                if (subnet.Type == Models.SubnetType.Internet)
                                {
                                    RouteTable Internet = await _context.RouteTables.FindAsync(1);
                                    requestA.RouteTableId = Internet.AWSVPCRouteTableReference;
                                }
                                else if (subnet.Type == Models.SubnetType.Extranet)
                                {
                                    RouteTable Extranet = await _context.RouteTables.FindAsync(2);
                                    requestA.RouteTableId = Extranet.AWSVPCRouteTableReference;
                                }
                                else if (subnet.Type == Models.SubnetType.Intranet)
                                {
                                    RouteTable Intranet = await _context.RouteTables.FindAsync(3);
                                    requestA.RouteTableId = Intranet.AWSVPCRouteTableReference;
                                }
                                await EC2Client.AssociateRouteTableAsync(requestA);
                                ViewData["Exception"] = "Edit Failed! - Route Table linking Failed";
                                return View(subnet);
                            }
                        }
                        else
                        {
                            ViewData["Exception"] = "Edit Failed!";
                            return View(subnet);
                        }
                    }
                    catch (AmazonEC2Exception e)
                    {
                        ViewData["Exception"] = e;
                        return View(subnet);
                    }
                }
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
            VPC vpc = await _context.VPCs.FindAsync(1);
            DescribeVpcsResponse response = await EC2Client.DescribeVpcsAsync(new DescribeVpcsRequest
            {
                Filters = new List<Filter>
                {
                    new Filter("vpc-id", new List<string>
                    {
                        vpc.AWSVPCReference
                    })
                }
            });
            String[] IPBlocks = response.Vpcs[0].CidrBlock.Split(".");
            ViewData["IPCIDR"] = IPBlocks[0] + "." + IPBlocks[1];
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create([Bind("Name,Type,IPv4CIDR,SubnetSize")] Subnet subnet)
        {
            if (ModelState.IsValid)
            {
                subnet.editable = true;
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
                VPC vpc = await _context.VPCs.FindAsync(1);
                subnet.VPCID = vpc.ID;
                DescribeSubnetsResponse response = await EC2Client.DescribeSubnetsAsync(new DescribeSubnetsRequest
                {
                    Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {vpc.AWSVPCReference}}
                }
                });
                List<int> ipv6CIDR = new List<int>();
                List<Amazon.EC2.Model.Subnet> subnets = response.Subnets;
                int ipv6Subnet = 0;
                string[] ipv6CIDRstr = new string[6];
                DescribeVpcsResponse responseV = await EC2Client.DescribeVpcsAsync(new DescribeVpcsRequest
                {
                    Filters = new List<Amazon.EC2.Model.Filter>
                        {
                            new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {vpc.AWSVPCReference}}
                        }
                });
                Vpc vpcR = responseV.Vpcs[0];
                VpcIpv6CidrBlockAssociation ipv6CidrBlockAssociation = vpcR.Ipv6CidrBlockAssociationSet[0];
                ipv6CIDRstr = ipv6CidrBlockAssociation.Ipv6CidrBlock.Split(":");
                if (subnets.Count != 0)
                {
                    foreach (Amazon.EC2.Model.Subnet s in subnets)
                    {
                        List<SubnetIpv6CidrBlockAssociation> ipv6 = s.Ipv6CidrBlockAssociationSet;
                        ipv6CIDRstr = ipv6.ElementAt(0).Ipv6CidrBlock.Split(":");
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

                            if (ipv6Subnet <= ipv6CIDR[i])
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
                if (ipv6CIDRstr[5].Equals("/56"))
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
                CreateSubnetRequest requestS = new CreateSubnetRequest()
                {
                    CidrBlock = subnet.IPv4CIDR,
                    VpcId = vpc.AWSVPCReference,
                    Ipv6CidrBlock = subnet.IPv6CIDR
                };
                try
                {
                    CreateSubnetResponse responseS = await EC2Client.CreateSubnetAsync(requestS);
                    if (responseS.HttpStatusCode == HttpStatusCode.OK)
                    {
                        subnet.AWSVPCSubnetReference = responseS.Subnet.SubnetId;
                        await EC2Client.CreateTagsAsync(new CreateTagsRequest
                        {
                            Resources = new List<string>
                            {
                                responseS.Subnet.SubnetId
                            },
                            Tags = new List<Tag>
                            {
                                new Tag("Name",subnet.Name)
                            }
                        });
                        AssociateRouteTableRequest requestRT = new AssociateRouteTableRequest
                        {
                            SubnetId = responseS.Subnet.SubnetId,
                        };
                        if (subnet.Type == Models.SubnetType.Internet)
                        {
                            RouteTable Internet = await _context.RouteTables.FindAsync(2);
                            requestRT.RouteTableId = Internet.AWSVPCRouteTableReference;
                            subnet.RouteTableID = Internet.ID;
                            await EC2Client.ModifySubnetAttributeAsync(new ModifySubnetAttributeRequest
                            {
                                SubnetId = responseS.Subnet.SubnetId,
                                MapPublicIpOnLaunch = true
                            });
                            await EC2Client.ModifySubnetAttributeAsync(new ModifySubnetAttributeRequest
                            {
                                SubnetId = responseS.Subnet.SubnetId,
                                AssignIpv6AddressOnCreation = true
                            });
                        }
                        else if (subnet.Type == Models.SubnetType.Extranet)
                        {
                            RouteTable Extranet = await _context.RouteTables.FindAsync(3);
                            requestRT.RouteTableId = Extranet.AWSVPCRouteTableReference;
                            subnet.RouteTableID = Extranet.ID;
                        }
                        else if (subnet.Type == Models.SubnetType.Intranet)
                        {
                            RouteTable Intranet = await _context.RouteTables.FindAsync(2);
                            requestRT.RouteTableId = Intranet.AWSVPCRouteTableReference;
                            subnet.RouteTableID = Intranet.ID;
                        }
                        AssociateRouteTableResponse responseRT = await EC2Client.AssociateRouteTableAsync(requestRT);
                        if (responseRT.HttpStatusCode == HttpStatusCode.OK)
                        {
                            subnet.AWSVPCRouteTableAssoicationID = responseRT.AssociationId;
                            _context.Subnets.Add(subnet);
                            await _context.SaveChangesAsync();
                            TempData["Result"] = "Successfully Created!";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            await EC2Client.DeleteSubnetAsync(new DeleteSubnetRequest
                            {
                                SubnetId = subnet.AWSVPCSubnetReference
                            });
                            DescribeVpcsResponse responseD = await EC2Client.DescribeVpcsAsync(new DescribeVpcsRequest
                            {
                                Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {vpc.AWSVPCReference}}
                }
                            });
                            String[] IPBlocks = responseD.Vpcs[0].CidrBlock.Split(".");
                            ViewData["IPCIDR"] = IPBlocks[0] + "." + IPBlocks[1];
                            ViewData["Exception"] = "Failed to Create!";
                            return View();
                        }
                    }
                    else
                    {
                        DescribeVpcsResponse responseD = await EC2Client.DescribeVpcsAsync(new DescribeVpcsRequest
                        {
                            Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {vpc.AWSVPCReference}}
                }
                        });
                        String[] IPBlocks = responseD.Vpcs[0].CidrBlock.Split(".");
                        ViewData["IPCIDR"] = IPBlocks[0] + "." + IPBlocks[1];
                        ViewData["Exception"] = "Failed to Create!";
                        ViewData["Exception"] = "Failed to Create!";
                        return View();
                    }
                }
                catch (Amazon.EC2.AmazonEC2Exception e)
                {
                    DescribeVpcsResponse responseD = await EC2Client.DescribeVpcsAsync(new DescribeVpcsRequest
                    {
                        Filters = new List<Amazon.EC2.Model.Filter>
                {
                     new Amazon.EC2.Model.Filter {Name = "vpc-id", Values = new List<string> {vpc.AWSVPCReference}}
                }
                    });
                    String[] IPBlocks = responseD.Vpcs[0].CidrBlock.Split(".");
                    ViewData["IPCIDR"] = IPBlocks[0] + "." + IPBlocks[1];
                    ViewData["Exception"] = "Failed to Create!";
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