using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Shared.Models.Authentication;
using System.Net.Http.Json;
using System.Text.Json;
using Client.Shared.Dialogs;
using Client.Services;
using MudSeverity = MudBlazor.Severity;

namespace Client.Pages.Authentication;

public class UtilizatoriBase : ComponentBase, IDisposable
{
    #region Dependencies

    [Inject] protected HttpClient Http { get; set; } = null!;
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected IDialogService DialogService { get; set; } = null!;
    [Inject] protected IJsonService JsonService { get; set; } = null!;

    #endregion

    #region Properties

    protected UtilizatoriState State { get; set; } = new();
    private Timer? searchTimer;

    #endregion

    #region Lifecycle

    protected override async Task OnInitializedAsync()
    {
        await LoadUsersAsync();
    }

    public void Dispose()
    {
        searchTimer?.Dispose();
    }

    #endregion

    #region User Management

    protected async Task LoadUsersAsync()
    {
        State.IsLoading = true;
        StateHasChanged();

        try
        {
            var response = await Http.GetAsync("api/utilizatori");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonService.Deserialize<List<Utilizator>>(content);
                State.Users = result ?? new List<Utilizator>();
                State.FilterUsers();
                ShowSuccess("Utilizatorii au fost incarcati cu succes");
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

    protected async Task OpenCreateUserDialog()
    {
        var parameters = new DialogParameters
        {
            { nameof(UserDialog.IsEditMode), false },
            { nameof(UserDialog.IsReadOnly), false }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true,
            BackdropClick = false,
            Position = DialogPosition.Center
        };

        var dialog = await DialogService.ShowAsync<UserDialog>("Adauga Utilizator", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await LoadUsersAsync();
        }
    }

    // Vizualizare utilizator (read-only)
    protected async Task ViewUser(Utilizator utilizator)
    {
        var parameters = new DialogParameters
        {
            { nameof(UserDialog.Utilizator), utilizator },
            { nameof(UserDialog.IsEditMode), false },
            { nameof(UserDialog.IsReadOnly), true }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true,
            BackdropClick = false,
            Position = DialogPosition.Center
        };

        await DialogService.ShowAsync<UserDialog>("Vizualizare Utilizator", parameters, options);
    }

    protected async Task OpenEditUserDialog(Utilizator utilizator)
    {
        var parameters = new DialogParameters
        {
            { nameof(UserDialog.Utilizator), utilizator },
            { nameof(UserDialog.IsEditMode), true },
            { nameof(UserDialog.IsReadOnly), false }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true,
            BackdropClick = false,
            Position = DialogPosition.Center
        };

        var dialog = await DialogService.ShowAsync<UserDialog>("Editeaza Utilizator", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await LoadUsersAsync();
        }
    }

    protected async Task DeleteUser(Utilizator utilizator)
    {
        var parameters = new DialogParameters
        {
            { nameof(ConfirmDialog.Title), "Confirmare Stergere" },
            { nameof(ConfirmDialog.ContentText), $"Sunteti sigur ca doriti sa stergeti utilizatorul '{utilizator.NumeUtilizator}'?" },
            { nameof(ConfirmDialog.Color), Color.Error }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            CloseButton = true,
            BackdropClick = false,
            Position = DialogPosition.Center
        };

        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmare", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is true)
        {
            await DeleteUserAsync(utilizator.Id);
        }
    }

    private async Task DeleteUserAsync(int userId)
    {
        try
        {
            var response = await Http.DeleteAsync($"api/utilizatori/{userId}");

            if (response.IsSuccessStatusCode)
            {
                ShowSuccess("Utilizatorul a fost sters cu succes");
                await LoadUsersAsync();
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
        catch (Exception ex)
        {
            ShowError($"Eroare neasteptata: {ex.Message}");
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

    // Noi metode pentru controlul expansiunii conform principiilor PLAN_REFACTORING
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
        Snackbar.Add(message, MudSeverity.Success, configure =>
        {
            configure.VisibleStateDuration = 3000;
            configure.ShowTransitionDuration = 200;
            configure.HideTransitionDuration = 200;
        });
    }

    private void ShowError(string message)
    {
        Snackbar.Add(message, MudSeverity.Error, configure =>
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