using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AdminSide.Data
{
    public class NewsFeedFactory : IDesignTimeDbContextFactory<NewsFeedContext>
    {
        public NewsFeedContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string hostname = configuration.GetValue<string>("RDS_HOSTNAME");
            string port = configuration.GetValue<string>("RDS_PORT");
            string dbname = "RSSFeedDb";
            string username = configuration.GetValue<string>("RDS_USERNAME");
            string password = configuration.GetValue<string>("RDS_PASSWORD");
            string connectionString = $"Data Source={hostname},{port};Initial Catalog={dbname};User ID={username};Password={password};";

            var builder = new DbContextOptionsBuilder<NewsFeedContext>();

            builder.UseSqlServer(connectionString);

            return new NewsFeedContext(builder.Options);
        }
    }
}