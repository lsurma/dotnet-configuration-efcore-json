namespace ConfigurationProvider.WebApi.Models;

public class NotificationsSettings
{
    public bool Enabled { get; set; } = false;

    public UserNotificationSettings UserSettings { get; set; } = new();
}
