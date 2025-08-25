using Microsoft.AspNetCore.Components;
using MudBlazor;
using global::Shared.DTOs.Medical;
using global::Shared.Models.Medical;
using global::Shared.Validators.Medical;
using global::FluentValidation;
using System.Net.Http.Json;

namespace Client.Pages.Medical;

public partial class VitalSigns
{
    private MudForm _form = null!;
    private bool _isProcessing = false;
    private bool _showVitalSignsForm = false;
    
    private PacientListDto? _selectedPatient = null;
    private List<SemneVitaleDto> _recentVitalSigns = new();
    
    private CreateSemneVitaleRequest _vitalSignsRequest = new();

    private readonly CreateSemneVitaleValidator _validator = new();

    private async Task<IEnumerable<PacientListDto>> SearchPatients(string value, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
            return new List<PacientListDto>();

        try
        {
            var encodedValue = Uri.EscapeDataString(value);
            var response = await Http.GetFromJsonAsync<List<PacientListDto>>($"api/medical/pacienti/search?query={encodedValue}", token);
            
            if (response != null)
            {
                return response;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching patients: {ex.Message}");
        }

        // Mock data for development
        return new List<PacientListDto>
        {
            new()
            {
                PacientID = Guid.NewGuid(),
                Nume = "Popescu",
                Prenume = "Ion",
                CNP = "1800101123456",
                DataNasterii = new DateTime(1980, 1, 1),
                Gen = "Masculin"
            },
            new()
            {
                PacientID = Guid.NewGuid(),
                Nume = "Ionescu",
                Prenume = "Maria",
                CNP = "2851202234567",
                DataNasterii = new DateTime(1985, 12, 2),
                Gen = "Feminin"
            }
        }.Where(p => p.NumeComplet.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                    (p.CNP?.Contains(value) == true));
    }

    private void StartNewVitalSigns()
    {
        if (_selectedPatient == null) return;

        _vitalSignsRequest.PacientID = _selectedPatient.PacientID;
        _showVitalSignsForm = true;
        LoadRecentVitalSigns();
    }

    private async void LoadRecentVitalSigns()
    {
        if (_selectedPatient == null) return;

        try
        {
            var response = await Http.GetFromJsonAsync<List<SemneVitaleDto>>($"api/medical/pacienti/{_selectedPatient.PacientID}/semne-vitale?limit=5");
            
            if (response != null)
            {
                _recentVitalSigns = response;
            }
            else
            {
                // Mock recent vital signs
                _recentVitalSigns = new List<SemneVitaleDto>
                {
                    new()
                    {
                        SemneVitaleID = Guid.NewGuid(),
                        PacientID = _selectedPatient.PacientID,
                        DataMasurare = DateTime.Today.AddDays(-1),
                        TensiuneArterialaMax = 130,
                        TensiuneArterialaMin = 85,
                        FrecariaCardiaca = 75,
                        Temperatura = 36.8m,
                        Greutate = 75.5m,
                        Inaltime = 175,
                        SaturatieOxigen = 98.5m
                    },
                    new()
                    {
                        SemneVitaleID = Guid.NewGuid(),
                        PacientID = _selectedPatient.PacientID,
                        DataMasurare = DateTime.Today.AddDays(-7),
                        TensiuneArterialaMax = 125,
                        TensiuneArterialaMin = 80,
                        FrecariaCardiaca = 72,
                        Temperatura = 36.6m,
                        Greutate = 76.0m,
                        Inaltime = 175,
                        SaturatieOxigen = 99.0m
                    }
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading recent vital signs: {ex.Message}");
            _recentVitalSigns = new List<SemneVitaleDto>();
        }
        
        StateHasChanged();
    }

    private async Task SaveVitalSigns()
    {
        if (_isProcessing) return;

        await _form.Validate();
        if (!_form.IsValid)
        {
            Snackbar.Add("V? rug?m s? corecta?i erorile din formular.", MudBlazor.Severity.Error);
            return;
        }

        _isProcessing = true;
        StateHasChanged();

        try
        {
            var response = await Http.PostAsJsonAsync("api/medical/semne-vitale", _vitalSignsRequest);
            
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Semnele vitale au fost înregistrate cu succes!", MudBlazor.Severity.Success);
                _showVitalSignsForm = false;
                LoadRecentVitalSigns();
                ResetForm();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error saving vital signs: {response.StatusCode} - {errorContent}");
                Snackbar.Add("A ap?rut o eroare la salvarea semnelor vitale.", MudBlazor.Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception saving vital signs: {ex.Message}");
            
            // For development - simulate success
            Snackbar.Add("Demo: Semnele vitale au fost salvate local (API indisponibil).", MudBlazor.Severity.Info);
            _showVitalSignsForm = false;
            
            // Add to mock history
            var newVitalSigns = new SemneVitaleDto
            {
                SemneVitaleID = Guid.NewGuid(),
                PacientID = _selectedPatient!.PacientID,
                DataMasurare = DateTime.Now,
                TensiuneArterialaMax = _vitalSignsRequest.TensiuneArterialaMax,
                TensiuneArterialaMin = _vitalSignsRequest.TensiuneArterialaMin,
                FrecariaCardiaca = _vitalSignsRequest.FrecariaCardiaca,
                Temperatura = _vitalSignsRequest.Temperatura,
                Greutate = _vitalSignsRequest.Greutate,
                Inaltime = _vitalSignsRequest.Inaltime,
                FrecariaRespiratorie = _vitalSignsRequest.FrecariaRespiratorie,
                SaturatieOxigen = _vitalSignsRequest.SaturatieOxigen
            };
            
            _recentVitalSigns.Insert(0, newVitalSigns);
            ResetForm();
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private void CancelVitalSigns()
    {
        _showVitalSignsForm = false;
        ResetForm();
    }

    private void ResetForm()
    {
        _vitalSignsRequest = new CreateSemneVitaleRequest
        {
            PacientID = _selectedPatient?.PacientID ?? Guid.Empty
        };
    }

    private void ViewVitalSignsHistory()
    {
        if (_selectedPatient != null)
        {
            Navigation.NavigateTo($"/medical/pacienti/{_selectedPatient.PacientID}/semne-vitale");
        }
    }

    // Helper methods for interpretations
    private double CalculateBMI(decimal weight, int height)
    {
        var heightInMeters = height / 100.0;
        return (double)(weight / (decimal)(heightInMeters * heightInMeters));
    }

    private MudBlazor.Severity GetBloodPressureSeverity(int systolic, int diastolic)
    {
        if (systolic >= 180 || diastolic >= 120) return MudBlazor.Severity.Error;
        if (systolic >= 140 || diastolic >= 90) return MudBlazor.Severity.Warning;
        if (systolic >= 130 || diastolic >= 80) return MudBlazor.Severity.Info;
        return MudBlazor.Severity.Success;
    }

    private string GetBloodPressureInterpretation(int systolic, int diastolic)
    {
        if (systolic >= 180 || diastolic >= 120) return "Criz? hipertensiv? - aten?ie medical? imediat?!";
        if (systolic >= 160 || diastolic >= 100) return "Hipertensiune arterial? grad 2";
        if (systolic >= 140 || diastolic >= 90) return "Hipertensiune arterial? grad 1";
        if (systolic >= 130 || diastolic >= 80) return "Tensiune arterial? crescut?";
        if (systolic < 90 || diastolic < 60) return "Hipotensiune arterial?";
        return "Tensiune arterial? normal?";
    }

    private MudBlazor.Severity GetTemperatureSeverity(decimal temperature)
    {
        if (temperature >= 39.0m) return MudBlazor.Severity.Error;
        if (temperature >= 37.8m) return MudBlazor.Severity.Warning;
        if (temperature >= 37.2m) return MudBlazor.Severity.Info;
        if (temperature < 35.0m) return MudBlazor.Severity.Warning;
        return MudBlazor.Severity.Success;
    }

    private string GetTemperatureInterpretation(decimal temperature)
    {
        if (temperature >= 39.0m) return "Febr? înalt?";
        if (temperature >= 37.8m) return "Febr? moderat?";
        if (temperature >= 37.2m) return "Subfebril?";
        if (temperature < 35.0m) return "Hipotermie";
        return "Temperatur? normal?";
    }

    private MudBlazor.Severity GetBMISeverity(double bmi)
    {
        if (bmi >= 30.0) return MudBlazor.Severity.Error;
        if (bmi >= 25.0) return MudBlazor.Severity.Warning;
        if (bmi < 18.5) return MudBlazor.Severity.Info;
        return MudBlazor.Severity.Success;
    }

    private string GetBMIInterpretation(double bmi)
    {
        if (bmi >= 35.0) return "Obezitate sever?";
        if (bmi >= 30.0) return "Obezitate";
        if (bmi >= 25.0) return "Supraponderal";
        if (bmi < 18.5) return "Subponderal";
        return "Greutate normal?";
    }

    private MudBlazor.Severity GetOxygenSaturationSeverity(decimal saturation)
    {
        if (saturation < 90.0m) return MudBlazor.Severity.Error;
        if (saturation < 95.0m) return MudBlazor.Severity.Warning;
        return MudBlazor.Severity.Success;
    }

    private string GetOxygenSaturationInterpretation(decimal saturation)
    {
        if (saturation < 90.0m) return "Hipoxie sever? - aten?ie medical? urgent?!";
        if (saturation < 95.0m) return "Hipoxie u?oar?";
        return "Satura?ie normal?";
    }
}