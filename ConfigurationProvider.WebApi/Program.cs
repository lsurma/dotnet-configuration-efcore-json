using ConfigurationProvider.WebApi.Configuration;
using ConfigurationProvider.WebApi.Services;

namespace ConfigurationProvider.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Add custom configuration provider that fetches settings from internal service
                var settingsService = new MockSettingsService();
                config.AddAsyncObjectConfiguration(() => settingsService.GetSettingsAsync());
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
