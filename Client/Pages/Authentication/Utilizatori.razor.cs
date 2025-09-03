using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using Shared.Models.Authentication;
using System.Net.Http.Json;
using System.Text.Json;
using Client.Services;
using Client.Extensions;

namespace Client.Pages.Authentication;

public class UtilizatoriBase : ComponentBase, IDisposable
{
    #region Dependencies

    [Inject] protected HttpClient Http { get; set; } = null!;
    [Inject] protected NotificationService NotificationService { get; set; } = null!;
    [Inject] protected DialogService DialogService { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;
    [Inject] protected IJsonService JsonService { get; set; } = null!;
    [Inject] protected IDataGridSettingsService DataGridSettingsService { get; set; } = null!;

    #endregion

    #region Properties

    protected UtilizatoriState State { get; set; } = new();
    private Timer? searchTimer;
    private bool _isDisposed = false;

    // DataGrid Settings Persistence
    protected DataGridSettings? _gridSettings;
    private const string GRID_SETTINGS_KEY = "utilizatori_grid_settings";

    #endregion

    #region Lifecycle

    protected override async Task OnInitializedAsync()
    {
        // Load grid settings first
        await LoadGridSettingsAsync();
        
        await LoadUsersAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _gridSettings != null)
        {
            try
            {
                // Mic? întârziere pentru ini?ializarea complet? a grid-ului
                await Task.Delay(100);
                if (!_isDisposed)
                    StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Utilizatori] OnAfterRender error: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        _isDisposed = true;
        
        try
        {
            // 1. Dispose timer safely
            searchTimer?.Dispose();
            searchTimer = null;
            
            // 2. Save settings to memory cache before disposal - IMPROVED
            if (_gridSettings != null && DataGridSettingsService != null)
            {
                try
                {
                    // Sync save to memory cache
                    DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, _gridSettings);
                    Console.WriteLine($"[Utilizatori] Settings saved to memory cache before disposal");
                    
                    // Fire and forget pentru localStorage
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
        }
        catch (Exception)
        {
            // Ignore all errors during disposal
        }
        finally
        {
            // 3. Suppress finalization
            GC.SuppressFinalize(this);
        }
    }

    #endregion

    #region Grid Settings Management

    protected async Task LoadGridSettingsAsync()
    {
        if (_isDisposed) return;

        try
        {
            _gridSettings = await DataGridSettingsService.LoadSettingsAsync(GRID_SETTINGS_KEY);
            
            if (_gridSettings == null)
            {
                // Set?ri implicite pentru grid
                _gridSettings = new DataGridSettings();
                
                // Salveaz? set?rile implicite în memory cache
                DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, _gridSettings);
                Console.WriteLine($"[Utilizatori] Using default settings");
            }
            else
            {
                Console.WriteLine($"[Utilizatori] Settings loaded successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Utilizatori] LoadGridSettings error: {ex.Message}");
            
            // Folose?te set?ri implicite cu fallback
            _gridSettings = new DataGridSettings();
            
            try
            {
                DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, _gridSettings);
                Console.WriteLine($"[Utilizatori] Fallback to default settings successful");
            }
            catch
            {
                // Continue with defaults
                Console.WriteLine($"[Utilizatori] Using basic default settings");
            }
        }
    }

    protected async Task OnSettingsChanged(DataGridSettings settings)
    {
        if (_isDisposed) return;

        _gridSettings = settings;
        
        try
        {
            await DataGridSettingsService.SaveSettingsAsync(GRID_SETTINGS_KEY, settings);
            Console.WriteLine($"[Utilizatori] Settings saved for {GRID_SETTINGS_KEY}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Utilizatori] OnSettingsChanged error: {ex.Message}");
            
            // Fallback explicit la memory cache
            try
            {
                DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, settings);
                Console.WriteLine($"[Utilizatori] Fallback to memory cache successful");
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"[Utilizatori] Memory fallback also failed: {fallbackEx.Message}");
            }
        }
    }

    protected async Task ResetGridSettings()
    {
        try
        {
            await DataGridSettingsService.ClearSettingsAsync(GRID_SETTINGS_KEY);
            await LoadGridSettingsAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Utilizatori] ResetGridSettings error: {ex.Message}");
        }
    }

    #endregion

    #region User Management

    protected async Task LoadUsersAsync()
    {
        if (_isDisposed) return;

        State.IsLoading = true;
        StateHasChanged();

        try
        {
            var response = await Http.GetAsync("api/utilizatori");
            var result = await response.HandleApiResponse<List<Utilizator>>(
                NotificationService, 
                "Utilizatorii au fost incarcati cu succes"
            );
            
            State.Users = result ?? new List<Utilizator>();
            State.FilterUsers();
        }
        catch (HttpRequestException ex)
        {
            NotificationService.ShowError($"Eroare de conectare: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            NotificationService.ShowError("Cererea a expirat. Va rugam incercati din nou.");
        }
        catch (JsonException ex)
        {
            NotificationService.ShowError($"Eroare la procesarea datelor: {ex.Message}");
        }
        catch (Exception ex)
        {
            NotificationService.ShowError($"Eroare neasteptata: {ex.Message}");
        }
        finally
        {
            State.IsLoading = false;
            if (!_isDisposed)
                StateHasChanged();
        }
    }

    protected void CreateNewUser()
    {
        Navigation.NavigateTo("/administrare/utilizatori/nou");
    }

    protected void ViewUser(Utilizator utilizator)
    {
        Navigation.NavigateTo($"/administrare/utilizatori/view/{utilizator.Id}");
    }

    protected void EditUser(Utilizator utilizator)
    {
        Navigation.NavigateTo($"/administrare/utilizatori/editare/{utilizator.Id}");
    }

    protected async Task DeleteUser(Utilizator utilizator)
    {
        var confirmed = await DialogService.Confirm(
            $"Sunteti sigur ca doriti sa stergeti utilizatorul '{utilizator.NumeUtilizator}'?",
            "Confirmare Stergere",
            new ConfirmOptions() { OkButtonText = "Sterge", CancelButtonText = "Anuleaza" }
        );

        if (confirmed == true)
        {
            await DeleteUserAsync(utilizator.Id);
        }
    }

    private async Task DeleteUserAsync(int userId)
    {
        try
        {
            var response = await Http.DeleteAsync($"api/utilizatori/{userId}");

            if (await response.HandleApiResponse(NotificationService, "Utilizatorul a fost sters cu succes"))
            {
                await LoadUsersAsync();
            }
        }
        catch (HttpRequestException ex)
        {
            NotificationService.ShowError($"Eroare de conectare: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            NotificationService.ShowError("Cererea a expirat. Va rugam incercati din nou.");
        }
        catch (Exception ex)
        {
            NotificationService.ShowError($"Eroare neasteptata: {ex.Message}");
        }
    }

    #endregion

    #region Search and Filtering

    protected Task OnSearchChanged(string text)
    {
        State.SearchTerm = text ?? string.Empty;
        State.FilterUsers();
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected void FilterUsers()
    {
        State.FilterUsers();
        StateHasChanged();
    }

    protected void ResetFilters()
    {
        State.SearchTerm = string.Empty;
        State.FilterUsers();
        StateHasChanged();
    }

    protected void ToggleRowExpansion(int userId)
    {
        State.ToggleRowExpansion(userId);
        StateHasChanged();
    }

    protected void ExpandAllRows()
    {
        State.ExpandAllRows();
        StateHasChanged();
    }

    protected void CollapseAllRows()
    {
        State.CollapseAllRows();
        StateHasChanged();
    }

    #endregion

    #region Models

    private record ErrorResponse(string Message, List<string>? Errors = null);

    #endregion
}