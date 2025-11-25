using ConfigurationProvider.Core.Configuration;

namespace ConfigurationProvider.WebApi.Models;

public class NotificationsSettings : ISettings
{
    public string SectionName => "Notifications";

    public bool Enabled { get; set; } = false;

    public UserNotificationSettings UserSettings { get; set; } = new();
}
