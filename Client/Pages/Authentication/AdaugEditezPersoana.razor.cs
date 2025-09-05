using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Authentication;
using global::Shared.Common;
using global::Shared.Enums;
using global::Shared.DTOs.Common;
using Radzen;
using System.Net.Http.Json;
using Client.Services.Common;

namespace Client.Pages.Authentication;

public partial class AdaugEditezPersoana : ComponentBase, IDisposable
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILocationApiService LocationService { get; set; } = null!;

    [Parameter] public int? PersoanaId { get; set; }

    // Form model
    private CreatePersoanaRequest _model = new() { EsteActiva = true };
    
    // Form state
    private bool _isProcessing = false;
    private bool _isLoading = false;
    private bool _isEditMode => PersoanaId.HasValue && PersoanaId.Value > 0;
    private bool _isDisposed = false;
    
    // Dropdown options
    // Use enum values directly to match the model type (Gen?)
    private readonly Gen[] _genOptions = System.Enum.GetValues<Gen>();
    private List<JudetDto> _judeteOptions = new();
    private List<LocalitateDto> _localitatiOptions = new();
    
    // Preview data for grid
    private List<PersoanaPreview> _previewData = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadJudeteAsync();
        
        if (_isEditMode)
        {
            await LoadPersoanaDataAsync();
        }
        else
        {
            _model.EsteActiva = true;
            UpdatePreviewData();
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

    private async Task LoadJudeteAsync()
    {
        try
        {
            _judeteOptions = await LocationService.GetJudeteAsync();
        }
        catch (Exception ex)
        {
            ShowErrorNotification($"Eroare la incarcarea judetelor: {ex.Message}");
            _judeteOptions = new List<JudetDto>();
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
                    
                    // Load localities based on loaded county
                    if (!string.IsNullOrEmpty(_model.Judet))
                    {
                        await OnJudetChangedAsync(_model.Judet);
                    }
                    
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
        if (_isDisposed) return;
        
        _isDisposed = true;
        
        try
        {
            // Clear preview data collection
            _previewData?.Clear();
            _judeteOptions?.Clear();
            _localitatiOptions?.Clear();
        }
        catch (Exception)
        {
            // Ignore errors during disposal
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

    private void OnFieldChanged(string fieldName)
    {
        // Auto-complete from CNP when the CNP field changes (on input or change)
        if (fieldName == nameof(_model.CNP))
        {
            AutoFillFromCnp(_model.CNP);
        }
        
        UpdatePreviewData();
        InvokeAsync(StateHasChanged);
    }

    private async Task OnJudetChangedAsync(string? selectedJudet)
    {
        // Clear locality when county changes
        if (_model.Judet != selectedJudet)
        {
            _model.Localitate = null;
        }
        
        _model.Judet = selectedJudet;
        
        // Load localities based on selected county
        if (!string.IsNullOrEmpty(selectedJudet))
        {
            try
            {
                _localitatiOptions = await LocationService.GetLocalitatiByJudetAsync(selectedJudet);
            }
            catch (Exception ex)
            {
                ShowErrorNotification($"Eroare la incarcarea localitatilor: {ex.Message}");
                _localitatiOptions = new List<LocalitateDto>();
            }
        }
        else
        {
            _localitatiOptions = new List<LocalitateDto>();
        }
        
        UpdatePreviewData();
        await InvokeAsync(StateHasChanged);
    }

    private void OnJudetChanged(object? selectedJudet)
    {
        var judet = selectedJudet?.ToString();
        _ = OnJudetChangedAsync(judet);
    }

    private void OnStatusChanged(bool value)
    {
        _model.EsteActiva = value;
        OnFieldChanged(nameof(_model.EsteActiva));
    }

    private void UpdatePreviewData()
    {
        // Rebuild preview list from current model
        _previewData.Clear();

        var preview = new PersoanaPreview
        {
            Nume = string.IsNullOrWhiteSpace(_model.Nume) ? "Nu este specificat" : _model.Nume,
            Prenume = string.IsNullOrWhiteSpace(_model.Prenume) ? "Nu este specificat" : _model.Prenume,
            CNP = string.IsNullOrWhiteSpace(_model.CNP) ? "Nu este specificat" : _model.CNP,
            Gen = _model.Gen?.ToString() ?? "Nu este specificat",
            StatusText = _model.EsteActiva ? "Activ" : "Inactiv",
            Email = string.IsNullOrWhiteSpace(_model.Email) ? "Nu este specificat" : _model.Email,
            Telefon = string.IsNullOrWhiteSpace(_model.Telefon) ? "Nu este specificat" : _model.Telefon,
            Judet = string.IsNullOrWhiteSpace(_model.Judet) ? "Nu este specificat" : _model.Judet!,
            Localitate = string.IsNullOrWhiteSpace(_model.Localitate) ? "Nu este specificat" : _model.Localitate!,
            Varsta = CalculateAge(_model.DataNasterii)
        };

        _previewData.Add(preview);
        // force re-render
        InvokeAsync(StateHasChanged);
    }

    private static int CalculateAge(DateTime? dataNasterii)
    {
        if (!dataNasterii.HasValue) return 0;
        var today = DateTime.Today;
        var age = today.Year - dataNasterii.Value.Year;
        if (dataNasterii.Value.Date > today.AddYears(-age)) age--;
        return Math.Max(age, 0);
    }

    private async Task SubmitForm()
    {
        // Basic validation before submit
        if (string.IsNullOrEmpty(_model.Nume))
        {
            ShowErrorNotification("Numele este obligatoriu");
            return;
        }
        
        if (string.IsNullOrEmpty(_model.Prenume))
        {
            ShowErrorNotification("Prenumele este obligatoriu");
            return;
        }
        
        if (_model.DataNasterii == null)
        {
            ShowErrorNotification("Data nasterii este obligatorie");
            return;
        }
        
        if (_model.Gen == null)
        {
            ShowErrorNotification("Genul este obligatoriu");
            return;
        }

        if (_isEditMode)
        {
            await UpdatePersoana();
        }
        else
        {
            await SavePersoana();
        }
    }

    private async Task OnSubmitAsync(CreatePersoanaRequest model)
    {
        await SubmitForm();
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

    private void CancelProcessing()
    {
        _isProcessing = false;
        StateHasChanged();
        ShowErrorNotification("Operatiunea a fost anulata de utilizator");
    }

    // ==========================
    // CNP -> Gen + Data Nasterii
    // ==========================
    private void AutoFillFromCnp(string? cnp)
    {
        if (string.IsNullOrWhiteSpace(cnp)) return;

        // keep only digits
        var clean = new string(cnp.Where(char.IsDigit).ToArray());
        if (!string.Equals(clean, _model.CNP))
        {
            _model.CNP = clean;
        }

        if (clean.Length != 13 || !IsValidCnp(clean))
        {
            return; // don't modify when invalid/incomplete
        }

        var firstDigit = int.Parse(clean[0].ToString());
        // gender
        if (firstDigit is 1 or 3 or 5 or 7)
            _model.Gen = Gen.Masculin;
        else if (firstDigit is 2 or 4 or 6 or 8)
            _model.Gen = Gen.Feminin;
        else
            _model.Gen = Gen.Neprecizat;

        // date
        try
        {
            var year = int.Parse(clean.Substring(1, 2));
            var month = int.Parse(clean.Substring(3, 2));
            var day = int.Parse(clean.Substring(5, 2));

            if (firstDigit is 1 or 2) year += 1900;
            else if (firstDigit is 3 or 4) year += 1800;
            else if (firstDigit is 5 or 6 or 7 or 8) year += 2000;

            var birth = new DateTime(year, month, day);
            if (birth <= DateTime.Today)
            {
                _model.DataNasterii = birth;
            }
        }
        catch
        {
            // ignore invalid date
        }

        // Immediately refresh preview after auto-fill
        UpdatePreviewData();
        InvokeAsync(StateHasChanged);
    }

    private static bool IsValidCnp(string cnp)
    {
        if (cnp.Length != 13 || !cnp.All(char.IsDigit)) return false;
        var weights = new[] { 2,7,9,1,4,6,3,5,8,2,7,9 };
        var sum = 0;
        for (int i = 0; i < 12; i++)
        {
            sum += (cnp[i] - '0') * weights[i];
        }
        var check = sum % 11;
        if (check == 10) check = 1;
        return check == (cnp[12] - '0');
    }
}

// Preview class for grid display (moved outside the component to avoid generic binding issues)
public class PersoanaPreview
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