using ConfigurationProvider.WebApi.Models;
using ConfigurationProvider.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConfigurationProvider.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ISettingsService _settingsService;

    public ConfigurationController(IConfiguration configuration, ISettingsService settingsService)
    {
        _configuration = configuration;
        _settingsService = settingsService;
    }

    [HttpGet("all")]
    public IActionResult GetAllConfiguration()
    {
        var allSettings = new Dictionary<string, string?>();
        
        foreach (var kvp in _configuration.AsEnumerable())
        {
            allSettings[kvp.Key] = kvp.Value;
        }

        return Ok(allSettings);
    }

    [HttpGet("notifications")]
    public IActionResult GetNotificationSettings()
    {
        var notificationsSettings = new NotificationsSettings();
        _configuration.GetSection("Notifications").Bind(notificationsSettings);
        
        return Ok(notificationsSettings);
    }

    [HttpGet("value/{key}")]
    public IActionResult GetConfigurationValue(string key)
    {
        var value = _configuration[key];
        
        if (value == null)
        {
            return NotFound(new { message = $"Configuration key '{key}' not found" });
        }

        return Ok(new { key, value });
    }

    [HttpPost("reload")]
    public async Task<IActionResult> ReloadConfiguration()
    {
        // Reload settings from the source (e.g., database) and SetData will automatically trigger reload
        await _settingsService.ReloadSettingsAsync();
        return Ok(new { message = "Configuration reloaded successfully" });
    }
}
