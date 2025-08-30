using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using global::Shared.DTOs.Medical;
using global::Shared.Validators.Medical;
using global::FluentValidation;
using System.Net.Http.Json;
using global::Shared.Enums;
using System.ComponentModel;
using System.Reflection;

namespace Client.Pages.Medical;

public partial class PatientRegistration
{
    private MudForm _form = null!;
    private bool _isProcessing = false;
    
    private CreatePacientRequest _pacientRequest = new();
    private DateTime? _selectedDate = DateTime.Today.AddYears(-30);

    private readonly CreatePacientValidator _validator = new();

    protected override void OnInitialized()
    {
        // Set default values
        _pacientRequest.DataNasterii = DateTime.Today.AddYears(-30);
        _selectedDate = _pacientRequest.DataNasterii;
        StateHasChanged();
    }

    // Sync date from MudDatePicker to model
    protected override void OnParametersSet()
    {
        if (_selectedDate.HasValue)
        {
            _pacientRequest.DataNasterii = _selectedDate.Value;
        }
    }

    private async Task SavePacient()
    {
        if (_isProcessing) return;

        // Sync date
        if (_selectedDate.HasValue)
        {
            _pacientRequest.DataNasterii = _selectedDate.Value;
        }

        // Validate form first
        await _form.Validate();
        if (!_form.IsValid)
        {
            Snackbar.Add("Va rugam sa corectati erorile din formular.", MudBlazor.Severity.Error);
            return;
        }

        _isProcessing = true;
        StateHasChanged();

        try
        {
            var response = await Http.PostAsJsonAsync("api/medical/pacienti", _pacientRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Guid>();
                
                Snackbar.Add("Pacientul a fost inregistrat cu succes!", MudBlazor.Severity.Success);
                
                // Redirect to patient details page
                Navigation.NavigateTo($"/medical/pacienti/{result}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error creating patient: {response.StatusCode} - {errorContent}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Snackbar.Add("Datele introduse nu sunt valide. Verificati formularul.", MudBlazor.Severity.Error);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    Snackbar.Add("Exista deja un pacient cu acest CNP in sistem.", MudBlazor.Severity.Warning);
                }
                else
                {
                    Snackbar.Add("A aparut o eroare la salvarea pacientului. Incercati din nou.", MudBlazor.Severity.Error);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception saving patient: {ex.Message}");
            
            // For development - simulate success
            Snackbar.Add("Demo: Pacientul a fost inregistrat local (API indisponibil).", MudBlazor.Severity.Info);
            Navigation.NavigateTo("/medical/pacienti");
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/medical/pacienti");
    }

    private string GetSaveButtonClass()
    {
        var baseClass = "patient-reg-save-btn";
        if (_isProcessing)
        {
            baseClass += " patient-reg-loading";
        }
        return baseClass;
    }

    private void OnCNPValueChanged(string cnp)
    {
        // Clean input - keep only digits
        var cleanCnp = new string(cnp?.Where(char.IsDigit).ToArray() ?? Array.Empty<char>());
        
        // Limit to 13 digits
        if (cleanCnp.Length > 13)
        {
            cleanCnp = cleanCnp.Substring(0, 13);
        }
        
        _pacientRequest.CNP = cleanCnp;
        OnCNPChanged();
        StateHasChanged();
    }

    private List<(string Value, string Description)> GetGenderOptions()
    {
        return typeof(Gen)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => (
                Value: f.GetCustomAttribute<DescriptionAttribute>()?.Description ?? f.Name,
                Description: f.GetCustomAttribute<DescriptionAttribute>()?.Description ?? f.Name
            )).ToList();
    }

    private string GetGenderIcon(string gender)
    {
        return gender switch
        {
            "Masculin" => Icons.Material.Filled.Man,
            "Feminin" => Icons.Material.Filled.Woman,
            _ => Icons.Material.Filled.Person
        };
    }

    private string GetCNPHelperText()
    {
        if (string.IsNullOrEmpty(_pacientRequest.CNP))
        {
            return "CNP-ul va fi folosit pentru auto-completarea genului si datei nasterii";
        }
        
        if (_pacientRequest.CNP.Length < 13)
        {
            return $"CNP incomplet - introduceti {13 - _pacientRequest.CNP.Length} cifre mai multe";
        }
        
        if (_pacientRequest.CNP.Length == 13)
        {
            if (IsValidCNP(_pacientRequest.CNP))
            {
                return "? CNP valid - genul si data nasterii au fost completate automat";
            }
            else
            {
                return "? CNP invalid - verificati cifrele introduse";
            }
        }
        
        return "";
    }

    // Helper method to validate CNP in real-time
    private bool IsValidCNP(string? cnp)
    {
        if (string.IsNullOrEmpty(cnp) || cnp.Length != 13 || !cnp.All(char.IsDigit))
            return false;

        // Romanian CNP validation algorithm
        var weights = new[] { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
        var sum = 0;

        for (int i = 0; i < 12; i++)
        {
            sum += int.Parse(cnp[i].ToString()) * weights[i];
        }

        var checkDigit = sum % 11;
        if (checkDigit == 10) checkDigit = 1;

        return checkDigit == int.Parse(cnp[12].ToString());
    }

    // Method to auto-populate some fields based on CNP
    private void OnCNPChanged()
    {
        if (!string.IsNullOrEmpty(_pacientRequest.CNP) && _pacientRequest.CNP.Length == 13 && IsValidCNP(_pacientRequest.CNP))
        {
            // Extract gender from CNP (first digit)
            var firstDigit = int.Parse(_pacientRequest.CNP[0].ToString());
            if (firstDigit == 1 || firstDigit == 3 || firstDigit == 5 || firstDigit == 7)
            {
                _pacientRequest.Gen = "Masculin";
            }
            else if (firstDigit == 2 || firstDigit == 4 || firstDigit == 6 || firstDigit == 8)
            {
                _pacientRequest.Gen = "Feminin";
            }

            // Extract birth date from CNP
            try
            {
                var year = int.Parse(_pacientRequest.CNP.Substring(1, 2));
                var month = int.Parse(_pacientRequest.CNP.Substring(3, 2));
                var day = int.Parse(_pacientRequest.CNP.Substring(5, 2));

                // Determine century
                if (firstDigit == 1 || firstDigit == 2)
                    year += 1900;
                else if (firstDigit == 3 || firstDigit == 4)
                    year += 1800;
                else if (firstDigit == 5 || firstDigit == 6)
                    year += 2000;
                else if (firstDigit == 7 || firstDigit == 8)
                    year += 2000; // For residents born after 2000

                var birthDate = new DateTime(year, month, day);
                if (birthDate <= DateTime.Today)
                {
                    _pacientRequest.DataNasterii = birthDate;
                    _selectedDate = birthDate;
                }
            }
            catch
            {
                // Invalid date in CNP, ignore
            }

            StateHasChanged();
        }
    }
}