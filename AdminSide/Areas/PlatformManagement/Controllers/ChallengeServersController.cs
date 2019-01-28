﻿using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Models;
using AdminSide.Areas.PlatformManagement.Services;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ASPJ_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Protocol = AdminSide.Areas.PlatformManagement.Models.Protocol;
using State = AdminSide.Areas.PlatformManagement.Models.State;
using Subnet = AdminSide.Areas.PlatformManagement.Models.Subnet;
using Tag = Amazon.EC2.Model.Tag;
using Tenancy = AdminSide.Areas.PlatformManagement.Models.Tenancy;

namespace AdminSide.Areas.PlatformManagement.Controllers
{
    [Area("PlatformManagement")]
    [Authorize]
    public class ChallengeServersController : Controller
    {
        private readonly PlatformResourcesContext _context;

        private IAmazonEC2 EC2Client { get; set; }

        private IAmazonS3 S3Client { get; set; }

        private IAmazonCloudWatch CWClient { get; set; }

        private ChallengeServersCreationFormModel creationReference;

        private readonly IApplicationLifetime _appLifetime;
        private readonly ILogger _logger;

        public IBackgroundTaskQueue Backgroundqueue { get; }

        public ChallengeServersController(PlatformResourcesContext context, IAmazonEC2 ec2Client, IBackgroundTaskQueue Queue, IApplicationLifetime appLifetime, ILogger<ChallengeServersController> logger, IAmazonCloudWatch CWClient)
        {
            this._context = context;
            this.EC2Client = ec2Client;
            AmazonS3Config config = new AmazonS3Config
            {
                ForcePathStyle = true
            };
            this.S3Client = new AmazonS3Client(config);
            Backgroundqueue = Queue;
            _appLifetime = appLifetime;
            _logger = logger;
            this.CWClient = CWClient;
        }

        public async Task<IActionResult> Index()
        {
            if (_context.VPCs.ToList().Count == 0)
            {
                return RedirectToAction("", "Home", "");
            }
            return View(await _context.Servers.ToListAsync());
        }

        public async Task<IActionResult> SelectTemplate()
        {
            if (_context.VPCs.ToList().Count == 0)
            {
                return RedirectToAction("", "Home", "");
            }
            return View(await _context.Templates.ToListAsync());
        }

        public IActionResult SpecifySettings()
        {
            if (_context.VPCs.ToList().Count == 0)
            {
                return RedirectToAction("", "Home", "");
            }
            return RedirectToAction("SelectTemplate");
        }

        public IActionResult VerifySettings()
        {
            if (_context.VPCs.ToList().Count == 0)
            {
                return RedirectToAction("", "Home", "");
            }
            if (creationReference == null)
            {
                return RedirectToAction("SelectTemplate");
            }
            else
            {
                return View(creationReference);
            }
        }

