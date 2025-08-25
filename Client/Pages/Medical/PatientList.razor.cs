using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using global::Shared.DTOs.Medical;
using global::Shared.Common;
using System.Net.Http.Json;
using Client.Services;
using System.Text.Json;

namespace Client.Pages.Medical;

public class PatientListBase : ComponentBase, IDisposable
{
    #region Dependencies

    [Inject] protected HttpClient Http { get; set; } = null!;
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;
    [Inject] protected IDialogService DialogService { get; set; } = null!;
    [Inject] protected IJsonService JsonService { get; set; } = null!;

    #endregion

    #region Properties

    protected PatientListState State { get; set; } = new();
    private Timer? searchTimer;

    #endregion

    #region Lifecycle

    protected override async Task OnInitializedAsync()
    {
        await LoadPatientsAsync();
    }

    public void Dispose()
    {
        searchTimer?.Dispose();
    }

    #endregion

    #region Patient Management

    protected async Task LoadPatientsAsync()
    {
        State.IsLoading = true;
        StateHasChanged();

        try
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(State.SearchQuery.Search))
                queryParams.Add($"search={Uri.EscapeDataString(State.SearchQuery.Search)}");
            
            if (!string.IsNullOrEmpty(State.SearchQuery.Judet))
                queryParams.Add($"judet={Uri.EscapeDataString(State.SearchQuery.Judet)}");
            
            if (!string.IsNullOrEmpty(State.SearchQuery.Gen))
                queryParams.Add($"gen={Uri.EscapeDataString(State.SearchQuery.Gen)}");
            
            queryParams.Add($"page={State.SearchQuery.Page}");
            queryParams.Add($"pageSize={State.SearchQuery.PageSize}");
            
            if (!string.IsNullOrEmpty(State.SearchQuery.Sort))
                queryParams.Add($"sort={Uri.EscapeDataString(State.SearchQuery.Sort)}");

            var queryString = string.Join("&", queryParams);
            var url = $"api/medical/pacienti?{queryString}";

            Console.WriteLine($"Loading patients with URL: {url}"); // Debug log

            var response = await Http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonService.Deserialize<PagedResult<PacientListDto>>(content);
                
