using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;

namespace Smnetcore.Rene.Webhost
{
    internal class StartupRene
    {
          public IHostingEnvironment Environment { get; }
          public static IConfiguration ReneConfig { get; private set; }
        private readonly Serilog.ILogger Logger;
          
        public StartupRene(IConfiguration config, IHostingEnvironment environment, Serilog.ILogger logger)
        {
         
            ReneConfig = config;
            Environment = Environment;
            Logger = logger;
        
           

        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services, Serilog.ILogger Logger)
        {
        //services.AddTransient<Serilog.ILogger, Logger>();
            string connectionString = ReneConfig.GetConnectionString("DefaultConnection");
            services.AddOptions();
            Logger.Information(connectionString);



            //services.Configure<ReneConfig>(ReneConfig);

            //services.AddDbContext<ReneIdentityClientDbContext>(options =>
            //   options.UseSqlServer(
            //       connectionString));
            //services.AddDefaultIdentity<IdentityUser>()
            //    .AddEntityFrameworkStores<ReneIdentityClientDbContext>();

            //services.Configure<IdentityOptions>(options =>
            //{
            //    // Password settings
            //    options.Password.RequireDigit = true;
            //    options.Password.RequiredLength = 8;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequireUppercase = true;
            //    options.Password.RequireLowercase = false;
            //    options.Password.RequiredUniqueChars = 6;

            //    // Lockout settings
            //    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            //    options.Lockout.MaxFailedAccessAttempts = 10;
            //    options.Lockout.AllowedForNewUsers = true;

            //    // User settings
            //    options.User.RequireUniqueEmail = true;
            //});

            //services.ConfigureApplicationCookie(options =>
            //{
            //    // Cookie settings
            //    options.Cookie.HttpOnly = true;
            //    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            //    // If the LoginPath isn't set, ASP.NET Core defaults 
            //    // the path to /Account/Login.
            //    options.LoginPath = "/Account/Login";
            //    // If the AccessDeniedPath isn't set, ASP.NET Core defaults 
            //    // the path to /Account/AccessDenied.
            //    options.AccessDeniedPath = "/Account/AccessDenied";
            //    options.SlidingExpiration = true;
            //});
            // services.AddMvcCore()
            //    .AddAuthorization()
            //    .AddJsonFormatters();

            //services.AddAuthentication("Bearer")
            //    // namespace: IdentityServer4.AccessTokenValidators
            //    .AddIdentityServerAuthentication(options =>
            //    {
            //        options.Authority = "http://localhost:9977";
            //        options.RequireHttpsMetadata = false;

            //        options.ApiName = "rene-identity-api";
            //    });

            services.AddMvc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            
            
          
            if (env.IsDevelopment())
            {
             
               
                app.UseDeveloperExceptionPage();
            }

           

            app.UseExceptionHandler("/Home/Error"); // Call first to catch exceptions
                                                                                         // thrown in the following middleware.
            // Return static files and end pipeline
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
             Path.Combine(Directory.GetCurrentDirectory(), "renewebhost-dev-statics/")),
                RequestPath = ""
            });                  

            app.UseAuthentication();               // Authenticate before you access
                                                       // secure resources.

            app.UseMvcWithDefaultRoute();

            // RUN

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync(ReneConfig.GetValue<string>("ExceptionHandlerUri"));
            });

        }

    }
}