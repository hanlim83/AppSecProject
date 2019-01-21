using System;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public enum VPCType
    {
        Platform,DefaultVM,VM
    }
    public class VPC
    {
        public int ID { get; set; }
        public string AWSVPCReference { get; set; }
        public string AWSVPCDefaultSecurityGroup { get; set; }
        public VPCType type { get; set; }
    }
}
