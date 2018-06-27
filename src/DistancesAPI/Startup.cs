using System;
using System.IO;
using System.IO.Compression;
using AD.ApiExtensions.Conventions;
using AD.ApiExtensions.Http;
using AD.ApiExtensions.Mvc;
using AD.ApiExtensions.Primitives;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;

namespace DistancesAPI
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class Startup : IStartup
    {
        /// <summary>
        /// Represents the application configuration properties available during construction.
        /// </summary>
        private IConfiguration Configuration { get; }

        /// <summary>
        /// Constructs an <see cref="IStartup"/> for configuration.
        /// </summary>
        /// <param name="configuration">
        /// The application configuration properties available from the <see cref="IWebHostBuilder"/>.
        /// </param>
        /// <exception cref="ArgumentNullException" />
        public Startup([NotNull] IConfiguration configuration) => Configuration = configuration;

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <exception cref="ArgumentNullException" />
        [Pure]
        [NotNull]
        public IServiceProvider ConfigureServices([NotNull] IServiceCollection services)
            => services.AddLogging(x => x.AddConsole())
                       .AddApiVersioning(
                            x =>
                            {
                                x.AssumeDefaultVersionWhenUnspecified = true;
                                x.DefaultApiVersion = new ApiVersion(1, 0);
                            })
                       .AddResponseCompression(x => x.Providers.Add<GzipCompressionProvider>())
                       .Configure<GzipCompressionProviderOptions>(x => x.Level = CompressionLevel.Fastest)
                       .AddRouting(x => x.LowercaseUrls = true)
                       .AddAntiforgery(
                            x =>
                            {
                                x.HeaderName = "x-xsrf-token";
                                x.Cookie.HttpOnly = true;
                                x.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                            })
                       .AddSwaggerGen(
                            x =>
                            {
                                x.DescribeAllEnumsAsStrings();
                                x.DescribeStringEnumsInCamelCase();
                                x.DescribeAllParametersInCamelCase();
                                x.IgnoreObsoleteActions();
                                x.IgnoreObsoleteProperties();
                                x.IncludeXmlComments(Path.Combine(ApplicationEnvironment.ApplicationBasePath, $"{nameof(DistancesAPI)}.xml"));
                                x.MapType<GroupingValues<string, string>>(() => new Schema { Type = "string" });
                                x.OrderActionsBy(y => y.RelativePath);
                                x.SwaggerDoc("v1", new Info { Title = "Distances API", Version = "v1" });
                            })
                       .AddExceptionProvider<OperationCanceledException>(HttpMethods.Get, StatusCodes.Status400BadRequest)
                       .AddExceptionProvider<OperationCanceledException>(HttpMethods.Post, StatusCodes.Status400BadRequest)
                       .AddMvc(
                            x =>
                            {
                                x.AddModelValidationFilter();
                                x.Conventions.Add(new KebabControllerModelConvention());
                                x.InputFormatters.Add(new DelimitedTextInputFormatter());
                                x.ModelMetadataDetailsProviders.Add(new KebabBindingMetadataProvider());
                                x.ModelMetadataDetailsProviders.Add(new RequiredBindingMetadataProvider());
                                x.RespectBrowserAcceptHeader = true;
                            })
                       .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                       .AddJsonOptions(
                            x =>
                            {
                                x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                x.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                                x.SerializerSettings.Formatting = Formatting.Indented;
                            })
                       .Services
                       .BuildServiceProvider();

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <exception cref="ArgumentNullException" />
        public void Configure([NotNull] IApplicationBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.UseServerHeader(string.Empty)
                   .UseHeadMethod()
                   .UseResponseCompression()
                   .UseSwagger(x => x.RouteTemplate = "docs/{documentName}/swagger.json")
                   .UseSwaggerUI(
                        x =>
                        {
                            x.DefaultModelRendering(ModelRendering.Model);
                            x.DisplayRequestDuration();
                            x.DocExpansion(DocExpansion.None);
                            x.DocumentTitle = "Distances API Documentation";
                            x.HeadContent = "Distances API Documentation";
                            x.RoutePrefix = "docs";
                            x.SwaggerEndpoint("v1/swagger.json", "Distances API Documentation");
                        })
                   .UseMvc();
        }
    }
}