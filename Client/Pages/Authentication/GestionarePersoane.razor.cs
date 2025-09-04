using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Authentication;
using global::Shared.Common;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using Client.Extensions;
using Client.Services;

namespace Client.Pages.Authentication;

public partial class GestionarePersoane : ComponentBase, IDisposable
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private IDataGridSettingsService DataGridSettingsService { get; set; } = null!;

    // PUBLIC PROPERTIES FOR RAZOR BINDING
    public Radzen.Blazor.RadzenDataGrid<PersoanaListDto> _dataGrid = null!;
    public IEnumerable<PersoanaListDto> _data = new List<PersoanaListDto>();
    public PersoanaSearchQuery _searchQuery = new() { PageSize = 10 };
    public int _totalCount = 0;
    public bool _isLoading = false;
    public bool _hasError = false;
    public string _errorMessage = string.Empty;

    // PAGINATION
    public int _pageSize = 10;
    public int[] _pageSizeOptions = { 5, 10, 20, 50, 100 };

    // PRIVATE FIELDS - FIXED MEMORY MANAGEMENT
    private Timer? _searchTimer;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isDisposed = false;
    private bool _isGrouped = false;
    private List<DataGridGroupRequest> _currentGroups = new();
    private bool _allGroupsExpanded = true;

    // Filters
    private string? _filterNume;
    private string? _filterPrenume;
    private string? _filterCNP;
    private string? _filterTelefon;
    private string? _filterEmail;
    private string? _selectedJudet;
    private string? _selectedLocalitate;
    private bool? _selectedStatus;

    // Enhanced Grouping info using native RadzenDataGrid data
    private EnhancedGroupInfo? _currentGroupInfo;
    
    // Settings persistence
    private DataGridSettings? _gridSettings;
    private const string GRID_SETTINGS_KEY = "gestionare_persoane_grid_settings";
    
    // Group state tracking for sync with manual expand/collapse
    private Dictionary<string, bool> _groupStates = new();
    private bool _isUpdatingGroupStates = false;

    // SERVICES - pentru State Management centralizat
    // ELIMINAT: DataGridStateService duplicat - folosim IDataGridSettingsService centralizat

    private class EnhancedGroupInfo
    {
        public string GroupName { get; set; } = string.Empty;
        public int TotalGroups { get; set; }
        public int TotalItems { get; set; }
        public int ExpandedGroups { get; set; }
        public int CollapsedGroups { get; set; }
        public List<GroupDetails> GroupDetails { get; set; } = new();
    }
    
    private class GroupDetails
    {
        public string Name { get; set; } = string.Empty;
        public object Value { get; set; } = null!;
        public int ItemCount { get; set; }
        public bool IsExpanded { get; set; } = true;
    }

    private class DataGridGroupRequest
    {
        public string Property { get; set; } = string.Empty;
        public string SortOrder { get; set; } = "asc";
    }

    private string GetGroupDisplayName(string property)
    {
        return property?.ToLower() switch
        {
            "judet" => "Judet",
            "localitate" => "Localitate", 
            "gen" => "Gen",
            "esteactiva" => "Status",
            "nume" => "Nume",
            "prenume" => "Prenume",
            "cnp" => "CNP",
            _ => property ?? "Unknown"
        };
    }

    protected override async Task OnInitializedAsync()
    {
        // Initialize cancellation token
        _cancellationTokenSource = new CancellationTokenSource();
        
        // Load grid settings first
        await LoadGridSettings();
        
        // Initialize search query
        _searchQuery = new PersoanaSearchQuery 
        { 
            PageSize = _pageSize,
            Page = 1,
            Search = string.Empty
        };
        
        // Initial data load
        await LoadDataAsync(new LoadDataArgs());
        
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_isDisposed)
        {
            if (_gridSettings != null)
            {
                try
                {
                    await Task.Delay(100, _cancellationTokenSource?.Token ?? CancellationToken.None);
                    if (_isDisposed) return;

                    // Force RadzenDataGrid to treat Settings as changed by creating a new instance
                    var json = JsonSerializer.Serialize(_gridSettings);
                    _gridSettings = JsonSerializer.Deserialize<DataGridSettings>(json);

                    StateHasChanged();
                }
                catch (OperationCanceledException)
                {
                    // Normal during disposal
                }
                catch (Exception)
                {
                    // Ignore when applying settings
                }
            }
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    // FIXED DISPOSE PATTERN - COMPLET
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        
        try
        {
            // 1. Cancel all pending operations FIRST
            _cancellationTokenSource?.Cancel();
            
            // 2. Dispose timer safely
            _searchTimer?.Dispose();
            _searchTimer = null;
            
            // 3. Save current state before disposal - IMPROVED WITH PROPER FALLBACK
            if (_gridSettings != null && DataGridSettingsService != null)
            {
                try
                {
                    // Salvare sincron? în memory cache înainte de disposal
                    DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, _gridSettings);
                    Console.WriteLine($"[GestionarePersoane] Settings saved to memory cache before disposal");
                    
                    // Fire and forget pentru localStorage save
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // Try to persist to localStorage as well
                            await DataGridSettingsService.SaveSettingsAsync(GRID_SETTINGS_KEY, _gridSettings);
                        }
                        catch
                        {
                            // Best effort - memory cache deja are datele
                        }
                    });
                }
                catch
                {
                    // Ignore errors during disposal
                }
            }
            
            // 4. Dispose cancellation token
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            
            // 5. Clear collections to help GC
            _groupStates?.Clear();
            _currentGroups?.Clear();
            
            // 6. Null out references - ELIMINAT: _stateService nu mai este necesar
        }
        catch (Exception)
        {
            // Ignore all errors during disposal
        }
        finally
        {
            // 7. Suppress finalization
            GC.SuppressFinalize(this);
        }
    }

    // IMPROVED LOCALSTORAGE WITH CENTRALIZED SERVICE AND PROPER FALLBACK
    private async Task SaveGridSettingsAsync(DataGridSettings settings)
    {
        if (_isDisposed) return;

        try
        {
            // Serviciul salveaz? automat în memory cache ÎNTÂI (reliable)
            // apoi încearc? localStorage (poate e?ua)
            await DataGridSettingsService.SaveSettingsAsync(GRID_SETTINGS_KEY, settings);
            
            // Success - set?rile sunt salvate cel pu?in în memory cache
            Console.WriteLine($"[GestionarePersoane] Settings saved successfully for {GRID_SETTINGS_KEY}");
        }
        catch (Exception ex)
        {
            // Chiar dac? SaveSettingsAsync e?ueaz? complet,
            // salv?m explicit în memory cache ca fallback final
            Console.WriteLine($"[GestionarePersoane] SaveGridSettings failed: {ex.Message}");
            Console.WriteLine($"[GestionarePersoane] Using explicit memory fallback");
            
            try
            {
                // Fallback explicit la memory cache
                DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, settings);
                Console.WriteLine($"[GestionarePersoane] Memory fallback successful");
            }
            catch (Exception fallbackEx)
            {
                // Dac? ?i fallback-ul e?ueaz?, log?m dar continu?m
                Console.WriteLine($"[GestionarePersoane] Memory fallback also failed: {fallbackEx.Message}");
            }
        }
    }

    private async Task LoadGridSettings()
    {
        if (_isDisposed) return;

        try
        {
            // LoadSettingsAsync încearc? localStorage APOI memory cache automat
            _gridSettings = await DataGridSettingsService.LoadSettingsAsync(GRID_SETTINGS_KEY);
            
            if (_gridSettings == null)
            {
                // Set?ri implicite pentru grid
                _gridSettings = new DataGridSettings();
                
                // Salveaz? set?rile implicite în memory cache pentru sesiunea curent?
                DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, _gridSettings);
                Console.WriteLine($"[GestionarePersoane] Created default settings for {GRID_SETTINGS_KEY}");
            }
            else
            {
                Console.WriteLine($"[GestionarePersoane] Loaded settings from storage/cache");
            }
            
            // Restore grouping state
            if (_gridSettings?.Groups?.Any() == true)
            {
                _isGrouped = true;
                await UpdateCurrentGroupsInternal();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GestionarePersoane] LoadGridSettings error: {ex.Message}");
            
            // În caz de eroare total?, folose?te set?ri implicite
            _gridSettings = new DataGridSettings();
            
            // ?i salveaz? în memory cache pentru consisten??
            try
            {
                DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, _gridSettings);
                Console.WriteLine($"[GestionarePersoane] Fallback to default settings successful");
            }
            catch
            {
                // Ignore fallback errors - continue with defaults
                Console.WriteLine($"[GestionarePersoane] Using basic default settings");
            }
        }
    }

    // Settings Management - IMPROVED WITH PROPER FALLBACK
    public async Task OnSettingsChanged(DataGridSettings settings)
    {
        if (_isDisposed) return;
        
        _gridSettings = settings;
        
        // Salveaz? cu fallback complet
        await SaveGridSettingsAsync(settings);
    }

    public async Task ResetGridSettings()
    {
        try
        {
            await DataGridSettingsService.ClearSettingsAsync(GRID_SETTINGS_KEY);
            _gridSettings = null; // trigger reset
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

    public void OnRender(DataGridRenderEventArgs<PersoanaListDto> args)
    {
        var hasGroups = args.Grid.Groups?.Any() == true;
        var groupsChanged = hasGroups && GroupsChanged(args.Grid.Groups);
        
        // Batch state changes to avoid multiple renders
        var shouldUpdateState = false;
        
        if (hasGroups != _isGrouped)
        {
            _isGrouped = hasGroups;
            shouldUpdateState = true;
        }
        
        if (groupsChanged)
        {
            shouldUpdateState = true;
        }
        
        if (shouldUpdateState)
        {
            // Use InvokeAsync for better performance with batch updates
            InvokeAsync(async () => 
            {
                if (_isDisposed) return;

                try
                {
                    if (hasGroups)
                    {
                        await UpdateCurrentGroupsInternal();
                        await UpdateGroupInfoInternal();
                        
                        // Initialize group states for new groups
                        if (args.Grid.GroupedPagedView?.Any() == true)
                        {
                            foreach (var group in args.Grid.GroupedPagedView)
                            {
                                var groupKey = $"{group.Key}";
                                if (!_groupStates.ContainsKey(groupKey))
                                {
                                    _groupStates[groupKey] = _allGroupsExpanded;
                                }
                            }
                        }
                        
                        // Reload data for server-side grouping
                        if (args.Grid != null)
                        {
                            await args.Grid.Reload();
                        }
                    }
                    else
                    {
                        // Clear grouping state
                        _currentGroups.Clear();
                        _currentGroupInfo = null;
                        _groupStates.Clear();
                        
                        if (args.Grid != null)
                        {
                            await args.Grid.Reload();
                        }
                    }
                    
                    // Single state update at the end
                    if (!_isDisposed)
                        StateHasChanged();
                }
                catch (OperationCanceledException)
                {
                    // Normal during disposal
                }
                catch (Exception)
                {
                    // Log error if needed
                }
            });
        }
    }

    private async Task UpdateCurrentGroupsInternal()
    {
        if (_dataGrid?.Groups?.Any() == true)
        {
            _currentGroups = _dataGrid.Groups.Select(g => new DataGridGroupRequest
            {
                Property = g.Property,
                SortOrder = g.SortOrder?.ToString().ToLower() ?? "asc"
            }).ToList();
        }
    }

    private async Task UpdateGroupInfoInternal()
    {
        if (!_isGrouped || _dataGrid?.Groups?.Any() != true)
        {
            _currentGroupInfo = null;
            return;
        }

        try
        {
            var groups = _dataGrid.Groups;
            var groupedView = _dataGrid.GroupedPagedView;
            
            if (groupedView?.Any() == true)
            {
                var groupDetails = groupedView.Select(g => new GroupDetails
                {
                    Name = g.Key?.ToString() ?? "Unknown",
                    Value = g.Key ?? "Unknown",
                    ItemCount = g.Count,
                    IsExpanded = _groupStates.GetValueOrDefault($"{g.Key}", true)
                }).ToList();

                _currentGroupInfo = new EnhancedGroupInfo
                {
                    GroupName = string.Join(", ", groups.Select(g => GetGroupDisplayName(g.Property))),
                    TotalGroups = groupDetails.Count,
                    TotalItems = groupDetails.Sum(g => g.ItemCount),
                    ExpandedGroups = groupDetails.Count(g => g.IsExpanded),
                    CollapsedGroups = groupDetails.Count(g => !g.IsExpanded),
                    GroupDetails = groupDetails
                };
            }
        }
        catch (Exception ex)
        {
            // Log error silently
        }
    }

    private void UpdateGroupingStatistics()
    {
        if (_data?.Any() == true && _currentGroups.Any())
        {
            var groupProperty = _currentGroups.First().Property;
            var uniqueGroups = _data.Select(item => 
                groupProperty.ToLower() switch
                {
                    "judet" => item.Judet ?? "Nu este specificat",
                    "localitate" => item.Localitate ?? "Nu este specificat", 
                    "gen" => item.Gen ?? "Nu este specificat",
                    "esteactiva" => item.EsteActiva ? "Activa" : "Inactiva",
                    _ => "Unknown"
                }).Distinct().Count();

            _currentGroupInfo = new EnhancedGroupInfo
            {
                GroupName = groupProperty,
                TotalGroups = uniqueGroups,
                TotalItems = _data.Count()
            };
        }
    }

    public async Task OnGroupRowExpand(Group group)
    {
        var groupKey = $"{group.Data}";
        _groupStates[groupKey] = true;
        
        await UpdateGroupInfoInternal();
        
        if (!_isUpdatingGroupStates && _groupStates.Values.All(expanded => expanded))
        {
            _allGroupsExpanded = true;
            StateHasChanged();
        }
    }

    public async Task OnGroupRowCollapse(Group group)
    {
        var groupKey = $"{group.Data}";
        _groupStates[groupKey] = false;
        
        await UpdateGroupInfoInternal();
        
        if (!_isUpdatingGroupStates)
        {
            _allGroupsExpanded = false;
            StateHasChanged();
        }
    }

    public async Task OnAllGroupsExpandedChanged(bool? value)
    {
        _allGroupsExpanded = value ?? true;
        
        _isUpdatingGroupStates = true;
        try
        {
            foreach (var key in _groupStates.Keys.ToList())
            {
                _groupStates[key] = _allGroupsExpanded;
            }
            
            await UpdateGroupInfoInternal();
        }
        finally
        {
            _isUpdatingGroupStates = false;
        }
        
        StateHasChanged();
    }

    public async Task OnAllGroupsExpandedChange(bool value)
    {
        _allGroupsExpanded = value;
        StateHasChanged();
    }

    private bool GroupsChanged(IEnumerable<GroupDescriptor> newGroups)
    {
        if (newGroups == null)
            return _currentGroups.Any();
            
        var newGroupsList = newGroups.ToList();
        
        if (newGroupsList.Count != _currentGroups.Count)
            return true;
            
        for (int i = 0; i < newGroupsList.Count; i++)
        {
            var newGroup = newGroupsList[i];
            var currentGroup = _currentGroups[i];
            
            if (newGroup.Property != currentGroup.Property ||
                (newGroup.SortOrder?.ToString().ToLower() ?? "asc") != currentGroup.SortOrder)
            {
                return true;
            }
        }
        
        return false;
    }

    public async Task LoadDataAsync(LoadDataArgs args)
    {
        if (_isDisposed) return;

        _isLoading = true;
        _hasError = false;
        _errorMessage = string.Empty;

        try
        {
            // Update search query from args
            if (args != null)
            {
                _searchQuery.Page = ((args.Skip ?? 0) / (args.Top ?? _pageSize)) + 1;
                _searchQuery.PageSize = args.Top ?? _pageSize;
                _pageSize = _searchQuery.PageSize;
                
                if (!string.IsNullOrEmpty(args.OrderBy))
                {
                    _searchQuery.Sort = args.OrderBy.ToLower();
                }

                // Handle filters
                if (args.Filters?.Any() == true)
                {
                    foreach (var filter in args.Filters)
                    {
                        ApplyFilter(filter);
                    }
                }
            }

            var queryString = BuildQueryString();
            
            // Use cancellation token for HTTP requests
            var response = await Http.GetAsync($"api/Persoane{queryString}", 
                _cancellationTokenSource?.Token ?? CancellationToken.None);
            
            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<PersoanaListDto>>(
                    cancellationToken: _cancellationTokenSource?.Token ?? CancellationToken.None);
                
                if (pagedResult != null)
                {
                    _data = pagedResult.Items;
                    _totalCount = pagedResult.TotalCount;
                    
                    if (_isGrouped)
                    {
                        UpdateGroupingStatistics();
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
                _hasError = true;
                _errorMessage = "Eroare la incarcarea datelor de persoane";
                _data = new List<PersoanaListDto>();
                _totalCount = 0;
            }
        }
        catch (OperationCanceledException)
        {
            // Normal during disposal or cancellation
            return;
        }
        catch (HttpRequestException)
        {
            _hasError = true;
            _errorMessage = "Nu se poate conecta la server";
            _data = new List<PersoanaListDto>();
            _totalCount = 0;
        }
        catch (Exception ex)
        {
            _hasError = true;
            _errorMessage = $"Eroare nespecificata: {ex.Message}";
            _data = new List<PersoanaListDto>();
            _totalCount = 0;
        }
        finally
        {
            _isLoading = false;
            if (!_isDisposed)
                StateHasChanged();
        }
    }

    private void ApplyFilter(FilterDescriptor filter)
    {
        var property = filter.Property?.ToLower();
        var value = filter.FilterValue?.ToString();

        if (string.IsNullOrEmpty(property) || string.IsNullOrEmpty(value))
            return;

        switch (property)
        {
            case "numecomplet":
                if (string.IsNullOrEmpty(_searchQuery.Search))
                    _searchQuery.Search = value;
                break;
            case "judet":
                _searchQuery.Judet = value;
                break;
            case "localitate":
                _searchQuery.Localitate = value;
                break;
            case "esteactiva":
                if (bool.TryParse(value, out bool status))
                    _searchQuery.EsteActiv = status;
                break;
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
            queryParams.Add($"EsteActiva={_searchQuery.EsteActiv.Value}");

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
        if (_isDisposed) return;

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
            // Check disposal status in timer callback
            if (_isDisposed) return;

            try
            {
                _searchQuery.Page = 1;
                await InvokeAsync(async () => 
                {
                    if (_isDisposed) return;

                    try
                    {
                        if (_dataGrid != null)
                            await _dataGrid.Reload();
                        else
                            await LoadDataAsync(new LoadDataArgs());
                        
                        if (!_isDisposed)
                            StateHasChanged();
                    }
                    catch (OperationCanceledException)
                    {
                        // Normal during disposal
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
        }, null, 300, Timeout.Infinite);
    }

    public async Task ResetFilters()
    {
        _searchQuery = new PersoanaSearchQuery { PageSize = _pageSize };
        if (_dataGrid != null)
        {
            _dataGrid.Reset(true);
            await _dataGrid.Reload();
        }
    }

    public async Task ClearGrouping()
    {
        try
        {
            if (_dataGrid != null)
            {
                _dataGrid.Groups.Clear();
                
                _isGrouped = false;
                _currentGroups.Clear();
                _currentGroupInfo = null;
                _groupStates.Clear();
                _allGroupsExpanded = true;
                
                await _dataGrid.Reload();
                
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            NotificationService.ShowError("Nu s-a putut elimina gruparea");
        }
    }

    // Navigation Actions
    public void CreateNewPersoana()
    {
        Navigation.NavigateTo("/administrare/persoane/nou");
    }

    public void EditPersoana(int persoanaId)
    {
        Navigation.NavigateTo($"/administrare/persoane/editare/{persoanaId}");
    }

    public void ViewPersoana(int persoanaId)
    {
        Navigation.NavigateTo($"/administrare/persoane-view/{persoanaId}");
    }

    public async Task DeletePersoana(int persoanaId, string numeComplet)
    {
        var confirmed = await DialogService.Confirm(
            $"Esti sigur ca vrei sa stergi persoana '{numeComplet}'?",
            "Confirmare stergere",
            new ConfirmOptions() { OkButtonText = "Sterge", CancelButtonText = "Anuleaza" });

        if (confirmed == true)
        {
            try
            {
                var response = await Http.DeleteAsync($"api/Persoane/{persoanaId}",
                    _cancellationTokenSource?.Token ?? CancellationToken.None);
                
                if (await response.HandleApiResponse(NotificationService, $"Persoana '{numeComplet}' a fost stearsa cu succes"))
                {
                    if (_dataGrid != null) await _dataGrid.Reload();
                }
            }
            catch (OperationCanceledException)
            {
                // Normal during disposal
                return;
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Eroare la stergerea persoanei: {ex.Message}");
            }
        }
    }
}