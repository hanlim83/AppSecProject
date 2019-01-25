using System;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public enum VPCType
    {
        DefaultVM,Platform,VM
    }
    public class VPC
    {
        public int ID { get; set; }
        public string AWSVPCReference { get; set; }
        public string AWSVPCDefaultSecurityGroup { get; set; }
        public VPCType type { get; set; }
        public string BaseIPv4CIDR { get; set; }
        public string BaseIPv6CIDR { get; set; }
    }
}
