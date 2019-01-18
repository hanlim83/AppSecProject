using System;
using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AdminSide
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
                    DbInitializer.InitializePlatformResources(contextPR);
                    DbInitializer.InitializeCompetitions(contextC);
                    DbInitializer.InitializeForum(contextF);
                    DbInitializer.InitializeNewsFeed(contextNF);
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
    }
}
