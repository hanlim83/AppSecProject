using System;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class VPC
    {
        public int ID { get; set; }
        public string AWSVPCReference { get; set; }
        public string AWSVPCDefaultSecurityGroup { get; set; }
    }
}
