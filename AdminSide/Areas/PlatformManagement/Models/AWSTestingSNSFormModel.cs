using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class AWSTestingSNSFormModel
    {
        [DisplayName("Display ID")]
        [Required]
        public string DisplayID { get; set; }

        [Required]
        public string Number { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
