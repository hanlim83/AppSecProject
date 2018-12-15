using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class Server
    {
        public int ID { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "A Server must have a name")]
        [Display(Name = "Server Name")]
        public string Name { get; set; }
        [Display(Name = "Date Created")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateCreated { get; set; }
        [Display(Name = "Operating System")]
        public string OperatingSystem { get; set; }
        public string AWSEC2Reference { get; set; }
        public Subnet LinkedSubnet { get; set; }
    }
}
