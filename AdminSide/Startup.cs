﻿using System;
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

        //For ASP.NET Identity Only | You can reuse this but replace dbname with your own database name
        private string GetRdsConnectionString()
        {
            string hostname = Configuration.GetValue<string>("RDS_HOSTNAME");
            string port = Configuration.GetValue<string>("RDS_PORT");
            string dbname = "ASPNETIdentityAdmin";
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

            //Using RDS
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
            GetRdsConnectionString()));

            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //Core AWS Initialization
            var awsOptions = Configuration.GetAWSOptions();
            awsOptions.Credentials = new EnvironmentVariablesAWSCredentials();
            services.AddDefaultAWSOptions(awsOptions);
            //S3 Initialization
            IAmazonS3 s3Client = awsOptions.CreateServiceClient<IAmazonS3>();
            services.AddAWSService<IAmazonS3>();
            //EC2 and VPC Initialization
            IAmazonEC2 ec2Client = awsOptions.CreateServiceClient<IAmazonEC2>();
            services.AddAWSService<IAmazonEC2>();
            //Cloudwatch Initialization
            IAmazonCloudWatch cloudwatchClient = awsOptions.CreateServiceClient<IAmazonCloudWatch>();
            services.AddAWSService<IAmazonCloudWatch>();
            IAmazonCloudWatchLogs cloudwatchLogsClient = awsOptions.CreateServiceClient<IAmazonCloudWatchLogs>();
            services.AddAWSService<IAmazonCloudWatchLogs>();
            //SNS Initialization
            IAmazonSimpleNotificationService snsClient = awsOptions.CreateServiceClient<IAmazonSimpleNotificationService>();
            services.AddAWSService<IAmazonSimpleNotificationService>();
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
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
