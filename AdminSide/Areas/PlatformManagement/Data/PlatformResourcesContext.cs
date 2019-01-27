using Microsoft.EntityFrameworkCore;
using AdminSide.Areas.PlatformManagement.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AdminSide.Areas.PlatformManagement.Data
{
    public class PlatformResourcesContext : DbContext
    {
        public PlatformResourcesContext(DbContextOptions<PlatformResourcesContext> options) : base(options)
        {

        }
        public DbSet<Route> Routes { get; set; }
        public DbSet<RouteTable> RouteTables { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Subnet> Subnets { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<FirewallRule> FirewallRules { get; set; }
        public DbSet<VPC> VPCs { get; set; }
        public DbSet<CloudWatchLogGroup> CloudWatchLogGroups { get; set; }
        public DbSet<CloudWatchLogStream> CloudWatchLogStreams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Route>().ToTable("Routes");
            modelBuilder.Entity<RouteTable>().ToTable("RouteTables");
            modelBuilder.Entity<Server>().ToTable("Servers");
            modelBuilder.Entity<Subnet>().ToTable("Subnets");
            modelBuilder.Entity<Template>().ToTable("Templates");
            modelBuilder.Entity<FirewallRule>().ToTable("FirewallRules");
            modelBuilder.Entity<VPC>().ToTable("VPCs");
            modelBuilder.Entity<CloudWatchLogGroup>().ToTable("CloudWatchLogGroups");
            modelBuilder.Entity<CloudWatchLogStream>().ToTable("CloudWatchLogStreams");
            modelBuilder.Query<RDSSQLLog>();
        }

        public async Task<List<RDSSQLLog>> GetRDSLogs()
        {
            List<RDSSQLLog> getLogs = await this.Query<RDSSQLLog>().FromSql("EXEC rdsadmin.dbo.rds_read_error_log @index = 0, @type = 1;").ToListAsync();
            return getLogs;
        }
    }
}
