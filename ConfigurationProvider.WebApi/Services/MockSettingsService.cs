using ConfigurationProvider.WebApi.Models;

namespace ConfigurationProvider.WebApi.Services;

public class MockSettingsService : ISettingsService
{
    public Task<object> GetSettingsAsync()
    {
        // Simulate async operation
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
            Version = "1.0.0"
        };

        return Task.FromResult<object>(settings);
    }
}
