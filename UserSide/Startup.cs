using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserSide.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.EC2;
using Amazon.CloudWatch;
using Amazon.CloudWatchLogs;
using Amazon.SimpleNotificationService;
using Amazon.CloudWatchEvents;
using Amazon.RDS;
using UserSide.Areas.Identity.Services;
using UserSide.Hubs;
using Amazon.Route53Domains;
using Microsoft.AspNetCore.SignalR;

namespace UserSide
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        //For ASP.NET Identity Only | You can reuse this but replace dbname with your own database name
        private string GetRdsConnectionString()
        {
            string hostname = Configuration.GetValue<string>("RDS_HOSTNAME");
            string port = Configuration.GetValue<string>("RDS_PORT");
            string dbname = "ASPNETIdentityUser";
            string username = Configuration.GetValue<string>("RDS_USERNAME");
            string password = Configuration.GetValue<string>("RDS_PASSWORD");

            return $"Data Source={hostname},{port};Initial Catalog={dbname};User ID={username};Password={password};";
        }

        private string GetRdsConnectionStringCompetition()
        {
            string hostname = Configuration.GetValue<string>("RDS_HOSTNAME");
            string port = Configuration.GetValue<string>("RDS_PORT");
            string dbname = "Competition";
            string username = Configuration.GetValue<string>("RDS_USERNAME");
            string password = Configuration.GetValue<string>("RDS_PASSWORD");

            return $"Data Source={hostname},{port};Initial Catalog={dbname};User ID={username};Password={password};";
        }

        private string GetRdsConnectionStringForum()
        {
            string hostname = Configuration.GetValue<string>("RDS_HOSTNAME");
            string port = Configuration.GetValue<string>("RDS_PORT");
            string dbname = "Forum";
            string username = Configuration.GetValue<string>("RDS_USERNAME");
            string password = Configuration.GetValue<string>("RDS_PASSWORD");

            return $"Data Source={hostname},{port};Initial Catalog={dbname};User ID={username};Password={password};";
        }

        private string GetRdsConnectionStringChat()
        {
            string hostname = Configuration.GetValue<string>("RDS_HOSTNAME");
            string port = Configuration.GetValue<string>("RDS_PORT");
            string dbname = "Chat";
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

            //Using RDS
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
            GetRdsConnectionString()));

            //services.AddIdentity<IdentityUser, IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();

            //Competition Db Context
            services.AddDbContext<CompetitionContext>(options =>
            options.UseSqlServer(
            GetRdsConnectionStringCompetition()));

            //Forum Db Context
            services.AddDbContext<ForumContext>(options =>
            options.UseSqlServer(
            GetRdsConnectionStringForum()));

            //Chat DB Context
            services.AddDbContext<ChatContext>(options =>
            options.UseSqlServer(
            GetRdsConnectionStringChat()));

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
            //RDS Initialization
            services.AddAWSService<IAmazonRDS>();
            //Route53 Initialization
            services.AddAWSService<IAmazonRoute53Domains>();



            //Chat Signalr
            services.AddSignalR();


            //User 
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
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

            //Signalr app 
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chatHub");
            });

            

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.Use(async (context, next) =>
            {
                var hubContext = context.RequestServices
                                        .GetRequiredService<IHubContext<ChatHub>>();
                //...
            });

        }
    }
}