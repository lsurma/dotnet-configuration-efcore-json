using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ConfigurationEFCoreJson;

/// <summary>
/// Configuration provider that reads settings from EF Core database
/// </summary>
public class EFCoreJsonConfigurationProvider : ConfigurationProvider
{
    private readonly Action<DbContextOptionsBuilder> _optionsAction;

    public EFCoreJsonConfigurationProvider(Action<DbContextOptionsBuilder> optionsAction)
    {
        _optionsAction = optionsAction ?? throw new ArgumentNullException(nameof(optionsAction));
    }

    public override void Load()
    {
        var builder = new DbContextOptionsBuilder<ConfigurationDbContext>();
        _optionsAction(builder);

        using var context = new ConfigurationDbContext(builder.Options);
        
        // Ensure database is created
        context.Database.EnsureCreated();

        // Load all settings from database
        var settings = context.ConfigurationSettings.ToList();

        Data.Clear();

        foreach (var setting in settings)
        {
            try
            {
                // Parse JSON and flatten it into configuration keys
                FlattenJsonToConfiguration(setting.Key, setting.JsonValue);
            }
            catch (JsonException)
            {
                // If JSON parsing fails, treat as plain string value
                Data[setting.Key] = setting.JsonValue;
            }
        }
    }

    private void FlattenJsonToConfiguration(string prefix, string jsonValue)
    {
        if (string.IsNullOrWhiteSpace(jsonValue))
        {
            Data[prefix] = string.Empty;
            return;
        }

        // Try to parse as JSON
        using var document = JsonDocument.Parse(jsonValue);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Object)
        {
            // Process as object with nested properties
            FlattenJsonObject(prefix, root);
        }
        else if (root.ValueKind == JsonValueKind.Array)
        {
            // Process as array
            FlattenJsonArray(prefix, root);
        }
        else
        {
            // Primitive value
            Data[prefix] = GetValueAsString(root);
        }
    }

    private void FlattenJsonObject(string prefix, JsonElement element)
    {
        foreach (var property in element.EnumerateObject())
        {
            var key = string.IsNullOrEmpty(prefix) 
                ? property.Name 
                : $"{prefix}:{property.Name}";

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                FlattenJsonObject(key, property.Value);
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                FlattenJsonArray(key, property.Value);
            }
            else
            {
                Data[key] = GetValueAsString(property.Value);
            }
        }
    }

    private void FlattenJsonArray(string prefix, JsonElement element)
    {
        int index = 0;
        foreach (var item in element.EnumerateArray())
        {
            var key = $"{prefix}:{index}";

            if (item.ValueKind == JsonValueKind.Object)
            {
                FlattenJsonObject(key, item);
            }
            else if (item.ValueKind == JsonValueKind.Array)
            {
                FlattenJsonArray(key, item);
            }
            else
            {
                Data[key] = GetValueAsString(item);
            }

            index++;
        }
    }

    private static string GetValueAsString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "True",
            JsonValueKind.False => "False",
            JsonValueKind.Null => string.Empty,
            _ => element.GetRawText()
        };
    }
}
