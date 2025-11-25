using ConfigurationProvider.Core.Configuration;

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
                // Add empty custom configuration provider (similar to Azure Key Vault approach)
                // The provider will be populated later by ISettingsService after DI container is initialized
                // This is added AFTER all default sources (appsettings, user secrets, env vars, command line)
                // so it will override all other configuration sources
                config.AddAsyncObjectConfiguration();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
