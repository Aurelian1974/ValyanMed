using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.Models.Authentication;
using Shared.DTOs.Authentication;
using System.Net.Http.Json;
using System.Text.Json;
using MudSeverity = MudBlazor.Severity;
using Client.Services;

namespace Client.Shared.Dialogs;

public class UserDialogBase : ComponentBase, IDisposable
{
    #region Dependencies

    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected HttpClient Http { get; set; } = null!;
    [Inject] protected IJsonService JsonService { get; set; } = null!;

    #endregion

    #region Parameters

    [CascadingParameter] 
    public IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] 
    public Utilizator? Utilizator { get; set; }

    [Parameter]
    public bool IsEditMode { get; set; }

    [Parameter]
    public bool IsReadOnly { get; set; } = false;

    #endregion

    #region Properties

    protected MudForm form = null!;
    protected UserFormModel Model { get; set; } = new();
    protected UserFormValidator Validator { get; set; } = new();
    protected bool IsSubmitting { get; set; } = false;
    protected bool ShowPassword { get; set; } = false;
    
    protected List<Persoana> AvailablePersons { get; set; } = new();

    #endregion

    #region Lifecycle

    protected override async Task OnInitializedAsync()
    {
        await LoadPersonsAsync();
        InitializeModel();
    }

    private async Task LoadPersonsAsync()
    {
        try
        {
            var response = await Http.GetAsync("api/persoane");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonService.Deserialize<List<Persoana>>(content);
                AvailablePersons = result ?? new List<Persoana>();
            }
            else
            {
                ShowError("Eroare la incarcarea persoanelor");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Eroare la incarcarea persoanelor: {ex.Message}");
        }
    }

    private void InitializeModel()
    {
        if (Utilizator != null)
        {
            Model = new UserFormModel
            {
                NumeUtilizator = Utilizator.NumeUtilizator,
                Email = Utilizator.Email,
                Telefon = Utilizator.Telefon,
                SelectedPersoanaId = Utilizator.PersoanaId
            };
        }
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    #endregion

    #region Actions

    protected virtual async Task Submit()
    {
        await form.Validate();
        
        if (!form.IsValid)
        {
            ShowError("Va rugam sa corectati erorile din formular");
            return;
        }

        IsSubmitting = true;
        StateHasChanged();
        try
        {
            if (IsEditMode)
            {
                await UpdateUserAsync();
            }
            else
            {
                await CreateUserAsync();
            }
        }
        catch (Exception ex)
        {
            ShowError($"Eroare neasteptata: {ex.Message}");
        }
        finally
        {
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    private async Task CreateUserAsync()
    {
        var request = new CreateUtilizatorRequest(
            Model.NumeUtilizator,
            Model.Parola,
            Model.Email,
            Model.Telefon,
            Model.SelectedPersoanaId!.Value
        );

        try
        {
            var response = await Http.PostAsJsonAsync("api/utilizatori", request, JsonService.Options);
            
            if (response.IsSuccessStatusCode)
            {
                ShowSuccess("Utilizatorul a fost adaugat cu succes");
                MudDialog.Close(DialogResult.Ok(true));
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
    }

    private async Task UpdateUserAsync()
    {
        if (Utilizator == null) return;

        var request = new UpdateUtilizatorRequest(
            Utilizator.Id,
            Model.NumeUtilizator,
            Model.Email,
            Model.Telefon,
            Model.SelectedPersoanaId!.Value,
            string.IsNullOrWhiteSpace(Model.NovaParola) ? null : Model.NovaParola
        );

        try
        {
            var response = await Http.PutAsJsonAsync($"api/utilizatori/{Utilizator.Id}", request, JsonService.Options);
            
            if (response.IsSuccessStatusCode)
            {
                ShowSuccess("Utilizatorul a fost actualizat cu succes");
                MudDialog.Close(DialogResult.Ok(true));
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
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    #endregion

    #region Helper Methods

    protected string GetPersonDisplayName(int? personId)
    {
        if (!personId.HasValue) return string.Empty;
        return AvailablePersons.FirstOrDefault(p => p.Id == personId.Value)?.NumeComplet ?? string.Empty;
    }

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
        Snackbar.Add(message, MudSeverity.Success, configure => {
            configure.VisibleStateDuration = 3000;
            configure.ShowTransitionDuration = 200;
            configure.HideTransitionDuration = 200;
        });
    }
    
    private void ShowError(string message)
    {
        Snackbar.Add(message, MudSeverity.Error, configure => {
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