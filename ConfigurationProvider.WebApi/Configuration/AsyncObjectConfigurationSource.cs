using Microsoft.Extensions.Configuration;

namespace ConfigurationProvider.WebApi.Configuration;

public class AsyncObjectConfigurationSource : IConfigurationSource
{
    private readonly Func<Task<object>> _settingsFactory;

    public AsyncObjectConfigurationSource(Func<Task<object>> settingsFactory)
    {
        _settingsFactory = settingsFactory ?? throw new ArgumentNullException(nameof(settingsFactory));
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new AsyncObjectConfigurationProvider(_settingsFactory);
    }
}
