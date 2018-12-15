using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