        public async Task<IActionResult> RedirectServerCreation(String selectedTemplate)
        {
            if (_context.VPCs.ToList().Count == 0)
            {
                return RedirectToAction("", "Home", "");
            }
            if (selectedTemplate != null)
            {
                Template selected = await _context.Templates.FindAsync(int.Parse(selectedTemplate));
                if (selected != null)
                {
                    return await SpecifySettings(Convert.ToString(selected.ID));
                }
                else
                {
                    return RedirectToAction("", "ServerTemplates", "");
                }
            }
            else
            {
                return RedirectToAction("", "ServerTemplates", "");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Credentials(string serverID)
        {
            Server selected = await _context.Servers.FindAsync(int.Parse(serverID));
            if (selected != null)
            {
                ChallengeServersCredentialsPageModel model = new ChallengeServersCredentialsPageModel();
                model.serverName = selected.Name;
                model.serverIP = selected.IPAddress;
                model.serverDNS = selected.DNSHostname;
                String keyPairString = null;
                TransferUtility fileTransferUtility = new TransferUtility(S3Client);
                fileTransferUtility.Download(@"C:\Tempt\" + selected.KeyPairName + ".pem", "ectf-keypair", selected.KeyPairName + ".pem");
                using (FileStream fileDownloaded = new FileStream(@"C:\Tempt\" + selected.KeyPairName + ".pem", FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(fileDownloaded))
                    {
                        keyPairString = reader.ReadToEnd();
                    }
                }
                System.IO.File.Delete(@"C:\Tempt\" + selected.KeyPairName + ".pem");
                if (selected.OperatingSystem.Contains("Windows"))
                {
                    model.Windows = true;
                    GetPasswordDataResponse response = await EC2Client.GetPasswordDataAsync(new GetPasswordDataRequest
                    {
                        InstanceId = selected.AWSEC2Reference
                    });
                    if (String.IsNullOrEmpty(response.PasswordData))
                    {
                        model.Available = false;
                    }
                    else
                    {
                        model.retrievedPassword = response.GetDecryptedPassword(keyPairString);
                        model.Available = true;
                    }
                }
                else
                {
                    model.keyPairDownloadURL = S3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                    {
                        BucketName = "ectf-keypair",
                        Key = selected.KeyPairName + ".pem",
                        Expires = DateTime.Now.AddMinutes(5),
                        Protocol = Amazon.S3.Protocol.HTTPS,
                        ResponseHeaderOverrides = new ResponseHeaderOverrides
                        {
                            ContentDisposition = "attachement"
                        }
                    });
                }
                return View(model);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]

        public async Task<IActionResult> SpecifySettings(String selectedTemplate)
        {
            if (selectedTemplate != null)
            {
                Template selected = await _context.Templates.FindAsync(int.Parse(selectedTemplate));
                if (selected != null)
                {
                    ViewData["selectedTemplate"] = selected.ID;
                    if (selected.SpecificMinimumSize == true)
                    {
                        ViewData["minimumSize"] = selected.MinimumStorage;
                    }
                    else
                    {
                        ViewData["minimumSize"] = 8;
                    }

                    return View(await _context.Subnets.ToListAsync());
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return RedirectToAction("SelectTemplate");
            }
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
            Guid g = Guid.NewGuid();
            string randomKeypairString = Convert.ToBase64String(g.ToByteArray());
            randomKeypairString = randomKeypairString.Replace("=", "");
            randomKeypairString = randomKeypairString.Replace("+", "");
            randomKeypairString = randomKeypairString.Replace("\\", "");
            randomKeypairString = randomKeypairString.Replace("/", "");
            CreateKeyPairResponse responseCreateKeyPair = await EC2Client.CreateKeyPairAsync(new CreateKeyPairRequest
            {
                KeyName = randomKeypairString
            });
            if (responseCreateKeyPair.HttpStatusCode == HttpStatusCode.OK)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(@"C:\Tempt\" + responseCreateKeyPair.KeyPair.KeyName + ".pem"));
                using (FileStream s = new FileStream(@"C:\Tempt\" + responseCreateKeyPair.KeyPair.KeyName + ".pem", FileMode.Create))
                using (StreamWriter writer = new StreamWriter(s))
                {
                    writer.WriteLine(responseCreateKeyPair.KeyPair.KeyMaterial);
                }
                PutObjectResponse responsePutObject = await S3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = "ectf-keypair",
                    Key = responseCreateKeyPair.KeyPair.KeyName + ".pem",
                    FilePath = @"C:\Tempt\" + responseCreateKeyPair.KeyPair.KeyName + ".pem"
                });
                if (responsePutObject.HttpStatusCode == HttpStatusCode.OK)
                {
                    System.IO.File.Delete(@"C:\Tempt\" + responseCreateKeyPair.KeyPair.KeyName + ".pem");
                }
            }
            RunInstancesRequest request = new RunInstancesRequest
            {
                BlockDeviceMappings = new List<BlockDeviceMapping> {
                        new BlockDeviceMapping {
                            DeviceName = "/dev/xvda",
                            Ebs = new EbsBlockDevice { VolumeSize = Model2.ServerStorage }
                        }
                    },
                ImageId = selectedT.AWSAMIReference,
                KeyName = responseCreateKeyPair.KeyPair.KeyName,
                MaxCount = 1,
                MinCount = 1,
                SubnetId = selectedS.AWSVPCSubnetReference,
                Monitoring = true
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
                        SubnetID = selectedS.ID,
                        IPAddress = response.Reservation.Instances[0].PrivateIpAddress,
                        DNSHostname = response.Reservation.Instances[0].PrivateDnsName,
                        KeyPairName = responseCreateKeyPair.KeyPair.KeyName
                    };
                    if (Model2.ServerWorkload.Equals("Low"))
                    {
                        newlyCreated.Workload = Workload.Low;
                    }
                    else if (Model2.ServerWorkload.Equals("Medium"))
                    {
                        newlyCreated.Workload = Workload.Medium;
                    }
                    else if (Model2.ServerWorkload.Equals("Large"))
                    {
                        newlyCreated.Workload = Workload.Large;
                    }

                    if (selectedS.Type == SubnetType.Internet)
                    {
                        newlyCreated.Visibility = Visibility.Internet;
                    }
                    else
                    {
                        if (selectedS.Type == SubnetType.Extranet)
                        {
                            newlyCreated.Visibility = Visibility.Extranet;
                        }
                        else
                        {
                            newlyCreated.Visibility = Visibility.Intranet;
                        }
                    }
                    if (response.Reservation.Instances[0].State.Code == 0)
                    {
                        newlyCreated.State = State.Starting;
                    }
                    else if (response.Reservation.Instances[0].State.Code == 16)
                    {
                        newlyCreated.State = State.Running;
                    }

                    if (Model2.ServerTenancy.Equals("Shared"))
                    {
                        newlyCreated.Tenancy = Tenancy.Shared;
                    }
                    else if (Model2.ServerTenancy.Equals("Dedicated Instance"))
                    {
                        newlyCreated.Tenancy = Tenancy.DedicatedInstance;
                    }
                    else if (Model2.ServerTenancy.Equals("Dedicated Hardware"))
                    {
                        newlyCreated.Tenancy = Tenancy.DedicatedHardware;
                    }
                    _context.Servers.Add(newlyCreated);
                    _context.SaveChanges();
                    await CWClient.PutMetricAlarmAsync(new PutMetricAlarmRequest
                    {
                        Threshold = 1,
                        Period = 60,
                        MetricName = "StatusCheckFailed_System",
                        Dimensions = new List<Dimension>
                        {
                            new Dimension
                            {
                                Name = "InstanceId",
                                Value = response.Reservation.Instances[0].InstanceId
                            }
                        },
                        EvaluationPeriods = 2,
                        ActionsEnabled = true,
                        ComparisonOperator = ComparisonOperator.GreaterThanOrEqualToThreshold,
                        Namespace = "AWS/EC2",
                        AlarmName = newlyCreated.Name + "-eCTFVM-High-Status-Check-Failed-System-",
                        AlarmActions = new List<string>
                        {
                            "arn:aws:sns:ap-southeast-1:188363912800:eCTF_Notifications",
                            "arn:aws:automate:ap-southeast-1:ec2:reboot"
                        },
                        Statistic = Statistic.Maximum,
                        AlarmDescription = "Created Via Platform for " + newlyCreated.Name
                    });
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
            catch (AmazonCloudWatchException e)
            {
                return RedirectToAction("");
            }
        }

        [HttpPost]

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
                    TerminateInstancesResponse responseEC2 = await EC2Client.TerminateInstancesAsync(request);
                    if (responseEC2.HttpStatusCode == HttpStatusCode.OK)
                    {
                        DescribeAlarmsResponse responseCW = await CWClient.DescribeAlarmsAsync(new DescribeAlarmsRequest
                        {
                            AlarmNames = new List<string>
                            {
                                deleted.Name + "-eCTFVM-High-Status-Check-Failed-System-"
                            }
                        });
                        if (responseCW.HttpStatusCode == HttpStatusCode.OK && responseCW.MetricAlarms.Count == 1)
                        {
                            await CWClient.DeleteAlarmsAsync(new DeleteAlarmsRequest
                            {
                                AlarmNames = new List<string>
                                {
                                    deleted.Name + "-eCTFVM-High-Status-Check-Failed-System-"
                                }
                            });
                        }
                        Backgroundqueue.QueueBackgroundWorkItem(async token =>
                        {
                            _logger.LogInformation("Deletion of server's resources scheduled");
                            await Task.Delay(TimeSpan.FromMinutes(3), token);
                            await EC2Client.DeleteKeyPairAsync(new DeleteKeyPairRequest
                            {
                                KeyName = deleted.KeyPairName
                            });
                            await S3Client.DeleteObjectAsync(new DeleteObjectRequest
                            {
                                BucketName = "ectf-keypair",
                                Key = deleted.KeyPairName + ".pem"
                            }); if (!deleted.LinkedSubnet.LinkedVPC.AWSVPCDefaultSecurityGroup.Equals(deleted.AWSSecurityGroupReference))
                            {
                                await EC2Client.DeleteSecurityGroupAsync(new DeleteSecurityGroupRequest
                                {
                                    GroupId = deleted.AWSSecurityGroupReference
                                });
                            }
                            _logger.LogInformation("Deletion of server's resources done!");
                        });
                        _context.Servers.Remove(deleted);
                        await _context.SaveChangesAsync();
                        TempData["Result"] = "Successfully Deleted!";
                        return RedirectToAction("");
                    }
                    else
                    {
                        TempData["Result"] = "Deletion Failed!";
                        return RedirectToAction("");
                    }
                }
                catch (AmazonEC2Exception e)
                {
                    TempData["Result"] = e.Message;
                    return RedirectToAction("");
                }
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> ModifyServer(String serverID)
        {
            if (_context.VPCs.ToList().Count == 0)
            {
                return RedirectToAction("", "Home", "");
            }
            Server selected = await _context.Servers.FindAsync(Int32.Parse(serverID));
            if (selected != null)
            {
                StopInstancesResponse response = await EC2Client.StopInstancesAsync(new StopInstancesRequest
                {
                    InstanceIds = new List<string> {
                        selected.AWSEC2Reference
                }
                });
                selected.State = State.Stopping;
                _context.Servers.Update(selected);
                _context.SaveChanges();
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    return View(selected);
                }
                else
                {
                    return StatusCode(500);
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]

        public async Task<IActionResult> Modification(String ServerName, String ID, String ServerWorkload, int ServerStorage, String ServerTenancy)
        {
            Server retrieved = await _context.Servers.FindAsync(Int32.Parse(ID));
            if (retrieved == null)
            {
                return NotFound();
            }
            else
            {
                if (!retrieved.Name.Equals(ServerName))
                {
                    DeleteTagsResponse responseDeleteTag = await EC2Client.DeleteTagsAsync(new DeleteTagsRequest
                    {
                        Resources = new List<string>
                        {
                            retrieved.AWSEC2Reference
                        },
                        Tags = new List<Tag>
                        {
                            new Tag("Name")
                        }
                    });
                    if (responseDeleteTag.HttpStatusCode == HttpStatusCode.OK)
                    {
                        await EC2Client.CreateTagsAsync(new CreateTagsRequest
                        {
                            Resources = new List<string>
                        {
                            retrieved.AWSEC2Reference
                        },
                            Tags = new List<Tag>
                        {
                            new Tag("Name",ServerName)
                        }
                        });
                    }
                    retrieved.Name = ServerName;
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
                if (ServerWorkload.Equals("Low") && retrieved.Workload != Workload.Low)
                {
                    Irequest.InstanceType = "t2.micro";
                    retrieved.Workload = Workload.Low;
                }
                else if (ServerWorkload.Equals("Medium") && retrieved.Workload != Workload.Medium)
                {
                    Irequest.InstanceType = "t2.medium";
                    retrieved.Workload = Workload.Medium;
                }
                else if (ServerWorkload.Equals("Large") && retrieved.Workload != Workload.Large)
                {
                    Irequest.InstanceType = "t2.xlarge";
                    retrieved.Workload = Workload.Large;
                }
                if (retrieved.StorageAssigned != ServerStorage && ServerStorage > retrieved.StorageAssigned)
                {
                    Vrequest.Size = ServerStorage;
                    retrieved.StorageAssigned = ServerStorage;
                }
                Tenancy previous = retrieved.Tenancy;
                if (ServerTenancy.Equals("Dedicated Instance") && retrieved.Tenancy != Tenancy.DedicatedInstance)
                {
                    Prequest.Tenancy = HostTenancy.Dedicated;
                    retrieved.Tenancy = Tenancy.DedicatedInstance;
                }
                else if (ServerTenancy.Equals("Dedicated Hardware") && retrieved.Tenancy != Tenancy.DedicatedHardware)
                {
                    Prequest.Tenancy = HostTenancy.Host;
                    retrieved.Tenancy = Tenancy.DedicatedHardware;
                }
                if (Irequest.InstanceType != null)
                {
                    try
                    {
                        ModifyInstanceAttributeResponse response = await EC2Client.ModifyInstanceAttributeAsync(Irequest);
                    }
                    catch (AmazonEC2Exception)
                    {
                        TempData["Exception"] = "Failed to Modify Server Workload";
                        return RedirectToAction("");
                    }
                }
                if (Vrequest.Size != 0)
                {
                    try
                    {
                        ModifyVolumeResponse response = await EC2Client.ModifyVolumeAsync(Vrequest);
                    }
                    catch (AmazonEC2Exception)
                    {
                        TempData["Exception"] = "Failed to Modify Server Storage Size";
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
                    Backgroundqueue.QueueBackgroundWorkItem(async token =>
                    {
                        _logger.LogInformation("Scheduled Server Start");
                        await Task.Delay(TimeSpan.FromMinutes(1), token);
                        try
                        {
                            await EC2Client.StartInstancesAsync(new StartInstancesRequest
                            {
                                InstanceIds = new List<string>
                        {
                            retrieved.AWSEC2Reference
                        }
                            });
                        } catch (AmazonEC2Exception)
                        {       
                        }
                       
                    });
                    TempData["Result"] = "Modification Sucessful! Server will restart automatically within a minute";
                    return RedirectToAction("");
                }
                catch (DbUpdateConcurrencyException)
                {
                    retrieved = await _context.Servers.FindAsync(Int32.Parse(ID));
                    if (Irequest.InstanceType != null)
                    {
                        if (ServerWorkload.Equals("Low"))
                        {
                            Irequest.InstanceType = "t2.micro";
                            retrieved.Workload = Workload.Low;
                        }
                        else if (ServerWorkload.Equals("Medium"))
                        {
                            Irequest.InstanceType = "t2.medium";
                            retrieved.Workload = Workload.Medium;
                        }
                        else if (ServerWorkload.Equals("Large"))
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
                catch (AmazonEC2Exception)
                {
                    retrieved = await _context.Servers.FindAsync(Int32.Parse(ID));
                    retrieved.Tenancy = previous;
                    _context.Servers.Update(retrieved);
                    await _context.SaveChangesAsync();
                    TempData["Exception"] = "Failed to Modify Server Tenancy";
                    return RedirectToAction("");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeState(string serverID, string action)
        {
            Server retrieved = await _context.Servers.FindAsync(int.Parse(serverID));
            if (retrieved != null && action.Equals("Run"))
            {
                StartInstancesResponse response = await EC2Client.StartInstancesAsync(new StartInstancesRequest
                {
                    InstanceIds = new List<string>
                    {
                        retrieved.AWSEC2Reference
                    }
                });
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    retrieved.State = State.Starting;
                    _context.Servers.Update(retrieved);
                    await _context.SaveChangesAsync();
                    TempData["Result"] = "Server is now starting";
                    return RedirectToAction("");
                }
                else
                {
                    return StatusCode(500);
                }
            }
            else if (retrieved != null && action.Equals("Stop"))
            {
                StopInstancesResponse response = await EC2Client.StopInstancesAsync(new StopInstancesRequest
                {
                    InstanceIds = new List<string>
                    {
                        retrieved.AWSEC2Reference
                    }
                });
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    retrieved.State = State.Stopping;
                    _context.Servers.Update(retrieved);
                    await _context.SaveChangesAsync();
                    TempData["Result"] = "Server is now stopping";
                    return RedirectToAction("");
                }
                else
                {
                    return StatusCode(500);
                }
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> CreateFirewallRule(String ServerID)
        {
            if (_context.VPCs.ToList().Count == 0)
            {
                return RedirectToAction("", "Home", "");
            }
            Server modified = await _context.Servers.FindAsync(int.Parse(ServerID));
            if (modified != null)
            {
                ViewData["ServerID"] = ServerID;
                return View();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFirewallRule([Bind("ID", "Type", "Protocol", "Port", "Direction", "ServerID", "IPCIDR")]FirewallRule Rule)
        {
            Server modified = await _context.Servers.FindAsync(Rule.ServerID);
            if (modified != null)
            {
                try
                {
                    if (Rule.ID == 0)
                    {
                        if (modified.AWSSecurityGroupReference.Equals(modified.LinkedSubnet.LinkedVPC.AWSVPCDefaultSecurityGroup))
                        {
                            CreateSecurityGroupResponse responseCreateSecurityGroup = await EC2Client.CreateSecurityGroupAsync(new CreateSecurityGroupRequest
                            {
                                VpcId = modified.LinkedSubnet.LinkedVPC.AWSVPCReference,
                                GroupName = "Security Group for " + modified.Name,
                                Description = "Created via Platform"
                            });
                            await EC2Client.CreateTagsAsync(new CreateTagsRequest
                            {
                                Resources = new List<string>
                                {
                                    responseCreateSecurityGroup.GroupId
                                },
                                Tags = new List<Tag>
                                {
                                    new Tag("Name","Security Group for " + modified.Name)
                                }
                            });
                            ModifyInstanceAttributeResponse responseModifyInstanceAttribute = await EC2Client.ModifyInstanceAttributeAsync(new ModifyInstanceAttributeRequest
                            {
                                InstanceId = modified.AWSEC2Reference,
                                Groups = new List<string>
                                {
                                    responseCreateSecurityGroup.GroupId
                                }
                            });
                            if (responseModifyInstanceAttribute.HttpStatusCode == HttpStatusCode.OK)
                            {
                                modified.AWSSecurityGroupReference = responseCreateSecurityGroup.GroupId;
                                _context.Servers.Update(modified);
                                await _context.SaveChangesAsync();
                            }
                            if (Rule.Direction == Direction.Incoming)
                            {
                                AuthorizeSecurityGroupIngressRequest AuthorizeSecurityGroupIngressRequest = new AuthorizeSecurityGroupIngressRequest
                                {
                                    GroupId = responseCreateSecurityGroup.GroupId,
                                    IpPermissions = new List<IpPermission>
                                    {
                                        new IpPermission()
                                    }
                                };
                                if (Rule.Protocol == Protocol.ALL)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = "-1";
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMP)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = "icmp";
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMPv6)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = "58";
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.TCP || Rule.Protocol == Protocol.UDP)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = Rule.Port;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = Rule.Port;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = Rule.Protocol.ToString().ToLower();
                                }
                                if (Regex.IsMatch(Rule.IPCIDR, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\\/([0-9]|[1-2][0-9]|3[0-2]))$"))
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].Ipv4Ranges = new List<IpRange>
                                    {
                                        new IpRange
                                        {
                                            CidrIp = Rule.IPCIDR
                                        }
                                    };
                                }
                                else if (Regex.IsMatch(Rule.IPCIDR, "^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))$"))
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].Ipv6Ranges = new List<Ipv6Range>
                                    {
                                        new Ipv6Range
                                        {
                                            CidrIpv6 = Rule.IPCIDR
                                        }
                                    };
                                }
                                else
                                {
                                    ViewData["Exception"] = "You have entered an invaild IP CIDR";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                                AuthorizeSecurityGroupIngressResponse responseUpdateSecurityGroupIngress = await EC2Client.AuthorizeSecurityGroupIngressAsync(AuthorizeSecurityGroupIngressRequest);
                                if (responseUpdateSecurityGroupIngress.HttpStatusCode == HttpStatusCode.OK)
                                {
                                    _context.FirewallRules.Add(Rule);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    ViewData["Exception"] = "Creation Failed!";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                            }
                            else if (Rule.Direction == Direction.Outgoing)
                            {
                                AuthorizeSecurityGroupEgressRequest AuthorizeSecurityGroupEgressRequest = new AuthorizeSecurityGroupEgressRequest
                                {
                                    GroupId = responseCreateSecurityGroup.GroupId,
                                    IpPermissions = new List<IpPermission>
                                    {
                                        new IpPermission()
                                    }
                                };
                                if (Rule.Protocol == Protocol.ALL)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = "-1";
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMP)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = "icmp";
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMPv6)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = "58";
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.TCP || Rule.Protocol == Protocol.UDP)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = Rule.Port;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = Rule.Port;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = Rule.Protocol.ToString().ToLower();
                                }
                                if (Regex.IsMatch(Rule.IPCIDR, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\\/([0-9]|[1-2][0-9]|3[0-2]))$"))
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].Ipv4Ranges = new List<IpRange>
                                    {
                                        new IpRange
                                        {
                                            CidrIp = Rule.IPCIDR
                                        }
                                    };
                                }
                                else if (Regex.IsMatch(Rule.IPCIDR, "^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))$"))
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].Ipv6Ranges = new List<Ipv6Range>
                                    {
                                        new Ipv6Range
                                        {
                                            CidrIpv6 = Rule.IPCIDR
                                        }
                                    };
                                }
                                else
                                {
                                    ViewData["Exception"] = "You have entered an invaild IP CIDR";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                                AuthorizeSecurityGroupEgressResponse responseUpdateSecurityGroupEgress = await EC2Client.AuthorizeSecurityGroupEgressAsync(AuthorizeSecurityGroupEgressRequest);
                                if (responseUpdateSecurityGroupEgress.HttpStatusCode == HttpStatusCode.OK)
                                {
                                    _context.FirewallRules.Add(Rule);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    ViewData["Exception"] = "Creation Failed!";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                            }
                            else if (Rule.Direction == Direction.Both)
                            {
                                AuthorizeSecurityGroupIngressRequest AuthorizeSecurityGroupIngressRequest = new AuthorizeSecurityGroupIngressRequest
                                {
                                    GroupId = responseCreateSecurityGroup.GroupId,
                                    IpPermissions = new List<IpPermission>
                                    {
                                        new IpPermission()
                                    }
                                };
                                if (Rule.Protocol == Protocol.ALL)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = "-1";
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMP)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = "icmp";
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMPv6)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = "58";
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.TCP || Rule.Protocol == Protocol.UDP)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = Rule.Port;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = Rule.Port;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = Rule.Protocol.ToString().ToLower();
                                }
                                if (Regex.IsMatch(Rule.IPCIDR, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\\/([0-9]|[1-2][0-9]|3[0-2]))$"))
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].Ipv4Ranges = new List<IpRange>
                                    {
                                        new IpRange
                                        {
                                            CidrIp = Rule.IPCIDR
                                        }
                                    };
                                }
                                else if (Regex.IsMatch(Rule.IPCIDR, "^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))$"))
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].Ipv6Ranges = new List<Ipv6Range>
                                    {
                                        new Ipv6Range
                                        {
                                            CidrIpv6 = Rule.IPCIDR
                                        }
                                    };
                                }
                                else
                                {
                                    ViewData["Exception"] = "You have entered an invaild IP CIDR";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                                AuthorizeSecurityGroupEgressRequest AuthorizeSecurityGroupEgressRequest = new AuthorizeSecurityGroupEgressRequest
                                {
                                    GroupId = responseCreateSecurityGroup.GroupId,
                                    IpPermissions = new List<IpPermission>
                                    {
                                        new IpPermission()
                                    }
                                };
                                if (Rule.Protocol == Protocol.ALL)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = "-1";
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMP)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = "icmp";
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMPv6)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = "58";
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.TCP || Rule.Protocol == Protocol.UDP)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = Rule.Port;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = Rule.Port;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = Rule.Protocol.ToString().ToLower();
                                }
                                if (Regex.IsMatch(Rule.IPCIDR, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\\/([0-9]|[1-2][0-9]|3[0-2]))$"))
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].Ipv4Ranges = new List<IpRange>
                                    {
                                        new IpRange
                                        {
                                            CidrIp = Rule.IPCIDR
                                        }
                                    };
                                }
                                else if (Regex.IsMatch(Rule.IPCIDR, "^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))$"))
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].Ipv6Ranges = new List<Ipv6Range>
                                    {
                                        new Ipv6Range
                                        {
                                            CidrIpv6 = Rule.IPCIDR
                                        }
                                    };
                                }
                                AuthorizeSecurityGroupEgressResponse responseAuthorizeSecurityGroupEgress = await EC2Client.AuthorizeSecurityGroupEgressAsync(AuthorizeSecurityGroupEgressRequest);
                                AuthorizeSecurityGroupIngressResponse responseAuthorizeSecurityGroupIngress = await EC2Client.AuthorizeSecurityGroupIngressAsync(AuthorizeSecurityGroupIngressRequest);
                                if (responseAuthorizeSecurityGroupEgress.HttpStatusCode == HttpStatusCode.OK && responseAuthorizeSecurityGroupIngress.HttpStatusCode == HttpStatusCode.OK)
                                {
                                    _context.FirewallRules.Add(Rule);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    ViewData["Exception"] = "Creation Failed!";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                            }
                        }
                        else
                        {
                            if (Rule.Direction == Direction.Incoming)
                            {
                                AuthorizeSecurityGroupIngressRequest AuthorizeSecurityGroupIngressRequest = new AuthorizeSecurityGroupIngressRequest
                                {
                                    GroupId = modified.AWSSecurityGroupReference,
                                    IpPermissions = new List<IpPermission>
                                    {
                                        new IpPermission()
                                    }
                                };
                                if (Rule.Protocol == Protocol.ALL)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = "-1";
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMP)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = "icmp";
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMPv6)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = "58";
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.TCP || Rule.Protocol == Protocol.UDP)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = Rule.Port;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = Rule.Port;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = Rule.Protocol.ToString().ToLower();
                                }
                                if (Regex.IsMatch(Rule.IPCIDR, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\\/([0-9]|[1-2][0-9]|3[0-2]))$"))
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].Ipv4Ranges = new List<IpRange>
                                    {
                                        new IpRange
                                        {
                                            CidrIp = Rule.IPCIDR
                                        }
                                    };
                                }
                                else if (Regex.IsMatch(Rule.IPCIDR, "^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))$"))
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].Ipv6Ranges = new List<Ipv6Range>
                                    {
                                        new Ipv6Range
                                        {
                                            CidrIpv6 = Rule.IPCIDR
                                        }
                                    };
                                }
                                else
                                {
                                    ViewData["Exception"] = "You have entered an invaild IP CIDR";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                                AuthorizeSecurityGroupIngressResponse responseUpdateSecurityGroupIngress = await EC2Client.AuthorizeSecurityGroupIngressAsync(AuthorizeSecurityGroupIngressRequest);
                                if (responseUpdateSecurityGroupIngress.HttpStatusCode == HttpStatusCode.OK)
                                {
                                    _context.FirewallRules.Add(Rule);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    ViewData["Exception"] = "Creation Failed!";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                            }
                            else if (Rule.Direction == Direction.Outgoing)
                            {
                                AuthorizeSecurityGroupEgressRequest AuthorizeSecurityGroupEgressRequest = new AuthorizeSecurityGroupEgressRequest
                                {
                                    GroupId = modified.AWSSecurityGroupReference,
                                    IpPermissions = new List<IpPermission>
                                    {
                                        new IpPermission()
                                    }
                                };
                                if (Rule.Protocol == Protocol.ALL)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = "-1";
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMP)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = "icmp";
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMPv6)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = "58";
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.TCP || Rule.Protocol == Protocol.UDP)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = Rule.Port;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = Rule.Port;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = Rule.Protocol.ToString().ToLower();
                                }
                                if (Regex.IsMatch(Rule.IPCIDR, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\\/([0-9]|[1-2][0-9]|3[0-2]))$"))
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].Ipv4Ranges = new List<IpRange>
                                    {
                                        new IpRange
                                        {
                                            CidrIp = Rule.IPCIDR
                                        }
                                    };
                                }
                                else if (Regex.IsMatch(Rule.IPCIDR, "^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))$"))
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].Ipv6Ranges = new List<Ipv6Range>
                                    {
                                        new Ipv6Range
                                        {
                                            CidrIpv6 = Rule.IPCIDR
                                        }
                                    };
                                }
                                else
                                {
                                    ViewData["Exception"] = "You have entered an invaild IP CIDR";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                                AuthorizeSecurityGroupEgressResponse responseUpdateSecurityGroupEgress = await EC2Client.AuthorizeSecurityGroupEgressAsync(AuthorizeSecurityGroupEgressRequest);
                                if (responseUpdateSecurityGroupEgress.HttpStatusCode == HttpStatusCode.OK)
                                {
                                    _context.FirewallRules.Add(Rule);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    ViewData["Exception"] = "Creation Failed!";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                            }
                            else if (Rule.Direction == Direction.Both)
                            {
                                AuthorizeSecurityGroupIngressRequest AuthorizeSecurityGroupIngressRequest = new AuthorizeSecurityGroupIngressRequest
                                {
                                    GroupId = modified.AWSSecurityGroupReference,
                                    IpPermissions = new List<IpPermission>
                                    {
                                        new IpPermission()
                                    }
                                };
                                if (Rule.Protocol == Protocol.ALL)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = "-1";
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMP)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = "icmp";
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMPv6)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = "58";
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.TCP || Rule.Protocol == Protocol.UDP)
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].FromPort = Rule.Port;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].ToPort = Rule.Port;
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].IpProtocol = Rule.Protocol.ToString().ToLower();
                                }
                                if (Regex.IsMatch(Rule.IPCIDR, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\\/([0-9]|[1-2][0-9]|3[0-2]))$"))
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].Ipv4Ranges = new List<IpRange>
                                    {
                                        new IpRange
                                        {
                                            CidrIp = Rule.IPCIDR
                                        }
                                    };
                                }
                                else if (Regex.IsMatch(Rule.IPCIDR, "^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))$"))
                                {
                                    AuthorizeSecurityGroupIngressRequest.IpPermissions[0].Ipv6Ranges = new List<Ipv6Range>
                                    {
                                        new Ipv6Range
                                        {
                                            CidrIpv6 = Rule.IPCIDR
                                        }
                                    };
                                }
                                else
                                {
                                    ViewData["Exception"] = "You have entered an invaild IP CIDR";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                                AuthorizeSecurityGroupEgressRequest AuthorizeSecurityGroupEgressRequest = new AuthorizeSecurityGroupEgressRequest
                                {
                                    GroupId = modified.AWSSecurityGroupReference,
                                    IpPermissions = new List<IpPermission>
                                    {
                                        new IpPermission
                                        {
                                            FromPort = Rule.Port,
                                            ToPort = Rule.Port,
                                            IpProtocol = Rule.Protocol.ToString().ToLower()
                                        }
                                    }
                                };
                                if (Rule.Protocol == Protocol.ALL)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = "-1";
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMP)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = "icmp";
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.ICMPv6)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = "58";
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = -1;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = -1;
                                }
                                else if (Rule.Protocol == Protocol.TCP || Rule.Protocol == Protocol.UDP)
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].FromPort = Rule.Port;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].ToPort = Rule.Port;
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].IpProtocol = Rule.Protocol.ToString().ToLower();
                                }
                                if (Regex.IsMatch(Rule.IPCIDR, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\\/([0-9]|[1-2][0-9]|3[0-2]))$"))
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].Ipv4Ranges = new List<IpRange>
                                    {
                                        new IpRange
                                        {
                                            CidrIp = Rule.IPCIDR
                                        }
                                    };
                                }
                                else if (Regex.IsMatch(Rule.IPCIDR, "^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))$"))
                                {
                                    AuthorizeSecurityGroupEgressRequest.IpPermissions[0].Ipv6Ranges = new List<Ipv6Range>
                                    {
                                        new Ipv6Range
                                        {
                                            CidrIpv6 = Rule.IPCIDR
                                        }
                                    };
                                }
                                else
                                {
                                    ViewData["Exception"] = "You have entered an invaild IP CIDR";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                                AuthorizeSecurityGroupEgressResponse responseAuthorizeSecurityGroupEgress = await EC2Client.AuthorizeSecurityGroupEgressAsync(AuthorizeSecurityGroupEgressRequest);
                                AuthorizeSecurityGroupIngressResponse responseAuthorizeSecurityGroupIngress = await EC2Client.AuthorizeSecurityGroupIngressAsync(AuthorizeSecurityGroupIngressRequest);
                                if (responseAuthorizeSecurityGroupEgress.HttpStatusCode == HttpStatusCode.OK && responseAuthorizeSecurityGroupIngress.HttpStatusCode == HttpStatusCode.OK)
                                {
                                    _context.FirewallRules.Add(Rule);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    ViewData["Exception"] = "Creation Failed!";
                                    ViewData["ServerID"] = modified.ID;
                                    return View();
                                }
                            }
                        }
                        return RedirectToAction("ModifyServer", new { serverID = modified.ID });
                    }
                    else
                    {
                        return StatusCode(500);
                    }
                }
                catch (AmazonEC2Exception e)
                {
                    TempData["Exception"] = e.Message;
                    return RedirectToAction("ModifyServer", new { serverID = modified.ID });
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFirewallRule(string ID)
        {
            FirewallRule deleted = await _context.FirewallRules.FindAsync(int.Parse(ID));
            if (deleted != null)
            {
                try
                {
                    if (deleted.Direction == Direction.Incoming)
                    {
                        RevokeSecurityGroupIngressRequest requestRevokeSecurityGroupIngress = new RevokeSecurityGroupIngressRequest
                        {
                            GroupId = deleted.LinkedServer.AWSSecurityGroupReference,
                            IpPermissions = new List<IpPermission>
                            {
                                new IpPermission()
                            }
                        };
                        if (Regex.IsMatch(deleted.IPCIDR, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\\/([0-9]|[1-2][0-9]|3[0-2]))$"))
                        {
                            requestRevokeSecurityGroupIngress.IpPermissions[0].Ipv4Ranges = new List<IpRange>
                            {
                                new IpRange
                                {
                                    CidrIp = deleted.IPCIDR
                                }
                            };
                            if (deleted.Protocol == Protocol.UDP)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "udp";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = deleted.Port;
                            }
                            else if (deleted.Protocol == Protocol.TCP)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "tcp";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = deleted.Port;

                            }
                            else if (deleted.Protocol == Protocol.ICMP)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "icmp";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = -1;
                            }
                            else if (deleted.Protocol == Protocol.ICMPv6)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "58";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = -1;
                            }
                            else if (deleted.Protocol == Protocol.ALL)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "-1";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = -1;
                            }

                        }
                        else if (Regex.IsMatch(deleted.IPCIDR, "^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))$"))
                        {
                            requestRevokeSecurityGroupIngress.IpPermissions[0].Ipv6Ranges = new List<Ipv6Range>
                            {
                                new Ipv6Range
                                {
                                    CidrIpv6 = deleted.IPCIDR
                                }
                            };
                            if (deleted.Protocol == Protocol.UDP)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "udp";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = deleted.Port;
                            }
                            else if (deleted.Protocol == Protocol.TCP)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "tcp";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = deleted.Port;

                            }
                            else if (deleted.Protocol == Protocol.ICMP)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "icmp";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = -1;
                            }
                            else if (deleted.Protocol == Protocol.ICMPv6)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "58";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = -1;
                            }
                            else if (deleted.Protocol == Protocol.ALL)
                            {
                            }
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "-1";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = -1;
                            }
                        }
                        RevokeSecurityGroupIngressResponse responseRevokeSecurityGroupIngress = await EC2Client.RevokeSecurityGroupIngressAsync(requestRevokeSecurityGroupIngress);
                        if (responseRevokeSecurityGroupIngress.HttpStatusCode == HttpStatusCode.OK)
                        {
                            _context.FirewallRules.Remove(deleted);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            TempData["Exception"] = "Delete Firewall Rule Failed!";
                            return RedirectToAction("ModifyServer", new { serverID = deleted.ServerID });
                        }
                    }
                    else if (deleted.Direction == Direction.Outgoing)
                    {
                        RevokeSecurityGroupEgressRequest requestRevokeSecurityGroupEgress = new RevokeSecurityGroupEgressRequest
                        {
                            GroupId = deleted.LinkedServer.AWSSecurityGroupReference,
                            IpPermissions = new List<IpPermission>
                            {
                                new IpPermission()
                            }
                        };
                        if (Regex.IsMatch(deleted.IPCIDR, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\\/([0-9]|[1-2][0-9]|3[0-2]))$"))
                        {
                            requestRevokeSecurityGroupEgress.IpPermissions[0].Ipv4Ranges = new List<IpRange>
                            {
                                new IpRange
                                {
                                    CidrIp = deleted.IPCIDR
                                }
                            };
                            if (deleted.Protocol == Protocol.UDP)
                            {
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "udp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = deleted.Port;
                            }
                            else if (deleted.Protocol == Protocol.TCP)
                            {
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "tcp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = deleted.Port;

                            }
                            else if (deleted.Protocol == Protocol.ICMP)
                            {
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "icmp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = -1;
                            }
                            else if (deleted.Protocol == Protocol.ICMPv6)
                            {
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "58";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = -1;
                            }
                            else if (deleted.Protocol == Protocol.ALL)
                            {
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "-1";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = -1;
                            }

                        }
                        else if (Regex.IsMatch(deleted.IPCIDR, "^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))$"))
                        {
                            requestRevokeSecurityGroupEgress.IpPermissions[0].Ipv6Ranges = new List<Ipv6Range>
                            {
                                new Ipv6Range
                                {
                                    CidrIpv6 = deleted.IPCIDR
                                }
                            };
                            if (deleted.Protocol == Protocol.UDP)
                            {
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "udp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = deleted.Port;
                            }
                            else if (deleted.Protocol == Protocol.TCP)
                            {
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "tcp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = deleted.Port;

                            }
                            else if (deleted.Protocol == Protocol.ICMP)
                            {
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "icmp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = -1;
                            }
                            else if (deleted.Protocol == Protocol.ICMPv6)
                            {
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "58";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = -1;
                            }
                            else if (deleted.Protocol == Protocol.ALL)
                            {
                            }
                            {
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "-1";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = -1;
                            }
                        }
                        RevokeSecurityGroupEgressResponse responseRevokeSecurityGroupEgress = await EC2Client.RevokeSecurityGroupEgressAsync(requestRevokeSecurityGroupEgress);
                        if (responseRevokeSecurityGroupEgress.HttpStatusCode == HttpStatusCode.OK)
                        {
                            _context.FirewallRules.Remove(deleted);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            TempData["Exception"] = "Delete Firewall Rule Failed!";
                            return RedirectToAction("ModifyServer", new { serverID = deleted.ServerID });
                        }
                    }
                    else if (deleted.Direction == Direction.Both)
                    {
                        RevokeSecurityGroupIngressRequest requestRevokeSecurityGroupIngress = new RevokeSecurityGroupIngressRequest
                        {
                            GroupId = deleted.LinkedServer.AWSSecurityGroupReference,
                            IpPermissions = new List<IpPermission>
                            {
                                new IpPermission()
                            }
                        };
                        RevokeSecurityGroupEgressRequest requestRevokeSecurityGroupEgress = new RevokeSecurityGroupEgressRequest
                        {
                            GroupId = deleted.LinkedServer.AWSSecurityGroupReference,
                            IpPermissions = new List<IpPermission>
                            {
                                new IpPermission()
                            }
                        };
                        if (Regex.IsMatch(deleted.IPCIDR, "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\\/([0-9]|[1-2][0-9]|3[0-2]))$"))
                        {
                            requestRevokeSecurityGroupIngress.IpPermissions[0].Ipv4Ranges = new List<IpRange>
                            {
                                new IpRange
                                {
                                    CidrIp = deleted.IPCIDR
                                }
                            };
                            requestRevokeSecurityGroupEgress.IpPermissions[0].Ipv4Ranges = new List<IpRange>
                            {
                                new IpRange
                                {
                                    CidrIp = deleted.IPCIDR
                                }
                            };
                            if (deleted.Protocol == Protocol.UDP)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "udp";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = deleted.Port;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "udp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = deleted.Port;
                            }
                            else if (deleted.Protocol == Protocol.TCP)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "tcp";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = deleted.Port;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "tcp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = deleted.Port;

                            }
                            else if (deleted.Protocol == Protocol.ICMP)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "icmp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "icmp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = -1;
                            }
                            else if (deleted.Protocol == Protocol.ICMPv6)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "58";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "58";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = -1;
                            }
                            else if (deleted.Protocol == Protocol.ALL)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "-1";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "-1";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = -1;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = -1;
                            }

                        }
                        else if (Regex.IsMatch(deleted.IPCIDR, "^s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]d|1dd|[1-9]?d)(.(25[0-5]|2[0-4]d|1dd|[1-9]?d)){3}))|:)))(%.+)?s*(\\/([0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]))$"))
                        {
                            requestRevokeSecurityGroupIngress.IpPermissions[0].Ipv6Ranges = new List<Ipv6Range>
                            {
                                new Ipv6Range
                                {
                                    CidrIpv6 = deleted.IPCIDR
                                }
                            };
                            requestRevokeSecurityGroupEgress.IpPermissions[0].Ipv6Ranges = new List<Ipv6Range>
                            {
                                new Ipv6Range
                                {
                                    CidrIpv6 = deleted.IPCIDR
                                }
                            };
                            if (deleted.Protocol == Protocol.UDP)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "udp";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = deleted.Port;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "udp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = deleted.Port;
                            }
                            else if (deleted.Protocol == Protocol.TCP)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "tcp";
                                requestRevokeSecurityGroupIngress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupIngress.IpPermissions[0].ToPort = deleted.Port;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "tcp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].FromPort = deleted.Port;
                                requestRevokeSecurityGroupEgress.IpPermissions[0].ToPort = deleted.Port;

                            }
                            else if (deleted.Protocol == Protocol.ICMP)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "icmp";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "icmp";
                            }
                            else if (deleted.Protocol == Protocol.ICMPv6)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "58";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "58";
                            }
                            else if (deleted.Protocol == Protocol.ALL)
                            {
                                requestRevokeSecurityGroupIngress.IpPermissions[0].IpProtocol = "-1";
                                requestRevokeSecurityGroupEgress.IpPermissions[0].IpProtocol = "-1";
                            }
                        }
                        RevokeSecurityGroupIngressResponse responseRevokeSecurityGroupIngress = await EC2Client.RevokeSecurityGroupIngressAsync(requestRevokeSecurityGroupIngress);
                        RevokeSecurityGroupEgressResponse responseRevokeSecurityGroupEgress = await EC2Client.RevokeSecurityGroupEgressAsync(requestRevokeSecurityGroupEgress);
                        if (responseRevokeSecurityGroupIngress.HttpStatusCode == HttpStatusCode.OK && responseRevokeSecurityGroupEgress.HttpStatusCode == HttpStatusCode.OK)
                        {
                            _context.FirewallRules.Remove(deleted);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            TempData["Exception"] = "Delete Firewall Rule Failed!";
                            return RedirectToAction("ModifyServer", new { serverID = deleted.ServerID });
                        }
                    }
                    TempData["Result"] = "Firewall Rule Deleted Sucessfully!";
                    return RedirectToAction("ModifyServer", new { serverID = deleted.ServerID });
                }
                catch (AmazonEC2Exception e)
                {
                    TempData["Exception"] = "Delete Firewall Rule Failed! " + e.Message;
                    return RedirectToAction("ModifyServer", new { serverID = deleted.ServerID });
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