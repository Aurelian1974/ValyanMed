using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Authentication;
using Radzen;
using System.Net.Http.Json;

namespace Client.Pages.Authentication;

public partial class PersoanaDetails : ComponentBase, IDisposable
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [Parameter] public int PersoanaId { get; set; }

    private PersoanaListDto? _persoana;
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadPersoanaAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (PersoanaId > 0)
        {
            await LoadPersoanaAsync();
        }
    }

    private async Task LoadPersoanaAsync()
    {
        _isLoading = true;
        StateHasChanged();

        try
        {
            var response = await Http.GetAsync($"api/Persoane/{PersoanaId}");
            
            if (response.IsSuccessStatusCode)
            {
                _persoana = await response.Content.ReadFromJsonAsync<PersoanaListDto>();
                
                if (_persoana != null)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Date incarcate",
                        Detail = $"Datele pentru '{_persoana.NumeComplet}' au fost incarcate cu succes.",
                        Duration = 3000
                    });
                }
                else
                {
                    ShowNotFoundNotification();
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _persoana = null;
                ShowNotFoundNotification();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ShowErrorNotification($"Eroare la incarcarea datelor: {errorContent}");
            }
        }
        catch (HttpRequestException)
        {
            ShowErrorNotification("Nu se poate conecta la server");
        }
        catch (TaskCanceledException)
        {
            ShowErrorNotification("Cererea a expirat");
        }
        catch (Exception ex)
        {
            ShowErrorNotification($"Eroare neasteptata: {ex.Message}");
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
            Detail = "Persoana cautata nu a fost gasita.",
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

    private void EditPersoana(int persoanaId)
    {
        Navigation.NavigateTo($"/administrare/persoane/editare/{persoanaId}");
    }

    private void GoBack()
    {
        Navigation.NavigateTo("/administrare/gestionare-persoane");
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}