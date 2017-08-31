using JetBrains.Annotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DistancesAPI
{
    [PublicAPI]
    public static class Program
    {
        public static void Main([NotNull][ItemNotNull] string[] args)
        {
            BuildWebHost(args).Run();
        }

        [Pure]
        [NotNull]
        public static IWebHost BuildWebHost([NotNull][ItemNotNull] string[] args)
        {
            IConfigurationRoot configuration =
                new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build();

            return
                WebHost.CreateDefaultBuilder(args)
                       .UseConfiguration(configuration)
                       .UseStartup<Startup>()
                       .UseApplicationInsights()
                       .UseHttpSys(
                           x =>
                           {
                               x.EnableResponseCaching = true;
                           })
                       //.UseKestrel(
                       //    options =>
                       //    {
                       //        options.AddServerHeader = false;
                       //    })
                       .Build();
        }
    }
}