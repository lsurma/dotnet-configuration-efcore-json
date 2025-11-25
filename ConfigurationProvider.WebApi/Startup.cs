using ConfigurationProvider.WebApi.Services;

namespace ConfigurationProvider.WebApi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Register settings service that will populate configuration provider
        services.AddSingleton<ISettingsService, MockSettingsService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISettingsService settingsService)
    {
        // Load settings from database and populate configuration provider
        // This happens after DI container is initialized, so we can use services with dependencies
        settingsService.LoadAndPopulateConfigurationAsync().GetAwaiter().GetResult();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
