using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class CloudWatchLogStream
    {
        public int ID { get; set; }
        public string ARN { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime FirstEventTime { get; set; }
        public DateTime LastEventTime { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public int LinkedGroupID { get; set; }
        public virtual CloudWatchLogGroup LinkedGroup { get; set; }
    }
}
