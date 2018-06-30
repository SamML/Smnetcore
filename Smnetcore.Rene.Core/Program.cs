using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Smnetcore.Rene.Webhost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Smnetcore.Rene.Core
{
    class Program
    {
  
    public static IConfigurationRoot Configuration {get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            try { 
                 DateTime Date = new DateTime();
              Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
             .Enrich.FromLogContext()
             .WriteTo.Console()
             .WriteTo.File($"Log-StartupRene-{Date}.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}")
             .CreateLogger();
            

            Console.OutputEncoding = Encoding.UTF8;
            
            Console.WriteLine("Hello, This is Rene...");
            
            ConfigurationBuilder Configuration = new ConfigurationBuilder();
            /**
             * Environment variable loading. 
             * First try to fill from system.
             * Then file.
             * Then DB
             * --------------------------------------
             * Last filled overide previous
             **/
            Console.WriteLine("Force environment variable? [development,stagging, production] (enter to continue and check System Environment variable):");
            String env = "";
            if (new List<String>() { "development", "staging", "production" }
            .IndexOf(Console.ReadLine().Trim().ToLower()) > -1)
            {
                env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            }
            else if (new List<String>() { "development", "staging", "production" }
            .IndexOf(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToLower().Trim()) > -1)
            {
                env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            }
            else
            {
                env = (String.IsNullOrEmpty(env)) ? "Development" : env;
            }
            
           Log.Logger.Information("Environment: {0}", env);

              /** DO
             * Configuration sources loading. 
             * Try to load json file or generate default.
             * Convert json to xml.
             * File: $"rene.config.{environment}.json"
             * from default: rene.config.default.json
             * --------------------------------------
             * Load configuration from files
             * Get configuration sections
             **/
            Console.WriteLine("Loading configuration from config file");
            Console.WriteLine($"rene.config.json");
            Console.WriteLine($"rene.config.{env}.json");
            Configuration
            .SetBasePath(Environment.CurrentDirectory.ToString())
            .AddJsonFile($"rene.config.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"rene.config.{env}.json", optional: false, reloadOnChange: true)
                       .AddEnvironmentVariables();
            

            IConfiguration config = Configuration.Build();
            String[] reneWebhostArgs = new string[0];
            
            ReneWebhost.Launch(reneWebhostArgs, config.GetSection("ReneWebhostConfig"), Log.Logger);

            Console.ReadLine();
            //Console.WriteLine("Launching Rene Identity Server...");
            //String[] identityServerArgs = new string[0];
            //IConfiguration config = Configuration.Build();
            //ReneIdentityServer.launch(identityServerArgs, config.GetSection("ReneIdentityServerConfig"));

            //Console.WriteLine("Launching Rene...");
            //String[] reneArgs = new string[0];
            //ReneWebhost.go(reneArgs, config.GetSection("ReneConfig"));
            //ConsoleKeyInfo console = Console.ReadKey();
            //Console.ReadKey(true);

            //var services = new ServiceCollection();
                }
            catch (Exception e)
            {
            Log.Error("Main Program Error");

            }
           
        }
    }
}
