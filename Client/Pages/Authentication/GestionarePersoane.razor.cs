using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Authentication;
using global::Shared.Common;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;

namespace Client.Pages.Authentication;

public partial class GestionarePersoane : ComponentBase, IDisposable
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

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

    // PRIVATE FIELDS
    private Timer? _searchTimer;
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
        if (firstRender)
        {
            // Apply loaded settings if any
            if (_gridSettings != null && _dataGrid != null)
            {
                try
                {
                    // Force apply settings after first render
                    await Task.Delay(100); // Small delay to ensure grid is fully initialized
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    // Log error silently
                }
            }
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    public void Dispose()
    {
        _searchTimer?.Dispose();
        
        // Save current state before disposal
        if (_gridSettings != null)
        {
            try
            {
                // Best effort save - don't await in Dispose
                _ = Task.Run(async () =>
                {
                    var json = JsonSerializer.Serialize(_gridSettings);
                    await JSRuntime.InvokeVoidAsync("localStorage.setItem", GRID_SETTINGS_KEY, json);
                });
            }
            catch
            {
                // Ignore errors during disposal
            }
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

    // Enhanced Group Event Handlers
    public async Task OnGroupRowExpand(Group group)
    {
        var groupKey = $"{group.Data}";
        _groupStates[groupKey] = true;
        
        await UpdateGroupInfo();
        
        // Sync checkbox state if all groups are now expanded
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
        
        await UpdateGroupInfo();
        
        // Sync checkbox state if any group is collapsed
        if (!_isUpdatingGroupStates)
        {
            _allGroupsExpanded = false;
            StateHasChanged();
        }
    }

    public async Task OnAllGroupsExpandedChanged(bool? value)
    {
        _allGroupsExpanded = value ?? true;
        
        // Update internal group states
        _isUpdatingGroupStates = true;
        try
        {
            foreach (var key in _groupStates.Keys.ToList())
            {
                _groupStates[key] = _allGroupsExpanded;
            }
            
            await UpdateGroupInfo();
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
        
        // This will trigger the RadzenDataGrid's internal logic
        StateHasChanged();
    }

    private bool GroupsChanged(IEnumerable<GroupDescriptor> newGroups)
    {
        if (newGroups == null)
            return _currentGroups.Any();
            
        var newGroupsList = newGroups.ToList();
        
        if (newGroupsList.Count != _currentGroups.Count)
            return true;
            
        // Enhanced comparison including sort order
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
            var response = await Http.GetAsync($"api/Persoane{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<PersoanaListDto>>();
                
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
            StateHasChanged();
        }
    }

    private void ApplyFilter(FilterDescriptor filter)
    {
        // Basic filter implementation - can be extended
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
        _searchTimer?.Dispose();
        _searchTimer = new Timer(async _ =>
        {
            _searchQuery.Page = 1;
            await InvokeAsync(async () => 
            {
                if (_dataGrid != null)
                    await _dataGrid.Reload();
                else
                    await LoadDataAsync(new LoadDataArgs());
                StateHasChanged();
            });
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
                // Clear groups using RadzenDataGrid API
                _dataGrid.Groups.Clear();
                
                // Reset internal state
                _isGrouped = false;
                _currentGroups.Clear();
                _currentGroupInfo = null;
                _groupStates.Clear();
                _allGroupsExpanded = true;
                
                // Reload data
                await _dataGrid.Reload();
                
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Eroare",
                Detail = "Nu s-a putut elimina gruparea",
                Duration = 4000
            });
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
                var response = await Http.DeleteAsync($"api/Persoane/{persoanaId}");
                
                if (response.IsSuccessStatusCode)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Succes",
                        Detail = $"Persoana '{numeComplet}' a fost stearsa cu succes",
                        Duration = 3000
                    });
                    if (_dataGrid != null) await _dataGrid.Reload();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Eroare",
                        Detail = $"Eroare la stergerea persoanei: {errorContent}",
                        Duration = 4000
                    });
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Eroare",
                    Detail = $"Eroare la stergerea persoanei: {ex.Message}",
                    Duration = 4000
                });
            }
        }
    }

    // Settings Management
    public async Task OnSettingsChanged(DataGridSettings settings)
    {
        _gridSettings = settings;
        
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", GRID_SETTINGS_KEY, json);
        }
        catch (Exception ex)
        {
            // Log error silently
        }
    }

    private async Task LoadGridSettings()
    {
        try
        {
            var json = await JSRuntime.InvokeAsync<string>("localStorage.getItem", GRID_SETTINGS_KEY);
            
            if (!string.IsNullOrEmpty(json))
            {
                _gridSettings = JsonSerializer.Deserialize<DataGridSettings>(json, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                // Restore grouping state
                if (_gridSettings?.Groups?.Any() == true)
                {
                    _isGrouped = true;
                    await UpdateCurrentGroups();
                }
            }
        }
        catch (Exception ex)
        {
            // Log error silently
        }
    }

    private async Task UpdateCurrentGroups()
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

    private async Task UpdateGroupInfo()
    {
        if (!_isGrouped || _dataGrid?.Groups?.Any() != true)
        {
            _currentGroupInfo = null;
            return;
        }

        try
        {
            // Get group information from RadzenDataGrid's internal state
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
                if (hasGroups)
                {
                    await UpdateCurrentGroups();
                    await UpdateGroupInfo();
                    
                    // Initialize group states for new groups
                    if (_dataGrid.GroupedPagedView?.Any() == true)
                    {
                        foreach (var group in _dataGrid.GroupedPagedView)
                        {
                            var groupKey = $"{group.Key}";
                            if (!_groupStates.ContainsKey(groupKey))
                            {
                                _groupStates[groupKey] = _allGroupsExpanded;
                            }
                        }
                    }
                    
                    // Reload data for server-side grouping
                    if (_dataGrid != null)
                    {
                        await _dataGrid.Reload();
                    }
                }
                else
                {
                    // Clear grouping state
                    _currentGroups.Clear();
                    _currentGroupInfo = null;
                    _groupStates.Clear();
                    
                    if (_dataGrid != null)
                    {
                        await _dataGrid.Reload();
                    }
                }
                
                // Single state update at the end
                StateHasChanged();
            });
        }
    }
}