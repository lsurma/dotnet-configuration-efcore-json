namespace ConfigurationEFCoreJson;

/// <summary>
/// Entity representing a configuration setting stored in the database
/// </summary>
public class ConfigurationSetting
{
    /// <summary>
    /// Unique identifier for the setting
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The key name for the configuration setting
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The JSON value for the configuration setting
    /// </summary>
    public string JsonValue { get; set; } = string.Empty;
}