                if (result != null)
                {
                    State.UpdatePagedResult(result);
                    Console.WriteLine($"Loaded {result.Items.Count()} patients out of {result.TotalCount} total"); // Debug log
                }
                else
                {
                    ShowError("Datele primite de la server sunt invalide");
                }
            }
            else
            {
                await HandleErrorResponse(response);
            }
        }
        catch (HttpRequestException ex)
        {
            ShowError($"Eroare de conectare: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            ShowError("Cererea a expirat. Va rugam incercati din nou.");
        }
        catch (JsonException ex)
        {
            ShowError($"Eroare la procesarea datelor: {ex.Message}");
        }
        catch (Exception ex)
        {
            ShowError($"Eroare neasteptata: {ex.Message}");
        }
        finally
        {
            State.IsLoading = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Search and Filtering

    protected async Task OnSearchChanged(string text)
    {
        State.SearchQuery.Search = text ?? string.Empty;
        
        // Debounce search with proper timer management
        searchTimer?.Dispose();
        searchTimer = new Timer(async _ =>
        {
            State.SearchQuery.Page = 1; // Reset to first page when searching
            await InvokeAsync(async () =>
            {
                await LoadPatientsAsync();
                StateHasChanged();
            });
        }, null, 300, Timeout.Infinite); // Faster response - 300ms
    }

    protected async Task OnJudetChanged(string judet)
    {
        // Interpret "ALL" as no filter
        State.SearchQuery.Judet = (judet == "ALL") ? string.Empty : judet;
        State.SearchQuery.Page = 1; // Reset to first page when filtering
        await LoadPatientsAsync();
    }

    protected async Task OnGenChanged(string gen)
    {
        // Interpret "ALLGEN" as no filter
        State.SearchQuery.Gen = (gen == "ALLGEN") ? string.Empty : gen;
        State.SearchQuery.Page = 1; // Reset to first page when filtering
        await LoadPatientsAsync();
    }

    protected async Task OnSearchKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            searchTimer?.Dispose(); // Cancel debounce timer
            State.SearchQuery.Page = 1; // Reset to first page when searching
            await LoadPatientsAsync();
        }
    }

    protected async Task SearchPacienti()
    {
        searchTimer?.Dispose(); // Cancel debounce timer
        State.SearchQuery.Page = 1; // Reset to first page when searching
        await LoadPatientsAsync();
    }

    protected async Task ResetFilters()
    {
        var currentPageSize = State.SearchQuery.PageSize; // Preserve page size
        
        State.SearchQuery = new PacientiSearchQuery
        {
            Page = 1,
            PageSize = currentPageSize
        };
        
        await LoadPatientsAsync();
    }

    #endregion

    #region Pagination

    protected async Task OnPageChanged(int page)
    {
        if (page != State.SearchQuery.Page)
        {
            State.SearchQuery.Page = page;
            await LoadPatientsAsync();
        }
    }

    protected async Task OnPageSizeChanged(int pageSize)
    {
        if (pageSize != State.SearchQuery.PageSize)
        {
            State.SearchQuery.PageSize = pageSize;
            State.SearchQuery.Page = 1; // Reset to first page when changing page size
            await LoadPatientsAsync();
        }
    }

    #endregion

    #region Master-Detail Controls

    protected void ToggleRowExpansion(Guid pacientId)
    {
        State.ToggleRowExpansion(pacientId);
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

    #region Patient Actions

    protected void ViewPacient(Guid pacientId)
    {
        // For demo purposes, show a message since the actual patient page might not exist
        Snackbar.Add($"Functionalitate in dezvoltare. ID pacient: {pacientId}", MudBlazor.Severity.Info);
        // Navigation.NavigateTo($"/medical/pacienti/{pacientId}");
    }

    protected void EditPacient(Guid pacientId)
    {
        // For demo purposes, show a message since the actual edit page might not exist
        Snackbar.Add($"Functionalitate in dezvoltare. Editare pacient: {pacientId}", MudBlazor.Severity.Info);
        // Navigation.NavigateTo($"/medical/pacienti/{pacientId}/editare");
    }

    protected void NewAppointment(Guid pacientId)
    {
        // For demo purposes, show a message since the actual appointment page might not exist
        Snackbar.Add($"Functionalitate in dezvoltare. Programare pentru pacient: {pacientId}", MudBlazor.Severity.Info);
        // Navigation.NavigateTo($"/medical/programari/noua?pacientId={pacientId}")
    }

    protected void ViewMedicalHistory(Guid pacientId)
    {
        // For demo purposes, show a message since the actual history page might not exist
        Snackbar.Add($"Functionalitate in dezvoltare. Istoric medical pentru pacient: {pacientId}", MudBlazor.Severity.Info);
        // Navigation.NavigateTo($"/medical/pacienti/{pacientId}/istoric");
    }

    protected async Task ExportToExcel()
    {
        try
        {
            // TODO: Implement Excel export functionality
            Snackbar.Add("Functionalitatea de export va fi implementata curand.", MudBlazor.Severity.Info);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Export error: {ex.Message}");
            Snackbar.Add("Eroare la exportul datelor.", MudBlazor.Severity.Error);
        }
    }

    protected void CreateNewPatient()
    {
        // For demo purposes, show a message since the actual new patient page might not exist
        Snackbar.Add("Navighez la pagina de inregistrare pacient nou...", MudBlazor.Severity.Success);
        // Navigation.NavigateTo("/medical/pacienti/nou");
    }

    #endregion

    #region Helper Methods

    private async Task HandleErrorResponse(HttpResponseMessage response)
    {
        try
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonService.Deserialize<ErrorResponse>(errorContent);

            if (errorResponse?.Errors != null && errorResponse.Errors.Any())
            {
                foreach (var error in errorResponse.Errors)
                {
                    ShowError(error);
                }
            }
            else if (!string.IsNullOrEmpty(errorResponse?.Message))
            {
                ShowError(errorResponse.Message);
            }
            else
            {
                ShowError($"Eroare HTTP: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (Exception)
        {
            ShowError($"Eroare HTTP: {response.StatusCode} - {response.ReasonPhrase}");
        }
    }

    private void ShowSuccess(string message)
    {
        Snackbar.Add(message, MudBlazor.Severity.Success, configure =>
        {
            configure.VisibleStateDuration = 3000;
            configure.ShowTransitionDuration = 200;
            configure.HideTransitionDuration = 200;
        });
    }

    private void ShowError(string message)
    {
        Snackbar.Add(message, MudBlazor.Severity.Error, configure =>
        {
            configure.VisibleStateDuration = 5000;
            configure.ShowTransitionDuration = 200;
            configure.HideTransitionDuration = 200;
            configure.RequireInteraction = true;
        });
    }

    #endregion

    #region Models

    private record ErrorResponse(string Message, List<string>? Errors = null);

    #endregion
}