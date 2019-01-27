using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace AdminSide
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SetEbConfig();
            var host = CreateWebHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var contextPR = services.GetRequiredService<PlatformResourcesContext>();
                    var contextC = services.GetRequiredService<CompetitionContext>();
                    var contextF = services.GetRequiredService<ForumContext>();
                    var contextNF = services.GetRequiredService<NewsFeedContext>();
                    var contextIdentity = services.GetRequiredService<ApplicationDbContext>();
                    try
                    {
                        DbInitializer.InitializePlatformResources(contextPR);
                    }
                    catch (SqlException ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred creating the DB for Platform Manangement");
                    }
                    try
                    {
                        DbInitializer.InitializeCompetitions(contextC);
                    }
                    catch (SqlException ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred creating the DB for Competition");
                    }
                    try
                    {
                        DbInitializer.InitializeForum(contextF);
                    }
                    catch (SqlException ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred creating the DB for Forum");
                    }
                    try
                    {
                        DbInitializer.InitializeNewsFeed(contextNF);
                    }
                    catch (SqlException ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred creating the DB for NewsFeed");
                    }
                    try
                    {
                        DbInitializer.InitializeIdentity(contextIdentity);
                    }
                    catch (SqlException ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred creating the DB for Identity");
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating the DB for one or more services!");
                }
            }
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseSetting(WebHostDefaults.DetailedErrorsKey, "true")
                .UseStartup<Startup>();

        private static void SetEbConfig()
        {
            var tempConfigBuilder = new ConfigurationBuilder();

            tempConfigBuilder.AddJsonFile(
                @"C:\Program Files\Amazon\ElasticBeanstalk\config\containerconfiguration",
                optional: true,
                reloadOnChange: true
            );

            var configuration = tempConfigBuilder.Build();

            var ebEnv =
                configuration.GetSection("iis:env")
                    .GetChildren()
                    .Select(pair => pair.Value.Split(new[] { '=' }, 2))
                    .ToDictionary(keypair => keypair[0], keypair => keypair[1]);

            foreach (var keyVal in ebEnv)
            {
                Environment.SetEnvironmentVariable(keyVal.Key, keyVal.Value);
            }
        }
    }
}
