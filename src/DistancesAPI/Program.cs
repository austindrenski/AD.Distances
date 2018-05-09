using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Primitives;
using AD.ApiExtensions.Hosting;

namespace DistancesAPI
{
    /// <summary>
    ///
    /// </summary>
    [PublicAPI]
    public static class Program
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        public static void Main([NotNull] [ItemNotNull] string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [Pure]
        [NotNull]
        public static IWebHostBuilder CreateWebHostBuilder(StringValues args)
        {
            return
                new WebHostBuilder()
                    .UseStartup<Startup>(args)
                    .UseHttpSys();
        }
    }
}