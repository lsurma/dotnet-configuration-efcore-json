using ConfigurationProvider.WebApi.Configuration;
using ConfigurationProvider.WebApi.Models;
using Microsoft.Extensions.Configuration;

namespace ConfigurationProvider.Tests;

public class ConfigurationReloadTests
{
    [Fact]
    public async Task ReloadAsync_UpdatesConfiguration()
    {
        // Arrange
        int callCount = 0;
        Func<Task<object>> settingsFactory = () =>
        {
            callCount++;
            return Task.FromResult<object>(new
            {
                Value = $"Load{callCount}"
            });
        };

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddAsyncObjectConfiguration(settingsFactory);
        var configuration = configBuilder.Build();

        // Assert initial load
        Assert.Equal("Load1", configuration["Value"]);

        // Act - Find the provider and reload it
        var provider = configuration.Providers
            .OfType<AsyncObjectConfigurationProvider>()
            .First();
        await provider.ReloadAsync();

        // Assert - Configuration should be updated
        Assert.Equal("Load2", configuration["Value"]);
    }

    [Fact]
    public async Task ReloadAsync_TriggersChangeNotification()
    {
        // Arrange
        int changeCallbackCount = 0;
        int callCount = 0;
        
        Func<Task<object>> settingsFactory = () =>
        {
            callCount++;
            return Task.FromResult<object>(new
            {
                Notifications = new NotificationsSettings
                {
                    Enabled = callCount % 2 == 0 // Toggle between true/false
                }
            });
        };

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddAsyncObjectConfiguration(settingsFactory);
        var configuration = configBuilder.Build();

        // Register for change notifications
        configuration.GetReloadToken().RegisterChangeCallback(_ => changeCallbackCount++, null);

        // Act - Reload
        var provider = configuration.Providers
            .OfType<AsyncObjectConfigurationProvider>()
            .First();
        await provider.ReloadAsync();

        // Assert
        Assert.Equal(1, changeCallbackCount);
        Assert.Equal("True", configuration["Notifications:Enabled"]); // Second call returns true (callCount = 2)
    }

    [Fact]
    public async Task PeriodicReload_AutomaticallyReloadsConfiguration()
    {
        // Arrange
        int callCount = 0;
        Func<Task<object>> settingsFactory = () =>
        {
            callCount++;
            return Task.FromResult<object>(new
            {
                Value = $"Load{callCount}"
            });
        };

        // Configure with 100ms reload interval
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddAsyncObjectConfiguration(settingsFactory, TimeSpan.FromMilliseconds(100));
        var configuration = configBuilder.Build();

        // Assert initial load
        Assert.Equal("Load1", configuration["Value"]);

        // Wait for periodic reload to trigger
        await Task.Delay(250); // Wait for at least 2 reload cycles

        // Assert - Configuration should have been reloaded automatically
        var value = configuration["Value"];
        Assert.NotNull(value);
        Assert.True(int.Parse(value.Replace("Load", "")) > 1);
    }

    [Fact]
    public async Task Dispose_StopsPeriodicReload()
    {
        // Arrange
        int callCount = 0;
        Func<Task<object>> settingsFactory = () =>
        {
            callCount++;
            return Task.FromResult<object>(new
            {
                Value = $"Load{callCount}"
            });
        };

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddAsyncObjectConfiguration(settingsFactory, TimeSpan.FromMilliseconds(50));
        var configuration = configBuilder.Build();

        var provider = configuration.Providers
            .OfType<AsyncObjectConfigurationProvider>()
            .First();

        // Wait for at least one reload
        await Task.Delay(100);
        var countBeforeDispose = callCount;

        // Act - Dispose the provider
        provider.Dispose();

        // Wait to ensure no more reloads happen
        await Task.Delay(150);

        // Assert - Count should not increase after dispose
        Assert.Equal(countBeforeDispose, callCount);
    }
}
