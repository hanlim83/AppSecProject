using AdminSide.Areas.Identity.Services;
using AdminSide.Areas.PlatformManagement.Data;
using AdminSide.Areas.PlatformManagement.Services;
using AdminSide.Data;
using Amazon.CloudWatch;
using Amazon.CloudWatchEvents;
using Amazon.CloudWatchLogs;
using Amazon.EC2;
using Amazon.GuardDuty;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AdminSide
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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

        // For FORUM
        private string GetRdsConnectionStringForum()
        {
            string hostname = Configuration.GetValue<string>("RDS_HOSTNAME");
            string port = Configuration.GetValue<string>("RDS_PORT");
            string dbname = "Forum";
            string username = Configuration.GetValue<string>("RDS_USERNAME");
            string password = Configuration.GetValue<string>("RDS_PASSWORD");

            return $"Data Source={hostname},{port};Initial Catalog={dbname};User ID={username};Password={password};";
        }

        // For NewsFeed
        private string GetRdsConnectionStringNewsFeed()
        {
            string hostname = Configuration.GetValue<string>("RDS_HOSTNAME");
            string port = Configuration.GetValue<string>("RDS_PORT");
            string dbname = "RSSFeedDb";
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

            //Using Password hasher V3
            services.Configure<PasswordHasherOptions>(
                 o => o.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3);

            //Identity Db Context
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
            GetRdsConnectionStringIdentity()));

            //services.AddIdentity<IdentityUser, IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();

            //Competition Db Context
            services.AddDbContext<CompetitionContext>(options =>
            options.UseSqlServer(
            GetRdsConnectionStringCompetition()));

            //Platform Resources Db Context
            services.AddDbContext<PlatformResourcesContext>(options =>
            {
                options.UseLazyLoadingProxies().UseSqlServer(GetRdsConnectionStringPlatformResources(),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            });

            //Forum Db Context
            services.AddDbContext<ForumContext>(options =>
            options.UseSqlServer(
            GetRdsConnectionStringForum()));

            //NewsFeed Db Context
            services.AddDbContext<NewsFeedContext>(options =>
            options.UseSqlServer(
            GetRdsConnectionStringNewsFeed()));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            .AddRazorPagesOptions(options =>
            {
                options.AllowAreas = true;
                options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
            })
            .AddMvcOptions(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });

            //// using Microsoft.AspNetCore.Identity.UI.Services;
            //services.AddSingleton<IEmailSender, EmailSender>();

            // requires
            services.AddSingleton<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);

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
            //GuardDuty Initialization
            services.AddAWSService<IAmazonGuardDuty>();

            //Background Processing
            services.AddHostedService<ConsumeScopedServicesHostedService>();
            services.AddScoped<IScopedUpdatingService, ScopedUpdatingService>();
            services.AddScoped<IScopedSetupService, ScopedSetupService>();
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
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

            //Testing programtically migrating DB
            //using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            //{
            //    var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //    context.Database.Migrate();
            //}

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

    //public class EmailSender : IEmailSender
    //{
    //    public Task SendEmailAsync(string email, string subject, string message)
    //    {
    //        return Task.CompletedTask;
    //    }
    //}
}
