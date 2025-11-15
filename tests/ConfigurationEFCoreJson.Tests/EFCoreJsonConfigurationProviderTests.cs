using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConfigurationEFCoreJson.Tests;

public class EFCoreJsonConfigurationProviderTests : IDisposable
{
    private readonly string _testDbName;
    private readonly DbContextOptions<ConfigurationDbContext> _options;

    public EFCoreJsonConfigurationProviderTests()
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
    public void Load_SimpleStringValue_ReturnsCorrectValue()
    {
        // Arrange
        using (var context = new ConfigurationDbContext(_options))
        {
            context.ConfigurationSettings.Add(new ConfigurationSetting
            {
                Key = "AppName",
                JsonValue = "\"MyApp\""
            });
            context.SaveChanges();
        }

        // Act
        var configuration = new ConfigurationBuilder()
            .AddEFCoreJsonConfiguration(options => options.UseInMemoryDatabase(_testDbName))
            .Build();

        // Assert
        Assert.Equal("MyApp", configuration["AppName"]);
    }

    [Fact]
    public void Load_NestedObject_ReturnsCorrectValues()
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
        var configuration = new ConfigurationBuilder()
            .AddEFCoreJsonConfiguration(options => options.UseInMemoryDatabase(_testDbName))
            .Build();

