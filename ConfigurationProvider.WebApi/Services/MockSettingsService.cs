using ConfigurationProvider.Core.Configuration;
using ConfigurationProvider.WebApi.Models;

namespace ConfigurationProvider.WebApi.Services;

public class MockSettingsService : ISettingsService
{
    private int _loadCount = 0;

    /// <summary>
    /// Simulates loading settings from database and populating the configuration provider.
    /// In real application, this would fetch data from database using EF Core.
    /// </summary>
    public async Task LoadAndPopulateConfigurationAsync()
    {
        // Simulate async database call
        await Task.Delay(100);

        _loadCount++;

        // Create settings objects - in real app, these would be loaded from database
        var notificationSettings = new NotificationsSettings
        {
            Enabled = true,
            UserSettings = new UserNotificationSettings
            {
                UseMail = false
            }
        };

        var userSettings = new UserSettings
        {
            DefaultLanguage = "en",
            Theme = "dark"
        };

        var generalSettings = new GeneralSettings
        {
            AppName = "CustomConfigApp",
            Version = "1.0.0",
            MaxItemsPerPage = 50,
            LoadCount = _loadCount
        };

        // Create collection of settings
        var settingsData = new List<ISettings>
        {
            notificationSettings,
            userSettings,
            generalSettings
        };

        // Populate the configuration provider with settings
        AsyncObjectConfigurationProvider.SetData(settingsData);
    }

    public async Task ReloadSettingsAsync()
    {
        // Reload settings and update configuration provider
        await LoadAndPopulateConfigurationAsync();
    }
}

// Additional settings models
public class UserSettings : ISettings
{
    public string SectionName => "User";

    public string DefaultLanguage { get; set; } = "en";
    public string Theme { get; set; } = "light";
}

public class GeneralSettings : ISettings
{
    public string SectionName => "General";

    public string AppName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int MaxItemsPerPage { get; set; } = 10;
    public int LoadCount { get; set; } = 0;
}
