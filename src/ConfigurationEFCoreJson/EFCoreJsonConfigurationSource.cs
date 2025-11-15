using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConfigurationEFCoreJson;

/// <summary>
/// Configuration source for EF Core JSON configuration provider
/// </summary>
public class EFCoreJsonConfigurationSource : IConfigurationSource
{
    private readonly Action<DbContextOptionsBuilder> _optionsAction;

    public EFCoreJsonConfigurationSource(Action<DbContextOptionsBuilder> optionsAction)
    {
        _optionsAction = optionsAction ?? throw new ArgumentNullException(nameof(optionsAction));
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new EFCoreJsonConfigurationProvider(_optionsAction);
    }
}
