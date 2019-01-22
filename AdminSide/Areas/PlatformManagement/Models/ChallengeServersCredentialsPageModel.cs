using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public class ChallengeServersCredentialsPageModel
    {
        public Boolean Windows { get; set; }
        public Boolean Available { get; set; }
        public string keyPairDownloadURL { get; set; }
        public string retrievedPassword { get; set; }
        public string serverIP { get; set; }
        public string serverDNS { get; set; }
        public string serverName { get; set; }
    }
}
