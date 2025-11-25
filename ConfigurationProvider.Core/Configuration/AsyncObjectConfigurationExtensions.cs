using Microsoft.Extensions.Configuration;

namespace ConfigurationProvider.Core.Configuration;

public static class AsyncObjectConfigurationExtensions
{
    /// <summary>
    /// Adds an empty async object configuration provider that can be populated later using AsyncObjectConfigurationProvider.SetData().
    /// Similar to Azure Key Vault provider approach - the provider is added empty, and data is populated after DI container is initialized.
    /// When SetData() is called, all provider instances are automatically reloaded.
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <returns>The configuration builder</returns>
    public static IConfigurationBuilder AddAsyncObjectConfiguration(this IConfigurationBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        return builder.Add(new AsyncObjectConfigurationSource());
    }
}
