using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Authentication;
using global::Shared.Common;
using global::Shared.Enums;
using Radzen;
using System.Net.Http.Json;

namespace Client.Pages.Authentication;

public partial class AdaugEditezPersoana : ComponentBase, IDisposable
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [Parameter] public int? PersoanaId { get; set; }

    // Form model
    private CreatePersoanaRequest _model = new() { EsteActiva = true };
    
    // Form state
    private bool _isProcessing = false;
    private bool _isLoading = false;
    private bool _isEditMode => PersoanaId.HasValue && PersoanaId.Value > 0;
    
    // Dropdown options
    private readonly string[] _genOptions = { "Masculin", "Feminin", "Neprecizat" };
    private readonly string[] _judeteOptions = {
        "Alba", "Arad", "Arges", "Bacau", "Bihor", "Bistrita-Nasaud", "Botosani", "Brasov",
        "Braila", "Buzau", "Caras-Severin", "Calarasi", "Cluj", "Constanta", "Covasna", "Dambovita",
        "Dolj", "Galati", "Giurgiu", "Gorj", "Harghita", "Hunedoara", "Ialomita", "Iasi",
        "Ilfov", "Maramures", "Mehedinti", "Mures", "Neamt", "Olt", "Prahova", "Salaj",
        "Satu Mare", "Sibiu", "Suceava", "Teleorman", "Timis", "Tulcea", "Vaslui", "Valcea", 
        "Vrancea", "Bucuresti"
    };

    private readonly string[] _localitatiOptions = {
        "Bucuresti", "Cluj-Napoca", "Timisoara", "Iasi", "Constanta", "Craiova", "Brasov", 
        "Galati", "Ploiesti", "Oradea", "Braila", "Arad", "Pitesti", "Sibiu", "Bacau",
        "Targu Mures", "Baia Mare", "Buzau", "Botosani", "Satu Mare", "Ramnicu Valcea"
    };
    
    // Preview data for grid
    private List<PersoanaPreview> _previewData = new();

    // Preview class for grid display  
    private class PersoanaPreview
    {
        public string Nume { get; set; } = string.Empty;
        public string Prenume { get; set; } = string.Empty;
        public string Gen { get; set; } = string.Empty;
        public string Judet { get; set; } = string.Empty;
        public string Localitate { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string CNP { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public int Varsta { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        UpdatePreviewData();
        
        if (_isEditMode)
        {
            await LoadPersoanaDataAsync();
        }
        else
        {
            _model.EsteActiva = true;
        }
        
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_isEditMode && PersoanaId.HasValue)
        {
            await LoadPersoanaDataAsync();
        }
    }

    private async Task LoadPersoanaDataAsync()
    {
        if (!PersoanaId.HasValue) return;
        
        _isLoading = true;
        StateHasChanged();
        
        try
        {
            var response = await Http.GetAsync($"api/Persoane/{PersoanaId.Value}");
            
            if (response.IsSuccessStatusCode)
            {
                var persoana = await response.Content.ReadFromJsonAsync<PersoanaListDto>();
                
                if (persoana != null)
                {
                    _model = new CreatePersoanaRequest
                    {
                        Nume = persoana.Nume,
                        Prenume = persoana.Prenume,
                        Judet = persoana.Judet,
                        Localitate = persoana.Localitate,
                        Adresa = persoana.Adresa,
                        Telefon = persoana.Telefon,
                        Email = persoana.Email,
                        DataNasterii = persoana.DataNasterii,
                        CNP = persoana.CNP,
                        Gen = ParseGen(persoana.Gen),
                        EsteActiva = persoana.EsteActiva
                    };
                    
                    UpdatePreviewData();
                    
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Info,
                        Summary = "Date incarcate",
                        Detail = $"Datele pentru '{persoana.Nume} {persoana.Prenume}' au fost incarcate pentru editare.",
                        Duration = 3000
                    });
                }
                else
                {
                    await ShowNotFoundError();
                }
            }
            else
            {
                await ShowNotFoundError();
            }
        }
        catch (HttpRequestException)
        {
            ShowErrorNotification("Nu se poate conecta la server");
        }
        catch (Exception ex)
        {
            ShowErrorNotification($"Eroare la incarcarea datelor: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private Gen? ParseGen(string? genString)
    {
        if (string.IsNullOrEmpty(genString)) return null;
        
        return genString.ToLower() switch
        {
            "masculin" => Gen.Masculin,
            "feminin" => Gen.Feminin,
            "neprecizat" => Gen.Neprecizat,
            _ => null
        };
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    private void OnFieldChanged(string fieldName)
    {
        UpdatePreviewData();
        StateHasChanged();
    }

    private void UpdatePreviewData()
    {
        if (!string.IsNullOrEmpty(_model.Nume) || !string.IsNullOrEmpty(_model.Prenume) || _isEditMode)
        {
            var varsta = _model.DataNasterii.HasValue ? 
                DateTime.Now.Year - _model.DataNasterii.Value.Year : 0;
            
            _previewData = new List<PersoanaPreview>
            {
                new PersoanaPreview
                {
                    Nume = _model.Nume ?? "...",
                    Prenume = _model.Prenume ?? "...",
                    CNP = _model.CNP ?? "Nu este specificat",
                    Varsta = varsta,
                    Gen = _model.Gen?.ToString() ?? "Nu este specificat",
                    Telefon = _model.Telefon ?? "Nu este specificat",
                    Email = _model.Email ?? "Nu este specificat",
                    Judet = _model.Judet ?? "Nu este specificat",
                    Localitate = _model.Localitate ?? "Nu este specificat",
                    StatusText = _model.EsteActiva ? "Activ" : "Inactiv"
                }
            };
        }
        else
        {
            _previewData = new List<PersoanaPreview>();
        }
        
        StateHasChanged(); // For?ez re-render pentru preview
    }

    private async Task OnSubmitAsync(CreatePersoanaRequest model)
    {
        if (_isEditMode)
        {
            await UpdatePersoana();
        }
        else
        {
            await SavePersoana();
        }
    }

    private async Task SavePersoana()
    {
        if (_isProcessing) return;

        _isProcessing = true;
        
        try
        {
            var response = await Http.PostAsJsonAsync("api/Persoane", _model);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<int>>();
                
                if (result?.IsSuccess == true)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Datele au fost salvate",
                        Duration = 3000
                    });
                    
                    await Task.Delay(500);
                    Navigation.NavigateTo($"/administrare/persoane-view/{result.Value}");
                }
                else
                {
                    ShowErrorNotification("Eroare la salvarea persoanei");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ShowErrorNotification($"Eroare la salvare: {errorContent}");
            }
        }
        catch (HttpRequestException)
        {
            ShowErrorNotification("Nu se poate conecta la server");
        }
        catch (Exception ex)
        {
            ShowErrorNotification($"Eroare neasteptata: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task UpdatePersoana()
    {
        if (_isProcessing || !PersoanaId.HasValue) return;

        _isProcessing = true;
        
        try
        {
            var updateRequest = new UpdatePersoanaRequest
            {
                Id = PersoanaId.Value,
                Nume = _model.Nume,
                Prenume = _model.Prenume,
                Judet = _model.Judet,
                Localitate = _model.Localitate,
                Adresa = _model.Adresa,
                Telefon = _model.Telefon,
                Email = _model.Email,
                DataNasterii = _model.DataNasterii,
                CNP = _model.CNP,
                Gen = _model.Gen,
                EsteActiva = _model.EsteActiva
            };

            var response = await Http.PutAsJsonAsync($"api/Persoane/{PersoanaId.Value}", updateRequest);
            
            if (response.IsSuccessStatusCode)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Datele au fost salvate",
                    Duration = 3000
                });
                
                await Task.Delay(500);
                Navigation.NavigateTo($"/administrare/persoane-view/{PersoanaId.Value}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ShowErrorNotification($"Eroare la actualizare: {errorContent}");
            }
        }
        catch (HttpRequestException)
        {
            ShowErrorNotification("Nu se poate conecta la server");
        }
        catch (Exception ex)
        {
            ShowErrorNotification($"Eroare neasteptata: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task ShowNotFoundError()
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Warning,
            Summary = "Nu s-a gasit",
            Detail = "Persoana cu ID-ul specificat nu a fost gasita in baza de date sau a fost stearsa intre timp.",
            Duration = 5000
        });
        
        await Task.Delay(2000);
        Navigation.NavigateTo("/administrare/gestionare-persoane");
    }

    private void BackToList()
    {
        Navigation.NavigateTo("/administrare/gestionare-persoane");
    }

    private string GetSaveButtonText()
    {
        if (_isProcessing)
        {
            return _isEditMode ? "Se actualizeaza..." : "Se salveaza...";
        }
        return _isEditMode ? "Actualizeaza Persoana" : "Salveaza Persoana";
    }

    private string GetSaveButtonIcon()
    {
        if (_isProcessing)
        {
            return "hourglass_empty";
        }
        return _isEditMode ? "update" : "save";
    }

    private string GetSaveButtonStyle()
    {
        return "font-weight: bold;";
    }

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Eroare",
            Detail = message,
            Duration = 4000
        });
    }
}