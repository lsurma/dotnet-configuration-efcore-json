# Configuration EF Core JSON

A .NET 8 library that provides an IConfiguration provider for Entity Framework Core with SQLite, allowing you to store configuration settings as JSON in a database and access them through the standard `IConfiguration` interface.

## Features

- ✅ Store configuration settings in SQLite database as JSON
- ✅ Full support for nested objects and arrays
- ✅ Compatible with .NET's `IConfiguration` interface
- ✅ Automatic JSON flattening to configuration keys
- ✅ Simple and intuitive API
- ✅ Entity Framework Core integration

## Installation

Add the library project to your solution or use the NuGet package (if published):

```bash
dotnet add package ConfigurationEFCoreJson
```

## Quick Start

### 1. Add the required NuGet packages

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.Extensions.Configuration
```

### 2. Use the configuration provider

```csharp
using ConfigurationEFCoreJson;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddEFCoreJsonConfiguration("Data Source=config.db")
    .Build();

// Access configuration values
var appName = configuration["AppName"];
var dbHost = configuration["Database:Host"];
var dbPort = configuration["Database:Port"];
```

### 3. Seed configuration data

```csharp
using ConfigurationEFCoreJson;
using Microsoft.EntityFrameworkCore;

var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>();
optionsBuilder.UseSqlite("Data Source=config.db");

using var context = new ConfigurationDbContext(optionsBuilder.Options);
context.Database.EnsureCreated();

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
    }
);

await context.SaveChangesAsync();
```

## Usage Examples

### Simple String Values

```csharp
// Database entry
Key: "AppName"
JsonValue: "\"My Application\""

// Access in code
var appName = configuration["AppName"]; // "My Application"
```

### Nested Objects

```csharp
// Database entry
Key: "Database"
JsonValue: {
    "Host": "localhost",
    "Port": 5432,
    "Name": "mydb"
}

// Access in code
var host = configuration["Database:Host"];     // "localhost"
var port = configuration["Database:Port"];     // "5432"
var name = configuration["Database:Name"];     // "mydb"
```

### Deeply Nested Objects

```csharp
// Database entry
Key: "Logging"
JsonValue: {
    "LogLevel": {
        "Default": "Information",
        "Microsoft": "Warning"
    }
}

// Access in code
var defaultLevel = configuration["Logging:LogLevel:Default"];      // "Information"
var microsoftLevel = configuration["Logging:LogLevel:Microsoft"];  // "Warning"
```

### Arrays

```csharp
// Database entry
Key: "AllowedHosts"
JsonValue: ["localhost", "example.com", "*.mydomain.com"]

// Access in code
var host0 = configuration["AllowedHosts:0"];  // "localhost"
var host1 = configuration["AllowedHosts:1"];  // "example.com"
var host2 = configuration["AllowedHosts:2"];  // "*.mydomain.com"
```

### Arrays of Objects

```csharp
// Database entry
Key: "ApiEndpoints"
JsonValue: [
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

// Access in code
var name0 = configuration["ApiEndpoints:0:Name"];      // "Users API"
var url0 = configuration["ApiEndpoints:0:Url"];        // "https://api.example.com/users"
var timeout0 = configuration["ApiEndpoints:0:Timeout"]; // "30"
var name1 = configuration["ApiEndpoints:1:Name"];      // "Orders API"
```

## API Reference

### Extension Methods

#### `AddEFCoreJsonConfiguration(Action<DbContextOptionsBuilder>)`

Adds an EF Core JSON configuration source with custom DbContext options.

```csharp
var configuration = new ConfigurationBuilder()
    .AddEFCoreJsonConfiguration(options => 
        options.UseSqlite("Data Source=config.db"))
    .Build();
```

#### `AddEFCoreJsonConfiguration(string connectionString)`

Adds an EF Core JSON configuration source using SQLite with a connection string.

```csharp
var configuration = new ConfigurationBuilder()
    .AddEFCoreJsonConfiguration("Data Source=config.db")
    .Build();
```

### Classes

#### `ConfigurationSetting`

Entity representing a configuration setting in the database.

**Properties:**
- `Id` (int): Unique identifier
- `Key` (string): Configuration key name
- `JsonValue` (string): JSON value for the configuration

#### `ConfigurationDbContext`

DbContext for accessing configuration settings.

**DbSet:**
- `ConfigurationSettings`: Collection of configuration settings

#### `EFCoreJsonConfigurationProvider`

Configuration provider that reads settings from EF Core database and flattens JSON to configuration keys.

#### `EFCoreJsonConfigurationSource`

Configuration source for EF Core JSON configuration provider.

## How It Works

1. **Storage**: Configuration settings are stored in a SQLite database with a key-value structure where values are JSON strings.

2. **JSON Parsing**: When the configuration is loaded, each JSON value is parsed and flattened into the standard .NET configuration key format using colons (`:`) as separators.

3. **Nested Support**: 
   - Nested objects become colon-separated keys (e.g., `Database:Host`)
   - Arrays use numeric indices (e.g., `AllowedHosts:0`)
   - Complex nested structures are fully supported

4. **Type Conversion**: JSON values are converted to strings:
   - Strings: Stored as-is
   - Numbers: Converted to string representation
   - Booleans: Converted to "True" or "False"
   - Null: Converted to empty string

## Running the Sample

The repository includes a sample console application demonstrating the usage:

```bash
cd samples/ConfigurationEFCoreJson.Sample
dotnet run
```

## Running Tests

The project includes comprehensive unit tests:

```bash
dotnet test
```

## Project Structure

```
.
├── src/
│   └── ConfigurationEFCoreJson/        # Main library project
│       ├── ConfigurationSetting.cs
│       ├── ConfigurationDbContext.cs
│       ├── EFCoreJsonConfigurationProvider.cs
│       ├── EFCoreJsonConfigurationSource.cs
│       └── EFCoreJsonConfigurationExtensions.cs
├── samples/
│   └── ConfigurationEFCoreJson.Sample/ # Sample console application
│       └── Program.cs
├── tests/
│   └── ConfigurationEFCoreJson.Tests/  # Unit tests
│       └── EFCoreJsonConfigurationProviderTests.cs
└── ConfigurationEFCoreJson.sln         # Solution file
```

## Requirements

- .NET 8.0 or later
- Entity Framework Core 8.0 or later
- SQLite support

## License

This project is provided as-is for educational and commercial use.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.
