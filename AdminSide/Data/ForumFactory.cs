using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AdminSide.Data
{
    public class ForumFactory : IDesignTimeDbContextFactory<ForumContext>
    {
        public ForumContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string hostname = configuration.GetValue<string>("RDS_HOSTNAME");
            string port = configuration.GetValue<string>("RDS_PORT");
            string dbname = "Forum";
            string username = configuration.GetValue<string>("RDS_USERNAME");
            string password = configuration.GetValue<string>("RDS_PASSWORD");
            string connectionString = $"Data Source={hostname},{port};Initial Catalog={dbname};User ID={username};Password={password};";

            var builder = new DbContextOptionsBuilder<ForumContext>();

            builder.UseSqlServer(connectionString);

            return new ForumContext(builder.Options);
        }
    }
}
