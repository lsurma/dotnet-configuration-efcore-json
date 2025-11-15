using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConfigurationEFCoreJson;

/// <summary>
/// Extension methods for adding remote configuration sources
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds a remote configuration source to the configuration builder
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="remoteProvider">The remote configuration provider</param>
    /// <returns>The configuration builder</returns>
    public static IConfigurationBuilder AddRemoteConfig(
        this IConfigurationBuilder builder,
        IRemoteConfigurationProvider remoteProvider)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        
        if (remoteProvider == null)
            throw new ArgumentNullException(nameof(remoteProvider));

        return builder.Add(new RemoteConfigurationSource(remoteProvider));
    }

    /// <summary>
    /// Adds a remote configuration source using a DbContext
    /// </summary>
    /// <typeparam name="TContext">The DbContext type that contains ConfigurationSettings</typeparam>
    /// <param name="builder">The configuration builder</param>
    /// <param name="context">The DbContext instance</param>
    /// <returns>The configuration builder</returns>
    public static IConfigurationBuilder AddRemoteConfig<TContext>(
        this IConfigurationBuilder builder,
        TContext context) 
        where TContext : DbContext
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        return builder.AddRemoteConfig(new DbContextConfigurationProvider<TContext>(context));
    }

    #region Backward Compatibility Methods

    /// <summary>
    /// Adds an EF Core JSON configuration source to the configuration builder
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="optionsAction">Action to configure the DbContext options</param>
    /// <returns>The configuration builder</returns>
    [Obsolete("Use AddRemoteConfig with IRemoteConfigurationProvider instead")]
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
    [Obsolete("Use AddRemoteConfig with IRemoteConfigurationProvider instead")]
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

    #endregion
}