        // Assert
        Assert.Equal("localhost", configuration["Database:Host"]);
        Assert.Equal("5432", configuration["Database:Port"]);
        Assert.Equal("testdb", configuration["Database:Name"]);
    }

    [Fact]
    public void Load_DeeplyNestedObject_ReturnsCorrectValues()
    {
        // Arrange
        using (var context = new ConfigurationDbContext(_options))
        {
            context.ConfigurationSettings.Add(new ConfigurationSetting
            {
                Key = "Logging",
                JsonValue = """
                    {
                        "LogLevel": {
                            "Default": "Information",
                            "Microsoft": "Warning"
                        }
                    }
                    """
            });
            context.SaveChanges();
        }

        // Act
        var configuration = new ConfigurationBuilder()
            .AddEFCoreJsonConfiguration(options => options.UseInMemoryDatabase(_testDbName))
            .Build();

        // Assert
        Assert.Equal("Information", configuration["Logging:LogLevel:Default"]);
        Assert.Equal("Warning", configuration["Logging:LogLevel:Microsoft"]);
    }

    [Fact]
    public void Load_Array_ReturnsCorrectValues()
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
        var configuration = new ConfigurationBuilder()
            .AddEFCoreJsonConfiguration(options => options.UseInMemoryDatabase(_testDbName))
            .Build();

        // Assert
        Assert.Equal("localhost", configuration["AllowedHosts:0"]);
        Assert.Equal("example.com", configuration["AllowedHosts:1"]);
        Assert.Equal("*.domain.com", configuration["AllowedHosts:2"]);
    }

    [Fact]
    public void Load_ArrayOfObjects_ReturnsCorrectValues()
    {
        // Arrange
        using (var context = new ConfigurationDbContext(_options))
        {
            context.ConfigurationSettings.Add(new ConfigurationSetting
            {
                Key = "Endpoints",
                JsonValue = """
                    [
                        {
                            "Name": "API1",
                            "Url": "https://api1.com"
                        },
                        {
                            "Name": "API2",
                            "Url": "https://api2.com"
                        }
                    ]
                    """
            });
            context.SaveChanges();
        }

        // Act
        var configuration = new ConfigurationBuilder()
            .AddEFCoreJsonConfiguration(options => options.UseInMemoryDatabase(_testDbName))
            .Build();

        // Assert
        Assert.Equal("API1", configuration["Endpoints:0:Name"]);
        Assert.Equal("https://api1.com", configuration["Endpoints:0:Url"]);
        Assert.Equal("API2", configuration["Endpoints:1:Name"]);
        Assert.Equal("https://api2.com", configuration["Endpoints:1:Url"]);
    }

    [Fact]
    public void Load_BooleanValues_ReturnsCorrectStrings()
    {
        // Arrange
        using (var context = new ConfigurationDbContext(_options))
        {
            context.ConfigurationSettings.Add(new ConfigurationSetting
            {
                Key = "Features",
                JsonValue = """
                    {
                        "EnableLogging": true,
                        "EnableCache": false
                    }
                    """
            });
            context.SaveChanges();
        }

        // Act
        var configuration = new ConfigurationBuilder()
            .AddEFCoreJsonConfiguration(options => options.UseInMemoryDatabase(_testDbName))
            .Build();

        // Assert
        Assert.Equal("True", configuration["Features:EnableLogging"]);
        Assert.Equal("False", configuration["Features:EnableCache"]);
    }

    [Fact]
    public void Load_NumericValues_ReturnsCorrectStrings()
    {
        // Arrange
        using (var context = new ConfigurationDbContext(_options))
        {
            context.ConfigurationSettings.Add(new ConfigurationSetting
            {
                Key = "Settings",
                JsonValue = """
                    {
                        "Timeout": 30,
                        "MaxRetries": 5,
                        "Multiplier": 1.5
                    }
                    """
            });
            context.SaveChanges();
        }

        // Act
        var configuration = new ConfigurationBuilder()
            .AddEFCoreJsonConfiguration(options => options.UseInMemoryDatabase(_testDbName))
            .Build();

        // Assert
        Assert.Equal("30", configuration["Settings:Timeout"]);
        Assert.Equal("5", configuration["Settings:MaxRetries"]);
        Assert.Equal("1.5", configuration["Settings:Multiplier"]);
    }

    [Fact]
    public void Load_EmptyDatabase_ReturnsEmptyConfiguration()
    {
        // Act
        var configuration = new ConfigurationBuilder()
            .AddEFCoreJsonConfiguration(options => options.UseInMemoryDatabase(_testDbName))
            .Build();

        // Assert
        Assert.Null(configuration["NonExistent"]);
    }

    [Fact]
    public void Load_MultipleSettings_AllSettingsLoaded()
    {
        // Arrange
        using (var context = new ConfigurationDbContext(_options))
        {
            context.ConfigurationSettings.AddRange(
                new ConfigurationSetting { Key = "Setting1", JsonValue = "\"Value1\"" },
                new ConfigurationSetting { Key = "Setting2", JsonValue = "\"Value2\"" },
                new ConfigurationSetting { Key = "Setting3", JsonValue = "\"Value3\"" }
            );
            context.SaveChanges();
        }

        // Act
        var configuration = new ConfigurationBuilder()
            .AddEFCoreJsonConfiguration(options => options.UseInMemoryDatabase(_testDbName))
            .Build();

        // Assert
        Assert.Equal("Value1", configuration["Setting1"]);
        Assert.Equal("Value2", configuration["Setting2"]);
        Assert.Equal("Value3", configuration["Setting3"]);
    }

    [Fact]
    public void Load_InvalidJson_TreatsAsPlainString()
    {
        // Arrange
        using (var context = new ConfigurationDbContext(_options))
        {
            context.ConfigurationSettings.Add(new ConfigurationSetting
            {
                Key = "InvalidJson",
                JsonValue = "not valid json"
            });
            context.SaveChanges();
        }

        // Act
        var configuration = new ConfigurationBuilder()
            .AddEFCoreJsonConfiguration(options => options.UseInMemoryDatabase(_testDbName))
            .Build();

        // Assert
        Assert.Equal("not valid json", configuration["InvalidJson"]);
    }

    [Fact]
    public void AddEFCoreJsonConfiguration_WithConnectionString_WorksCorrectly()
    {
        // Arrange
        var tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        var connectionString = $"Data Source={tempDbPath}";

        try
        {
            using (var context = new ConfigurationDbContext(
                new DbContextOptionsBuilder<ConfigurationDbContext>()
                    .UseSqlite(connectionString)
                    .Options))
            {
                context.Database.EnsureCreated();
                context.ConfigurationSettings.Add(new ConfigurationSetting
                {
                    Key = "TestKey",
                    JsonValue = "\"TestValue\""
                });
                context.SaveChanges();
            }

            // Act
            var configuration = new ConfigurationBuilder()
                .AddEFCoreJsonConfiguration(connectionString)
                .Build();

            // Assert
            Assert.Equal("TestValue", configuration["TestKey"]);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempDbPath))
            {
                File.Delete(tempDbPath);
            }
        }
    }
}
