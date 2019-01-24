using System;
using System.Collections.Generic;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class CloudWatchLogGroup
    {
        public int ID { get; set; }
        public string ARN { get; set; }
        public DateTime CreationTime { get; set; }
        public string Name { get; set; }
        public int RetentionDuration { get; set; }

        public virtual ICollection<CloudWatchLogStream> LogStreams { get; set; }
    }
}
