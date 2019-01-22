using System.ComponentModel.DataAnnotations;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public enum Type
    {
        IMPLICT_DENY,CUSTOM,HTTP,HTTPS,SSH,Telnet,FTP,ALL,ICMP,ICMPv6
    }

    public enum Protocol
    {
        TCP,UDP,ICMP,ALL,ICMPv6
    }

    public enum Direction
    {
        Incoming,Outgoing,Both
    }

    public class FirewallRule
    {
        public int ID { get; set; }

        [Required]
        public Type Type { get; set; }
        [Required]
        public Protocol Protocol { get; set; }
        public int Port { get; set; }
        [Required]
        public Direction Direction { get; set; }
        public int ServerID { get; set; }
        [Required]
        [Display(Name = "IP CIDR")]
        public string IPCIDR { get; set; }
        public virtual Server LinkedServer { get; set; }
    }
}
