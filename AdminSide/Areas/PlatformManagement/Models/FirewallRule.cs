﻿namespace AdminSide.Areas.PlatformManagement.Models
{
    public enum Type
    {
        IMPLICT_DENY,CUSTOM,HTTP,HTTPS,SSH,Telnet,FTP
    }

    public enum Protocol
    {
        TCP,UDP
    }

    public enum Direction
    {
        Incoming,Outgoing,Both
    }

    public class FirewallRule
    {
        public int ID { get; set; }

        public Type Type { get; set; }
        public Protocol Protocol { get; set; }
        public int Port { get; set; }
        public Direction Direction { get; set; }
        public int ServerID { get; set; }
        public Server LinkedServer { get; set; }
    }
}
