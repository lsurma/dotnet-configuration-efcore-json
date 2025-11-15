using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConfigurationEFCoreJson.Tests;

public class RemoteConfigurationProviderTests : IDisposable
{
    private readonly string _testDbName;
    private readonly DbContextOptions<ConfigurationDbContext> _options;

    public RemoteConfigurationProviderTests()
    {
        _testDbName = $"TestDb_{Guid.NewGuid()}";
        _options = new DbContextOptionsBuilder<ConfigurationDbContext>()
            .UseInMemoryDatabase(_testDbName)
            .Options;
    }

    public void Dispose()
    {
        // Cleanup is automatic with in-memory database
    }

    [Fact]
    public void AddRemoteConfig_WithDbContext_LoadsConfiguration()
    {
        // Arrange
        using (var context = new ConfigurationDbContext(_options))
        {
            context.ConfigurationSettings.Add(new ConfigurationSetting
            {
                Key = "TestKey",
                JsonValue = "\"TestValue\""
            });
            context.SaveChanges();
        }

        // Act
        using (var context = new ConfigurationDbContext(_options))
        {
            var configuration = new ConfigurationBuilder()
                .AddRemoteConfig(context)
                .Build();

            // Assert
            Assert.Equal("TestValue", configuration["TestKey"]);
        }
    }

    [Fact]
    public void AddRemoteConfig_WithNestedObject_ReturnsCorrectValues()
    {
        // Arrange
        using (var context = new ConfigurationDbContext(_options))
        {
            context.ConfigurationSettings.Add(new ConfigurationSetting
            {
                Key = "Database",
                JsonValue = """
                    {
                        "Host": "localhost",
                        "Port": 5432,
                        "Name": "testdb"
                    }
                    """
            });
            context.SaveChanges();
        }

        // Act
        using (var context = new ConfigurationDbContext(_options))
        {
            var configuration = new ConfigurationBuilder()
                .AddRemoteConfig(context)
                .Build();

            // Assert
            Assert.Equal("localhost", configuration["Database:Host"]);
            Assert.Equal("5432", configuration["Database:Port"]);
            Assert.Equal("testdb", configuration["Database:Name"]);
        }
    }

    [Fact]
    public void AddRemoteConfig_WithArray_ReturnsCorrectValues()
    {
        // Arrange
        using (var context = new ConfigurationDbContext(_options))
        {
            context.ConfigurationSettings.Add(new ConfigurationSetting
            {
                Key = "AllowedHosts",
                JsonValue = """
                    [
                        "localhost",
                        "example.com",
                        "*.domain.com"
                    ]
                    """
            });
            context.SaveChanges();
        }

        // Act
        using (var context = new ConfigurationDbContext(_options))
        {
            var configuration = new ConfigurationBuilder()
                .AddRemoteConfig(context)
                .Build();

            // Assert
            Assert.Equal("localhost", configuration["AllowedHosts:0"]);
            Assert.Equal("example.com", configuration["AllowedHosts:1"]);
            Assert.Equal("*.domain.com", configuration["AllowedHosts:2"]);
        }
    }

    [Fact]
    public void AddRemoteConfig_ChainsWithOtherSources_OverridesCorrectly()
    {
        // Arrange
        using (var context = new ConfigurationDbContext(_options))
        {
            context.ConfigurationSettings.Add(new ConfigurationSetting
            {
                Key = "SharedKey",
                JsonValue = "\"ValueFromDatabase\""
            });
            context.SaveChanges();
        }

        // Act
        using (var context = new ConfigurationDbContext(_options))
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "SharedKey", "ValueFromMemory" },
                    { "MemoryOnlyKey", "MemoryValue" }
                })
                .AddRemoteConfig(context)  // This should override SharedKey
                .Build();

            // Assert
            Assert.Equal("ValueFromDatabase", configuration["SharedKey"]); // Overridden by remote config
            Assert.Equal("MemoryValue", configuration["MemoryOnlyKey"]); // From memory
        }
    }

    [Fact]
    public void DbContextConfigurationProvider_LoadsDataCorrectly()
    {
        // Arrange
        using (var context = new ConfigurationDbContext(_options))
        {
            context.ConfigurationSettings.Add(new ConfigurationSetting
            {
                Key = "Setting1",
                JsonValue = """
                    {
                        "Nested": {
                            "Value": "test"
                        }
                    }
                    """
            });
            context.SaveChanges();
        }

        // Act
        using (var context = new ConfigurationDbContext(_options))
        {
            var provider = new DbContextConfigurationProvider<ConfigurationDbContext>(context);
            var data = provider.LoadConfiguration();

            // Assert
            Assert.Contains("Setting1:Nested:Value", data.Keys);
            Assert.Equal("test", data["Setting1:Nested:Value"]);
        }
    }

    [Fact]
    public void AddRemoteConfig_WithCustomProvider_LoadsConfiguration()
    {
        // Arrange
        var customProvider = new TestRemoteConfigurationProvider(new Dictionary<string, string?>
        {
            { "CustomKey", "CustomValue" },
            { "Custom:Nested", "NestedValue" }
        });

        // Act
        var configuration = new ConfigurationBuilder()
            .AddRemoteConfig(customProvider)
            .Build();

        // Assert
        Assert.Equal("CustomValue", configuration["CustomKey"]);
        Assert.Equal("NestedValue", configuration["Custom:Nested"]);
    }

    private class TestRemoteConfigurationProvider : IRemoteConfigurationProvider
    {
        private readonly IDictionary<string, string?> _data;

        public TestRemoteConfigurationProvider(IDictionary<string, string?> data)
        {
            _data = data;
        }

        public IDictionary<string, string?> LoadConfiguration()
        {
            return _data;
        }
    }
}
