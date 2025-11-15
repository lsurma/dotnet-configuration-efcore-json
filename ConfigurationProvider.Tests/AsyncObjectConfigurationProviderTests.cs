using ConfigurationProvider.WebApi.Configuration;
using ConfigurationProvider.WebApi.Models;
using Microsoft.Extensions.Configuration;

namespace ConfigurationProvider.Tests;

public class AsyncObjectConfigurationProviderTests
{
    [Fact]
    public void Load_WithSimpleObject_FlattensCorrectly()
    {
        // Arrange
        var testSettings = new
        {
            AppName = "TestApp",
            Version = "1.0.0"
        };

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddAsyncObjectConfiguration(() => Task.FromResult<object>(testSettings));

        // Act
        var configuration = configBuilder.Build();

        // Assert
        Assert.Equal("TestApp", configuration["AppName"]);
        Assert.Equal("1.0.0", configuration["Version"]);
    }

    [Fact]
    public void Load_WithNestedObject_FlattensCorrectly()
    {
        // Arrange
        var testSettings = new
        {
            Notifications = new NotificationsSettings
            {
                Enabled = true,
                UserSettings = new UserNotificationSettings
                {
                    UseMail = false
                }
            }
        };

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddAsyncObjectConfiguration(() => Task.FromResult<object>(testSettings));

        // Act
        var configuration = configBuilder.Build();

        // Assert
        Assert.Equal("True", configuration["Notifications:Enabled"]);
        Assert.Equal("False", configuration["Notifications:UserSettings:UseMail"]);
    }

    [Fact]
    public void Load_WithComplexObject_AllPropertiesAccessible()
    {
        // Arrange
        var testSettings = new
        {
            Notifications = new NotificationsSettings
            {
                Enabled = true,
                UserSettings = new UserNotificationSettings
                {
                    UseMail = true
                }
            },
            AppName = "ConfigApp",
            Version = "2.0.0"
        };

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddAsyncObjectConfiguration(() => Task.FromResult<object>(testSettings));

        // Act
        var configuration = configBuilder.Build();

        // Assert
        Assert.Equal("True", configuration["Notifications:Enabled"]);
        Assert.Equal("True", configuration["Notifications:UserSettings:UseMail"]);
        Assert.Equal("ConfigApp", configuration["AppName"]);
        Assert.Equal("2.0.0", configuration["Version"]);
    }

    [Fact]
    public void Bind_ToNotificationsSettings_BindsCorrectly()
    {
        // Arrange
        var testSettings = new
        {
            Notifications = new NotificationsSettings
            {
                Enabled = true,
                UserSettings = new UserNotificationSettings
                {
                    UseMail = false
                }
            }
        };

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddAsyncObjectConfiguration(() => Task.FromResult<object>(testSettings));
        var configuration = configBuilder.Build();

        // Act
        var boundSettings = new NotificationsSettings();
        configuration.GetSection("Notifications").Bind(boundSettings);

        // Assert
        Assert.True(boundSettings.Enabled);
        Assert.False(boundSettings.UserSettings.UseMail);
    }

    [Fact]
    public async Task Load_WithAsyncMethod_LoadsSuccessfully()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddAsyncObjectConfiguration(async () =>
        {
            await Task.Delay(10); // Simulate async work
            return new
            {
                AsyncSetting = "LoadedAsync"
            };
        });

        // Act
        var configuration = configBuilder.Build();

        // Assert
        Assert.Equal("LoadedAsync", configuration["AsyncSetting"]);
    }
}
