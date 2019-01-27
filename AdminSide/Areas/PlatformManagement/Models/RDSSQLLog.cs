using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class RDSSQLLog
    {
        public DateTime LogDate { get; set; }
        public String ProcessInfo { get; set; }
        public String Text { get; set; }
    }
}
