using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class ServerTemplateCreationFormModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "A template must have a name")]
        [Display(Name = "Template Name")]
        public string Name { get; set; }

        [Display(Name = "Template Description")]
        public string TemplateDescription { get; set; }

        public string serverID { get; set; }
    }
}
