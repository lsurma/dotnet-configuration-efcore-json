using ConfigurationProvider.Core.Configuration;

namespace ConfigurationProvider.WebApi.Models;

public class GeneralSettings : ISettings
{
    public string SectionName => "General";

    public Uri? ApplicationUrl { get; set; } = null;
}