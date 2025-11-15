using ConfigurationEFCoreJson;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// Define connection string for SQLite database
const string connectionString = "Data Source=config.db";

// Initialize database and seed some sample data
await InitializeDatabaseAsync(connectionString);

// Create DbContext for remote configuration
var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>();
optionsBuilder.UseSqlite(connectionString);
var dbContext = new ConfigurationDbContext(optionsBuilder.Options);

// Create configuration with built-in sources (appsettings, env, secrets) + remote config
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .AddRemoteConfig(dbContext)  // Add remote configuration from database
    .Build();

// Display configuration values
Console.WriteLine("=== Configuration Values from Multiple Sources ===");
Console.WriteLine();

// Simple value from database
Console.WriteLine($"AppName (from DB): {configuration["AppName"]}");
Console.WriteLine();

// Simple value from database
Console.WriteLine($"AppName (from DB): {configuration["AppName"]}");
Console.WriteLine();

// Nested object values from database
Console.WriteLine("Database Settings:");
Console.WriteLine($"  Host: {configuration["Database:Host"]}");
Console.WriteLine($"  Port: {configuration["Database:Port"]}");
Console.WriteLine($"  Name: {configuration["Database:Name"]}");
Console.WriteLine();

// Nested object with deeper nesting
Console.WriteLine("Logging Settings:");
Console.WriteLine($"  LogLevel Default: {configuration["Logging:LogLevel:Default"]}");
Console.WriteLine($"  LogLevel Microsoft: {configuration["Logging:LogLevel:Microsoft"]}");
Console.WriteLine($"  Console Enabled: {configuration["Logging:Console:Enabled"]}");
Console.WriteLine();

// Array values
Console.WriteLine("Allowed Hosts:");
Console.WriteLine($"  [0]: {configuration["AllowedHosts:0"]}");
Console.WriteLine($"  [1]: {configuration["AllowedHosts:1"]}");
Console.WriteLine($"  [2]: {configuration["AllowedHosts:2"]}");
Console.WriteLine();

// Complex nested structure with arrays of objects
Console.WriteLine("API Endpoints (from DB):");
Console.WriteLine($"  [0] Name: {configuration["ApiEndpoints:0:Name"]}");
Console.WriteLine($"  [0] Url: {configuration["ApiEndpoints:0:Url"]}");
Console.WriteLine($"  [0] Timeout: {configuration["ApiEndpoints:0:Timeout"]}");
Console.WriteLine($"  [1] Name: {configuration["ApiEndpoints:1:Name"]}");
Console.WriteLine($"  [1] Url: {configuration["ApiEndpoints:1:Url"]}");
Console.WriteLine($"  [1] Timeout: {configuration["ApiEndpoints:1:Timeout"]}");
Console.WriteLine();

// Configuration from appsettings.json (if it exists)
Console.WriteLine("Configuration from appsettings.json:");
Console.WriteLine($"  Version: {configuration["AppSettings:Version"]}");
Console.WriteLine($"  Environment: {configuration["AppSettings:Environment"]}");
Console.WriteLine($"  LocalSetting: {configuration["LocalSetting"]}");
Console.WriteLine();

Console.WriteLine("Configuration loaded successfully from multiple sources!");
Console.WriteLine("(appsettings.json, environment variables, and SQLite database)");

// Helper method to initialize database with sample data
static async Task InitializeDatabaseAsync(string connectionString)
{
    var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>();
    optionsBuilder.UseSqlite(connectionString);

    using var context = new ConfigurationDbContext(optionsBuilder.Options);
    
    // Ensure database is created
    await context.Database.EnsureCreatedAsync();

    // Clear existing data
    context.ConfigurationSettings.RemoveRange(context.ConfigurationSettings);
    await context.SaveChangesAsync();

    // Seed sample configuration data
    context.ConfigurationSettings.AddRange(
        new ConfigurationSetting
        {
            Key = "AppName",
            JsonValue = "\"My Application\""
        },
        new ConfigurationSetting
        {
            Key = "Database",
            JsonValue = """
                {
                    "Host": "localhost",
                    "Port": 5432,
                    "Name": "mydb"
                }
                """
        },
        new ConfigurationSetting
        {
            Key = "Logging",
            JsonValue = """
                {
                    "LogLevel": {
                        "Default": "Information",
                        "Microsoft": "Warning"
                    },
                    "Console": {
                        "Enabled": true
                    }
                }
                """
        },
        new ConfigurationSetting
        {
            Key = "AllowedHosts",
            JsonValue = """
                [
                    "localhost",
                    "example.com",
                    "*.mydomain.com"
                ]
                """
        },
        new ConfigurationSetting
        {
            Key = "ApiEndpoints",
            JsonValue = """
                [
                    {
                        "Name": "Users API",
                        "Url": "https://api.example.com/users",
                        "Timeout": 30
                    },
                    {
                        "Name": "Orders API",
                        "Url": "https://api.example.com/orders",
                        "Timeout": 60
                    }
                ]
                """
        }
    );

    await context.SaveChangesAsync();
    Console.WriteLine("Database initialized with sample configuration data.");
    Console.WriteLine();
}
