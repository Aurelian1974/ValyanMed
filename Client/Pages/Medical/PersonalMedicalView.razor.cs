using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Medical;
using Radzen;
using Client.Services.Medical;

namespace Client.Pages.Medical;

public partial class PersonalMedicalView : ComponentBase
{
    [Inject] private IPersonalMedicalApiService PersonalMedicalApiService { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [Parameter] public Guid PersonalId { get; set; }

    private PersonalMedicalDetailDto? _personalDetail;
    private bool _isLoading = true;
    private bool _hasError = false;
    private string _errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadPersonalDetails();
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (PersonalId != Guid.Empty)
        {
            await LoadPersonalDetails();
        }
        await base.OnParametersSetAsync();
    }

    private async Task LoadPersonalDetails()
    {
        _isLoading = true;
        _hasError = false;
        _errorMessage = string.Empty;

        try
        {
            var result = await PersonalMedicalApiService.GetByIdAsync(PersonalId);
            
            if (result.IsSuccess)
            {
                _personalDetail = result.Value;
                
                if (_personalDetail == null)
                {
                    _hasError = true;
                    _errorMessage = "Personal medical nu a fost gasit.";
                }
            }
            else
            {
                _hasError = true;
                _errorMessage = string.Join(", ", result.Errors);
                ShowErrorNotification(_errorMessage);
            }
        }
        catch (Exception ex)
        {
            _hasError = true;
            _errorMessage = "Eroare la incarcarea datelor personalului medical.";
            ShowErrorNotification($"{_errorMessage} Detalii: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void GoBack()
    {
        Navigation.NavigateTo("/medical/gestionare-personal");
    }

    private void EditPersonal(Guid personalId)
    {
        // Navigare direct? la pagina de editare
        Navigation.NavigateTo($"/medical/personal/editare/{personalId}");
    }

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Eroare",
            Detail = message,
            Duration = 6000
        });
    }
}