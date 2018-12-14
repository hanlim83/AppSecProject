using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminSide.Models;

namespace AdminSide.Data
{
    public class PlatformResourcesContext : DbContext
    {
        public PlatformResourcesContext(DbContextOptions<PlatformResourcesContext> options) : base(options)
        {

        }

        public DbSet<Route> Routes { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Subnet> Subnets { get; set; }
        public DbSet<Template> Templates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Route>().ToTable("Route");
            modelBuilder.Entity<Server>().ToTable("Server");
            modelBuilder.Entity<Subnet>().ToTable("Subnet");
            modelBuilder.Entity<Template>().ToTable("Template");
        }
    }
}
