using Microsoft.EntityFrameworkCore;

namespace ConfigurationEFCoreJson;

/// <summary>
/// DbContext for accessing configuration settings
/// </summary>
public class ConfigurationDbContext : DbContext
{
    public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configuration settings table
    /// </summary>
    public DbSet<ConfigurationSetting> ConfigurationSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ConfigurationSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(500);
            entity.HasIndex(e => e.Key)
                .IsUnique();
            entity.Property(e => e.JsonValue)
                .IsRequired();
        });
    }
}
