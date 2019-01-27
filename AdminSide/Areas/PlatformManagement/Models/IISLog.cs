using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class IISLog
    {
        public DateTime TimeStamp { get; set; }
        public string ServerIP { get; set; }
        public string HTTPMethod { get; set; }
        public string Path { get; set; }
        public string Query { get; set; }
        public int DestPort { get; set; }
        public string AuthenticatedUser { get; set; }
        public string SourceIP { get; set; }
        public string SourceUserAgent { get; set; }
        public string Referer { get; set; }
        public int HTTPStatusCode { get; set; }
        public int SubstatusCode { get; set; }
        public int Win32StatusCode { get; set; }
        public int Duration { get; set; }
    }
}
