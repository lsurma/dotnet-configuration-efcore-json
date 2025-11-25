using ConfigurationProvider.Core.Configuration;
using ConfigurationProvider.WebApi.Services;

namespace ConfigurationProvider.Tests;

public class TestSettingsProvider
{
    public IEnumerable<ISettings> GetAllSettings()
    {
        return
        [
            new ConfigurationProvider.WebApi.Models.GeneralSettings(),
            new ConfigurationProvider.WebApi.Models.NotificationsSettings(),
            new UserSettings(),
            new GeneralSettings()
        ];
    }
}