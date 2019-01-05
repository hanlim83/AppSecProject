﻿namespace AdminSide.Areas.PlatformManagement.Models
{
    public class ChallengeServersCreationFormModel
    {
        public string ServerName { get; set; }
        public string ServerWorkload { get; set; }
        public int ServerStorage { get; set; }
        public string ServerTenancy { get; set; }
        public Subnet ServerSubnet { get; set; }
        public Template selectedTemplate { get; set; }
        public string TemplateName { get; set; }
        public string OperatingSystem { get; set; }
        public string SubnetCIDR { get; set; }
        public string TemplateID { get; set; }
        public string SubnetID { get; set; }
    }
}
