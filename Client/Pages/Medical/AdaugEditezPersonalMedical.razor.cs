using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Medical;
using global::Shared.Common;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;
using Client.Services.Medical;
using Client.Extensions;

namespace Client.Pages.Medical;

public partial class AdaugEditezPersonalMedical : ComponentBase, IDisposable
{
    [Inject] private IPersonalMedicalApiService PersonalMedicalApiService { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    [Parameter] public Guid? PersonalId { get; set; }

    // Form model
    private CreatePersonalMedicalRequest _model = new();
    private bool _isEditMode = false;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _isDisposed = false;

    // Dropdown options
    private List<DropdownOption> _pozitiiOptions = new();
    private List<DropdownOption> _categoriiOptions = new();
    private List<DropdownOption> _specializariOptions = new();
    private List<DropdownOption> _subspecializariOptions = new();

    // Warning state
    private bool _showLicentaWarning = false;
    private bool _showSpecializareWarning = false;
    private bool _showBusinessRulesInfo => _showLicentaWarning || _showSpecializareWarning;

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
        // Noi propriet??i pentru ierarhie
        public string CategorieNume { get; set; } = string.Empty;
        public string SpecializareNume { get; set; } = string.Empty;
        public string SubspecializareNume { get; set; } = string.Empty;
    }

    // Dropdown option class
    private class DropdownOption
    {
        public string Text { get; set; } = string.Empty;
        public object Value { get; set; } = null!;
    }

    protected override async Task OnInitializedAsync()
    {
        _isEditMode = PersonalId.HasValue;
        
        await LoadDropdownData();
        
        if (_isEditMode && PersonalId.HasValue)
        {
            await LoadPersonalForEdit();
        }

        _isLoading = false;
        await UpdatePreviewData();
        StateHasChanged();
    }

    private async Task LoadDropdownData()
    {
        try
        {
            // Load pozitii options
            _pozitiiOptions = new List<DropdownOption>
            {
                new() { Text = "Doctor", Value = "Doctor" },
                new() { Text = "Medic specialist", Value = "Medic specialist" },
                new() { Text = "Medic primar", Value = "Medic primar" },
                new() { Text = "Medic rezident", Value = "Medic rezident" },
                new() { Text = "Asistent medical", Value = "Asistent medical" },
                new() { Text = "Asistent medical principal", Value = "Asistent medical principal" },
                new() { Text = "Tehnician medical", Value = "Tehnician medical" },
                new() { Text = "Kinetoterapeut", Value = "Kinetoterapeut" },
                new() { Text = "Psiholog", Value = "Psiholog" },
                new() { Text = "Nutritionist", Value = "Nutritionist" },
                new() { Text = "Receptioner", Value = "Receptioner" },
                new() { Text = "Administrator", Value = "Administrator" }
            };

            // Load categorii options  
            _categoriiOptions = new List<DropdownOption>
            {
                new() { Text = "Cardiologie", Value = Guid.NewGuid() },
                new() { Text = "Neurologie", Value = Guid.NewGuid() },
                new() { Text = "Pediatrie", Value = Guid.NewGuid() },
                new() { Text = "Chirurgie", Value = Guid.NewGuid() },
                new() { Text = "Medicina interna", Value = Guid.NewGuid() },
                new() { Text = "Radiologie", Value = Guid.NewGuid() },
                new() { Text = "Laborator", Value = Guid.NewGuid() },
                new() { Text = "Recuperare", Value = Guid.NewGuid() },
                new() { Text = "Receptie", Value = Guid.NewGuid() },
                new() { Text = "Administratie", Value = Guid.NewGuid() }
            };
        }
        catch (Exception ex)
        {
            NotificationService.ShowError($"Eroare la incarcarea datelor: {ex.Message}");
        }
    }

    private async Task OnPozitieChanged(object? value)
    {
        if (value is string pozitie)
        {
            _model.Pozitie = pozitie;
        }
        else
        {
            _model.Pozitie = string.Empty;
        }
        
        await UpdatePreviewData();
        CheckBusinessRules();
    }

    private async Task OnCategorieChanged(object? value)
    {
        if (value is Guid categorieId)
        {
            _model.CategorieID = categorieId;
            
            // Reset dependent dropdowns
            _model.SpecializareID = null;
            _model.SubspecializareID = null;
            _specializariOptions.Clear();
            _subspecializariOptions.Clear();
            
            // Load specializari for selected categorie
            await LoadSpecializariForCategorie(categorieId);
        }
        else
        {
            _model.CategorieID = null;
            _specializariOptions.Clear();
            _subspecializariOptions.Clear();
        }
        
        await UpdatePreviewData();
        CheckBusinessRules();
    }

    private async Task OnSpecializareChanged(object? value)
    {
        if (value is Guid specializareId)
        {
            _model.SpecializareID = specializareId;
            
            // Reset dependent dropdown
            _model.SubspecializareID = null;
            _subspecializariOptions.Clear();
            
            // Load subspecializari for selected specializare
            await LoadSubspecializariForSpecializare(specializareId);
        }
        else
        {
            _model.SpecializareID = null;
            _subspecializariOptions.Clear();
        }
        
        await UpdatePreviewData();
        CheckBusinessRules();
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
    }

