using ConfigurationProvider.WebApi.Models;

namespace ConfigurationProvider.WebApi.Services;

public class MockSettingsService : ISettingsService
{
    private int _callCount = 0;

    public Task<object> GetSettingsAsync()
    {
        // Simulate async operation
        _callCount++;
        
        var settings = new
        {
            Notifications = new NotificationsSettings
            {
                Enabled = true,
                UserSettings = new UserNotificationSettings
                {
                    UseMail = false
                }
            },
            AppName = "CustomConfigApp",
            Version = "1.0.0",
            ReloadCount = _callCount // Track how many times this has been reloaded
        };

        return Task.FromResult<object>(settings);
    }
}
