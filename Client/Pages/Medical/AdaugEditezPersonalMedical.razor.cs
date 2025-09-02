using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Medical;
using global::Shared.Common;
using Radzen;
using Client.Services.Medical;
using System.Linq;

namespace Client.Pages.Medical;

public partial class AdaugEditezPersonalMedical : ComponentBase, IDisposable
{
    [Inject] private IPersonalMedicalApiService PersonalMedicalApiService { get; set; } = null!;
    [Inject] private IDepartamenteApiService DepartamenteApiService { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [Parameter] public Guid? PersonalId { get; set; }

    // Form model
    private CreatePersonalMedicalRequest _model = new();
    private bool _isEditMode = false;
    private bool _isLoading = true;
    private bool _isProcessing = false;

    // Dropdown options - ACTUALIZAT pentru ierarhie
    private IEnumerable<DepartamentOptionDto> _categoriiOptions = new List<DepartamentOptionDto>();
    private IEnumerable<DepartamentOptionDto> _specializariOptions = new List<DepartamentOptionDto>();
    private IEnumerable<DepartamentOptionDto> _subspecializariOptions = new List<DepartamentOptionDto>();
    private IEnumerable<dynamic> _pozitiiOptions = new List<dynamic>();

    // Business rule warnings
    private bool _showLicentaWarning = false;
    private bool _showSpecializareWarning = false;
    private bool _showBusinessRulesInfo = false;

    // Preview data
    private IEnumerable<PersonalMedicalPreview> _previewData = new List<PersonalMedicalPreview>();

    // Preview class for grid display
    private class PersonalMedicalPreview
    {
        public string Nume { get; set; } = string.Empty;
        public string Prenume { get; set; } = string.Empty;
        public string Pozitie { get; set; } = string.Empty;
        public string Specializare { get; set; } = string.Empty;
        public string Categorie { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CategorieNume { get; set; } = string.Empty;
        public string SpecializareNume { get; set; } = string.Empty;
        public string SubspecializareNume { get; set; } = string.Empty;
    }

    protected override async Task OnInitializedAsync()
    {
        _isEditMode = PersonalId.HasValue;
        
        await LoadInitialData();
        
        if (_isEditMode && PersonalId.HasValue)
        {
            await LoadPersonalForEdit();
        }
        else
        {
            // Set defaults for new personal
            _model.EsteActiv = true;
        }

        _isLoading = false;
        await UpdatePreviewData();
        StateHasChanged(); // For?ez re-render ini?ial
    }

    private async Task LoadInitialData()
    {
        try
        {
            // Load categorii (fostele departamente)
            var categoriiResult = await DepartamenteApiService.GetCategoriiAsync();
            if (categoriiResult.IsSuccess)
            {
                _categoriiOptions = categoriiResult.Value ?? new List<DepartamentOptionDto>();
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Aten?ie",
                    Detail = "Nu s-au putut înc?rca categoriile de departamente",
                    Duration = 4000
                });
            }

            // Load pozitii options (acestea r?mân statice)
            _pozitiiOptions = new[]
            {
                new { Value = "Doctor", Text = "Doctor" },
                new { Value = "Medic Specialist", Text = "Medic Specialist" },
                new { Value = "Medic Primar", Text = "Medic Primar" },
                new { Value = "Asistent Medical", Text = "Asistent Medical" },
                new { Value = "Infirmiera", Text = "Infirmiera" },
                new { Value = "Tehnician Medical", Text = "Tehnician Medical" },
                new { Value = "Kinetoterapeut", Text = "Kinetoterapeut" },
                new { Value = "Farmacist", Text = "Farmacist" },
                new { Value = "Radiolog", Text = "Radiolog" },
                new { Value = "Laborant", Text = "Laborant" },
                new { Value = "Administrator", Text = "Administrator" },
                new { Value = "Personal Suport", Text = "Personal Suport" }
            };
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Eroare",
                Detail = $"Eroare la înc?rcarea datelor ini?iale: {ex.Message}",
                Duration = 5000
            });
        }
    }

    private async Task LoadPersonalForEdit()
    {
        if (!PersonalId.HasValue) return;

        try
        {
            var result = await PersonalMedicalApiService.GetByIdAsync(PersonalId.Value);
            if (result.IsSuccess && result.Value != null)
            {
                var personal = result.Value;
                _model = new CreatePersonalMedicalRequest
                {
                    Nume = personal.Nume,
                    Prenume = personal.Prenume,
                    Pozitie = personal.Pozitie,
                    Specializare = personal.SpecializareNume ?? personal.Specializare ?? string.Empty,
                    NumarLicenta = personal.NumarLicenta ?? string.Empty,
                    Telefon = personal.Telefon ?? string.Empty,
                    Email = personal.Email ?? string.Empty,
                    Departament = personal.DepartamentAfisare,
                    EsteActiv = personal.EsteActiv,
                    // Noi propriet??i ierarhice
                    CategorieID = personal.CategorieID,
                    SpecializareID = personal.SpecializareID,
                    SubspecializareID = personal.SubspecializareID
                };

                // Load dependent dropdowns based on existing data
                if (personal.CategorieID.HasValue)
                {
                    await LoadSpecializari(personal.CategorieID.Value);
                    
                    if (personal.SpecializareID.HasValue)
                    {
                        await LoadSubspecializari(personal.SpecializareID.Value);
                    }
                }
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Eroare",
                    Detail = string.Join(", ", result.Errors),
                    Duration = 4000
                });
                await BackToList();
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Eroare",
                Detail = $"Eroare la înc?rcarea personalului medical: {ex.Message}",
                Duration = 4000
            });
            await BackToList();
        }
    }

    // Cascade dropdown handlers - NOI METODE PENTRU IERARHIE
    private async Task OnCategorieChanged(object? value)
    {
        if (value is Guid categorieId)
        {
            _model.CategorieID = categorieId;
            _model.SpecializareID = null;
            _model.SubspecializareID = null;
            
            // Clear dependent dropdowns
            _specializariOptions = new List<DepartamentOptionDto>();
            _subspecializariOptions = new List<DepartamentOptionDto>();
            
            await LoadSpecializari(categorieId);
        }
        else
        {
            _model.CategorieID = null;
            _model.SpecializareID = null;
            _model.SubspecializareID = null;
            _specializariOptions = new List<DepartamentOptionDto>();
            _subspecializariOptions = new List<DepartamentOptionDto>();
        }
        
        await UpdatePreviewData();
        StateHasChanged();
    }

    private async Task OnSpecializareChanged(object? value)
    {
        if (value is Guid specializareId)
        {
            _model.SpecializareID = specializareId;
            _model.SubspecializareID = null;
            
            // Update Specializare text field based on selection
            var selectedSpecializare = _specializariOptions.FirstOrDefault(s => s.Value == specializareId);
            if (selectedSpecializare != null)
            {
                _model.Specializare = selectedSpecializare.Text;
            }
            
            // Clear subspecializari dropdown
            _subspecializariOptions = new List<DepartamentOptionDto>();
            
            await LoadSubspecializari(specializareId);
        }
        else
        {
            _model.SpecializareID = null;
            _model.SubspecializareID = null;
            _subspecializariOptions = new List<DepartamentOptionDto>();
        }
        
        await UpdatePreviewData();
        StateHasChanged();
    }

    private async Task OnSubspecializareChanged(object? value)
    {
        if (value is Guid subspecializareId)
        {
            _model.SubspecializareID = subspecializareId;
        }
        else
        {
            _model.SubspecializareID = null;
        }
        
        await UpdatePreviewData();
        StateHasChanged();
    }

    // Load methods for cascade dropdowns
    private async Task LoadSpecializari(Guid categorieId)
    {
        try
        {
            var result = await DepartamenteApiService.GetSpecializariByCategorieAsync(categorieId);
            if (result.IsSuccess)
            {
                _specializariOptions = result.Value ?? new List<DepartamentOptionDto>();
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Aten?ie",
                    Detail = "Nu s-au putut înc?rca specializ?rile pentru categoria selectat?",
                    Duration = 4000
                });
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Eroare",
                Detail = $"Eroare la înc?rcarea specializ?rilor: {ex.Message}",
                Duration = 4000
            });
        }
    }

    private async Task LoadSubspecializari(Guid specializareId)
    {
        try
        {
            var result = await DepartamenteApiService.GetSubspecializariBySpecializareAsync(specializareId);
            if (result.IsSuccess)
            {
                _subspecializariOptions = result.Value ?? new List<DepartamentOptionDto>();
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Aten?ie",
                    Detail = "Nu s-au putut înc?rca subspecializ?rile pentru specializarea selectat?",
                    Duration = 4000
                });
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Eroare",
                Detail = $"Eroare la înc?rcarea subspecializ?rilor: {ex.Message}",
                Duration = 4000
            });
        }
    }

    private async Task OnPozitieChanged(object? value)
    {
        if (value is string pozitie)
        {
            _model.Pozitie = pozitie;
            await CheckBusinessRules();
        }
        
        await UpdatePreviewData();
    }

    private async Task OnFieldChanged(string fieldName)
    {
        await CheckBusinessRules();
        await UpdatePreviewData();
    }

    private async Task CheckBusinessRules()
    {
        // Reset warnings
        _showLicentaWarning = false;
        _showSpecializareWarning = false;

        // Check if license number is required for doctors
        if (!string.IsNullOrEmpty(_model.Pozitie))
        {
            var pozitiiCareNecesitaLicenta = new[] { "Doctor", "Medic Specialist", "Medic Primar" };
            _showLicentaWarning = pozitiiCareNecesitaLicenta.Contains(_model.Pozitie) && string.IsNullOrEmpty(_model.NumarLicenta);
        }

        // Check if specialization is recommended
        if (!string.IsNullOrEmpty(_model.Pozitie))
        {
            var pozitiiCareNecesitaSpecializare = new[] { "Doctor", "Medic Specialist", "Medic Primar", "Asistent Medical" };
            _showSpecializareWarning = pozitiiCareNecesitaSpecializare.Contains(_model.Pozitie) && string.IsNullOrEmpty(_model.Specializare);
        }

        _showBusinessRulesInfo = _showLicentaWarning || _showSpecializareWarning;
        
        await Task.CompletedTask;
    }

    private async Task UpdatePreviewData()
    {
        if (!string.IsNullOrEmpty(_model.Nume) || !string.IsNullOrEmpty(_model.Prenume) || _isEditMode)
        {
            // Create preview data
            var categorieNume = _categoriiOptions.FirstOrDefault(c => c.Value == _model.CategorieID)?.Text;
            var specializareNume = _specializariOptions.FirstOrDefault(s => s.Value == _model.SpecializareID)?.Text;
            var subspecializareNume = _subspecializariOptions.FirstOrDefault(s => s.Value == _model.SubspecializareID)?.Text;

            _previewData = new[]
            {
                new PersonalMedicalPreview
                {
                    Nume = _model.Nume ?? "...",
                    Prenume = _model.Prenume ?? "...",
                    Pozitie = _model.Pozitie ?? "Nu este specificata",
                    Specializare = specializareNume ?? _model.Specializare ?? "Nu este specificata",
                    Categorie = categorieNume ?? _model.Departament ?? "Nu este specificat",
                    Status = _model.EsteActiv ? "Activ" : "Inactiv",
                    CategorieNume = categorieNume ?? string.Empty,
                    SpecializareNume = specializareNume ?? string.Empty,
                    SubspecializareNume = subspecializareNume ?? string.Empty
                }
            };
        }
        else
        {
            _previewData = new List<PersonalMedicalPreview>();
        }
        
        StateHasChanged(); // For?ez re-render pentru preview
        await Task.CompletedTask;
    }

    public async Task OnSubmitAsync(CreatePersonalMedicalRequest model)
    {
        _isProcessing = true;
        
        try
        {
            if (_isEditMode && PersonalId.HasValue)
            {
                var updateRequest = new UpdatePersonalMedicalRequest
                {
                    PersonalID = PersonalId.Value,
                    Nume = _model.Nume,
                    Prenume = _model.Prenume,
                    Pozitie = _model.Pozitie,
                    Specializare = _model.Specializare,
                    NumarLicenta = _model.NumarLicenta,
                    Telefon = _model.Telefon,
                    Email = _model.Email,
                    Departament = _model.Departament,
                    EsteActiv = _model.EsteActiv,
                    // Noi propriet??i ierarhice
                    CategorieID = _model.CategorieID,
                    SpecializareID = _model.SpecializareID,
                    SubspecializareID = _model.SubspecializareID
                };

                var result = await PersonalMedicalApiService.UpdateAsync(PersonalId.Value, updateRequest);
                
                if (result.IsSuccess)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Succes",
                        Detail = $"Personal medical '{_model.Nume} {_model.Prenume}' a fost actualizat cu succes",
                        Duration = 3000
                    });
                    await BackToList();
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Eroare",
                        Detail = string.Join(", ", result.Errors),
                        Duration = 4000
                    });
                }
            }
            else
            {
                var result = await PersonalMedicalApiService.CreateAsync(_model);
                
                if (result.IsSuccess)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Succes",
                        Detail = $"Personal medical '{_model.Nume} {_model.Prenume}' a fost creat cu succes",
                        Duration = 3000
                    });
                    await BackToList();
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Eroare",
                        Detail = string.Join(", ", result.Errors),
                        Duration = 4000
                    });
                }
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Eroare",
                Detail = $"Eroare nea?teptat?: {ex.Message}",
                Duration = 4000
            });
        }
        finally
        {
            _isProcessing = false;
        }
    }

    public async Task BackToList()
    {
        Navigation.NavigateTo("/medical/gestionare-personal");
        await Task.CompletedTask;
    }

    private string GetSaveButtonText()
    {
        return _isEditMode ? "Actualizeaza Personal" : "Salveaza Personal";
    }

    private string GetSaveButtonIcon()
    {
        return _isEditMode ? "save" : "add";
    }

    private string GetSaveButtonStyle()
    {
        return _isProcessing ? "opacity: 0.6;" : "";
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}