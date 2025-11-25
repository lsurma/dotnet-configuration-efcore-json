using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace ConfigurationProvider.Core.Configuration;

public class AsyncObjectConfigurationProvider : Microsoft.Extensions.Configuration.ConfigurationProvider
{
    private static IEnumerable<ISettings>? _settingsData;
    private static AsyncObjectConfigurationProvider? _self;
    private static readonly object _lock = new();

    public AsyncObjectConfigurationProvider()
    {
        // Register this instance
        lock (_lock)
        {
            if (_self != null)
            {
                throw new InvalidOperationException("Only one instance of AsyncObjectConfigurationProvider is allowed.");
            }
            
            _self = this;
        }
    }

    /// <summary>
    /// Sets the settings data that will be used by the configuration provider.
    /// This is typically called by a settings service after DI container is initialized.
    /// This method automatically triggers reload on all registered provider instances.
    /// </summary>
    /// <param name="settings">Collection of settings objects that implement ISettings interface</param>
    public static void SetData(IEnumerable<ISettings> settings)
    {
        lock (_lock)
        {
            _settingsData = settings;

            // Trigger reload on all provider instances
            _self!.Load();
        }
    }

    /// <summary>
    /// Clears the static settings data and triggers reload on all provider instances. Useful for testing.
    /// </summary>
    public static void ClearData()
    {
        lock (_lock)
        {
            _settingsData = null;

            // Trigger reload on all provider instances
            _self!.Load();
        }
    }

    public override void Load()
    {
        lock (_lock)
        {
            if (_settingsData != null)
            {
                Data = ConvertSettingsToConfigurationData(_settingsData);
            }
            else
            {
                // Provider is empty until SetData is called
                Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            }

            OnReload();
        }
    }

    /// <summary>
    /// Converts settings collection to configuration data by serializing each setting object to JSON
    /// and then parsing it to flatten the structure (similar to JsonConfigurationFileParser).
    /// </summary>
    private Dictionary<string, string?> ConvertSettingsToConfigurationData(IEnumerable<ISettings> settings)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var setting in settings)
        {
            var settingName = setting.SectionName;
            var settingObject = setting;

            // Serialize the object to JSON
            var concreteType = setting.GetType();
            var json = JsonSerializer.Serialize(setting, concreteType, new JsonSerializerOptions
            {
                WriteIndented = false,
                AllowDuplicateProperties = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            });

            // Parse JSON to flattened dictionary using JsonConfigurationFileParser approach
            var flattenedData = ParseJsonToConfigurationData(json);

            // Add all flattened entries with the setting name as prefix
            foreach (var entry in flattenedData)
            {
                var key = string.IsNullOrEmpty(entry.Key)
                    ? settingName
                    : $"{settingName}:{entry.Key}";
                result[key] = entry.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// Parses JSON string to configuration data dictionary.
    /// This mimics the behavior of JsonConfigurationFileParser.
    /// </summary>
    private Dictionary<string, string?> ParseJsonToConfigurationData(string json)
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        using var jsonDocument = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
            AllowDuplicateProperties = false,
            
        });

        if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
        {
            VisitElement(jsonDocument.RootElement, string.Empty, data);
        }

        return data;
    }

    private void VisitElement(JsonElement element, string path, Dictionary<string, string?> data)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var propertyPath = string.IsNullOrEmpty(path)
                        ? property.Name
                        : $"{path}:{property.Name}";
                    VisitElement(property.Value, propertyPath, data);
                }
                break;

            case JsonValueKind.Array:
                var index = 0;
                foreach (var arrayElement in element.EnumerateArray())
                {
                    var arrayPath = $"{path}:{index}";
                    VisitElement(arrayElement, arrayPath, data);
                    index++;
                }
                break;

            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
                data[path] = element.ToString();
                break;

            case JsonValueKind.Null:
                data[path] = null;
                break;
        }
    }
}
