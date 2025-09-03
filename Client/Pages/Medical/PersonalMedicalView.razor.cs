using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Medical;
using Radzen;
using Client.Services.Medical;
using Client.Extensions;

namespace Client.Pages.Medical;

public partial class PersonalMedicalView : ComponentBase, IDisposable
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
    private bool _isDisposed = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadPersonalDetails();
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (PersonalId != Guid.Empty && !_isDisposed)
        {
            await LoadPersonalDetails();
        }
        await base.OnParametersSetAsync();
    }

    private async Task LoadPersonalDetails()
    {
        if (_isDisposed) return;

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
                    NotificationService.ShowWarning(_errorMessage);
                }
            }
            else
            {
                _hasError = true;
                _errorMessage = string.Join(", ", result.Errors);
                result.ShowNotification(NotificationService);
            }
        }
        catch (Exception ex)
        {
            _hasError = true;
            _errorMessage = "Eroare la incarcarea datelor personalului medical.";
            NotificationService.ShowError($"{_errorMessage} Detalii: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            if (!_isDisposed)
                StateHasChanged();
        }
    }

    // SINGLE BACK BUTTON - eliminam duplicatul
    private void GoBack()
    {
        Navigation.NavigateTo("/medical/gestionare-personal");
    }

    private void EditPersonal(Guid personalId)
    {
        Navigation.NavigateTo($"/medical/personal/editare/{personalId}");
    }

    // IMPLEMENTARE DISPOSE PATTERN
    public void Dispose()
    {
        if (_isDisposed) return;
        
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}