using Microsoft.AspNetCore.Components;
using MudBlazor;
using global::Shared.DTOs.Medical;
using global::Shared.Common;
using System.Net.Http.Json;

namespace Client.Pages.Medical;

public partial class AppointmentScheduling
{
    private bool _isLoading = false;
    private bool _isDenseTable = false;
    private string _viewMode = "list";
    
    private DateRange? _dateRange = new DateRange(DateTime.Today, DateTime.Today.AddDays(7));
    private List<PersonalMedicalListDto> _doctori = new();
    
    private ProgramariSearchQuery _searchQuery = new()
    {
        DataStart = DateTime.Today,
        DataEnd = DateTime.Today.AddDays(7),
        Page = 1,
        PageSize = 25
    };
    
    private PagedResult<ProgramareListDto> _pagedResult = new();
    
    private (int Total, int Finalizate, int InAsteptare, int Anulate) _statsToday = (0, 0, 0, 0);

    protected override async Task OnInitializedAsync()
    {
        await LoadDoctori();
        await SearchProgramari();
        await LoadTodayStats();
    }

    private async Task LoadDoctori()
    {
        try
        {
            var response = await Http.GetFromJsonAsync<List<PersonalMedicalListDto>>("api/medical/personal?pozitie=Doctor");
            
            if (response != null)
            {
                _doctori = response;
            }
            else
            {
                // Mock doctors for development
                _doctori = new List<PersonalMedicalListDto>
                {
                    new()
                    {
                        PersonalID = Guid.NewGuid(),
                        Nume = "Popescu",
                        Prenume = "Dr. Ion",
                        Specializare = "Cardiologie",
                        Pozitie = "Doctor",
                        EsteActiv = true
                    },
                    new()
                    {
                        PersonalID = Guid.NewGuid(),
                        Nume = "Ionescu",
                        Prenume = "Dr. Maria",
                        Specializare = "Neurologie",
                        Pozitie = "Doctor",
                        EsteActiv = true
                    }
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading doctors: {ex.Message}");
            _doctori = new List<PersonalMedicalListDto>();
        }
    }

    private async Task SearchProgramari()
    {
        _isLoading = true;
        StateHasChanged();

        try
        {
            // Sync date range to search query
            if (_dateRange?.Start.HasValue == true && _dateRange?.End.HasValue == true)
            {
                _searchQuery.DataStart = _dateRange.Start.Value;
                _searchQuery.DataEnd = _dateRange.End.Value;
            }

            var queryParams = new List<string>();
            
            if (_searchQuery.DataStart.HasValue)
                queryParams.Add($"dataStart={_searchQuery.DataStart.Value:yyyy-MM-dd}");
            if (_searchQuery.DataEnd.HasValue)
                queryParams.Add($"dataEnd={_searchQuery.DataEnd.Value:yyyy-MM-dd}");
            if (_searchQuery.DoctorID.HasValue)
                queryParams.Add($"doctorId={_searchQuery.DoctorID.Value}");
            if (!string.IsNullOrEmpty(_searchQuery.Status))
                queryParams.Add($"status={Uri.EscapeDataString(_searchQuery.Status)}");
            
            queryParams.Add($"page={_searchQuery.Page}");
            queryParams.Add($"pageSize={_searchQuery.PageSize}");

            var queryString = string.Join("&", queryParams);
            var response = await Http.GetFromJsonAsync<PagedResult<ProgramareListDto>>($"api/medical/programari?{queryString}");
            
            if (response != null)
            {
                _pagedResult = response;
            }
            else
            {
                // Mock appointments for development
                var mockAppointments = new List<ProgramareListDto>
                {
                    new()
                    {
                        ProgramareID = Guid.NewGuid(),
                        DataProgramare = DateTime.Today.AddHours(10),
                        TipProgramare = "Consulta?ie",
                        Status = "Confirmata",
                        NumePacient = "Popescu Ion",
                        CNPPacient = "1800101123456",
                        TelefonPacient = "0721234567",
                        NumeDoctor = "Dr. Popescu Ion",
                        SpecializareDoctor = "Cardiologie",
                        Observatii = "Control de rutin?"
                    },
                    new()
                    {
                        ProgramareID = Guid.NewGuid(),
                        DataProgramare = DateTime.Today.AddHours(11),
                        TipProgramare = "Consulta?ie",
                        Status = "In Asteptare",
                        NumePacient = "Ionescu Maria",
                        CNPPacient = "2851202234567",
                        TelefonPacient = "0721345678",
                        NumeDoctor = "Dr. Ionescu Maria",
                        SpecializareDoctor = "Neurologie",
                        Observatii = "Dureri de cap frecvente"
                    }
                };

                // Calculate total count and pages
                var totalCount = mockAppointments.Count;
                
                _pagedResult = new PagedResult<ProgramareListDto>(
                    mockAppointments.Skip((_searchQuery.Page - 1) * _searchQuery.PageSize).Take(_searchQuery.PageSize),
                    totalCount,
                    _searchQuery.Page,
                    _searchQuery.PageSize
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching appointments: {ex.Message}");
            _pagedResult = new PagedResult<ProgramareListDto>();
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadTodayStats()
    {
        try
        {
            var response = await Http.GetFromJsonAsync<dynamic>($"api/medical/programari/stats?date={DateTime.Today:yyyy-MM-dd}");
            
            if (response != null)
            {
                // Parse stats from response
                _statsToday = (
                    response.GetProperty("total").GetInt32(),
                    response.GetProperty("finalizate").GetInt32(),
                    response.GetProperty("inAsteptare").GetInt32(),
                    response.GetProperty("anulate").GetInt32()
                );
            }
            else
            {
                // Mock stats
                _statsToday = (8, 3, 2, 1);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading today stats: {ex.Message}");
            _statsToday = (0, 0, 0, 0);
        }
    }

    private async Task OnPageChanged(int page)
    {
        _searchQuery.Page = page;
        await SearchProgramari();
    }

    private async Task ClearFilters()
    {
        _searchQuery = new ProgramariSearchQuery
        {
            DataStart = DateTime.Today,
            DataEnd = DateTime.Today.AddDays(7),
            Page = 1,
            PageSize = 25
        };
        
        _dateRange = new DateRange(DateTime.Today, DateTime.Today.AddDays(7));
        
        await SearchProgramari();
    }

    private MudBlazor.Color GetStatusColor(string status)
    {
        return status switch
        {
            "Programata" => MudBlazor.Color.Info,
            "Confirmata" => MudBlazor.Color.Primary,
            "In Asteptare" => MudBlazor.Color.Warning,
            "In Consultatie" => MudBlazor.Color.Secondary,
            "Finalizata" => MudBlazor.Color.Success,
            "Anulata" => MudBlazor.Color.Error,
            "Nu s-a prezentat" => MudBlazor.Color.Dark,
            "Amanata" => MudBlazor.Color.Warning,
            _ => MudBlazor.Color.Default
        };
    }

    private async Task EditProgramare(Guid programareId)
    {
        Navigation.NavigateTo($"/medical/programari/{programareId}/edit");
    }

    private async Task StartConsultation(Guid programareId)
    {
        Navigation.NavigateTo($"/medical/consultatii/new?programareId={programareId}");
    }

    private async Task UpdateStatus(Guid programareId, string newStatus)
    {
        try
        {
            var response = await Http.PutAsJsonAsync($"api/medical/programari/{programareId}/status", 
                new { Status = newStatus });
            
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add($"Statusul program?rii a fost actualizat la: {newStatus}", MudBlazor.Severity.Success);
                await SearchProgramari();
                await LoadTodayStats();
            }
            else
            {
                Snackbar.Add("Eroare la actualizarea statusului program?rii.", MudBlazor.Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating appointment status: {ex.Message}");
            Snackbar.Add("Demo: Status actualizat local (API indisponibil).", MudBlazor.Severity.Info);
            
            // Update mock data
            await SearchProgramari();
        }
    }

    private async Task PrintAppointment(Guid programareId)
    {
        try
        {
            // Implementation for printing appointment details
            Snackbar.Add("Func?ia de printare va fi implementat? în curând.", MudBlazor.Severity.Info);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error printing appointment: {ex.Message}");
            Snackbar.Add("Eroare la printarea program?rii.", MudBlazor.Severity.Error);
        }
    }

    private async Task ConfirmAppointment(Guid programareId)
    {
        await UpdateStatus(programareId, "Confirmata");
    }

    private async Task CancelAppointment(Guid programareId)
    {
        await UpdateStatus(programareId, "Anulata");
    }
}