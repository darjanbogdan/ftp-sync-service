using CommandLine;
using FluentFTP;
using FtpSyncService.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FtpSyncService
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
            => await Parser.Default.ParseArguments<FtpUploaderOptions>(args)
                .MapResult(async (opts) =>
                {
                    await CreateHostBuilder(args, opts).Build().RunAsync();
                    return 0;
                },
                errs => Task.FromResult(-1));

        public static IHostBuilder CreateHostBuilder(string[] args, FtpUploaderOptions options) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
#if DEBUG
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
#endif
                    loggingBuilder.AddEventLog(eventLogSettings =>
                    {
                        eventLogSettings.SourceName = "Ftp Sync Service";
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<FtpUploaderOptions>(options);

                    services.AddScoped(sp =>
                    {
                        return new FtpClient(options.Host, options.Port, options.User, options.Password);
                    });

                    services.AddHostedService<FtpUploaderService>();
                })
                .UseWindowsService();
    }
}
