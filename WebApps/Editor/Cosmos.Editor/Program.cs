using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Cosmos.Cms
{
    /// <summary>
    /// Application entry class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Program main method.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Create host builder
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}