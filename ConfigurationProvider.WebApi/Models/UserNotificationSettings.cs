namespace ConfigurationProvider.WebApi.Models;

/// <summary>
/// User notification settings - this is a nested object within NotificationsSettings, not a standalone settings section.
/// Therefore it should NOT implement ISettings.
/// </summary>
public class UserNotificationSettings
{
    public bool UseMail { get; set; } = true;

    public IEnumerable<string> PreferredChannels { get; set; } = new List<string> { "Email", "SMS" };

    public TimeSpan? DoNotDisturbPeriod { get; set; } = TimeSpan.FromHours(8);

    public DateTimeOffset? LastUpdated { get; set; } = null;

    public Guid? UserId { get; set; } = null;
}
