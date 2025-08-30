using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.DTOs.Medical;
using Shared.Models.Medical;
using System.Net.Http.Json;

namespace Client.Pages.Medical;

public partial class PatientTriage
{
    private bool _showQuickTriajDialog = false;
    private bool _isSavingTriaj = false;
    private int _selectedNivelTriaj = 1;
    private string _quickTriajComplaint = string.Empty;
    private string _quickTriajNotes = string.Empty;
    
    private List<TriajPacient> _triajAstazi = new();
    private (int Nivel1, int Nivel2, int Nivel3, int Nivel4, int Nivel5) _statisticiTriaj = (2, 3, 5, 8, 12);

    private readonly DialogOptions _dialogOptions = new()
    {
        CloseOnEscapeKey = true,
        MaxWidth = MaxWidth.Medium,
        FullWidth = true
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadTriajAstazi();
        await LoadStatisticiTriaj();
    }

    private async Task LoadTriajAstazi()
    {
        try
        {
            var response = await Http.GetFromJsonAsync<List<TriajPacient>>($"api/medical/triaj/today");
            
            if (response != null)
            {
                _triajAstazi = response;
            }
            else
            {
                // Mock data for development
                _triajAstazi = new List<TriajPacient>
                {
                    new()
                    {
                        TriajID = Guid.NewGuid(),
                        ProgramareID = Guid.NewGuid(),
                        NivelTriaj = 2,
                        PlangereaPrincipala = "Durere toracic? acut? cu iradiere în bra?ul stâng",
                        DataTriaj = DateTime.Today.AddHours(8).AddMinutes(30),
                        Observatii = "Pacient anxios, transpira?ii"
                    },
                    new()
                    {
                        TriajID = Guid.NewGuid(),
                        ProgramareID = Guid.NewGuid(),
                        NivelTriaj = 3,
                        PlangereaPrincipala = "Febr? ?i tuse persistent? de 3 zile",
                        DataTriaj = DateTime.Today.AddHours(9).AddMinutes(15),
                        Observatii = "Istoric de astm bron?ic"
                    },
                    new()
                    {
                        TriajID = Guid.NewGuid(),
                        ProgramareID = Guid.NewGuid(),
                        NivelTriaj = 4,
                        PlangereaPrincipala = "Durere în genunchiul drept dup? c?dere",
                        DataTriaj = DateTime.Today.AddHours(10),
                        Observatii = "Poate merge, durere la mobilizare"
                    }
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading today's triage: {ex.Message}");
            _triajAstazi = new List<TriajPacient>();
        }
    }

    private async Task LoadStatisticiTriaj()
    {
        try
        {
            var response = await Http.GetFromJsonAsync<dynamic>($"api/medical/triaj/stats?date={DateTime.Today:yyyy-MM-dd}");
            
            if (response != null)
            {
                // Parse stats from API response
                _statisticiTriaj = (1, 2, 4, 6, 8); // Mock values
            }
            else
            {
                // Mock stats for development
                _statisticiTriaj = (1, 3, 5, 8, 12);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading triage stats: {ex.Message}");
            _statisticiTriaj = (1, 2, 4, 6, 8);
        }
    }

    private void QuickTriaj(int nivelTriaj)
    {
        _selectedNivelTriaj = nivelTriaj;
        _quickTriajComplaint = string.Empty;
        _quickTriajNotes = string.Empty;
        _showQuickTriajDialog = true;
    }

    private void CloseQuickTriaj()
    {
        _showQuickTriajDialog = false;
        _quickTriajComplaint = string.Empty;
        _quickTriajNotes = string.Empty;
        _isSavingTriaj = false;
    }

    private async Task SaveQuickTriaj()
    {
        if (string.IsNullOrWhiteSpace(_quickTriajComplaint))
        {
            Snackbar.Add("Plângerea principal? este obligatorie.", Severity.Error);
            return;
        }

        _isSavingTriaj = true;
        StateHasChanged();

        try
        {
            var triajRequest = new CreateTriajRequest(
                ProgramareID: Guid.NewGuid(), // In real app, this would be selected
                NivelTriaj: _selectedNivelTriaj,
                PlangereaPrincipala: _quickTriajComplaint,
                AsistentTriajID: null, // Current user
                Observatii: _quickTriajNotes
            );

            var response = await Http.PostAsJsonAsync("api/medical/triaj", triajRequest);
            
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add($"Triajul de nivel {_selectedNivelTriaj} a fost salvat cu succes!", Severity.Success);
                CloseQuickTriaj();
                await LoadTriajAstazi();
                await LoadStatisticiTriaj();
            }
            else
            {
                Snackbar.Add("Eroare la salvarea triajului.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving triage: {ex.Message}");
            
            // For development - simulate success
            Snackbar.Add("Demo: Triajul a fost salvat local (API indisponibil).", Severity.Info);
            CloseQuickTriaj();
            
            // Add mock triaj to local list
            var mockTriaj = new TriajPacient
            {
                TriajID = Guid.NewGuid(),
                ProgramareID = Guid.NewGuid(),
                NivelTriaj = _selectedNivelTriaj,
                PlangereaPrincipala = _quickTriajComplaint,
                DataTriaj = DateTime.Now,
                Observatii = _quickTriajNotes
            };
            
            _triajAstazi.Insert(0, mockTriaj);
            StateHasChanged();
        }
        finally
        {
            _isSavingTriaj = false;
        }
    }

    private void StartDetailedTriaj()
    {
        Navigation.NavigateTo("/medical/triaj/detaliat");
    }

    private void SearchPatient()
    {
        Navigation.NavigateTo("/medical/pacienti/cautare");
    }

    private void EditTriaj(Guid triajId)
    {
        Navigation.NavigateTo($"/medical/triaj/{triajId}/editare");
    }

    private void StartConsultation(Guid programareId)
    {
        Navigation.NavigateTo($"/medical/consultatii/noua/{programareId}");
    }

    private Color GetTriajColor(int nivelTriaj)
    {
        return nivelTriaj switch
        {
            1 => Color.Error,
            2 => Color.Warning,
            3 => Color.Info,
            4 => Color.Primary,
            5 => Color.Success,
            _ => Color.Default
        };
    }

    private string GetTriajIcon(int nivelTriaj)
    {
        return nivelTriaj switch
        {
            1 => Icons.Material.Filled.LocalHospital,
            2 => Icons.Material.Filled.Warning,
            3 => Icons.Material.Filled.Schedule,
            4 => Icons.Material.Filled.Info,
            5 => Icons.Material.Filled.CheckCircle,
            _ => Icons.Material.Filled.Help
        };
    }
}