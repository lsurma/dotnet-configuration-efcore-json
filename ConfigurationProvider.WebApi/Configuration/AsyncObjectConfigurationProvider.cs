using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace ConfigurationProvider.WebApi.Configuration;

public class AsyncObjectConfigurationProvider : Microsoft.Extensions.Configuration.ConfigurationProvider, IDisposable
{
    private readonly Func<Task<object>> _settingsFactory;
    private readonly TimeSpan? _reloadInterval;
    private Timer? _reloadTimer;
    private bool _disposed;

    public AsyncObjectConfigurationProvider(Func<Task<object>> settingsFactory, TimeSpan? reloadInterval = null)
    {
        _settingsFactory = settingsFactory ?? throw new ArgumentNullException(nameof(settingsFactory));
        _reloadInterval = reloadInterval;
    }

    public override void Load()
    {
        // Since Load() is synchronous, we need to call the async method synchronously
        var settings = _settingsFactory().GetAwaiter().GetResult();
        
        if (settings != null)
        {
            Data = FlattenObject(settings);
        }

        // Start periodic reload timer if interval is specified
        if (_reloadInterval.HasValue && _reloadTimer == null)
        {
            _reloadTimer = new Timer(
                callback: _ => ReloadAsync().GetAwaiter().GetResult(),
                state: null,
                dueTime: _reloadInterval.Value,
                period: _reloadInterval.Value);
        }
    }

    /// <summary>
    /// Manually reload the configuration from the settings source.
    /// </summary>
    public async Task ReloadAsync()
    {
        if (_disposed)
            return;

        var settings = await _settingsFactory();
        
        if (settings != null)
        {
            Data = FlattenObject(settings);
            // Notify the configuration system that data has changed
            OnReload();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _reloadTimer?.Dispose();
            _disposed = true;
        }
    }

    private Dictionary<string, string?> FlattenObject(object obj, string prefix = "")
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        
        if (obj == null)
            return result;

        var objectType = obj.GetType();
        var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}:{property.Name}";

            if (value == null)
            {
                result[key] = null;
            }
            else if (IsSimpleType(property.PropertyType))
            {
                result[key] = value.ToString();
            }
            else if (property.PropertyType.IsClass)
            {
                // Recursively flatten nested objects
                var nestedValues = FlattenObject(value, key);
                foreach (var kvp in nestedValues)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
        }

        return result;
    }

    private bool IsSimpleType(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(Guid);
    }
}
