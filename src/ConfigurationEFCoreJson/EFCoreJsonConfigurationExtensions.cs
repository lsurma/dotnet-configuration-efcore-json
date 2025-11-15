using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConfigurationEFCoreJson;

/// <summary>
/// Extension methods for adding EF Core JSON configuration
/// </summary>
public static class EFCoreJsonConfigurationExtensions
{
    /// <summary>
    /// Adds an EF Core JSON configuration source to the configuration builder
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="optionsAction">Action to configure the DbContext options</param>
    /// <returns>The configuration builder</returns>
    public static IConfigurationBuilder AddEFCoreJsonConfiguration(
        this IConfigurationBuilder builder,
        Action<DbContextOptionsBuilder> optionsAction)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        
        if (optionsAction == null)
            throw new ArgumentNullException(nameof(optionsAction));

        return builder.Add(new EFCoreJsonConfigurationSource(optionsAction));
    }

    /// <summary>
    /// Adds an EF Core JSON configuration source using SQLite
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="connectionString">The SQLite connection string</param>
    /// <returns>The configuration builder</returns>
    public static IConfigurationBuilder AddEFCoreJsonConfiguration(
        this IConfigurationBuilder builder,
        string connectionString)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        return builder.AddEFCoreJsonConfiguration(options =>
            options.UseSqlite(connectionString));
    }
}
