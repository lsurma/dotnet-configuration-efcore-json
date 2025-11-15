# Custom Configuration Provider for .NET 8

This project demonstrates how to create a custom IConfiguration provider in .NET 8 that fetches settings from an internal async service and flattens nested objects into configuration key-value pairs.

## Project Structure

- **ConfigurationProvider.WebApi** - .NET 8 Web API using traditional Program.cs and Startup.cs pattern
- **ConfigurationProvider.Tests** - xUnit test project for testing the custom configuration provider

## Key Components

### 1. Custom Configuration Provider
Located in `ConfigurationProvider.WebApi/Configuration/`:

- **AsyncObjectConfigurationProvider.cs** - The custom configuration provider that loads settings from an async source
- **AsyncObjectConfigurationSource.cs** - The configuration source that creates the provider
- **AsyncObjectConfigurationExtensions.cs** - Extension methods for easy integration

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
        var settingsService = new MockSettingsService();
        config.AddAsyncObjectConfiguration(() => settingsService.GetSettingsAsync());
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
    });
```

### Accessing Configuration

In a controller:
```csharp
// Get individual value
var enabled = _configuration["Notifications:Enabled"];

// Bind to strongly-typed object
var settings = new NotificationsSettings();
_configuration.GetSection("Notifications").Bind(settings);
```

## API Endpoints

The sample application provides the following endpoints:

- `GET /api/configuration/all` - Returns all configuration key-value pairs
- `GET /api/configuration/notifications` - Returns bound NotificationsSettings object
- `GET /api/configuration/value/{key}` - Returns specific configuration value by key

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
```

## Implementation Details

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

The test project includes comprehensive tests:
- Simple object flattening
- Nested object flattening
- Complex object with multiple levels
- Binding to strongly-typed objects
- Async loading verification

All tests pass successfully, verifying the custom configuration provider works correctly.
