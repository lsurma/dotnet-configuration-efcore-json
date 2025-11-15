using Microsoft.Extensions.Configuration;

namespace ConfigurationProvider.WebApi.Configuration;

public class AsyncObjectConfigurationSource : IConfigurationSource
{
    private readonly Func<Task<object>> _settingsFactory;
    private readonly TimeSpan? _reloadInterval;

    public AsyncObjectConfigurationSource(Func<Task<object>> settingsFactory, TimeSpan? reloadInterval = null)
    {
        _settingsFactory = settingsFactory ?? throw new ArgumentNullException(nameof(settingsFactory));
        _reloadInterval = reloadInterval;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new AsyncObjectConfigurationProvider(_settingsFactory, _reloadInterval);
    }
}
