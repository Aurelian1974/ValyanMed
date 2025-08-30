using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.Models.Authentication;
using Shared.DTOs.Authentication;
using Shared.Validators.Authentication;
using FluentValidation;
using System.Net.Http.Json;
using System.Text.Json;
using MudSeverity = MudBlazor.Severity;
using Client.Services;

namespace Client.Shared.Dialogs;

public class PersonDialogBase : ComponentBase, IDisposable
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
    public Persoana? Persoana { get; set; }

    [Parameter]
    public bool IsEditMode { get; set; }

    #endregion

    #region Properties

    protected MudForm form = null!;
    protected PersonFormModel Model { get; set; } = new();
    protected PersonFormValidator Validator { get; set; } = new();
    protected bool IsSubmitting { get; set; } = false;
    
    // Masks
    protected readonly IMask CnpMask = new PatternMask("0000000000000");

    #endregion

    #region Lifecycle

    protected override void OnInitialized()
    {
        InitializeModel();
    }

    private void InitializeModel()
    {
        if (IsEditMode && Persoana != null)
        {
            Model = new PersonFormModel
            {
                Nume = Persoana.Nume,
                Prenume = Persoana.Prenume,
                Judet = Persoana.Judet,
                Localitate = Persoana.Localitate,
                Strada = Persoana.Strada,
                NumarStrada = Persoana.NumarStrada,
                CodPostal = Persoana.CodPostal,
                PozitieOrganizatie = Persoana.PozitieOrganizatie,
                DataNasterii = Persoana.DataNasterii,
                CNP = Persoana.CNP,
                TipActIdentitate = Persoana.TipActIdentitate,
                SerieActIdentitate = Persoana.SerieActIdentitate,
                NumarActIdentitate = Persoana.NumarActIdentitate,
                StareCivila = Persoana.StareCivila,
                Gen = Persoana.Gen
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
                await UpdatePersonAsync();
            }
            else
            {
                await CreatePersonAsync();
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

    private async Task CreatePersonAsync()
    {
        var request = new CreatePersoanaRequest(
            Model.Nume,
            Model.Prenume,
            Model.Judet,
            Model.Localitate,
            Model.Strada,
            Model.NumarStrada,
            Model.CodPostal,
            Model.PozitieOrganizatie,
            Model.DataNasterii,
            Model.CNP,
            Model.TipActIdentitate,
            Model.SerieActIdentitate,
            Model.NumarActIdentitate,
            Model.StareCivila,
            Model.Gen
        );

        try
        {
            var response = await Http.PostAsJsonAsync("api/persoane", request, JsonService.Options);
            
            if (response.IsSuccessStatusCode)
            {
                ShowSuccess("Persoana a fost adaugata cu succes");
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

    private async Task UpdatePersonAsync()
    {
        if (Persoana == null) return;

        var request = new UpdatePersoanaRequest(
            Persoana.Id,
            Model.Nume,
            Model.Prenume,
            Model.Judet,
            Model.Localitate,
            Model.Strada,
            Model.NumarStrada,
            Model.CodPostal,
            Model.PozitieOrganizatie,
            Model.DataNasterii,
            Model.CNP,
            Model.TipActIdentitate,
            Model.SerieActIdentitate,
            Model.NumarActIdentitate,
            Model.StareCivila,
            Model.Gen
        );

        try
        {
            var response = await Http.PutAsJsonAsync($"api/persoane/{Persoana.Id}", request, JsonService.Options);
            
            if (response.IsSuccessStatusCode)
            {
                ShowSuccess("Persoana a fost actualizata cu succes");
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

    #region Error Handling

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

    #endregion

    #region Helper Methods

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