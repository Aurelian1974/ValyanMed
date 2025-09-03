using Microsoft.JSInterop;
using System.Text.Json;
using Radzen;
using Radzen.Blazor;

namespace Client.Services;

/// <summary>
/// Service centralizat pentru persisten?a set?rilor DataGrid cu fallback memory cache
/// Implementeaz? best practices conform planului de refactoring
/// </summary>
public interface IDataGridSettingsService
{
    Task<DataGridSettings?> LoadSettingsAsync(string gridKey);
    Task SaveSettingsAsync(string gridKey, DataGridSettings settings);
    void SetFallbackSettings(string gridKey, DataGridSettings settings);
    DataGridSettings? GetFallbackSettings(string gridKey);
    Task ClearSettingsAsync(string gridKey);
}

public class DataGridSettingsService : IDataGridSettingsService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Dictionary<string, DataGridSettings> _memoryCache;
    private readonly JsonSerializerOptions _jsonOptions;

    public DataGridSettingsService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _memoryCache = new Dictionary<string, DataGridSettings>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<DataGridSettings?> LoadSettingsAsync(string gridKey)
    {
        if (string.IsNullOrEmpty(gridKey))
            return null;

        // 1. Încearc? din localStorage
        var settings = await LoadFromLocalStorageAsync(gridKey);
        if (settings != null)
        {
            // Salveaz? în memory cache pentru fallback
            _memoryCache[gridKey] = settings;
            return settings;
        }

        // 2. Fallback la memory cache
        if (_memoryCache.TryGetValue(gridKey, out var cachedSettings))
        {
            return cachedSettings;
        }

        // 3. Returneaz? null dac? nu exist?
        return null;
    }

    public async Task SaveSettingsAsync(string gridKey, DataGridSettings settings)
    {
        if (string.IsNullOrEmpty(gridKey) || settings == null)
            return;

        // 1. Salveaz? în memory cache ÎNTÂI (reliable)
        _memoryCache[gridKey] = settings;

        // 2. Încearc? s? salveze în localStorage (poate e?ua)
        try
        {
            await SaveToLocalStorageAsync(gridKey, settings);
        }
        catch (JSException jsEx)
        {
            // localStorage indisponibil sau plin - memory cache e fallback
            Console.WriteLine($"[DataGridSettings] localStorage failed for {gridKey}: {jsEx.Message}");
        }
        catch (Exception ex)
        {
            // Alte erori - logare pentru debugging
            Console.WriteLine($"[DataGridSettings] Save failed for {gridKey}: {ex.Message}");
        }
    }

    public void SetFallbackSettings(string gridKey, DataGridSettings settings)
    {
        if (string.IsNullOrEmpty(gridKey) || settings == null)
            return;

        _memoryCache[gridKey] = settings;
    }

    public DataGridSettings? GetFallbackSettings(string gridKey)
    {
        if (string.IsNullOrEmpty(gridKey))
            return null;

        return _memoryCache.TryGetValue(gridKey, out var settings) ? settings : null;
    }

    public async Task ClearSettingsAsync(string gridKey)
    {
        if (string.IsNullOrEmpty(gridKey))
            return;

        // Clear memory cache
        _memoryCache.Remove(gridKey);

        // Clear localStorage
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", gridKey);
        }
        catch (JSException)
        {
            // Ignore localStorage errors
        }
        catch (Exception)
        {
            // Ignore other errors
        }
    }

    private async Task<DataGridSettings?> LoadFromLocalStorageAsync(string gridKey)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", gridKey);
            
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<DataGridSettings>(json, _jsonOptions);
            }
        }
        catch (JSException jsEx)
        {
            // localStorage indisponibil - folose?te memory cache
            Console.WriteLine($"[DataGridSettings] localStorage read failed for {gridKey}: {jsEx.Message}");
        }
        catch (JsonException jsonEx)
        {
            // JSON corupt - ?terge ?i folose?te memory cache
            Console.WriteLine($"[DataGridSettings] Invalid JSON for {gridKey}: {jsonEx.Message}");
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", gridKey);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        catch (Exception ex)
        {
            // Alte erori
            Console.WriteLine($"[DataGridSettings] Load failed for {gridKey}: {ex.Message}");
        }

        return null;
    }

    private async Task SaveToLocalStorageAsync(string gridKey, DataGridSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", gridKey, json);
    }
}

/// <summary>
/// Extension methods pentru RadzenDataGrid settings
/// </summary>
public static class DataGridSettingsExtensions
{
    public static async Task<DataGridSettings?> LoadSettingsAsync<T>(this RadzenDataGrid<T> grid, IDataGridSettingsService service, string gridKey)
    {
        var settings = await service.LoadSettingsAsync(gridKey);
        if (settings != null && grid != null)
        {
            // Aplic? set?rile dac? sunt disponibile
            try
            {
                // Aplicarea se face automat prin binding-ul Settings property
                return settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DataGridSettings] Apply failed for {gridKey}: {ex.Message}");
                // Returneaz? set?rile pentru aplicare manual?
                return settings;
            }
        }
        return null;
    }

    public static async Task SaveSettingsAsync<T>(this RadzenDataGrid<T> grid, IDataGridSettingsService service, string gridKey, DataGridSettings settings)
    {
        await service.SaveSettingsAsync(gridKey, settings);
    }
}