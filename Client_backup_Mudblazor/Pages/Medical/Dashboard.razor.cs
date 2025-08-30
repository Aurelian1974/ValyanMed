using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.DTOs.Medical;
using System.Net.Http.Json;

namespace Client.Pages.Medical;

public partial class Dashboard
{
    private bool _isLoading = true;
    private DashboardStatisticiDto _statistici = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadDashboardData();
    }

    private async Task LoadDashboardData()
    {
        _isLoading = true;
        StateHasChanged();

        try
        {
            var response = await Http.GetFromJsonAsync<DashboardStatisticiDto>("api/medical/dashboard/statistici");
            if (response != null)
            {
                _statistici = response;
            }
            else
            {
                // Fallback data if API is not available
                _statistici = new DashboardStatisticiDto
                {
                    PacientiTotal = 0,
                    ProgramariAzi = 0,
                    ConsultatiiLunaAceasta = 0,
                    PersonalActiv = 0,
                    ProgramariAstazi = new List<ProgramareListDto>()
                };
                
                Snackbar.Add("Nu s-au putut înc?rca datele. API-ul nu este disponibil.", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            
            // Mock data for development
            _statistici = new DashboardStatisticiDto
            {
                PacientiTotal = 125,
                ProgramariAzi = 8,
                ConsultatiiLunaAceasta = 45,
                PersonalActiv = 12,
                ProgramariAstazi = new List<ProgramareListDto>
                {
                    new()
                    {
                        ProgramareID = Guid.NewGuid(),
                        DataProgramare = DateTime.Today.AddHours(9),
                        NumePacient = "Popescu Ion",
                        CNPPacient = "1234567890123",
                        TelefonPacient = "0721234567",
                        NumeDoctor = "Dr. Marinescu Ana",
                        SpecializareDoctor = "Cardiologie",
                        TipProgramare = "Consulta?ie",
                        Status = "Programata"
                    },
                    new()
                    {
                        ProgramareID = Guid.NewGuid(),
                        DataProgramare = DateTime.Today.AddHours(10).AddMinutes(30),
                        NumePacient = "Ionescu Maria",
                        CNPPacient = "2345678901234",
                        TelefonPacient = "0721345678",
                        NumeDoctor = "Dr. Vasilescu Mihai",
                        SpecializareDoctor = "Neurologie",
                        TipProgramare = "Control",
                        Status = "Confirmata"
                    },
                    new()
                    {
                        ProgramareID = Guid.NewGuid(),
                        DataProgramare = DateTime.Today.AddHours(14),
                        NumePacient = "Georgescu Ana",
                        CNPPacient = "3456789012345",
                        TelefonPacient = "0721456789",
                        NumeDoctor = "Dr. Radu Constantin",
                        SpecializareDoctor = "Ortopedie",
                        TipProgramare = "Investiga?ie",
                        Status = "In Asteptare"
                    }
                }
            };
            
            Snackbar.Add("Se afi?eaz? date demo. Verifica?i conexiunea la API.", Severity.Info);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshData()
    {
        await LoadDashboardData();
        Snackbar.Add("Datele au fost actualizate cu succes!", Severity.Success);
    }

    private Color GetStatusColor(string status)
    {
        return status?.ToLower() switch
        {
            "programata" => Color.Primary,
            "confirmata" => Color.Success,
            "in asteptare" => Color.Warning,
            "in consultatie" => Color.Info,
            "finalizata" => Color.Success,
            "anulata" => Color.Error,
            "nu s-a prezentat" => Color.Error,
            "amanata" => Color.Warning,
            _ => Color.Default
        };
    }
}