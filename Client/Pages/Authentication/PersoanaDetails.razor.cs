using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Authentication;
using Radzen;
using System.Net.Http.Json;
using Client.Extensions;

namespace Client.Pages.Authentication;

public partial class PersoanaDetails : ComponentBase, IDisposable
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [Parameter] public int PersoanaId { get; set; }

    private PersoanaListDto? _persoana;
    private bool _isLoading = true;
    private bool _isDisposed = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadPersoanaAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (PersoanaId > 0 && !_isDisposed)
        {
            await LoadPersoanaAsync();
        }
    }

    private async Task LoadPersoanaAsync()
    {
        if (_isDisposed) return;

        _isLoading = true;
        StateHasChanged();

        try
        {
            var response = await Http.GetAsync($"api/Persoane/{PersoanaId}");
            
            _persoana = await response.HandleApiResponse<PersoanaListDto>(
                NotificationService, 
                _persoana != null ? $"Datele pentru '{_persoana.NumeComplet}' au fost incarcate cu succes." : null
            );
            
            if (_persoana == null && response.IsSuccessStatusCode)
            {
                NotificationService.ShowWarning("Persoana cautata nu a fost gasita.");
            }
        }
        catch (HttpRequestException)
        {
            NotificationService.ShowError("Nu se poate conecta la server");
        }
        catch (TaskCanceledException)
        {
            NotificationService.ShowError("Cererea a expirat");
        }
        catch (Exception ex)
        {
            NotificationService.ShowError($"Eroare neasteptata: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            if (!_isDisposed)
                StateHasChanged();
        }
    }

    private void GoBack()
    {
        Navigation.NavigateTo("/administrare/persoane");
    }

    private void EditPersoana(int persoanaId)
    {
        Navigation.NavigateTo($"/administrare/persoane/editare/{persoanaId}");
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}