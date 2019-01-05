using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class AWSTestingSubnetFormModel
    {
        [DisplayName("IP CIDR")]
        [Required]
        public string IPCIDR { get; set; }
    }
}
