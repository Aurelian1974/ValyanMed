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

public class PersoaneBase : ComponentBase, IDisposable
{
    #region Dependencies

    [Inject] protected HttpClient Http { get; set; } = null!;
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected IDialogService DialogService { get; set; } = null!;
    [Inject] protected IJsonService JsonService { get; set; } = null!;

    #endregion

    #region Properties

    protected PersoaneState State { get; set; } = new();
    private Timer? searchTimer;

    #endregion

    #region Lifecycle

    protected override async Task OnInitializedAsync()
    {
        await LoadPersonsAsync();
    }

    public void Dispose()
    {
        searchTimer?.Dispose();
    }

    #endregion

    #region Person Management

    protected async Task LoadPersonsAsync()
    {
        State.IsLoading = true;
        StateHasChanged();

        try
        {
            var response = await Http.GetAsync("api/persoane");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonService.Deserialize<List<Persoana>>(content);
                State.Persons = result ?? new List<Persoana>();
                State.FilterPersons();
                
                ShowSuccess("Persoanele au fost incarcate cu succes");
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

    protected async Task OpenCreatePersonDialog()
    {
        var parameters = new DialogParameters
        {
            { nameof(PersonDialog.IsEditMode), false }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true,
            BackdropClick = false,
            Position = DialogPosition.Center
        };

        var dialog = await DialogService.ShowAsync<PersonDialog>("Adauga Persoana", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await LoadPersonsAsync();
        }
    }

    protected async Task OpenEditPersonDialog(Persoana persoana)
    {
        var parameters = new DialogParameters
        {
            { nameof(PersonDialog.Persoana), persoana },
            { nameof(PersonDialog.IsEditMode), true }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true,
            BackdropClick = false,
            Position = DialogPosition.Center
        };

        var dialog = await DialogService.ShowAsync<PersonDialog>("Editeaza Persoana", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await LoadPersonsAsync();
        }
    }

    protected async Task DeletePerson(Persoana persoana)
    {
        var parameters = new DialogParameters
        {
            { nameof(ConfirmDialog.Title), "Confirmare Stergere" },
            { nameof(ConfirmDialog.ContentText), $"Sunteti sigur ca doriti sa stergeti persoana '{persoana.NumeComplet}'?" },
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
            await DeletePersonAsync(persoana.Id);
        }
    }

    private async Task DeletePersonAsync(int personId)
    {
        try
        {
            var response = await Http.DeleteAsync($"api/persoane/{personId}");

            if (response.IsSuccessStatusCode)
            {
                ShowSuccess("Persoana a fost stearsa cu succes");
                await LoadPersonsAsync();
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

    protected void FilterPersons()
    {
        State.FilterPersons();
        StateHasChanged();
    }

    protected void ResetFilters()
    {
        State.SearchTerm = string.Empty;
        State.FilterPersons();
        StateHasChanged();
    }

    protected void OnSearchKeyUp(KeyboardEventArgs e)
    {
        searchTimer?.Dispose();
        searchTimer = new Timer(async _ =>
        {
            await InvokeAsync(() =>
            {
                State.FilterPersons();
                StateHasChanged();
            });
        }, null, 300, Timeout.Infinite);
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