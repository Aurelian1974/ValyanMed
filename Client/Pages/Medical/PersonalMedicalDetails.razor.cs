using Microsoft.AspNetCore.Components;
using Radzen;
using Shared.DTOs.Medical;
using Shared.Common;
using System.Net.Http.Json;

namespace Client.Pages.Medical;

public partial class PersonalMedicalDetails : ComponentBase, IDisposable
{
    [Parameter] public Guid PersonalId { get; set; }
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private PersonalMedicalDetailDto? _personalDetail;
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadPersonalDetails();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (PersonalId != Guid.Empty)
        {
            await LoadPersonalDetails();
        }
    }

    private async Task LoadPersonalDetails()
    {
        _isLoading = true;
        StateHasChanged();

        try
        {
            var response = await Http.GetAsync($"api/PersonalMedical/{PersonalId}");

            if (response.IsSuccessStatusCode)
            {
                _personalDetail = await response.Content.ReadFromJsonAsync<PersonalMedicalDetailDto>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _personalDetail = null;
                ShowNotFoundNotification();
            }
            else
            {
                ShowErrorNotification("Eroare la comunicarea cu serverul");
            }
        }
        catch (HttpRequestException)
        {
            ShowErrorNotification("Nu se poate conecta la server");
        }
        catch (Exception ex)
        {
            ShowErrorNotification($"Eroare nespecificata: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void ShowNotFoundNotification()
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Warning,
            Summary = "Nu s-a gasit",
            Detail = "Personalul medical cautat nu a fost gasit.",
            Duration = 4000
        });
    }

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Eroare de conectare",
            Detail = $"{message}. Verificati ca API-ul ruleaza pe https://localhost:7294",
            Duration = 6000
        });
    }

    private void EditPersonal(Guid personalId)
    {
        // Navigare direct? la pagina de editare
        Navigation.NavigateTo($"/medical/personal/editare/{personalId}");
    }

    private void GoBack()
    {
        Navigation.NavigateTo("/medical/personal");
    }

    private void ViewAppointments(Guid personalId)
    {
        Navigation.NavigateTo($"/medical/programari?doctorId={personalId}");
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Navigare",
            Detail = "Se deschide lista de programari pentru acest doctor.",
            Duration = 3000
        });
    }

    private void ViewConsultations(Guid personalId)
    {
        Navigation.NavigateTo($"/medical/consultatii?doctorId={personalId}");
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Navigare",
            Detail = "Se deschide lista de consultatii pentru acest doctor.",
            Duration = 3000
        });
    }

    private BadgeStyle GetStatusBadgeStyle(string status)
    {
        return status?.ToLower() switch
        {
            "confirmata" => BadgeStyle.Success,
            "programata" => BadgeStyle.Primary,
            "in asteptare" => BadgeStyle.Warning,
            "anulata" => BadgeStyle.Danger,
            "finalizata" => BadgeStyle.Success,
            _ => BadgeStyle.Secondary
        };
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}