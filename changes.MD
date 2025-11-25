# Zmiany w implementacji Configuration Providera

## 1. Podejście wzorowane na Azure Key Vault

Zaimplementowano podejście similar to Azure Key Vault provider, gdzie:
- **Provider jest pusty na starcie** - dodajemy go do konfiguracji bez danych
- **Dane są wypełniane po inicjalizacji DI** - serwis z dostępem do DI (np. Entity Framework) wypełnia statyczne pole w providerze
- **Statyczne pole danych** - `AsyncObjectConfigurationProvider.SetData()` ustawia dane które są współdzielone przez wszystkie instancje providera

## 2. Struktura ustawień jako obiekty z JSON serializacją

Ustawienia są teraz reprezentowane jako klasy C# (np. `NotificationSettings`, `UserSettings`, `GeneralSettings`):
- Każdy obiekt ustawień jest serializowany do JSON używając `System.Text.Json`
- JSON jest następnie parsowany do płaskiego słownika (klucz:wartość) używając logiki podobnej do `JsonConfigurationFileParser`
- Parsowanie obsługuje:
  - Zagnieżdżone obiekty
  - Tablice
  - Wszystkie typy prymitywne
  - Wartości null

## 3. Zmiany w API

### AsyncObjectConfigurationProvider
- **Usunięto**: parametr `Func<Task<object>> settingsFactory` z konstruktora
- **Dodano**: `static void SetData(Dictionary<string, object> settings)` - ustawia dane konfiguracyjne
- **Dodano**: `static void ClearData()` - czyści dane (przydatne w testach)
- **Zmieniono**: `Load()` teraz czyta ze statycznego pola `_settingsData`

### AsyncObjectConfigurationExtensions
- **Usunięto**: parametr `settingsFactory` z metody `AddAsyncObjectConfiguration()`
- **Zostawiono**: opcjonalny parametr `reloadInterval`
- Metoda teraz dodaje pusty provider który czeka na wypełnienie danych przez `SetData()`

### ISettingsService
- **Zmieniono**: interfejs teraz ma metody:
  - `Task LoadAndPopulateConfigurationAsync()` - ładuje ustawienia i wypełnia provider
  - `Task ReloadSettingsAsync()` - przeładowuje ustawienia

## 4. Sposób użycia

### W Program.cs:
```csharp
.ConfigureAppConfiguration((context, config) =>
{
    // Dodaj pusty provider (bez danych)
    config.AddAsyncObjectConfiguration();
})
```

### W Startup.cs:
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Zarejestruj serwis ustawień
    services.AddSingleton<ISettingsService, MockSettingsService>();
}

public void Configure(IApplicationBuilder app, ISettingsService settingsService)
{
    // Załaduj ustawienia po inicjalizacji DI
    settingsService.LoadAndPopulateConfigurationAsync().GetAwaiter().GetResult();

    // ... reszta konfiguracji
}
```

### W serwisie (np. MockSettingsService):
```csharp
public async Task LoadAndPopulateConfigurationAsync()
{
    // Załaduj ustawienia z bazy danych używając EF Core
    var notificationSettings = await _dbContext.NotificationSettings.FirstAsync();
    var userSettings = await _dbContext.UserSettings.FirstAsync();

    // Utwórz słownik ustawień
    var settingsData = new Dictionary<string, object>
    {
        { "Notifications", notificationSettings },
        { "User", userSettings },
        { "General", generalSettings }
    };

    // Wypełnij provider danymi
    AsyncObjectConfigurationProvider.SetData(settingsData);
}
```

## 5. Kluczowe zalety

1. **Dependency Injection** - Serwis ustawień ma pełny dostęp do DI, może używać EF Core, loggera, itp.
2. **Asynchroniczność** - Ładowanie ustawień jest asynchroniczne
3. **Silne typowanie** - Ustawienia są reprezentowane jako klasy C# z properties
4. **Automatyczne spłaszczanie** - Obiekty są automatycznie konwertowane do płaskiej struktury konfiguracyjnej
5. **Integracja z IConfiguration** - Bezproblemowa integracja z systemem konfiguracji .NET
6. **Reload support** - Możliwość przeładowania konfiguracji bez restartu aplikacji

## 6. Zmiany w pakietach NuGet

Dodano:
- `Microsoft.Extensions.Configuration.Json` v10.0.0 - do parsowania JSON do struktury konfiguracyjnej

