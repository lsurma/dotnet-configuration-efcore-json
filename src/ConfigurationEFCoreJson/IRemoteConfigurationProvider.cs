namespace ConfigurationEFCoreJson;

/// <summary>
/// Interface for remote configuration providers that can load configuration data from external sources
/// </summary>
public interface IRemoteConfigurationProvider
{
    /// <summary>
    /// Loads configuration data from the remote source
    /// </summary>
    /// <returns>Dictionary of configuration key-value pairs with flattened keys</returns>
    IDictionary<string, string?> LoadConfiguration();
}
