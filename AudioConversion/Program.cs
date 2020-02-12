using System;
using System.IO;
using System.Threading;
using LoggingAdvanced.Console;
//using Microsoft.ApplicationInsights.Channel;
//using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace AudioConversions
{
    public class Program
    {
        // Channel is explicitly configured to do flush on it later.
        //private static InMemoryChannel channel = new InMemoryChannel();


        /// <summary>
        /// Entry point for the application.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // The main web host object.
            var host = CreateWebHostBuilder(args).Build();

            // Run the application, this is a blocking call.
            host.Run();

            // BUT I am seeing 'AI: Process was called on the TelemetrySink after it was disposed, the telemetry data was dropped.' in the Azure logs which needs to be fixed.

            // Explicitly call Flush() followed by sleep is required in Console Apps.
            // This is to ensure that even if application terminates, telemetry is still sent to the back-end.
            //channel.Flush();
            
            Thread.Sleep(1000);
            //channel.Dispose();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            var webHostBuilder = Host.CreateDefaultBuilder(args);
            webHostBuilder
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(options => { options.AddServerHeader = false; });
                    webBuilder.UseShutdownTimeout(TimeSpan.FromSeconds(30)); // Make 30 seconds in case the background worker takes a while.
                    webBuilder.UseStartup<Startup>();

                    // When debugging, allow connection from other machines on the LAN.
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
                    {
                        webBuilder.UseUrls("http://0.0.0.0:5002/");
                    }
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    // Add the additional settings configuration based upon where the server is running. Development, Staging or Production.
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName.ToString()}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    // Remove all default configuration.
                    logging.ClearProviders();
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

                    // Check if file logging is enabled.
                    if (hostingContext.Configuration.GetSection("FileLogging").Exists() == true)
                    {
                        logging.AddFile(hostingContext.Configuration.GetSection("FileLogging"));   // Only include file logging when running in Development.
                    }

                    // Check if console logging is enabled.
                    logging.AddConsole();  // Add console logging
                    //if (hostingContext.Configuration.GetSection("ConsoleLogging").Exists() == true)
                    //{
                    //    logging.AddConsoleAdvanced(hostingContext.Configuration.GetSection("ConsoleLogging"));  // Advanced console logging that includes time.
                    //}

                    // Check if Microsoft ApplicationInsights logging is enabled.
                    //if (hostingContext.Configuration.GetSection("ApplicationInsights").Exists() == true && hostingContext.Configuration.GetValue<string>("ApplicationInsights:Instrumentationkey") != null)
                    //{
                    //    logging.AddApplicationInsights(hostingContext.Configuration.GetValue<string>("ApplicationInsights:Instrumentationkey"));
                    //    logging.Services.Configure<TelemetryConfiguration>((config) =>
                    //    {
                    //        config.InstrumentationKey = hostingContext.Configuration.GetValue<string>("ApplicationInsights:Instrumentationkey");

                    //        // Use a local memory channel so we can flush it before close.
                    //        config.TelemetryChannel = channel;

                    //        // Optional adaptive sampling, we can enable this when traffic gets too high.
                    //        var UseAdaptiveSampling = hostingContext.Configuration.GetValue<string>("ApplicationInsights:UseAdaptiveSampling");
                    //        if (!string.IsNullOrEmpty(UseAdaptiveSampling) && UseAdaptiveSampling.ToLower().Equals("true"))
                    //        {
                    //            var builder = config.TelemetryProcessorChainBuilder;
                    //            builder.UseAdaptiveSampling(maxTelemetryItemsPerSecond: 5);
                    //            builder.Build();
                    //        }
                    //    });
                    //}

                });

            return webHostBuilder;
        }
    }
}
