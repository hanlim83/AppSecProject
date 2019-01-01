using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public enum Visibility
    {
        Internet, Extranet, Intranet
    }

    public class Server
    {
        public int ID { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "A Server must have a name")]
        [Display(Name = "Server Name")]
        public string Name { get; set; }
        public string Visibility { get; set; }
        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateCreated { get; set; }
        [Display(Name = "Operating System")]
        public string OperatingSystem { get; set; }
        [Display(Name = "Server IP Address")]
        public string IPAddress { get; set; }
        [Display(Name = "Server DNS Hostname")]
        public string DNSHostname { get; set; }
        [Display(Name = "Server Storage Space")]
        public int StorageAssigned { get; set; }
        public string AWSEC2Reference { get; set; }

        public int SubnetID { get; set; }
        public Subnet LinkedSubnet { get; set; }

        public ICollection<FirewallRule> FirewallRules { get; set; }
    }
}
