using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ConfigurationEFCoreJson;

/// <summary>
/// Remote configuration provider that loads settings from a DbContext
/// </summary>
/// <typeparam name="TContext">The DbContext type that contains ConfigurationSettings</typeparam>
public class DbContextConfigurationProvider<TContext> : IRemoteConfigurationProvider 
    where TContext : DbContext
{
    private readonly TContext _context;

    public DbContextConfigurationProvider(TContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IDictionary<string, string?> LoadConfiguration()
    {
        var data = new Dictionary<string, string?>();

        // Ensure database is created
        _context.Database.EnsureCreated();

        // Get ConfigurationSettings DbSet using reflection
        var settingsProperty = _context.GetType().GetProperty("ConfigurationSettings");
        if (settingsProperty == null)
        {
            throw new InvalidOperationException(
                $"DbContext type '{typeof(TContext).Name}' must have a 'ConfigurationSettings' property of type DbSet<ConfigurationSetting>");
        }

        var dbSet = settingsProperty.GetValue(_context) as IQueryable<ConfigurationSetting>;
        if (dbSet == null)
        {
            throw new InvalidOperationException(
                "Failed to retrieve ConfigurationSettings from DbContext");
        }

        var settings = dbSet.ToList();

        foreach (var setting in settings)
        {
            try
            {
                // Parse JSON and flatten it into configuration keys
                FlattenJsonToConfiguration(setting.Key, setting.JsonValue, data);
            }
            catch (JsonException)
            {
                // If JSON parsing fails, treat as plain string value
                data[setting.Key] = setting.JsonValue;
            }
        }

        return data;
    }

    private void FlattenJsonToConfiguration(string prefix, string jsonValue, IDictionary<string, string?> data)
    {
        if (string.IsNullOrWhiteSpace(jsonValue))
        {
            data[prefix] = string.Empty;
            return;
        }

        // Try to parse as JSON
        using var document = JsonDocument.Parse(jsonValue);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Object)
        {
            // Process as object with nested properties
            FlattenJsonObject(prefix, root, data);
        }
        else if (root.ValueKind == JsonValueKind.Array)
        {
            // Process as array
            FlattenJsonArray(prefix, root, data);
        }
        else
        {
            // Primitive value
            data[prefix] = GetValueAsString(root);
        }
    }

    private void FlattenJsonObject(string prefix, JsonElement element, IDictionary<string, string?> data)
    {
        foreach (var property in element.EnumerateObject())
        {
            var key = string.IsNullOrEmpty(prefix) 
                ? property.Name 
                : $"{prefix}:{property.Name}";

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                FlattenJsonObject(key, property.Value, data);
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                FlattenJsonArray(key, property.Value, data);
            }
            else
            {
                data[key] = GetValueAsString(property.Value);
            }
        }
    }

    private void FlattenJsonArray(string prefix, JsonElement element, IDictionary<string, string?> data)
    {
        int index = 0;
        foreach (var item in element.EnumerateArray())
        {
            var key = $"{prefix}:{index}";

            if (item.ValueKind == JsonValueKind.Object)
            {
                FlattenJsonObject(key, item, data);
            }
            else if (item.ValueKind == JsonValueKind.Array)
            {
                FlattenJsonArray(key, item, data);
            }
            else
            {
                data[key] = GetValueAsString(item);
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
