using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
//using Microsoft.ApplicationInsights.Channel;
//using Microsoft.ApplicationInsights.Extensibility;
using Newtonsoft.Json;
using System.Net.Mime;
using AudioConversion.HealthCheck;
using Microsoft.AspNetCore.Diagnostics;
using Ben.Diagnostics;
using AudioConversion.RESTApi.AudioConversion;
//using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Microsoft.OpenApi.Models;
using AudioConversion.RESTApi.Client.Users;

namespace AudioConversions
{
    /// <summary>
    /// Object is created and run when the configuration builder is complete.
    /// </summary>
    public class Startup
    {
        public ILogger<Program> _logger = null;
        private readonly IConfiguration _configuration = null;

        /// <summary>
        /// Once this object has been created the configuration builder, this function will be called.
        /// </summary>
        /// <param name="configuration">The configuration of this service built from json files and command line passed via dependency injection</param>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Allow gzip/br in the http response body.
            services.AddResponseCompression();


            // Add Microsoft Insights logging, if enabled in the configuration file.
            //if (_configuration.GetSection("ApplicationInsights").Exists() == true && _configuration.GetValue<string>("ApplicationInsights:Instrumentationkey") != null)
            //{
            //    services.AddSingleton<ITelemetryInitializer>(new CloudRoleNameInitializer(_configuration.GetValue<string>("Application:Name")));

            //    var aiOptions = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
            //    {
            //        InstrumentationKey = _configuration.GetValue<string>("ApplicationInsights:Instrumentationkey"),
            //        EnableAdaptiveSampling = false
            //    };
            //    services.AddApplicationInsightsTelemetry(aiOptions);

            //    var performanceCounterService = services.FirstOrDefault<ServiceDescriptor>(t => t.ImplementationType == typeof(PerformanceCollectorModule));
            //    if (performanceCounterService != null)
            //    {
            //        services.Remove(performanceCounterService);
            //    }


            //    //var eventCounterModule = services.FirstOrDefault<ServiceDescriptor>(t => t.ImplementationType == typeof(EventCounterCollectionModule));
            //    //if (eventCounterModule != null)
            //    //{
            //    //    services.Remove(eventCounterModule);
            //    //}

            //    //var dependencyTrackingTelemetryModule = services.FirstOrDefault<ServiceDescriptor>(t => t.ImplementationType == typeof(DependencyTrackingTelemetryModule));
            //    //if (dependencyTrackingTelemetryModule != null)
            //    //{
            //    //    services.Remove(dependencyTrackingTelemetryModule);
            //    //}
            //}


            // Add all of our 'global' objects.            
            services.AddScoped<IAudioConversionService, AudioConversionService>();  
            
            // Processing incoming REST api requests.
            services.AddScoped<IReSTApiUsersService, ReSTApiUsersService>();            // Connection to the Users REST api requests.

            services.AddHttpClient();

            // Event bus publisher.                            
            //services.AddSingleton<IEventBusPublisher>(s => new MicrosoftServiceBusTopicPublisher(_logger, _configuration.GetValue<string>("ServiceBus:Publisher:ConnectionString"))); // Use Microsoft for our publishing event bus.

            // Listen for event subscription messages.
            //  services.AddSingleton<IOrdersMicroserviceEventBusConsumerService>(s => new OrdersMicroserviceEventBusConsumerService(_logger, _configuration.GetValue<string>("ServiceBus:Consumer:OrdersMicroservice:ConnectionString"), _configuration.GetValue<string>("ServiceBus:Consumer:OrdersMicroservice:SubscriptionName"))); // Use Microsoft for our consumer event bus.
            //  services.AddSingleton<IPaymentsMicroserviceEventBusConsumerService>(s => new PaymentsMicroserviceEventBusConsumerService(_logger, _configuration.GetValue<string>("ServiceBus:Consumer:PaymentsMicroservice:ConnectionString"), _configuration.GetValue<string>("ServiceBus:Consumer:PaymentsMicroservice:SubscriptionName"))); // Use Microsoft for our consumer event bus.

            // Background services.
            //   services.AddHostedService<EventPublisherBackgroundService>();
            //   services.AddHostedService<UserManagementBackgroundService>();

            // Add health monitor checking.
            // Documentation here https://github.com/xabaril/AspNetCore.Diagnostics.HealthChecks.
            // There are many additional providers.
            services.AddHealthChecks()
                .AddDiskStorageHealthCheck(x => x.AddDrive("c:\\", 100), "Hard drive", HealthStatus.Degraded); // Local hard drive must have 100MB free space. See DriveInfo.GetDrives() for name.
 
            


