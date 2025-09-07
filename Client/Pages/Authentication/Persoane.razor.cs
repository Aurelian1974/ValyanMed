using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Authentication;
using global::Shared.Common;
using global::Shared.DTOs.Common;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;
using Client.Services;
using Client.Services.Common;
using System.Text.Json;

namespace Client.Pages.Authentication;

public partial class Persoane : ComponentBase, IDisposable
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IDataGridSettingsService DataGridSettingsService { get; set; } = null!;
    [Inject] private ILocationApiService LocationService { get; set; } = null!;

    private Radzen.Blazor.RadzenDataGrid<PersoanaListDto> _dataGrid = null!;
    private IEnumerable<PersoanaListDto> _data = new List<PersoanaListDto>();
    private PersoanaSearchQuery _searchQuery = new() { PageSize = 10 };
    private int _totalCount = 0;
    private bool _isLoading = false;
    private Timer? _searchTimer;

    // Grid settings persistence
    private DataGridSettings? _gridSettings;
    private const string GRID_SETTINGS_KEY = "persoane_grid_settings";

    // Page size options
    private readonly int[] _pageSizeOptions = new[] { 5, 10, 20, 50, 100 };

    // Dropdown options pentru jude?e ?i localit??i
    private List<JudetDto> _judeteOptions = new();
    private List<LocalitateDto> _localitatiOptions = new();

    private readonly List<DropDownOption> _statusOptions = new()
    {
        new DropDownOption { Text = "Activa", Value = true },
        new DropDownOption { Text = "Inactiva", Value = false }
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadGridSettings();
        await LoadJudeteAsync();
        await LoadDataAsync(new Radzen.LoadDataArgs());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _gridSettings != null)
        {
            try
            {
                await Task.Delay(100);
                // Force a new instance so RadzenDataGrid detects the change and applies settings
                var json = JsonSerializer.Serialize(_gridSettings);
                _gridSettings = JsonSerializer.Deserialize<DataGridSettings>(json);
                StateHasChanged();
            }
            catch
            {
                // ignore
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task LoadJudeteAsync()
    {
        try
        {
            _judeteOptions = await LocationService.GetJudeteAsync();
        }
        catch (Exception ex)
        {
            ShowErrorNotification($"Eroare la înc?rcarea jude?elor: {ex.Message}");
            _judeteOptions = new List<JudetDto>();
        }
    }

    private async Task OnJudetChanged(string? selectedJudet)
    {
        // Reset localitate când se schimb? jude?ul
        if (_searchQuery.Judet != selectedJudet)
        {
            _searchQuery.Localitate = null;
        }
        
        _searchQuery.Judet = selectedJudet;
        
        // Înc?rcare localit??i pentru jude?ul selectat
        if (!string.IsNullOrEmpty(selectedJudet))
        {
            try
            {
                _localitatiOptions = await LocationService.GetLocalitatiByJudetAsync(selectedJudet);
            }
            catch (Exception ex)
            {
                ShowErrorNotification($"Eroare la înc?rcarea localit??ilor: {ex.Message}");
                _localitatiOptions = new List<LocalitateDto>();
            }
        }
        else
        {
            _localitatiOptions = new List<LocalitateDto>();
        }
        
        // Aplicare filtru cu noul jude? selectat
        await OnFilterChanged();
    }

    private async Task OnLocalitateChanged()
    {
        await OnFilterChanged();
    }

    private async Task OnStatusChanged()
    {
        Console.WriteLine($"[DEBUG] Status changed to: {_searchQuery.EsteActiv}");
        await OnFilterChanged();
    }

    private async Task OnPageSizeChanged()
    {
        Console.WriteLine($"[DEBUG] Page size changed to: {_searchQuery.PageSize}");
        _searchQuery.Page = 1; // Reset to first page when page size changes
        await OnFilterChanged();
    }

    private async Task LoadGridSettings()
    {
        try
        {
            _gridSettings = await DataGridSettingsService.LoadSettingsAsync(GRID_SETTINGS_KEY) ?? new DataGridSettings();
            // keep defaults in memory cache for session
            DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, _gridSettings);
        }
        catch
        {
            _gridSettings = new DataGridSettings();
            try { DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, _gridSettings); } catch { }
        }
    }

    public async Task OnSettingsChanged(DataGridSettings settings)
    {
        _gridSettings = settings;
        try
        {
            await DataGridSettingsService.SaveSettingsAsync(GRID_SETTINGS_KEY, settings);
        }
        catch
        {
            try { DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, settings); } catch { }
        }
    }

    public async Task ResetGridSettings()
    {
        try
        {
            await DataGridSettingsService.ClearSettingsAsync(GRID_SETTINGS_KEY);
            _gridSettings = null; // notify grid
            if (_dataGrid != null)
            {
                _dataGrid.Reset(true);
                await _dataGrid.Reload();
            }
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Setari resetate",
                Detail = "Setarile grilei au fost resetate la valorile implicite",
                Duration = 3000
            });
        }
        catch
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = "Avertisment",
                Detail = "Resetarea setarilor nu a fost finalizata complet",
                Duration = 3000
            });
        }
    }

    public async Task LoadDataAsync(Radzen.LoadDataArgs args)
    {
        _isLoading = true;
        try
        {
            // Update search query from args
            if (args != null)
            {
                _searchQuery.Page = ((args.Skip ?? 0) / _searchQuery.PageSize) + 1;
                
                if (!string.IsNullOrEmpty(args.OrderBy))
                {
                    _searchQuery.Sort = args.OrderBy.ToLower();
                }
            }

            var queryString = BuildQueryString();
            Console.WriteLine($"[DEBUG] Making API call to: api/Persoane{queryString}");
            
            var response = await Http.GetAsync($"api/Persoane{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<PersoanaListDto>>();
                
                if (pagedResult != null)
                {
                    _data = pagedResult.Items;
                    _totalCount = pagedResult.TotalCount;
                    
                    Console.WriteLine($"[DEBUG] Received {_data.Count()} items, total count: {_totalCount}");
                    Console.WriteLine($"[DEBUG] Status filter value: {_searchQuery.EsteActiv}");
                    
                    // Debug: afi??m statusurile din rezultate
                    foreach (var item in _data.Take(5))
                    {
                        Console.WriteLine($"[DEBUG] Person: {item.NumeComplet}, EsteActiva: {item.EsteActiva}, StatusText: {item.StatusText}");
                    }
                }
                else
                {
                    _data = new List<PersoanaListDto>();
                    _totalCount = 0;
                }
            }
            else
            {
                Console.WriteLine($"[DEBUG] API Error: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Error content: {errorContent}");
                
                ShowErrorNotification("Eroare la incarcarea datelor de persoane");
                _data = new List<PersoanaListDto>();
                _totalCount = 0;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[DEBUG] HTTP Exception: {ex.Message}");
            ShowErrorNotification("Nu se poate conecta la server");
            _data = new List<PersoanaListDto>();
            _totalCount = 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] General Exception: {ex.Message}");
            ShowErrorNotification($"Eroare nespecificata: {ex.Message}");
            _data = new List<PersoanaListDto>();
            _totalCount = 0;
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private string BuildQueryString()
    {
        var queryParams = new List<string>
        {
            $"Page={_searchQuery.Page}",
            $"PageSize={_searchQuery.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(_searchQuery.Search))
            queryParams.Add($"Search={Uri.EscapeDataString(_searchQuery.Search)}");

        if (!string.IsNullOrWhiteSpace(_searchQuery.Judet))
            queryParams.Add($"Judet={Uri.EscapeDataString(_searchQuery.Judet)}");

        if (!string.IsNullOrWhiteSpace(_searchQuery.Localitate))
            queryParams.Add($"Localitate={Uri.EscapeDataString(_searchQuery.Localitate)}");

        if (_searchQuery.EsteActiv.HasValue)
        {
            queryParams.Add($"EsteActiv={_searchQuery.EsteActiv.Value}");
            Console.WriteLine($"[DEBUG] Status filter applied: EsteActiv={_searchQuery.EsteActiv.Value}");
        }

        if (!string.IsNullOrWhiteSpace(_searchQuery.Sort))
            queryParams.Add($"Sort={_searchQuery.Sort}");

        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        Console.WriteLine($"[DEBUG] Final query string: {queryString}");
        
        return queryString;
    }

    public async Task OnSearchInput(ChangeEventArgs args)
    {
        var searchValue = args.Value?.ToString() ?? string.Empty;
        
        // Trimiteam spa?iile ?i actualiz?m immediat searchQuery
        _searchQuery.Search = searchValue.Trim();
        
        await DelayedSearch();
    }

    private async Task DelayedSearch()
    {
        // Dispose previous timer safely
        try
        {
            _searchTimer?.Dispose();
        }
        catch
        {
            // Ignore timer disposal errors
        }

        _searchTimer = new Timer(async _ =>
        {
            try
            {
                _searchQuery.Page = 1; // Reset to first page on new search
                await InvokeAsync(async () => 
                {
                    try
                    {
                        if (_dataGrid != null)
                            await _dataGrid.Reload();
                        else
                            await LoadDataAsync(new Radzen.LoadDataArgs());
                        
                        StateHasChanged();
                    }
                    catch (Exception)
                    {
                        // Log error if needed, but don't throw in timer callback
                    }
                });
            }
            catch (Exception)
            {
                // Ignore all errors in timer callback to prevent crashes
            }
        }, null, 300, Timeout.Infinite); // 300ms delay pentru debouncing
    }

    public async Task OnFilterChanged()
    {
        _searchQuery.Page = 1; // Reset to first page on filter change
        await LoadDataAsync(new Radzen.LoadDataArgs());
    }

    public async Task ResetFilters()
    {
        // Reset all search criteria
        var oldPageSize = _searchQuery.PageSize;
        _searchQuery = new PersoanaSearchQuery { PageSize = oldPageSize };
        _localitatiOptions = new List<LocalitateDto>(); // Clear localities when resetting
        
        if (_dataGrid != null)
        {
            _dataGrid.Reset(true);
            await _dataGrid.Reload();
        }
        else
        {
            await LoadDataAsync(new Radzen.LoadDataArgs());
        }
    }

    public void ViewDetails(int persoanaId)
    {
        Navigation.NavigateTo($"/administrare/persoane-view/{persoanaId}");
    }

    public void EditPersoana(int persoanaId)
    {
        Navigation.NavigateTo($"/administrare/persoane/editare/{persoanaId}");
    }

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new Radzen.NotificationMessage
        {
            Severity = Radzen.NotificationSeverity.Error,
            Summary = "Eroare",
            Detail = message,
            Duration = 4000
        });
    }

    public void Dispose()
    {
        _searchTimer?.Dispose();
    }

    // Helper class for dropdown options
    public class DropDownOption
    {
        public string Text { get; set; } = string.Empty;
        public bool? Value { get; set; }
    }
}