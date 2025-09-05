using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Authentication;
using Radzen;
using System.Net.Http.Json;

namespace Client.Pages.Authentication;

public partial class VerificarePersoana : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [Parameter] public int PersoanaId { get; set; }

    private PersoanaListDto? _persoana;
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadPersoanaDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (PersoanaId > 0)
        {
            await LoadPersoanaDataAsync();
        }
    }

    private async Task LoadPersoanaDataAsync()
    {
        _isLoading = true;
        StateHasChanged();

        try
        {
            var response = await Http.GetAsync($"api/Persoane/{PersoanaId}");
            
            if (response.IsSuccessStatusCode)
            {
                _persoana = await response.Content.ReadFromJsonAsync<PersoanaListDto>();
            }
            else
            {
                Console.WriteLine($"Failed to load person with ID {PersoanaId}: {response.StatusCode}");
                _persoana = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading person data: {ex.Message}");
            _persoana = null;
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Eroare",
                Detail = "Nu s-au putut incarca datele persoanei.",
                Duration = 4000
            });
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void EditPersoana()
    {
        Navigation.NavigateTo($"/administrare/persoane/editare/{PersoanaId}");
    }

    private void AddNewPersoana()
    {
        Navigation.NavigateTo("/administrare/persoane/nou");
    }

    private void BackToList()
    {
        Navigation.NavigateTo("/administrare/gestionare-persoane");
    }

    private void AcceptData()
    {
        // Datele sunt corecte - procesul este finalizat
        Console.WriteLine($"AcceptData called for person ID: {PersoanaId}");
        
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Date confirmate",
            Detail = $"Datele pentru '{_persoana?.Nume} {_persoana?.Prenume}' au fost confirmate ca fiind corecte.",
            Duration = 4000
        });
        
        // Navigheaza catre lista de persoane
        Navigation.NavigateTo("/administrare/gestionare-persoane");
    }

    private void GoBack()
    {
        // Datele sunt incorecte - revin la pagina de adaugare/modificare
        Console.WriteLine($"GoBack called for person ID: {PersoanaId}");
        
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Revenire la editare",
            Detail = "Va intoarceti la pagina de editare pentru a corecta datele.",
            Duration = 3000
        });
        
        // Navigheaza catre pagina de editare
        Navigation.NavigateTo($"/administrare/persoane/editare/{PersoanaId}");
    }
}