using Microsoft.Extensions.Options;

namespace ConfigurationProvider.WebApi.Configuration;

/// <summary>
/// Service that allows manual reloading of the custom configuration provider.
/// </summary>
public interface IConfigurationReloadService
{
    /// <summary>
    /// Manually reload the configuration from the settings source.
    /// </summary>
    Task ReloadAsync();
}

public class ConfigurationReloadService : IConfigurationReloadService
{
    private readonly IConfigurationRoot _configurationRoot;

    public ConfigurationReloadService(IConfiguration configuration)
    {
        _configurationRoot = configuration as IConfigurationRoot 
            ?? throw new ArgumentException("Configuration must be IConfigurationRoot to support reloading", nameof(configuration));
    }

    public async Task ReloadAsync()
    {
        // Find all AsyncObjectConfigurationProvider instances and reload them
        var providers = _configurationRoot.Providers
            .OfType<AsyncObjectConfigurationProvider>()
            .ToList();

        foreach (var provider in providers)
        {
            await provider.ReloadAsync();
        }
    }
}
