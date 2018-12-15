using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AdminSide.Areas.PlatformManagement.Data
{
    public class PlatformResourcesFactory : IDesignTimeDbContextFactory<PlatformResourcesContext>
    {
        public PlatformResourcesContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string hostname = configuration.GetValue<string>("RDS_HOSTNAME");
            string port = configuration.GetValue<string>("RDS_PORT");
            string dbname = "PlatformResources";
            string username = configuration.GetValue<string>("RDS_USERNAME");
            string password = configuration.GetValue<string>("RDS_PASSWORD");
            string connectionString = $"Data Source={hostname},{port};Initial Catalog={dbname};User ID={username};Password={password};";

            var builder = new DbContextOptionsBuilder<PlatformResourcesContext>();
           
            builder.UseSqlServer(connectionString);

            return new PlatformResourcesContext(builder.Options);
        }
    }
}
