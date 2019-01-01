using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    public enum Type
    {
        IMPLICT_DENY,CUSTOM,HTTP,HTTPS,SSH,Telnet,FTP
    }

    public enum Protocol
    {
        TCP,UDP
    }


    public class FirewallRule
    {
        public Type Type;
        public Protocol Protocol;
        public int Port;

        public int ServerID { get; set; }
        public Server LinkedServer;
    }
}
