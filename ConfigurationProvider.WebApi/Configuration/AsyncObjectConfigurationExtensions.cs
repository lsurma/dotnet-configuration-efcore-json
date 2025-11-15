using Microsoft.Extensions.Configuration;

namespace ConfigurationProvider.WebApi.Configuration;

public static class AsyncObjectConfigurationExtensions
{
    public static IConfigurationBuilder AddAsyncObjectConfiguration(
        this IConfigurationBuilder builder,
        Func<Task<object>> settingsFactory)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        
        if (settingsFactory == null)
            throw new ArgumentNullException(nameof(settingsFactory));

        return builder.Add(new AsyncObjectConfigurationSource(settingsFactory));
    }
}
