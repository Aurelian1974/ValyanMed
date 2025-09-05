using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Authentication;
using global::Shared.Common;
using global::Shared.Enums;
using global::Shared.DTOs.Common;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;
using Client.Services.Common;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Rendering;

namespace Client.Pages.Authentication;

public partial class AdaugEditezPersoana : ComponentBase, IDisposable
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILocationApiService LocationService { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    [Parameter] public int? PersoanaId { get; set; }

    // Form model
    private CreatePersoanaRequest _model = new() { EsteActiva = true };
    
    // Form state
    private bool _isProcessing = false;
    private bool _isLoading = false;
    private bool _isEditMode => PersoanaId.HasValue && PersoanaId.Value > 0;
    private bool _isDisposed = false;
    
    // Dropdown options
    private readonly List<Gen> _genOptions = Enum.GetValues<Gen>().ToList();
    private List<JudetDto> _judeteOptions = new();
    private List<LocalitateDto> _localitatiOptions = new();

    protected override async Task OnInitializedAsync()
    {
        // Test DialogService availability
        Console.WriteLine($"DialogService is null: {DialogService == null}");
        
        await LoadJudeteAsync();
        
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
                    
                    if (!string.IsNullOrEmpty(_model.Judet))
                    {
                        await OnJudetChangedAsync(_model.Judet);
                    }
                    
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

    private void OnFieldChanged(string fieldName)
    {
        if (fieldName == nameof(_model.CNP))
        {
            AutoFillFromCnp(_model.CNP);
        }
        
        InvokeAsync(StateHasChanged);
    }

    private async Task OnJudetChangedAsync(string? selectedJudet)
    {
        if (_model.Judet != selectedJudet)
        {
            _model.Localitate = null;
        }
        
        _model.Judet = selectedJudet;
        
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
        
        await InvokeAsync(StateHasChanged);
    }

    private void OnJudetChanged(object? selectedJudet)
    {
        var judet = selectedJudet?.ToString();
        _ = OnJudetChangedAsync(judet);
    }

    private async Task SubmitForm()
    {
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
        Console.WriteLine("OnSubmitAsync called!");
        Console.WriteLine($"Model data: Nume={model.Nume}, Prenume={model.Prenume}");
        Console.WriteLine($"Current _isProcessing state: {_isProcessing}");
        
        await SubmitForm();
    }

    private void OnInvalidSubmit()
    {
        Console.WriteLine("OnInvalidSubmit called - Form validation failed!");
        ShowErrorNotification("Te rog completeaz? toate câmpurile obligatorii!");
    }

    private async Task SavePersoana()
    {
        if (_isProcessing) 
        {
            Console.WriteLine("SavePersoana called but _isProcessing is already true - aborting");
            return;
        }

        Console.WriteLine("SavePersoana starting...");
        _isProcessing = true;
        StateHasChanged();
        
        try
        {
            Console.WriteLine($"Sending request to save persoana: {_model.Nume} {_model.Prenume}");
            
            var response = await Http.PostAsJsonAsync("api/Persoane", _model);
            
            Console.WriteLine($"Response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                // Try to extract ID from response
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Raw response content: {responseContent}");
                
                int resultId = await ExtractPersonIdFromResponse(responseContent);
                
                if (resultId > 0)
                {
                    Console.WriteLine($"Save successful - received ID: {resultId}");
                    
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Persoana a fost salvat? cu succes",
                        Detail = $"Persoana '{_model.Nume} {_model.Prenume}' a fost ad?ugat? cu succes în sistem.",
                        Duration = 3000
                    });
                    
                    var navigationUrl = $"/administrare/persoane/verificare/{resultId}";
                    Console.WriteLine($"Navigating to: {navigationUrl}");
                    
                    _isProcessing = false;
                    StateHasChanged();
                    
                    await Task.Delay(500);
                    Navigation.NavigateTo(navigationUrl, forceLoad: false);
                    return;
                }
                else
                {
                    // Fallback - show success and go to list
                    Console.WriteLine("Save successful but could not extract ID - navigating to list");
                    
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Persoana a fost salvat? cu succes",
                        Detail = $"Persoana '{_model.Nume} {_model.Prenume}' a fost ad?ugat? cu succes în sistem.",
                        Duration = 5000
                    });
                    
                    _isProcessing = false;
                    StateHasChanged();
                    
                    await Task.Delay(1000);
                    Navigation.NavigateTo("/administrare/gestionare-persoane", forceLoad: false);
                    return;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"HTTP Error: {errorContent}");
                await HandleApiError(response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            await ShowBusinessErrorDialog("Eroare nea?teptat?", $"A ap?rut o eroare nea?teptat?: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("SavePersoana finally block - resetting _isProcessing");
            _isProcessing = false;
            if (!_isDisposed)
                StateHasChanged();
        }
    }

    private async Task<int> ExtractPersonIdFromResponse(string responseContent)
    {
        try
        {
            // Approach 1: Try as simple int
            if (int.TryParse(responseContent.Trim('"'), out int resultId))
            {
                Console.WriteLine($"Parsed as simple int: {resultId}");
                return resultId;
            }
            
            // Approach 2: Try as JSON object with various properties
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;
            
            // Try different property names
            var propertyNames = new[] { "value", "id", "personId", "result", "data" };
            
            foreach (var propName in propertyNames)
            {
                if (root.TryGetProperty(propName, out var element))
                {
                    if (element.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        resultId = element.GetInt32();
                        Console.WriteLine($"Extracted from {propName}: {resultId}");
                        return resultId;
                    }
                }
            }
            
            // Approach 3: If it's a Result<int> object
            if (root.TryGetProperty("isSuccess", out var isSuccessElement) && 
                isSuccessElement.GetBoolean() && 
                root.TryGetProperty("value", out var valueElement))
            {
                resultId = valueElement.GetInt32();
                Console.WriteLine($"Extracted from Result<int>.Value: {resultId}");
                return resultId;
            }
            
            Console.WriteLine("Could not extract ID from response, trying fallback methods");
            return await GetPersonIdByFallback();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing response: {ex.Message}");
            return await GetPersonIdByFallback();
        }
    }

    private async Task<int> GetPersonIdByFallback()
    {
        try
        {
            // Fallback 1: Search by CNP and name combination
            if (!string.IsNullOrEmpty(_model.CNP))
            {
                Console.WriteLine($"Trying to find person by CNP: {_model.CNP}");
                
                // Get recent persons and find by CNP
                var response = await Http.GetAsync("api/Persoane?pageSize=10&sortBy=DataCreare&sortDirection=desc");
                if (response.IsSuccessStatusCode)
                {
                    var recentPersonsJson = await response.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(recentPersonsJson);
                    
                    if (doc.RootElement.TryGetProperty("data", out var dataElement))
                    {
                        foreach (var person in dataElement.EnumerateArray())
                        {
                            if (person.TryGetProperty("cnp", out var cnpElement) &&
                                person.TryGetProperty("id", out var idElement) &&
                                cnpElement.GetString() == _model.CNP)
                            {
                                var foundId = idElement.GetInt32();
                                Console.WriteLine($"Found person by CNP: ID = {foundId}");
                                return foundId;
                            }
                        }
                    }
                }
            }
            
            // Fallback 2: Search by exact name match from recent additions
            Console.WriteLine($"Trying to find person by name: {_model.Nume} {_model.Prenume}");
            var nameSearchResponse = await Http.GetAsync("api/Persoane?pageSize=5&sortBy=DataCreare&sortDirection=desc");
            if (nameSearchResponse.IsSuccessStatusCode)
            {
                var personsJson = await nameSearchResponse.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(personsJson);
                
                if (doc.RootElement.TryGetProperty("data", out var dataElement))
                {
                    foreach (var person in dataElement.EnumerateArray())
                    {
                        if (person.TryGetProperty("nume", out var numeElement) &&
                            person.TryGetProperty("prenume", out var prenumeElement) &&
                            person.TryGetProperty("id", out var idElement) &&
                            numeElement.GetString() == _model.Nume &&
                            prenumeElement.GetString() == _model.Prenume)
                        {
                            var foundId = idElement.GetInt32();
                            Console.WriteLine($"Found person by name: ID = {foundId}");
                            return foundId;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in fallback search: {ex.Message}");
        }
        
        Console.WriteLine("All fallback methods failed, returning 0");
        return 0;
    }

    private async Task UpdatePersoana()
    {
        if (_isProcessing || !PersoanaId.HasValue) 
        {
            Console.WriteLine("UpdatePersoana called but conditions not met - aborting");
            return;
        }

        Console.WriteLine($"UpdatePersoana starting for ID: {PersoanaId.Value}");
        _isProcessing = true;
        StateHasChanged();
        
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
            
            Console.WriteLine($"Update response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Update successful");
                
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Persoana a fost actualizat? cu succes",
                    Detail = $"Datele pentru '{_model.Nume} {_model.Prenume}' au fost actualizate cu succes.",
                    Duration = 3000
                });
                
                var navigationUrl = $"/administrare/persoane/verificare/{PersoanaId.Value}";
                Console.WriteLine($"Navigating to: {navigationUrl}");
                
                _isProcessing = false;
                StateHasChanged();
                
                await Task.Delay(500);
                Navigation.NavigateTo(navigationUrl, forceLoad: false);
                return;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Update error: {errorContent}");
                await HandleApiError(response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update exception: {ex.Message}");
            await ShowBusinessErrorDialog("Eroare nea?teptat?", $"A ap?rut o eroare nea?teptat?: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("UpdatePersoana finally block");
            _isProcessing = false;
            if (!_isDisposed)
                StateHasChanged();
        }
    }

    private async Task HandleApiError(System.Net.HttpStatusCode statusCode, string errorContent)
    {
        Console.WriteLine($"HandleApiError called with status: {statusCode}");
        Console.WriteLine($"Error content: {errorContent}");
        
        try
        {
            // Try to parse the error response as JSON
            var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorContent, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Console.WriteLine($"Parsed error response: Errors count = {errorResponse?.Errors?.Length ?? 0}");
            
            if (errorResponse?.Errors != null && errorResponse.Errors.Any())
            {
                var primaryError = errorResponse.Errors.First();
                Console.WriteLine($"Primary error: {primaryError}");
                
                // Check for CNP duplicate error with various possible text variations
                if (primaryError.Contains("CNP") && (primaryError.Contains("exista") || primaryError.Contains("exist?") || primaryError.Contains("duplicat")))
                {
                    Console.WriteLine("Detected CNP duplicate error - STOPPING overlay then showing notification");
                    
                    // STOP processing overlay FIRST so notification is visible
                    _isProcessing = false;
                    StateHasChanged();
                    await Task.Delay(100); // Give UI time to update
                    
                    // Now show enhanced notification with maximum visibility
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "?? ATEN?IE: CNP DUPLICAT",
                        Detail = $"CNP-ul '{_model.CNP}' EXIST? DEJA în sistem! Verifica?i datele introduse.",
                        Duration = 15000, // 15 seconds for maximum visibility
                        CloseOnClick = true
                    });
                    Console.WriteLine("Enhanced CNP notification shown with overlay stopped");
                    
                    // Then try dialog
                    await ShowBusinessErrorDialog(
                        "CNP Duplicat", 
                        $"CNP-ul '{_model.CNP}' exist? deja în sistem.\n\nV? rug?m s? verifica?i CNP-ul introdus sau s? c?uta?i persoana existent? în lista de persoane."
                    );
                }
                else if (primaryError.Contains("email") && (primaryError.Contains("exista") || primaryError.Contains("exist?") || primaryError.Contains("duplicat")))
                {
                    Console.WriteLine("Detected email duplicate error - STOPPING overlay then showing notification");
                    
                    // STOP processing overlay FIRST
                    _isProcessing = false;
                    StateHasChanged();
                    await Task.Delay(100);
                    
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "?? EMAIL DUPLICAT",
                        Detail = $"Email-ul '{_model.Email}' EXIST? DEJA în sistem! Folosi?i alt email.",
                        Duration = 12000,
                        CloseOnClick = true
                    });
                    Console.WriteLine("Enhanced email notification shown with overlay stopped");
                    
                    await ShowBusinessErrorDialog(
                        "Email Duplicat", 
                        $"Adresa de email '{_model.Email}' este deja utilizat?.\n\nV? rug?m s? utiliza?i o alt? adres? de email."
                    );
                }
                else
                {
                    Console.WriteLine("Generic validation error - STOPPING overlay then showing notification");
                    
                    // STOP processing overlay FIRST
                    _isProcessing = false;
                    StateHasChanged();
                    await Task.Delay(100);
                    
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "?? EROARE DE VALIDARE",
                        Detail = string.Join(", ", errorResponse.Errors),
                        Duration = 10000,
                        CloseOnClick = true
                    });
                    Console.WriteLine("Enhanced validation error notification shown with overlay stopped");
                    
                    await ShowBusinessErrorDialog("Eroare de validare", string.Join("\n", errorResponse.Errors));
                }
            }
            else
            {
                Console.WriteLine("No specific errors found in response - STOPPING overlay then showing notification");
                
                // STOP processing overlay FIRST
                _isProcessing = false;
                StateHasChanged();
                await Task.Delay(100);
                
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "?? EROARE",
                    Detail = "A ap?rut o eroare la salvare. Verifica?i datele introduse.",
                    Duration = 8000,
                    CloseOnClick = true
                });
                Console.WriteLine("Generic error notification shown with overlay stopped");
                
                await ShowBusinessErrorDialog("Eroare", $"A ap?rut o eroare la salvare: {errorContent}");
            }
        }
        catch (System.Text.Json.JsonException jsonEx)
        {
            Console.WriteLine($"JSON parsing failed: {jsonEx.Message}");
            
            // Fallback: try to extract error message directly from the content
            if (errorContent.Contains("\"errors\"") && errorContent.Contains("CNP"))
            {
                Console.WriteLine("Detected CNP error in raw content - STOPPING overlay then showing notification");
                
                // STOP processing overlay FIRST
                _isProcessing = false;
                StateHasChanged();
                await Task.Delay(100);
                
                // Show enhanced immediate notification
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "?? ATEN?IE: CNP DUPLICAT",
                    Detail = $"CNP-ul '{_model.CNP}' EXIST? DEJA în sistem! Verifica?i datele introduse.",
                    Duration = 15000,
                    CloseOnClick = true
                });
                Console.WriteLine("Enhanced CNP fallback notification shown with overlay stopped");
                
                // Then try dialog
                await ShowBusinessErrorDialog(
                    "CNP Duplicat", 
                    $"CNP-ul '{_model.CNP}' exist? deja în sistem.\n\nV? rug?m s? verifica?i CNP-ul introdus sau s? c?uta?i persoana existent? în lista de persoane."
                );
            }
            else if (errorContent.Contains("\"errors\"") && errorContent.Contains("email"))
            {
                Console.WriteLine("Detected email error in raw content - STOPPING overlay then showing notification");
                
                // STOP processing overlay FIRST
                _isProcessing = false;
                StateHasChanged();
                await Task.Delay(100);
                
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "?? EMAIL DUPLICAT",
                    Detail = $"Email-ul '{_model.Email}' EXIST? DEJA în sistem!",
                    Duration = 12000,
                    CloseOnClick = true
                });
                Console.WriteLine("Enhanced email fallback notification shown with overlay stopped");
                
                await ShowBusinessErrorDialog(
                    "Email Duplicat", 
                    $"Adresa de email '{_model.Email}' este deja utilizat?.\n\nV? rug?m s? utiliza?i o alt? adres? de email."
                );
            }
            else
            {
                Console.WriteLine("Fallback generic error - STOPPING overlay then showing notification");
                
                // STOP processing overlay FIRST
                _isProcessing = false;
                StateHasChanged();
                await Task.Delay(100);
                
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "?? EROARE LA SALVARE",
                    Detail = "A ap?rut o eroare. Verifica?i datele ?i încerca?i din nou.",
                    Duration = 8000,
                    CloseOnClick = true
                });
                Console.WriteLine("Fallback error notification shown with overlay stopped");
                
                await ShowBusinessErrorDialog("Eroare", $"A ap?rut o eroare la salvare: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in HandleApiError: {ex.Message}");
            
            // STOP processing overlay FIRST
            _isProcessing = false;
            StateHasChanged();
            await Task.Delay(100);
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "?? EROARE NEA?TEPTAT?",
                Detail = "A ap?rut o eroare nea?teptat? la salvare.",
                Duration = 8000,
                CloseOnClick = true
            });
            Console.WriteLine("Unexpected error notification shown with overlay stopped");
            
            await ShowBusinessErrorDialog("Eroare", $"A ap?rut o eroare la salvare: {errorContent}");
        }
    }

    private async Task ShowBusinessErrorDialog(string title, string message)
    {
        Console.WriteLine($"ShowBusinessErrorDialog called with title: '{title}' and message: '{message}'");
        
        // Note: Primary notification is now shown BEFORE this method call, no need for duplicate
        
        try
        {
            // Try Confirm dialog first
            var result = await DialogService.Confirm(
                message,
                title,
                new ConfirmOptions()
                {
                    OkButtonText = "Am în?eles",
                    CancelButtonText = null,
                    CssClass = "business-error-dialog"
                }
            );
            Console.WriteLine("Confirm dialog shown successfully");
        }
        catch (Exception confirmEx)
        {
            Console.WriteLine($"Confirm dialog failed: {confirmEx.Message}, trying Alert...");
            
            try
            {
                // Fallback to Alert
                await DialogService.Alert(
                    message,
                    title,
                    new AlertOptions()
                    {
                        OkButtonText = "Am în?eles",
                        CssClass = "business-error-dialog"
                    }
                );
                Console.WriteLine("Alert dialog shown successfully");
            }
            catch (Exception alertEx)
            {
                Console.WriteLine($"Alert dialog also failed: {alertEx.Message}, trying JavaScript...");
                
                try
                {
                    // JavaScript fallback
                    await JSRuntime.InvokeVoidAsync("alert", $"{title}\n\n{message}");
                    Console.WriteLine("JavaScript alert shown successfully");
                }
                catch (Exception jsEx)
                {
                    Console.WriteLine($"All dialog methods failed. Primary notification was already shown. JS Error: {jsEx.Message}");
                }
            }
        }
    }

    private class ErrorResponse
    {
        public string[]? Errors { get; set; }
        public string? Title { get; set; }
        public int Status { get; set; }
    }
    
    private async Task ShowNotFoundError()
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Warning,
            Summary = "Nu s-a gasit",
            Detail = "Persoana cu ID-ul specificat nu a fost gasita.",
            Duration = 5000
        });
        
        await Task.Delay(2000);
        Navigation.NavigateTo("/administrare/gestionare-persoane");
    }

    private void BackToList()
    {
        Console.WriteLine("BackToList called");
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
        Console.WriteLine("CancelProcessing called - forcing _isProcessing to false");
        _isProcessing = false;
        StateHasChanged();
    }

    private void AutoFillFromCnp(string? cnp)
    {
        if (string.IsNullOrWhiteSpace(cnp)) 
        {
            Console.WriteLine("AutoFillFromCnp: CNP is null or empty");
            return;
        }

        var clean = new string(cnp.Where(char.IsDigit).ToArray());
        Console.WriteLine($"AutoFillFromCnp: Original CNP: '{cnp}', Clean CNP: '{clean}'");
        
        if (!string.Equals(clean, _model.CNP))
        {
            _model.CNP = clean;
            Console.WriteLine($"AutoFillFromCnp: Updated _model.CNP to: '{clean}'");
        }

        if (clean.Length != 13)
        {
            Console.WriteLine($"AutoFillFromCnp: CNP length is {clean.Length}, expecting 13. Skipping auto-fill.");
            return;
        }

        if (!IsValidCnp(clean))
        {
            Console.WriteLine("AutoFillFromCnp: CNP validation failed. Skipping auto-fill.");
            return;
        }

        Console.WriteLine("AutoFillFromCnp: CNP is valid, proceeding with auto-fill...");

        var firstDigit = int.Parse(clean[0].ToString());
        Console.WriteLine($"AutoFillFromCnp: First digit: {firstDigit}");
        
        if (firstDigit is 1 or 3 or 5 or 7)
        {
            _model.Gen = Gen.Masculin;
            Console.WriteLine("AutoFillFromCnp: Set gender to Masculin");
        }
        else if (firstDigit is 2 or 4 or 6 or 8)
        {
            _model.Gen = Gen.Feminin;
            Console.WriteLine("AutoFillFromCnp: Set gender to Feminin");
        }
        else
        {
            _model.Gen = Gen.Neprecizat;
            Console.WriteLine("AutoFillFromCnp: Set gender to Neprecizat");
        }

        try
        {
            var year = int.Parse(clean.Substring(1, 2));
            var month = int.Parse(clean.Substring(3, 2));
            var day = int.Parse(clean.Substring(5, 2));

            Console.WriteLine($"AutoFillFromCnp: Extracted date parts - Year: {year}, Month: {month}, Day: {day}");

            int fullYear;
            if (firstDigit is 1 or 2) 
                fullYear = year + 1900;
            else if (firstDigit is 3 or 4) 
                fullYear = year + 1800;
            else if (firstDigit is 5 or 6 or 7 or 8) 
                fullYear = year + 2000;
            else
                fullYear = year + 1900;

            Console.WriteLine($"AutoFillFromCnp: Full year calculated: {fullYear}");

            var birthDate = new DateTime(fullYear, month, day);
            
            if (birthDate <= DateTime.Today)
            {
                _model.DataNasterii = birthDate;
                Console.WriteLine($"AutoFillFromCnp: Set birth date to: {birthDate:dd.MM.yyyy}");
                
                InvokeAsync(StateHasChanged);
                Console.WriteLine("AutoFillFromCnp: Called StateHasChanged after updating birth date");
            }
            else
            {
                Console.WriteLine("AutoFillFromCnp: Extracted birth date is in the future, skipping automatic fill.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AutoFillFromCnp: Error while extracting date: {ex.Message}");
        }
    }

    private static bool IsValidCnp(string cnp)
    {
        if (cnp.Length != 13 || !cnp.All(char.IsDigit)) 
        {
            Console.WriteLine($"IsValidCnp: Invalid format - Length: {cnp.Length}, All digits: {cnp.All(char.IsDigit)}");
            return false;
        }
        
        var weights = new[] { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
        var sum = 0;
        
        for (int i = 0; i < 12; i++)
        {
            sum += (cnp[i] - '0') * weights[i];
        }
        
        var check = sum % 11;
        if (check == 10) check = 1;
        
        var isValid = check == (cnp[12] - '0');
        Console.WriteLine($"IsValidCnp: CNP '{cnp}' validation result: {isValid} (calculated check: {check}, actual: {cnp[12] - '0'})");
        
        return isValid;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        _isDisposed = true;
        
        try
        {
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
}