using Hydrix.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Hydrix.Tests
{
    /// <summary>
    /// TLS Data SQL DbClient Test Program
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Program Entry point
        /// </summary>
        private static async Task Main(string[] args)
        {
            var hydrixLoggerFactory = LoggerFactory.Create(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();

                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddHydrix(options =>
                    {
                        options.Logger = hydrixLoggerFactory.CreateLogger("Hydrix");
                    });
                    services.AddTransient<App>();
                })
                .Build();

            var app = host.Services.GetRequiredService<App>();
            await app.RunAsync();
        }
    }
}
