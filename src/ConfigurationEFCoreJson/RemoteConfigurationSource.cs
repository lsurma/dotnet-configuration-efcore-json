using Microsoft.Extensions.Configuration;

namespace ConfigurationEFCoreJson;

/// <summary>
/// Configuration source for remote configuration providers
/// </summary>
public class RemoteConfigurationSource : IConfigurationSource
{
    private readonly IRemoteConfigurationProvider _remoteProvider;

    public RemoteConfigurationSource(IRemoteConfigurationProvider remoteProvider)
    {
        _remoteProvider = remoteProvider ?? throw new ArgumentNullException(nameof(remoteProvider));
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new RemoteConfigurationProvider(_remoteProvider);
    }
}
