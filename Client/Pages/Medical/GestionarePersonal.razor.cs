using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Medical;
using global::Shared.Common;
using Radzen;
using Radzen.Blazor;
using Client.Services.Medical;
using Client.Services;
using System.Linq;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Client.Pages.Medical;

public partial class GestionarePersonal : ComponentBase, IDisposable
{
    [Inject] private IPersonalMedicalApiService PersonalMedicalApiService { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IDataGridSettingsService DataGridSettingsService { get; set; } = null!;

    // PUBLIC PROPERTIES FOR RAZOR BINDING
    public Radzen.Blazor.RadzenDataGrid<PersonalMedicalListDto> _dataGrid = null!;
    public IEnumerable<PersonalMedicalListDto> _data = new List<PersonalMedicalListDto>();
    public PersonalMedicalSearchQuery _searchQuery = new() { PageSize = 10 };
    public int _totalCount = 0;
    public bool _isLoading = false;
    public bool _hasError = false;
    public string _errorMessage = string.Empty;

    // PAGINATION
    public int _pageSize = 10;
    public int[] _pageSizeOptions = { 5, 10, 20, 50, 100 };

    // PRIVATE FIELDS
    private Timer? _searchTimer;
    private bool _isDisposed = false;
    private bool _isGrouped = false;
    private List<DataGridGroupRequest> _currentGroups = new();
    private bool _allGroupsExpanded = true;

    // Filters
    private string? _filterNume;
    private string? _filterPrenume;
    private string? _filterSpecializare;
    private string? _filterNumarLicenta;
    private string? _filterTelefon;
    private string? _filterEmail;
    private string? _selectedDepartament;
    private string? _selectedPozitie;
    private bool? _selectedStatus;

    // Enhanced Grouping info using native RadzenDataGrid data
    private EnhancedGroupInfo? _currentGroupInfo;
    
    // Settings persistence
    private DataGridSettings? _gridSettings;
    private const string GRID_SETTINGS_KEY = "gestionare_personal_grid_settings";
    
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

    private string GetGroupDisplayName(string property)
    {
        return property?.ToLower() switch
        {
            "departament" => "Departament",
            "pozitie" => "Pozitie", 
            "specializare" => "Specializare",
            "esteactiv" => "Status",
            "nume" => "Nume",
            "prenume" => "Prenume",
            _ => property ?? "Unknown"
        };
    }

    protected override async Task OnInitializedAsync()
    {
        // Load grid settings first
        await LoadGridSettings();
        
        // Initialize search query
        _searchQuery = new PersonalMedicalSearchQuery 
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
                    // Small delay to ensure grid is fully initialized
                    await Task.Delay(100);

                    // Force a new instance so RadzenDataGrid detects Settings param change and loads them
                    var json = JsonSerializer.Serialize(_gridSettings);
                    _gridSettings = JsonSerializer.Deserialize<DataGridSettings>(json);

                    StateHasChanged();
                }
                catch (Exception)
                {
                    // Ignore errors when applying settings
                }
            }
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    // COMPLETE DISPOSE PATTERN IMPLEMENTATION
    public void Dispose()
    {
        if (_isDisposed) return;
        
        _isDisposed = true;
        
        try
        {
            // 1. Dispose timer safely
            _searchTimer?.Dispose();
            _searchTimer = null;
            
            // 2. Save current state before disposal - IMPROVED WITH PROPER FALLBACK
            if (_gridSettings != null && DataGridSettingsService != null)
            {
                try
                {
                    // Sync save to memory cache
                    DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, _gridSettings);
                    Console.WriteLine($"[GestionarePersonal] Settings saved to memory cache before disposal");
                    
                    // Async save to localStorage - fire and forget
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await DataGridSettingsService.SaveSettingsAsync(GRID_SETTINGS_KEY, _gridSettings);
                        }
                        catch
                        {
                            // Memory cache has the data
                        }
                    });
                }
                catch
                {
                    // Ignore disposal errors
                }
            }
            
            // 3. Clear collections to help GC
            _groupStates?.Clear();
            _currentGroups?.Clear();
        }
        catch (Exception)
        {
            // Ignore all errors during disposal
        }
        finally
        {
            // 4. Suppress finalization
            GC.SuppressFinalize(this);
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
                    "departament" => item.Departament ?? "Nu este specificat",
                    "pozitie" => item.Pozitie ?? "Nu este specificat", 
                    "specializare" => item.Specializare ?? "Nu este specificata",
                    "esteactiv" => item.EsteActiv ? "Activ" : "Inactiv",
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

    public async Task OnGroupChanged(DataGridColumnGroupEventArgs<PersonalMedicalListDto> args)
    {
        // Reset group states when grouping changes
        _groupStates.Clear();
        
        if (args.GroupDescriptor != null)
        {
            // New group added
            _isGrouped = true;
            await UpdateCurrentGroups();
        }
        else
        {
            // Group removed
            if (!_dataGrid.Groups.Any())
            {
                _isGrouped = false;
                _currentGroups.Clear();
                _currentGroupInfo = null;
            }
        }
        
        await UpdateGroupInfo();
        StateHasChanged();
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

    // Enhanced helper methods
    private async Task<bool> IsGroupExpanded(string groupKey)
    {
        return _groupStates.GetValueOrDefault(groupKey, _allGroupsExpanded);
    }

    private async Task SetGroupExpanded(string groupKey, bool expanded)
    {
        _groupStates[groupKey] = expanded;
        await UpdateGroupInfo();
    }

    public async Task LoadDataAsync(LoadDataArgs args)
    {
        _isLoading = true;
        _hasError = false;
        _errorMessage = string.Empty;

        try
        {
            var apiQuery = new PersonalMedicalSearchQuery
            {
                Search = _searchQuery.Search,
                Departament = _selectedDepartament,
                Pozitie = _selectedPozitie,
                EsteActiv = _selectedStatus,
                Nume = _filterNume,
                Prenume = _filterPrenume,
                Specializare = _filterSpecializare,
                NumarLicenta = _filterNumarLicenta,
                Telefon = _filterTelefon,
                Email = _filterEmail,
                Page = 1,
                PageSize = _pageSize,
                Sort = _searchQuery.Sort
            };

            if (args != null)
            {
                apiQuery.Page = ((args.Skip ?? 0) / (args.Top ?? _pageSize)) + 1;
                apiQuery.PageSize = args.Top ?? _pageSize;

                // Pentru grupare, modific?m Sort s? includ? coloana de grupare PRIMA
                if (_isGrouped && _currentGroups.Any())
                {
                    var groupByColumns = string.Join(",", _currentGroups.Select(g => 
                    {
                        var direction = g.SortOrder?.ToLower() == "desc" ? "desc" : "asc";
                        return $"{g.Property}:{direction}";
                    }));
                    
                    // Combin?m gruparea cu sortarea existent?
                    if (!string.IsNullOrEmpty(args.OrderBy))
                    {
                        apiQuery.Sort = $"{groupByColumns},{args.OrderBy}";
                    }
                    else if (!string.IsNullOrEmpty(_searchQuery.Sort))
                    {
                        apiQuery.Sort = $"{groupByColumns},{_searchQuery.Sort}";
                    }
                    else
                    {
                        apiQuery.Sort = groupByColumns;
                    }
                }
                else if (!string.IsNullOrEmpty(args.OrderBy))
                {
                    apiQuery.Sort = args.OrderBy;
                }

                if (args.Filters != null && args.Filters.Any())
                {
                    ApplyDataGridFilters(args.Filters);
                    
                    apiQuery.Departament = _selectedDepartament;
                    apiQuery.Pozitie = _selectedPozitie;
                    apiQuery.EsteActiv = _selectedStatus;
                    apiQuery.Nume = _filterNume;
                    apiQuery.Prenume = _filterPrenume;
                    apiQuery.Specializare = _filterSpecializare;
                    apiQuery.NumarLicenta = _filterNumarLicenta;
                    apiQuery.Telefon = _filterTelefon;
                    apiQuery.Email = _filterEmail;
                }
            }

            var result = await PersonalMedicalApiService.GetPagedAsync(apiQuery);
            if (result.IsSuccess && result.Value != null)
            {
                _data = result.Value.Items;
                _totalCount = result.Value.TotalCount;
                
                if (_isGrouped)
                {
                    UpdateGroupingStatistics();
                }
            }
            else
            {
                _hasError = true;
                _errorMessage = string.Join(", ", result.Errors);
                _data = new List<PersonalMedicalListDto>();
                _totalCount = 0;
            }
        }
        catch (Exception ex)
        {
            _hasError = true;
            _errorMessage = $"Eroare: {ex.Message}";
            _data = new List<PersonalMedicalListDto>();
            _totalCount = 0;
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void ApplyDataGridFilters(IEnumerable<FilterDescriptor> filters)
    {
        _selectedDepartament = null;
        _selectedPozitie = null;
        _selectedStatus = null;
        _filterNume = _filterPrenume = _filterSpecializare = _filterNumarLicenta = _filterTelefon = _filterEmail = null;

        foreach (var filter in filters)
        {
            var prop = filter.Property?.ToLower();
            if (string.IsNullOrWhiteSpace(prop)) continue;
            var valText = filter.FilterValue?.ToString();
            if (string.IsNullOrWhiteSpace(valText)) continue;

            switch (prop)
            {
                case "departament": _selectedDepartament = valText; break;
                case "pozitie": _selectedPozitie = valText; break;
                case "esteactiv": if (bool.TryParse(valText, out bool b)) _selectedStatus = b; break;
                case "nume": _filterNume = valText; break;
                case "prenume": _filterPrenume = valText; break;
                case "specializare": _filterSpecializare = valText; break;
                case "numarlicenta": _filterNumarLicenta = valText; break;
                case "telefon": _filterTelefon = valText; break;
                case "email": _filterEmail = valText; break;
            }
        }
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
                    catch (Exception)
                    {
                        // Log error if needed, but don't throw in InvokeAsync
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
        _searchQuery.Search = string.Empty;
        _selectedDepartament = null;
        _selectedPozitie = null;
        _selectedStatus = null;
        _filterNume = _filterPrenume = _filterSpecializare = _filterNumarLicenta = _filterTelefon = _filterEmail = null;
        if (_dataGrid != null)
        {
            _dataGrid.Reset(true);
            await _dataGrid.Reload();
        }
    }

    public async Task ResetGridSettings()
    {
        try
        {
            await DataGridSettingsService.ClearSettingsAsync(GRID_SETTINGS_KEY);
            _gridSettings = null; // triggers grid internal reset via two-way binding
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
        catch (Exception)
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

    public async Task CreateNewPersonal()
    {
        // Navigate to dedicated add page instead of dialog - following refactoring plan
        Navigation.NavigateTo("/medical/personal/nou");
    }

    public void EditPersonal(Guid personalId)
    {
        // Navigare direct? la pagina de editare
        Navigation.NavigateTo($"/medical/personal/editare/{personalId}");
    }

    public async Task ViewPersonal(Guid personalId)
    {
        // Navigare c?tre pagina de vizualizare dedicat?
        Navigation.NavigateTo($"/medical/personal-view/{personalId}");
    }

    public async Task DeletePersonal(Guid personalId, string numeComplet)
    {
        var confirmed = await DialogService.Confirm(
            $"Esti sigur ca vrei sa stergi personalul medical '{numeComplet}'?",
            "Confirmare stergere",
            new ConfirmOptions() { OkButtonText = "Sterge", CancelButtonText = "Anuleaza" });

        if (confirmed == true)
        {
            try
            {
                var result = await PersonalMedicalApiService.DeleteAsync(personalId);
                
                if (result.IsSuccess)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Succes",
                        Detail = $"Personal medical '{numeComplet}' a fost sters cu succes",
                        Duration = 3000
                    });
                    if (_dataGrid != null) await _dataGrid.Reload();
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Eroare",
                        Detail = string.Join(", ", result.Errors),
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
                    Detail = $"Eroare la stergerea personalului: {ex.Message}",
                    Duration = 4000
                });
            }
        }
    }

    // Settings Management - IMPROVED WITH PROPER FALLBACK
    public async Task OnSettingsChanged(DataGridSettings settings)
    {
        if (_isDisposed) return;
        
        _gridSettings = settings;
        
        try
        {
            await DataGridSettingsService.SaveSettingsAsync(GRID_SETTINGS_KEY, settings);
            Console.WriteLine($"[GestionarePersonal] Settings saved for {GRID_SETTINGS_KEY}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GestionarePersonal] SaveSettings failed: {ex.Message}");
            
            // Fallback explicit la memory cache
            try
            {
                DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, settings);
                Console.WriteLine($"[GestionarePersonal] Fallback to memory cache successful");
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"[GestionarePersonal] Memory fallback also failed: {fallbackEx.Message}");
            }
        }
    }

    private async Task LoadGridSettings()
    {
        try
        {
            _gridSettings = await DataGridSettingsService.LoadSettingsAsync(GRID_SETTINGS_KEY);
            
            if (_gridSettings == null)
            {
                // Set?ri implicite
                _gridSettings = new DataGridSettings();
                
                // Salveaz? în memory cache
                DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, _gridSettings);
                Console.WriteLine($"[GestionarePersonal] Using default settings");
            }
            else
            {
                Console.WriteLine($"[GestionarePersonal] Settings loaded successfully");
            }
            
            // Restore grouping state
            if (_gridSettings?.Groups?.Any() == true)
            {
                _isGrouped = true;
                await UpdateCurrentGroups();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GestionarePersonal] LoadSettings error: {ex.Message}");
            
            // Folose?te set?ri implicite cu fallback
            _gridSettings = new DataGridSettings();
            
            try
            {
                DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, _gridSettings);
                Console.WriteLine($"[GestionarePersonal] Fallback to default settings successful");
            }
            catch
            {
                // Continue with defaults
                Console.WriteLine($"[GestionarePersonal] Using basic default settings");
            }
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
        catch (Exception)
        {
            // Log error silently
        }
    }

    public void OnRender(DataGridRenderEventArgs<PersonalMedicalListDto> args)
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