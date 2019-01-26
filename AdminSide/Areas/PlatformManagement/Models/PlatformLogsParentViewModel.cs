using Amazon.CloudWatchLogs.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class PlatformLogsParentViewModel
    {
        public List<CloudWatchLogStream>Streams { get; set; }
        public GetLogEventsResponse Response { get; set; }
        public int SelectedValue { get; set; }
        public List<RDSSQLLog> SQLlogs { get; set; }
        public List<IISLog>IISLogs { get; set; }
    }
}
