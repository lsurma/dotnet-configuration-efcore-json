namespace ConfigurationProvider.WebApi.Services;

public interface ISettingsService
{
    Task<object> GetSettingsAsync();
}
