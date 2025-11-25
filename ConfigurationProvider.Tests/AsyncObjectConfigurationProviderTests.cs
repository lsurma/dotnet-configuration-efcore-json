using ConfigurationProvider.Core.Configuration;
using ConfigurationProvider.WebApi.Models;
using Microsoft.Extensions.Configuration;
using GeneralSettings = ConfigurationProvider.WebApi.Services.GeneralSettings;
using UserSettings = ConfigurationProvider.WebApi.Services.UserSettings;

namespace ConfigurationProvider.Tests;

public class AsyncObjectConfigurationProviderTests
{
    [Fact]
    public void SetData_WorksCorrectly()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddAsyncObjectConfiguration();
        var configuration = configBuilder.Build();

        // Act
        var settingsProvider = new TestSettingsProvider();
        AsyncObjectConfigurationProvider.SetData(settingsProvider.GetAllSettings());

        // Assert - Check that configuration was loaded
        Assert.NotNull(configuration);
        var allKeys = configuration.AsEnumerable().Where(kvp => !string.IsNullOrEmpty(kvp.Key)).ToList();
        Assert.NotEmpty(allKeys);
    }

    [Fact]
    public void SetData_TriggersReload()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddAsyncObjectConfiguration();
        var configuration = configBuilder.Build();

        AsyncObjectConfigurationProvider.SetData(new List<ISettings>
        {
            new GeneralSettings { AppName = "Initial", Version = "1.0" }
        });

        var initialAppName = configuration["General:AppName"];

        // Act - Update data
        AsyncObjectConfigurationProvider.SetData(new List<ISettings>
        {
            new GeneralSettings { AppName = "Updated", Version = "2.0" }
        });

        // Assert
        Assert.Equal("Updated", configuration["General:AppName"]);
        Assert.NotEqual(initialAppName, configuration["General:AppName"]);
    }
}
