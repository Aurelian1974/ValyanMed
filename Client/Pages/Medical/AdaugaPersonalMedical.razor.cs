using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Medical;
using global::Shared.Enums.Medical;
using global::Shared.Common;
using Radzen;
using Client.Services.Medical;
using FluentValidation;
using Shared.Validators.Medical;

namespace Client.Pages.Medical;

public partial class AdaugaPersonalMedical : ComponentBase, IDisposable
{
    [Inject] private IPersonalMedicalApiService PersonalMedicalApiService { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    // Form model
    private CreatePersonalMedicalRequest _model = new() { EsteActiv = true };
    
    // Form state
    private bool _isProcessing = false;
    
    // Business logic state
    private bool _showSpecializareWarning = false;
    private bool _showLicentaWarning = false;
    
    // Dropdown options - following the refactoring plan: enums instead of magic strings
    private List<OptionItem> _pozitiiOptions = new();
    private List<OptionItem> _departamenteOptions = new();

    protected override async Task OnInitializedAsync()
    {
        LoadDropdownOptions();
        await base.OnInitializedAsync();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    private void LoadDropdownOptions()
    {
        // Load pozitii from enum - following refactoring plan: enums instead of hardcoded strings
        _pozitiiOptions = Enum.GetValues<PozitiePersonal>()
            .Select(p => new OptionItem
            {
                Value = p.GetDisplayName(),
                Text = p.GetDisplayName()
            })
            .OrderBy(o => o.Text)
            .ToList();

        // Load departamente from enum
        _departamenteOptions = Enum.GetValues<Departament>()
            .Select(d => new OptionItem
            {
                Value = d.GetDisplayName(),
                Text = d.GetDisplayName()
            })
            .OrderBy(o => o.Text)
            .ToList();
    }

    private void OnPozitieChanged(object value)
    {
        var pozitie = value?.ToString() ?? string.Empty;
        _model.Pozitie = pozitie;
        
        // Business logic: Show warnings based on position - following refactoring plan: rich services
        var isDoctorPosition = pozitie.Contains("Doctor") || pozitie.Contains("Medic");
        
        _showSpecializareWarning = isDoctorPosition && string.IsNullOrWhiteSpace(_model.Specializare);
        _showLicentaWarning = isDoctorPosition && string.IsNullOrWhiteSpace(_model.NumarLicenta);
        
        StateHasChanged();
    }

    private void OnFieldChanged(string fieldName)
    {
        // Simple field change handler - Radzen handles validation internally
        StateHasChanged();
    }

    // This method signature matches RadzenTemplateForm's Submit delegate
    private async Task OnSubmitAsync(CreatePersonalMedicalRequest model)
    {
        await SavePersonal();
    }

    private async Task SavePersonal()
    {
        if (_isProcessing) return;

        _isProcessing = true;
        
        try
        {
            // Additional business validation - following refactoring plan: business rules in services
            var businessValidation = ValidateBusinessRules();
            if (!businessValidation.IsSuccess)
            {
                await ShowBusinessRuleDialog(businessValidation.Errors);
                return;
            }

            // Call API
            var result = await PersonalMedicalApiService.CreateAsync(_model);
            
            if (result.IsSuccess)
            {
                await ShowSuccessNotification();
                
                // Navigate to details page or back to list
                Navigation.NavigateTo($"/medical/personal-view/{result.Value}");
            }
            else
            {
                await ShowErrorNotification(result.Errors);
            }
        }
        catch (HttpRequestException)
        {
            await ShowNetworkErrorNotification();
        }
        catch (TaskCanceledException)
        {
            await ShowTimeoutNotification();
        }
        catch (Exception ex)
        {
            await ShowUnexpectedErrorNotification(ex.Message);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task ShowSuccessNotification()
    {
        var message = $"Personal medical '{_model.Nume} {_model.Prenume}' a fost adaugat cu succes!";
        
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Succes",
            Detail = message,
            Duration = 4000
        });
    }

    private async Task ShowBusinessRuleDialog(IEnumerable<string> errors)
    {
        var message = "Atentie la regulile de afaceri:\n\n" + 
                     string.Join("\n", errors.Select(e => $"• {e}"));
        
        await DialogService.Alert(message, "Reguli de afaceri", new AlertOptions() 
        { 
            OkButtonText = "Am inteles" 
        });
    }

    private async Task ShowErrorNotification(IEnumerable<string> errors)
    {
        var friendlyErrors = errors.Select(GetUserFriendlyError);
        var message = string.Join(", ", friendlyErrors);
        
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Eroare de salvare",
            Detail = message,
            Duration = 6000
        });
    }

    private async Task ShowNetworkErrorNotification()
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Probleme de conectare",
            Detail = "Nu s-a putut conecta la server. Verificati conexiunea la internet.",
            Duration = 5000
        });
    }

    private async Task ShowTimeoutNotification()
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Warning,
            Summary = "Timp depasit",
            Detail = "Operatiunea a durat prea mult. Incercati din nou.",
            Duration = 5000
        });
    }

    private async Task ShowUnexpectedErrorNotification(string technicalMessage)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Eroare neasteptata",
            Detail = "A aparut o problema neobisnuita. Contactati suportul daca problema persista.",
            Duration = 6000
        });
    }

    private string GetUserFriendlyError(string error)
    {
        // Convert technical errors to user-friendly messages
        var lowerError = error.ToLower();
        
        if (lowerError.Contains("duplicate") || lowerError.Contains("unique"))
            return "Exista deja un personal medical cu aceste informatii in sistem";
        
        if (lowerError.Contains("email") && lowerError.Contains("invalid"))
            return "Adresa de email nu este valida";
        
        if (lowerError.Contains("phone") || lowerError.Contains("telefon"))
            return "Numarul de telefon nu este valid";
        
        if (lowerError.Contains("required") || lowerError.Contains("obligatoriu"))
            return "Unele campuri obligatorii nu sunt completate";
        
        if (lowerError.Contains("length") || lowerError.Contains("lung"))
            return "Unele campuri sunt prea lungi sau prea scurte";
        
        // Return original error if no specific mapping found
        return error;
    }

    private Result ValidateBusinessRules()
    {
        var errors = new List<string>();
        
        // Position-specific validations - following refactoring plan: business rules
        var isDoctorPosition = _model.Pozitie.Contains("Doctor") || _model.Pozitie.Contains("Medic");
        
        if (isDoctorPosition)
        {
            if (string.IsNullOrWhiteSpace(_model.NumarLicenta))
            {
                errors.Add("Numarul de licenta este obligatoriu pentru pozitiile de doctor/medic");
            }
        }
        
        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    private void BackToList()
    {
        Navigation.NavigateTo("/medical/gestionare-personal");
    }
}