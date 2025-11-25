namespace ConfigurationProvider.Core.Configuration;

/// <summary>
/// Marker interface for configuration settings.
/// Implement this interface on your settings classes to enable them to be used with AsyncObjectConfigurationProvider.
/// </summary>
public interface ISettings
{
    /// <summary>
    /// Gets the name of the settings section in configuration.
    /// For example, "Notifications", "User", "General", etc.
    /// </summary>
    string SectionName { get; }
}
