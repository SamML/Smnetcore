using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.AspNetCore;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;

namespace Smnetcore.Rene.Webhost
{
    public class ReneWebhost
    {
        public static IConfiguration ReneWebhostConfig { get; private set; }
        public static Serilog.ILogger Logger;

        public static void Launch(string[] args, IConfigurationSection reneWebhostConfig, ILogger logger)
        {
            Logger = logger;
            ReneWebhostConfig = reneWebhostConfig;
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
             Logger.Information("Starting web host");
              try {
        return ReneWebhost.CreateReneBuilder(args).ConfigureServices(services => services.AddLogging())
                // Use Serilog
                .UseSerilog(Logger,true)
                // Evitar inicio de hospedaje
                .UseSetting(WebHostDefaults.PreventHostingStartupKey, "true")
                .UseStartup<StartupRene>();
        }

        catch (Exception e) {
             Log.Fatal(e, "Host terminated unexpectedly");
            return null;
        }

        finally {
             Log.CloseAndFlush();
        }
}

       

        public static IWebHostBuilder CreateReneBuilder() =>
            CreateReneBuilder(args: null);

        public static IWebHostBuilder CreateReneBuilder(string[] args)
        {
            Console.WriteLine("CreateReneBuilder: Load certificate data from config");
            IConfiguration certificateConfig = ReneWebhostConfig.GetSection("CertificateConfig");
            string certificateFileName = certificateConfig.GetValue<string>("CertificateFilename");
            string certificatePassword = certificateConfig.GetValue<string>("CertificateSecret");

            Console.WriteLine("CreateReneBuilder: Generate certificate");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificateFileName, certificatePassword);
            if (certificate == null)
            {
                throw new CryptographicException("ERROR: Error trying to generate certificate. Certificate file exists?");
            }

            Console.WriteLine("CreateReneBuilder: Create webhost builder");
            Console.WriteLine("CreateReneBuilder: ..configure webhost values. Http("
                    + ReneWebhostConfig.GetValue<System.Int32>("HttpPort") + ") - Https("
                        + ReneWebhostConfig.GetValue<System.Int32>("HttpsPort") + ")");
            Console.WriteLine("CreateReneBuilder: .. set content root");
            Console.WriteLine("CreateReneBuilder: ..configure AppConfiguration(hostingContext, config)");
            Console.WriteLine("CreateReneBuilder: .. configure args command line configuration");
            Console.WriteLine("CreateReneBuilder: ..configure Logging");
            Console.WriteLine("CreateReneBuilder: ..configure IIS integration");
            Console.WriteLine("CreateReneBuilder: ..configure to use ASP .NET Core default Service Provider");
            
            var builder = new WebHostBuilder()
                   .UseKestrel(
                    options =>
                    {
                        options.Limits.MaxConcurrentConnections = long.Parse($"{ReneWebhostConfig["MaxConcurrentConnections"]}"); //100
                        options.Limits.MaxConcurrentUpgradedConnections = long.Parse($"{ReneWebhostConfig["MaxConcurrentUpgradedConnections"]}"); //100;
                        options.Limits.MaxRequestBodySize = long.Parse($"{ReneWebhostConfig["MaxRequestBodySizeMultiplier"]}"); // 10 * 1024;
                        options.Limits.MinRequestBodyDataRate =
                            new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                        options.Limits.MinResponseDataRate =
                            new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                        options.Listen(IPAddress.Loopback, int.Parse($"{ReneWebhostConfig["HttpPort"]}"));
                        options.Listen(IPAddress.Loopback, int.Parse($"{ReneWebhostConfig["HttpsPort"]}"), listenOptions =>
                        {
                            listenOptions.UseHttps(certificate);
                        });
                    })
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        var env = hostingContext.HostingEnvironment;

                        if (env.IsDevelopment())
                        {
                            var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                            if (appAssembly != null)
                            {
                                config.AddUserSecrets(appAssembly, optional: true);
                            }
                        }

                        if (args != null)
                        {
                            config.AddCommandLine(args);
                        }
                        config.SetBasePath(Directory.GetCurrentDirectory());
                        // Add settings to bootstrap EF config.
                    })

                    .UseIISIntegration()
                    .UseDefaultServiceProvider((context, options) =>
                    {
                        options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                    });

            if (args != null)
            {
                builder.UseConfiguration(new ConfigurationBuilder().AddCommandLine(args).Build());
            }
            
            return builder;
        }

        public static IWebHostBuilder CreateReneBuilder<TStartup>(string[] args) where TStartup : class =>
            CreateReneBuilder(args).UseStartup<TStartup>();
    }
}