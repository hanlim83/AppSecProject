namespace AdminSide.Areas.PlatformManagement.Models
{
    public class ChallengeServersModificationFormModel
    {
        public string ServerName { get; set; }
        public string ServerWorkload { get; set; }
        public int ServerStorage { get; set; }
        public string ServerTenancy { get; set; }
        public string ID { get; set; }
    }
}
