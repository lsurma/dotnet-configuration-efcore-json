namespace ConfigurationProvider.WebApi.Services;

/// <summary>
/// Service responsible for loading settings from external source (e.g., database)
/// and populating the configuration provider.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Loads settings from external source and populates the configuration provider.
    /// This should be called after DI container is initialized.
    /// </summary>
    Task LoadAndPopulateConfigurationAsync();

    /// <summary>
    /// Reloads settings from external source and updates the configuration provider.
    /// </summary>
    Task ReloadSettingsAsync();
}
