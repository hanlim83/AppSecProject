using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class ENILog
    {
        public DateTime TimeStamp { get; set; }
        public int Version { get; set; }
        public long AccountID { get; set; }
        public string Interface { get;set;}
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public string SourcePort { get; set; }
        public string DestinationPort { get; set; }
        public string Protocol { get; set; }
        public int Packets { get; set; }
        public int Bytes { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public string Action { get; set; }
        public string LogAction { get; set; }
    }
}
