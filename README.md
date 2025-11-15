# Custom Configuration Provider for .NET 8

This project demonstrates how to create a custom IConfiguration provider in .NET 8 that fetches settings from an internal async service and flattens nested objects into configuration key-value pairs.

## Key Features

- **Custom async configuration provider** - Loads settings from async sources
- **Nested object flattening** - Automatically converts complex objects to flat configuration keys
- **Override priority** - Custom configuration overrides all other sources (appsettings, environment variables, etc.)
- **Configuration reloading** - Supports both manual and automatic periodic reloading
- **Seamless integration** - Works with .NET's standard IConfiguration system

## Project Structure

- **ConfigurationProvider.WebApi** - .NET 8 Web API using traditional Program.cs and Startup.cs pattern
- **ConfigurationProvider.Tests** - xUnit test project for testing the custom configuration provider

## Key Components

### 1. Custom Configuration Provider
Located in `ConfigurationProvider.WebApi/Configuration/`:

- **AsyncObjectConfigurationProvider.cs** - The custom configuration provider that loads settings from an async source with reload support
- **AsyncObjectConfigurationSource.cs** - The configuration source that creates the provider
- **AsyncObjectConfigurationExtensions.cs** - Extension methods for easy integration
- **ConfigurationReloadService.cs** - Service to manage manual configuration reloading

### 2. Settings Models
Located in `ConfigurationProvider.WebApi/Models/`:

- **NotificationsSettings.cs** - Example settings class with nested properties
- **UserNotificationSettings.cs** - Nested settings class

### 3. Mock Service
Located in `ConfigurationProvider.WebApi/Services/`:

- **ISettingsService.cs** - Interface for settings service
- **MockSettingsService.cs** - Mock implementation that simulates async settings retrieval

## How It Works

The custom configuration provider:
1. Accepts a `Func<Task<object>>` that returns settings asynchronously
2. Flattens nested object properties into configuration keys using colon (`:`) notation
3. Integrates seamlessly with .NET's IConfiguration system

Example:
```csharp
new NotificationsSettings
{
    Enabled = true,
    UserSettings = new UserNotificationSettings
    {
        UseMail = false
    }
}
```

Becomes configuration keys:
- `Notifications:Enabled` = "True"
- `Notifications:UserSettings:UseMail` = "False"

## Usage

### Adding the Configuration Provider

In `Program.cs`:
```csharp
Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        // Add custom configuration provider that fetches settings from internal service
        // This is added AFTER all default sources (appsettings, user secrets, env vars, command line)
        // so it will override all other configuration sources
        var settingsService = new MockSettingsService();
        config.AddAsyncObjectConfiguration(() => settingsService.GetSettingsAsync());
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
    });
```

**Important:** The custom configuration provider is added in `ConfigureAppConfiguration`, which runs after all default configuration sources. This means it will **override** values from:
- appsettings.json
- appsettings.{Environment}.json
- User secrets (in Development)
- Environment variables
- Command-line arguments

### Accessing Configuration

In a controller:
```csharp
// Get individual value
var enabled = _configuration["Notifications:Enabled"];

// Bind to strongly-typed object
var settings = new NotificationsSettings();
_configuration.GetSection("Notifications").Bind(settings);
```

## Configuration Reloading

The custom configuration provider supports both manual and automatic reloading:

### Manual Reload

You can manually trigger a configuration reload using the `IConfigurationReloadService`:

```csharp
public class MyService
{
    private readonly IConfigurationReloadService _reloadService;
    
    public MyService(IConfigurationReloadService reloadService)
    {
        _reloadService = reloadService;
    }
    
    public async Task RefreshConfiguration()
    {
        await _reloadService.ReloadAsync();
    }
}
```

Or via the API endpoint:
```bash
curl -X POST http://localhost:5090/api/configuration/reload
```

### Automatic Periodic Reload

Configure automatic periodic reloading by specifying a reload interval:

```csharp
Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        var service = new MockSettingsService();
        // Reload configuration every 5 minutes
        config.AddAsyncObjectConfiguration(
            () => service.GetSettingsAsync(),
            reloadInterval: TimeSpan.FromMinutes(5));
    })
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
```

### Change Notifications

The configuration provider properly implements change notifications, so any code using `IOptionsMonitor<T>` or `IConfiguration.GetReloadToken()` will be notified when configuration changes:

```csharp
public class MyService
{
    public MyService(IOptionsMonitor<NotificationsSettings> optionsMonitor)
    {
        optionsMonitor.OnChange(settings =>
        {
            // This will be called when configuration is reloaded
            Console.WriteLine($"Settings changed: Enabled={settings.Enabled}");
        });
    }
}
```

## API Endpoints

The sample application provides the following endpoints:

- `GET /api/configuration/all` - Returns all configuration key-value pairs
- `GET /api/configuration/notifications` - Returns bound NotificationsSettings object
- `GET /api/configuration/value/{key}` - Returns specific configuration value by key
- `POST /api/configuration/reload` - Manually triggers configuration reload

## Running the Application

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Run the application
cd ConfigurationProvider.WebApi
dotnet run
```

The application will start at `http://localhost:5090` by default.

## Testing Examples

```bash
# Get all configuration
curl http://localhost:5090/api/configuration/all

# Get notifications settings
curl http://localhost:5090/api/configuration/notifications

# Get specific value
curl http://localhost:5090/api/configuration/value/Notifications:Enabled

# Trigger configuration reload
curl -X POST http://localhost:5090/api/configuration/reload
```

## Implementation Details

### Configuration Override Behavior

The custom configuration provider is designed to override all other configuration sources. Here's how it works:

**Configuration Source Order:**
1. appsettings.json (loaded by CreateDefaultBuilder)
2. appsettings.{Environment}.json (loaded by CreateDefaultBuilder)
3. User secrets (in Development environment)
4. Environment variables
5. Command-line arguments
6. **Custom configuration provider** ← Added last, overrides everything above

**Example Override:**
```
appsettings.json:         Notifications:Enabled = false
Custom configuration:     Notifications:Enabled = true
Final value:              true (custom config wins)
```

This ensures your internal service settings always take precedence over any other configuration source, making it ideal for centralized configuration management.

### Traditional Startup Pattern

The project uses the traditional .NET startup pattern with:
- `Program.cs` with `Main()` method and `CreateHostBuilder()`
- `Startup.cs` with `ConfigureServices()` and `Configure()` methods

### Nested Object Flattening

The provider uses reflection to recursively flatten nested objects:
- Simple types (primitives, strings, etc.) are converted to strings
- Complex types are recursively processed
- Null values are preserved
- Keys use colon (`:`) as separator (Microsoft.Extensions.Configuration standard)

## Tests

The test project includes comprehensive tests for:
- Simple object flattening
- Nested object flattening
- Complex object with multiple levels
- Binding to strongly-typed objects
- Async loading verification
- **Configuration override behavior** (verifies custom config overrides appsettings and environment variables)
- **Multi-source configuration order** (validates precedence rules)
- **Manual configuration reload** (verifies ReloadAsync updates configuration)
- **Automatic periodic reload** (validates timer-based reloading)
- **Change notifications** (ensures IConfiguration.GetReloadToken() works correctly)
- **Resource cleanup** (verifies Dispose stops periodic reload)

All 12 tests pass successfully, verifying the custom configuration provider works correctly including reload functionality.
