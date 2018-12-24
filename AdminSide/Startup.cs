using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminSide.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.EC2;
using Amazon.CloudWatch;
using Amazon.SimpleNotificationService;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchEvents;
using Microsoft.AspNetCore.Identity.UI.Services;
using Amazon.RDS;
using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticBeanstalk;
using AdminSide.Areas.PlatformManagement.Data;

namespace AdminSide
{
    public class Startup
    {
        /*
       public Startup(IConfiguration configuration)
       {
           Configuration = configuration;
       }
       */

        //Custom Startup Scripts for EBS and RDS

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile(@"C:\Program Files\Amazon\ElasticBeanstalk\config\containerconfiguration", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            // This adds EB environment variables.
            builder.AddInMemoryCollection(GetAwsDbConfig(builder.Build()));
            Configuration = builder.Build();
        }

        private Dictionary<string, string> GetAwsDbConfig(IConfiguration configuration)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (IConfigurationSection pair in configuration.GetSection("iis:env").GetChildren())
            {
                string[] keypair = pair.Value.Split(new[] { '=' }, 2);
                dict.Add(keypair[0], keypair[1]);
            }

            return dict;
        }

        //For ASP.NET Identity Only | You can reuse this but replace dbname with your own database name and change method name
        private string GetRdsConnectionStringIdentity()
        {
            string hostname = Configuration.GetValue<string>("RDS_HOSTNAME");
            string port = Configuration.GetValue<string>("RDS_PORT");
            string dbname = "ASPNETIdentityAdmin";
            string username = Configuration.GetValue<string>("RDS_USERNAME");
            string password = Configuration.GetValue<string>("RDS_PASSWORD");

            return $"Data Source={hostname},{port};Initial Catalog={dbname};User ID={username};Password={password};";
        }

        //For Competition Only
        private string GetRdsConnectionStringCompetition()
        {
            string hostname = Configuration.GetValue<string>("RDS_HOSTNAME");
            string port = Configuration.GetValue<string>("RDS_PORT");
            string dbname = "Competition";
            string username = Configuration.GetValue<string>("RDS_USERNAME");
            string password = Configuration.GetValue<string>("RDS_PASSWORD");

            return $"Data Source={hostname},{port};Initial Catalog={dbname};User ID={username};Password={password};";
        }

        //For Platform Resources Only
        private string GetRdsConnectionStringPlatformResources()
        {
            string hostname = Configuration.GetValue<string>("RDS_HOSTNAME");
            string port = Configuration.GetValue<string>("RDS_PORT");
            string dbname = "PlatformResources";
            string username = Configuration.GetValue<string>("RDS_USERNAME");
            string password = Configuration.GetValue<string>("RDS_PASSWORD");

            return $"Data Source={hostname},{port};Initial Catalog={dbname};User ID={username};Password={password};";
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //Identity Db Context
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
            GetRdsConnectionStringIdentity()));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //Competition Db Context
            services.AddDbContext<CompetitionContext>(options =>
            options.UseSqlServer(
            GetRdsConnectionStringCompetition()));

            //Platform Resources Db Context
            services.AddDbContext<PlatformResourcesContext>(options =>
            options.UseSqlServer(
            GetRdsConnectionStringPlatformResources()));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
        .AddRazorPagesOptions(options =>
        {
            options.AllowAreas = true;
            options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
            options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
        });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });

            // using Microsoft.AspNetCore.Identity.UI.Services;
            services.AddSingleton<IEmailSender, EmailSender>();

            //Core AWS Initialization
            var awsOptions = Configuration.GetAWSOptions();
            awsOptions.Credentials = new EnvironmentVariablesAWSCredentials();
            services.AddDefaultAWSOptions(awsOptions);
            //S3 Initialization
            services.AddAWSService<IAmazonS3>();
            //EC2 and VPC Initialization
            services.AddAWSService<IAmazonEC2>();
            //Cloudwatch Initialization
            services.AddAWSService<IAmazonCloudWatch>();
            services.AddAWSService<IAmazonCloudWatchLogs>();
            services.AddAWSService<IAmazonCloudWatchEvents>();
            //SNS Initialization
            services.AddAWSService<IAmazonSimpleNotificationService>();
            //RDS Initialization
            services.AddAWSService<IAmazonRDS>();
            //ELB Initialization
            services.AddAWSService<IAmazonElasticLoadBalancingV2>();
            //EBS Initialization
            services.AddAWSService<IAmazonElasticBeanstalk>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                //Investigate need for this extra configuration
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Task.CompletedTask;
        }
    }
}