    private async Task LoadSpecializariForCategorie(Guid categorieId)
    {
        try
        {
            // Mock data pentru demo - în realitate ar veni din API
            _specializariOptions = new List<DropdownOption>
            {
                new() { Text = "Cardiologie generala", Value = Guid.NewGuid() },
                new() { Text = "Cardiologie interventionala", Value = Guid.NewGuid() },
                new() { Text = "Electrofizologie", Value = Guid.NewGuid() }
            };
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            NotificationService.ShowError($"Eroare la incarcarea specializarilor: {ex.Message}");
        }
    }

    private async Task LoadSubspecializariForSpecializare(Guid specializareId)
    {
        try
        {
            // Mock data pentru demo
            _subspecializariOptions = new List<DropdownOption>
            {
                new() { Text = "Implant de pacemaker", Value = Guid.NewGuid() },
                new() { Text = "Ablatie cardiaca", Value = Guid.NewGuid() }
            };
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            NotificationService.ShowError($"Eroare la incarcarea subspecializarilor: {ex.Message}");
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
                    Specializare = personal.Specializare,
                    NumarLicenta = personal.NumarLicenta,
                    Telefon = personal.Telefon,
                    Email = personal.Email,
                    Departament = personal.Departament,
                    EsteActiv = personal.EsteActiv
                };
            }
            else
            {
                result.ShowNotification(NotificationService);
            }
        }
        catch (Exception ex)
        {
            NotificationService.ShowError($"Eroare la incarcarea personalului medical: {ex.Message}");
        }
    }

    private async Task OnFieldChanged(string fieldName)
    {
        await UpdatePreviewData();
        CheckBusinessRules();
    }

    private void CheckBusinessRules()
    {
        // Check licenta warning
        _showLicentaWarning = !string.IsNullOrEmpty(_model.Pozitie) && 
                              (_model.Pozitie.Contains("Doctor") || _model.Pozitie.Contains("Medic")) && 
                              string.IsNullOrEmpty(_model.NumarLicenta);

        // Check specializare warning
        _showSpecializareWarning = !string.IsNullOrEmpty(_model.Pozitie) && 
                                   (_model.Pozitie.Contains("Doctor") || _model.Pozitie.Contains("Medic")) && 
                                   !_model.SpecializareID.HasValue;
    }

    private async Task UpdatePreviewData()
    {
        if (!string.IsNullOrEmpty(_model.Nume) || !string.IsNullOrEmpty(_model.Prenume) || _isEditMode)
        {
            // Get names for preview
            var categorieNume = _model.CategorieID.HasValue ? 
                _categoriiOptions.FirstOrDefault(c => c.Value.Equals(_model.CategorieID.Value))?.Text ?? "" : "";
            var specializareNume = _model.SpecializareID.HasValue ? 
                _specializariOptions.FirstOrDefault(s => s.Value.Equals(_model.SpecializareID.Value))?.Text ?? "" : "";
            var subspecializareNume = _model.SubspecializareID.HasValue ? 
                _subspecializariOptions.FirstOrDefault(ss => ss.Value.Equals(_model.SubspecializareID.Value))?.Text ?? "" : "";

            _previewData = new[]
            {
                new PersonalMedicalPreview
                {
                    Nume = _model.Nume ?? "...",
                    Prenume = _model.Prenume ?? "...",
                    Pozitie = _model.Pozitie ?? "Nu este specificata",
                    Specializare = _model.Specializare ?? "Nu este specificata",
                    Categorie = categorieNume ?? "Nu este specificat",
                    Status = _model.EsteActiv ? "Activ" : "Inactiv",
                    CategorieNume = categorieNume,
                    SpecializareNume = specializareNume,
                    SubspecializareNume = subspecializareNume
                }
            };
        }
        else
        {
            _previewData = new List<PersonalMedicalPreview>();
        }
        
        StateHasChanged();
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
                    EsteActiv = _model.EsteActiv
                };

                var result = await PersonalMedicalApiService.UpdateAsync(PersonalId.Value, updateRequest);
                
                result.ShowNotification(NotificationService);
                
                if (result.IsSuccess)
                {
                    await BackToList();
                }
            }
            else
            {
                var result = await PersonalMedicalApiService.CreateAsync(_model);
                
                result.ShowNotification(NotificationService);
                
                if (result.IsSuccess)
                {
                    await BackToList();
                }
            }
        }
        catch (Exception ex)
        {
            NotificationService.ShowError($"Eroare neasteptata: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task BackToList()
    {
        Navigation.NavigateTo("/medical/gestionare-personal");
        await Task.CompletedTask;
    }

    private string GetSaveButtonText()
    {
        return _isEditMode ? "Actualizeaza Personal Medical" : "Salveaza Personal Medical";
    }

    private string GetSaveButtonIcon()
    {
        return _isEditMode ? "save" : "add";
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        _isDisposed = true;
        
        try
        {
            // Clear collections to help GC
            _pozitiiOptions?.Clear();
            _categoriiOptions?.Clear();
            _specializariOptions?.Clear();
            _subspecializariOptions?.Clear();
            
            // Clear preview data
            if (_previewData is List<PersonalMedicalPreview> list)
            {
                list.Clear();
            }
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
}