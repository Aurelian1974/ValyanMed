using Microsoft.AspNetCore.Components;
using Radzen;
using Shared.DTOs.Medical;
using Shared.Common;
using System.Net.Http.Json;
using Client.Services;
using System.Text.Json;

namespace Client.Pages.Medical;

public partial class PersonalMedical : ComponentBase, IDisposable
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IDataGridSettingsService DataGridSettingsService { get; set; } = null!;

    private Radzen.Blazor.RadzenDataGrid<PersonalMedicalListDto> _dataGrid = null!;
    private IEnumerable<PersonalMedicalListDto> _data = new List<PersonalMedicalListDto>();
    private PersonalMedicalSearchQuery _searchQuery = new() { PageSize = 10 };
    private int _totalCount = 0;
    private bool _isLoading = false;
    private Timer? _searchTimer;

    // Grid settings persistence
    private DataGridSettings? _gridSettings;
    private const string GRID_SETTINGS_KEY = "personal_medical_grid_settings";

    // Dropdown options
    private readonly string[] _departamente = {
        "Cardiologie", "Neurologie", "Pediatrie", "Chirurgie", 
        "Medicina interna", "Radiologie", "Laborator", "Recuperare"
    };

    private readonly string[] _pozitii = {
        "Doctor", "Medic specialist", "Asistent medical", 
        "Tehnician medical", "Kinetoterapeut", "Psiholog", "Nutritionist"
    };

    private readonly List<DropDownOption> _statusOptions = new()
    {
        new DropDownOption { Text = "Activ", Value = true },
        new DropDownOption { Text = "Inactiv", Value = false }
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadGridSettings();
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

    public async Task ResetFilters()
    {
        _searchQuery = new PersonalMedicalSearchQuery { PageSize = _searchQuery.PageSize };
        _totalCount = 0;
        if (_dataGrid != null)
        {
            _dataGrid.Reset(true);
        }
        await LoadDataAsync(new Radzen.LoadDataArgs());
    }

    public async Task LoadDataAsync(Radzen.LoadDataArgs args)
    {
        _isLoading = true;
        try
        {
            if (args != null)
            {
                _searchQuery.Page = ((args.Skip ?? 0) / _searchQuery.PageSize) + 1;
                
                if (!string.IsNullOrEmpty(args.OrderBy))
                {
                    _searchQuery.Sort = args.OrderBy.ToLower();
                }
            }

            var queryString = BuildQueryString();
            var response = await Http.GetAsync($"api/PersonalMedical{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<PersonalMedicalListDto>>();
                
                if (pagedResult != null)
                {
                    _data = pagedResult.Items;
                    _totalCount = pagedResult.TotalCount;
                }
                else
                {
                    _data = new List<PersonalMedicalListDto>();
                    _totalCount = 0;
                }
            }
            else
            {
                ShowErrorNotification("Eroare la incarcarea datelor de personal medical");
                _data = new List<PersonalMedicalListDto>();
                _totalCount = 0;
            }
        }
        catch (HttpRequestException)
        {
            ShowErrorNotification("Nu se poate conecta la server");
            _data = new List<PersonalMedicalListDto>();
            _totalCount = 0;
        }
        catch (Exception ex)
        {
            ShowErrorNotification($"Eroare nespecificata: {ex.Message}");
            _data = new List<PersonalMedicalListDto>();
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

        if (!string.IsNullOrWhiteSpace(_searchQuery.Departament))
            queryParams.Add($"Departament={Uri.EscapeDataString(_searchQuery.Departament)}");

        if (!string.IsNullOrWhiteSpace(_searchQuery.Pozitie))
            queryParams.Add($"Pozitie={Uri.EscapeDataString(_searchQuery.Pozitie)}");

        if (_searchQuery.EsteActiv.HasValue)
            queryParams.Add($"EsteActiv={_searchQuery.EsteActiv.Value}");

        if (!string.IsNullOrWhiteSpace(_searchQuery.Sort))
            queryParams.Add($"Sort={_searchQuery.Sort}");

        return queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
    }

    public async Task OnSearchInput(ChangeEventArgs args)
    {
        _searchQuery.Search = args.Value?.ToString();
        await DelayedSearch();
    }

    private async Task DelayedSearch()
    {
        _searchTimer?.Dispose();
        _searchTimer = new Timer(async _ =>
        {
            _searchQuery.Page = 1;
            await InvokeAsync(async () => 
            {
                await LoadDataAsync(new Radzen.LoadDataArgs());
                StateHasChanged();
            });
        }, null, 300, Timeout.Infinite);
    }

    public async Task OnFilterChanged()
    {
        _searchQuery.Page = 1;
        await LoadDataAsync(new Radzen.LoadDataArgs());
    }

    public void ViewDetails(Guid personalId)
    {
        Navigation.NavigateTo($"/medical/personal/{personalId}");
    }

    public void EditPersonal(Guid personalId)
    {
        Navigation.NavigateTo($"/medical/personal/editare/{personalId}");
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
        public bool Value { get; set; }
    }
}