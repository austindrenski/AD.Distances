using System.IO.Compression;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DistancesAPI
{
    [PublicAPI]
    public class Startup
    {
        [NotNull]
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            Configuration =
                new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddEnvironmentVariables()
                    .Build();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services. 
            services.AddApplicationInsightsTelemetry(Configuration)
                    .AddLogging()
                    .AddResponseCompression(
                        x =>
                        {
                            x.Providers.Add<GzipCompressionProvider>();
                        })
                    .Configure<GzipCompressionProviderOptions>(
                        x =>
                        {
                            x.Level = CompressionLevel.Fastest;
                        })
                    .AddRouting(
                        x =>
                        {
                            x.LowercaseUrls = true;
                        })
                    .AddMvc(
                        x =>
                        {
                            x.InputFormatters.Add(new DelimitedTextInputFormatter());
                        });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IMemoryCache memoryCache)
        {
            //loggerFactory.AddConsole(LogLevel.Information, false);

            app.UseMvc()
               .UseResponseCompression()
               .UseStaticFiles()
               .Use(
                   (context, next) =>
                   {
                       context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                       context.Response.Headers.Add("X-Frame-Options", "DENY");
                       context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
                       context.Response.Headers.Add("Referrer-Policy", "no-referrer");
                       context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "none");
                       context.Response.Headers.Remove("X-Powered-By");
                       return next();
                   })
               .UseWhen(
                   x => env.IsDevelopment(),
                   x => x.UseDeveloperExceptionPage())
               .UseWhen(
                   x => env.IsProduction(),
                   x => x.UseExceptionHandler(
                       y =>
                       {
                           y.Run(
                               async context =>
                               {
                                   context.Response.StatusCode = 500;
                                   context.Response.ContentType = "text/plain";
                                   await context.Response.WriteAsync("An internal server error has occured. Contact Austin.Drenski@usitc.gov.");
                               });
                       }));
        }
    }
}