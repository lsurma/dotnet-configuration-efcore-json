using Microsoft.Extensions.Configuration;

namespace ConfigurationEFCoreJson;

/// <summary>
/// Configuration provider that uses a remote configuration provider to load settings
/// </summary>
public class RemoteConfigurationProvider : ConfigurationProvider
{
    private readonly IRemoteConfigurationProvider _remoteProvider;

    public RemoteConfigurationProvider(IRemoteConfigurationProvider remoteProvider)
    {
        _remoteProvider = remoteProvider ?? throw new ArgumentNullException(nameof(remoteProvider));
    }

    public override void Load()
    {
        var data = _remoteProvider.LoadConfiguration();
        Data.Clear();

        foreach (var kvp in data)
        {
            Data[kvp.Key] = kvp.Value;
        }
    }
}
