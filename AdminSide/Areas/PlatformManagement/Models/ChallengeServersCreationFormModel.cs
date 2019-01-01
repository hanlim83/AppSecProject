using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class ChallengeServersCreationFormModel
    {
        public string ServerName { get; set; }
        public string ServerWorkload { get; set; }
        public int ServerStorage { get; set; }
        public string ServerTenancy { get; set; }
        public Subnet ServerSubnet { get; set; }
        public string selectedTemplate { get; set; }
        public string TemplateName { get; set; }
        public string OperatingSystem { get; set; }
    }
}
