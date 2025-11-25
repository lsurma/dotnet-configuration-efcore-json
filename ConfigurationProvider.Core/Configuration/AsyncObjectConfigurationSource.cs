using Microsoft.Extensions.Configuration;

namespace ConfigurationProvider.Core.Configuration;

public class AsyncObjectConfigurationSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new AsyncObjectConfigurationProvider();
    }
}