            // Register the Swagger generator, defining 1 or more Swagger documents.
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = _configuration.GetValue<string>("Application:Name") + " REST api", Version = "v1" });

                // Add Operation Filter
                c.OperationFilter<SwaggerUploadFileParametersFilter>();

                // Add examples.
                c.ExampleFilters();

                // Allow changing of the call model names with the [DisplayName()] attribute.
                c.CustomSchemaIds(x => x.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Count() == 0 ? x.FullName : x.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Single().DisplayName);

                // Set the comments path for the Swagger JSON and UI.
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                c.IncludeXmlComments(xmlPath);
            });
            services.AddSwaggerExamplesFromAssemblyOf<AudioConversions.Program>();

            // Dont include null parameters in rest json output.
            services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILogger<Program> logger)
        {
            _logger = logger;

            // Configure differently based on the deployment environment.
            if (env.IsDevelopment())
            {
                _logger.LogInformation("Configuring for Development environment");
                app.UseDeveloperExceptionPage();
            }
            else if (env.IsStaging())
            {
                _logger.LogInformation("Configuring for Staging environment");
            }
            else
            {
                _logger.LogInformation("Configuring for Production environment");
                // Force all http requests to redirect via 302 to https.
                // app.UseHttpsRedirection();
                // app.UseHsts(); // Adds a header Strict-Transport-Security to the response to tell the client they should ONLY use ssl. Disable now for internal services.
            }

            // Add swagger which creates the self documenting REST api pages.
            // Available at http://localhost:<port>/calls/swagger
            app.UseSwagger(o =>
            {
                o.RouteTemplate = "audioconversion/swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                // Specify the Swagger JSON endpoint.
                c.RoutePrefix = "audioconversion/swagger";
                c.SwaggerEndpoint("/audioconversion/swagger/v1/swagger.json", _configuration.GetValue<string>("Application:Name") + " REST api");
                c.DefaultModelExpandDepth(2);
                c.DisplayRequestDuration();
            });

            // Allow gzip/br in http response body;
            app.UseResponseCompression();

            // Default setting to access wwwroot
            //  app.UseStaticFiles();

            //app.UseStaticFiles(new StaticFileOptions()
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "staticfiles")),
            //    RequestPath = "/api/v1/calls/staticfiles"
            //});

            // Set up all of our health monitoring that can be accessed by http://localhost/health
            app.UseHealthChecks("/audioconversion/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                AllowCachingResponses = false,
                ResponseWriter = async (context, report) =>
                {
                    var result = JsonConvert.SerializeObject(
                        new
                        {
                            status = report.Status.ToString(),
                            // Let's make the description null if empty so that we can exclude this property completely from the JSON output.
                            components = report.Entries.Select(e => new { name = e.Key, status = Enum.GetName(typeof(HealthStatus), e.Value.Status).ToLower(), description = string.IsNullOrEmpty(e.Value.Description) ? null : e.Value.Description })
                        },
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
                    context.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.Response.WriteAsync(result);
                },
                ResultStatusCodes =
                  {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
                  }
            });

            // Global exception handler.
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    // Log the exception.
                    var ex = context.Features.Get<IExceptionHandlerFeature>().Error;
                    _logger.LogError(ex, "Test error -Global error handler caught exception, {0}", ex.Message);

                    // Send back a clean error message to the user.
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("Internal Server Error");
                });
            });

            //// Start the Ben.BlockingDetector to look for blocking code which doesn't use async. Only in development.
            //if (env.IsDevelopment())
            //    app.UseBlockingDetection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Force the service bus consumers to start running.
            //  var myOrdersMicroserviceEventBusConsumerService = app.ApplicationServices.GetRequiredService<IOrdersMicroserviceEventBusConsumerService>();
            //   var myPaymentsMicroserviceEventBusConsumerService = app.ApplicationServices.GetRequiredService<IPaymentsMicroserviceEventBusConsumerService>();

            //// Allow manny connections to each remote server. This is mainly for internal microservices.
            //HttpClientHandler.MaxConnectionsPerServer = int.MaxValue;
        }
    }

    /// <summary>
    /// Interface to allow you to configure the RoleName for the Azure Insights Logging.
    /// This RoleName is the name of this service so we differentiate between microservices.
    /// </summary>
    //public class CloudRoleNameInitializer : ITelemetryInitializer
    //{
    //    private readonly string roleName = string.Empty;
    //    public CloudRoleNameInitializer(string roleName)
    //    {
    //        this.roleName = roleName ?? throw new ArgumentNullException(nameof(roleName));
    //    }
    //    public void Initialize(ITelemetry telemetry)
    //    {
    //        telemetry.Context.Cloud.RoleName = this.roleName;
    //    }
    //}
}