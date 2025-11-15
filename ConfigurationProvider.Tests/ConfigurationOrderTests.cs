using ConfigurationProvider.Core.Configuration;
using ConfigurationProvider.WebApi.Models;
using Microsoft.Extensions.Configuration;

namespace ConfigurationProvider.Tests;

public class ConfigurationOrderTests
{
    [Fact]
    public void CustomConfiguration_ShouldOverride_AppSettings()
    {
        // Arrange - Create a configuration with appsettings first, then custom config
        var configBuilder = new ConfigurationBuilder();
        
        // Simulate appsettings.json
        var appSettingsDict = new Dictionary<string, string?>
        {
            ["Notifications:Enabled"] = "false",
            ["Notifications:UserSettings:UseMail"] = "true",
            ["AppName"] = "AppSettingsValue"
        };
        configBuilder.AddInMemoryCollection(appSettingsDict);
        
        // Add custom configuration (should override appsettings)
        var customSettings = new
        {
            Notifications = new NotificationsSettings
            {
                Enabled = true,
                UserSettings = new UserNotificationSettings
                {
                    UseMail = false
                }
            },
            AppName = "CustomConfigValue"
        };
        configBuilder.AddAsyncObjectConfiguration(() => Task.FromResult<object>(customSettings));

        // Act
        var configuration = configBuilder.Build();

        // Assert - Custom config should override appsettings
        Assert.Equal("True", configuration["Notifications:Enabled"]); // Custom: true, AppSettings: false
        Assert.Equal("False", configuration["Notifications:UserSettings:UseMail"]); // Custom: false, AppSettings: true
        Assert.Equal("CustomConfigValue", configuration["AppName"]); // Custom value should override
    }

    [Fact]
    public void CustomConfiguration_ShouldOverride_EnvironmentVariables()
    {
        // Arrange - Create a configuration with env vars first, then custom config
        var configBuilder = new ConfigurationBuilder();
        
        // Simulate environment variables
        var envVarsDict = new Dictionary<string, string?>
        {
            ["Notifications__Enabled"] = "false", // Note: Double underscore for env vars
            ["AppName"] = "EnvVarValue"
        };
        configBuilder.AddInMemoryCollection(envVarsDict);
        
        // Add custom configuration (should override env vars)
        var customSettings = new
        {
            Notifications = new NotificationsSettings
            {
                Enabled = true
            },
            AppName = "CustomConfigValue"
        };
        configBuilder.AddAsyncObjectConfiguration(() => Task.FromResult<object>(customSettings));

        // Act
        var configuration = configBuilder.Build();

        // Assert - Custom config should override environment variables
        Assert.Equal("True", configuration["Notifications:Enabled"]);
        Assert.Equal("CustomConfigValue", configuration["AppName"]);
    }

    [Fact]
    public void ConfigurationOrder_MultipleSourcesWithOverrides()
    {
        // Arrange - Simulate multiple configuration sources
        var configBuilder = new ConfigurationBuilder();
        
        // 1. Base configuration (like appsettings.json)
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Version"] = "1.0.0",
            ["AppName"] = "BaseApp",
            ["Notifications:Enabled"] = "false"
        });
        
        // 2. Environment-specific configuration (like appsettings.Development.json)
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Version"] = "1.1.0",
            ["AppName"] = "DevApp"
        });
        
        // 3. Environment variables
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Version"] = "1.2.0"
        });
        
        // 4. Custom configuration (should override all previous sources)
        var customSettings = new
        {
            Version = "2.0.0",
            AppName = "CustomApp",
            Notifications = new NotificationsSettings
            {
                Enabled = true
            }
        };
        configBuilder.AddAsyncObjectConfiguration(() => Task.FromResult<object>(customSettings));

        // Act
        var configuration = configBuilder.Build();

        // Assert - Last source (custom config) should win
        Assert.Equal("2.0.0", configuration["Version"]);
        Assert.Equal("CustomApp", configuration["AppName"]);
        Assert.Equal("True", configuration["Notifications:Enabled"]);
    }
}
