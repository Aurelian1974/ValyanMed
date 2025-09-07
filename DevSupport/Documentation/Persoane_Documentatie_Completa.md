# Documenta?ie Complet? - Pagina Persoane

## Informa?ii Generale

**Pagina:** Persoane  
**Loca?ie:** `Client/Pages/Authentication/Persoane.razor`  
**Namespace:** `Client.Pages.Authentication`  
**Framework:** Blazor WebAssembly (.NET 9)  
**UI Framework:** Radzen Blazor  
**Data Creare:** December 2024  
**Status:** Activ? în produc?ie  

---

## 1. Prezentare General?

### 1.1 Scop ?i Func?ionalitate
Pagina **Persoane** este o component? Blazor complex? care gestioneaz? afi?area, c?utarea ?i filtrarea persoanelor din sistemul ValyanMed. Aceasta serve?te ca pagin? principal? de listare ?i management pentru entitatea **Persoana**, oferind func?ionalit??i avansate de c?utare, filtrare ?i paginare.

### 1.2 Caracteristici Principale
- **DataGrid avansat**: Afi?are tabelar? cu sortare, redimensionare ?i reordonare coloane
- **Filtrare multi-criteriu**: C?utare global?, filtrare dup? jude?, localitate ?i status
- **Dropdown-uri cascade**: Jude? ? Localitate cu date din baza de date
- **Paginare flexibil?**: Selec?ie dimensiune pagin? (5, 10, 20, 50, 100)
- **Ac?iuni integrate**: Vizualizare ?i editare persoane
- **Persisten?? set?ri**: Salvare automat? preferin?e grid
- **Responsive design**: Adaptare automat? la dimensiuni diferite de ecran
- **Real-time feedback**: Loading states ?i notific?ri interactive

### 1.3 Integrare în Sistem
```
???????????????????????????????????????
?          Sistemul ValyanMed         ?
???????????????????????????????????????
? Administrare                        ?
? ??? Gestionare Persoane            ?
?     ??? Lista Persoane ??????????? ACEAST? PAGIN?
?     ??? Ad?ugare Persoan?          ?
?     ??? Editare Persoan?           ?
?     ??? Verificare Persoan?        ?
???????????????????????????????????????
```

---

## 2. Structura Tehnic?

### 2.1 Fi?iere Componente
```
Client/Pages/Authentication/
??? Persoane.razor                # Template UI
??? Persoane.razor.cs             # Logic C#
??? (css integrat în app.css)
```

### 2.2 Dependin?e Externe
```csharp
// Framework Dependencies
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using Radzen.Blazor;

// Application Dependencies  
using global::Shared.DTOs.Authentication;
using global::Shared.Common;
using global::Shared.DTOs.Common;
using Client.Services;
using Client.Services.Common;
```

### 2.3 Servicii Injectate
| Serviciu | Tip | Scop |
|----------|-----|------|
| `HttpClient` | Http | Comunicare cu API-ul |
| `NotificationService` | Radzen | Afi?are notific?ri |
| `NavigationManager` | Blazor | Navigare între pagini |
| `IDataGridSettingsService` | Custom | Persisten?? set?ri grid |
| `ILocationApiService` | Custom | Gestionare jude?e/localit??i |

---

## 3. Rutare ?i Parametri

### 3.1 Rut? Definit?
```csharp
@page "/administrare/persoane"
```

### 3.2 Parametri
Pagina nu prime?te parametri URL, fiind o pagin? de listare principal?.

---

## 4. Model de Date

### 4.1 Modele Principale
```csharp
// Query pentru c?utare ?i filtrare
private PersoanaSearchQuery _searchQuery = new() { PageSize = 10 };

// Date grid
private IEnumerable<PersoanaListDto> _data = new List<PersoanaListDto>();
private int _totalCount = 0;

// Op?iuni dropdown
private List<JudetDto> _judeteOptions = new();
private List<LocalitateDto> _localitatiOptions = new();
```

### 4.2 Structura PersoanaSearchQuery
```csharp
public record PersoanaSearchQuery
{
    public string? Search { get; set; }        // C?utare global?
    public string? Judet { get; set; }         // Filtru jude?
    public string? Localitate { get; set; }    // Filtru localitate
    public bool? EsteActiv { get; set; }       // Filtru status
    public int Page { get; set; } = 1;         // Pagina curent?
    public int PageSize { get; set; } = 10;    // Dimensiune pagin?
    public string? Sort { get; set; }          // Sortare
}
```

### 4.3 Structura PersoanaListDto
```csharp
public record PersoanaListDto
{
    public int Id { get; init; }
    public string Nume { get; init; } = string.Empty;
    public string Prenume { get; init; } = string.Empty;
    public string NumeComplet { get; init; } = string.Empty;
    public string? CNP { get; init; }
    public DateTime? DataNasterii { get; init; }
    public int Varsta { get; init; }
    public string? Gen { get; init; }
    public string? Telefon { get; init; }
    public string? Email { get; init; }
    public string? Judet { get; init; }
    public string? Localitate { get; init; }
    public string? Adresa { get; init; }
    public bool EsteActiva { get; init; }
    public string StatusText => EsteActiva ? "Activa" : "Inactiva";
    public DateTime? DataCreare { get; init; }
}
```

### 4.4 Variabile de Stare
```csharp
private bool _isLoading = false;                    // Loading state
private Timer? _searchTimer;                        // Debounce timer
private DataGridSettings? _gridSettings;            // Set?ri grid
private readonly int[] _pageSizeOptions = { 5, 10, 20, 50, 100 }; // Op?iuni pagin?
```

---

## 5. Func?ionalit??i Principale

### 5.1 C?utare ?i Filtrare

#### 5.1.1 C?utare Global? cu Debounce
```csharp
public async Task OnSearchInput(ChangeEventArgs args)
{
    var searchValue = args.Value?.ToString() ?? string.Empty;
    _searchQuery.Search = searchValue.Trim();
    await DelayedSearch();
}

private async Task DelayedSearch()
{
    _searchTimer?.Dispose();
    _searchTimer = new Timer(async _ =>
    {
        _searchQuery.Page = 1; // Reset la prima pagin?
        await InvokeAsync(async () => 
        {
            if (_dataGrid != null)
                await _dataGrid.Reload();
            StateHasChanged();
        });
    }, null, 300, Timeout.Infinite); // 300ms debounce
}
```

#### 5.1.2 Filtrare dup? Jude? ?i Localitate (Cascade)
```csharp
private async Task OnJudetChanged(string? selectedJudet)
{
    // Reset localitate când se schimb? jude?ul
    if (_searchQuery.Judet != selectedJudet)
    {
        _searchQuery.Localitate = null;
    }
    
    _searchQuery.Judet = selectedJudet;
    
    // Înc?rcare localit??i pentru jude?ul selectat
    if (!string.IsNullOrEmpty(selectedJudet))
    {
        _localitatiOptions = await LocationService.GetLocalitatiByJudetAsync(selectedJudet);
    }
    else
    {
        _localitatiOptions = new List<LocalitateDto>();
    }
    
    await OnFilterChanged();
}
```

#### 5.1.3 Filtrare dup? Status
```csharp
private readonly List<DropDownOption> _statusOptions = new()
{
    new DropDownOption { Text = "Activa", Value = true },
    new DropDownOption { Text = "Inactiva", Value = false }
};

private async Task OnStatusChanged()
{
    Console.WriteLine($"[DEBUG] Status changed to: {_searchQuery.EsteActiv}");
    await OnFilterChanged();
}
```

### 5.2 Paginare ?i Dimensiune Pagin?

#### 5.2.1 Gestionare Dimensiune Pagin?
```csharp
private async Task OnPageSizeChanged()
{
    Console.WriteLine($"[DEBUG] Page size changed to: {_searchQuery.PageSize}");
    _searchQuery.Page = 1; // Reset la prima pagin?
    await OnFilterChanged();
}
```

#### 5.2.2 Procesare LoadData Arguments
```csharp
public async Task LoadDataAsync(Radzen.LoadDataArgs args)
{
    if (args != null)
    {
        _searchQuery.Page = ((args.Skip ?? 0) / (args.Top ?? _searchQuery.PageSize)) + 1;
        
        // Detectare schimbare dimensiune pagin?
        if (args.Top.HasValue && args.Top.Value != _searchQuery.PageSize)
        {
            _searchQuery.PageSize = args.Top.Value;
        }
        
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            _searchQuery.Sort = args.OrderBy.ToLower();
        }
    }
    
    // ... logica de înc?rcare date
}
```

### 5.3 Persisten?a Set?rilor Grid

#### 5.3.1 Înc?rcare Set?ri
```csharp
private async Task LoadGridSettings()
{
    try
    {
        _gridSettings = await DataGridSettingsService.LoadSettingsAsync(GRID_SETTINGS_KEY) 
                       ?? new DataGridSettings();
        DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, _gridSettings);
    }
    catch
    {
        _gridSettings = new DataGridSettings();
    }
}
```

#### 5.3.2 Salvare Set?ri
```csharp
public async Task OnSettingsChanged(DataGridSettings settings)
{
    _gridSettings = settings;
    try
    {
        await DataGridSettingsService.SaveSettingsAsync(GRID_SETTINGS_KEY, settings);
    }
    catch
    {
        DataGridSettingsService.SetFallbackSettings(GRID_SETTINGS_KEY, settings);
    }
}
```

---

## 6. Interfa?a Utilizator

### 6.1 Structura Layout-ului

```
???????????????????????????????????????????????????????????????
? Header - Titlu Pagin?                                      ?
???????????????????????????????????????????????????????????????
? Search and Filters Card                                    ?
? ??????????????????????????????????????????????????????????? ?
? ? [C?utare Global?] [Jude? ?] [Localitate ?] [Status ?] ? ?
? ? [Reseteaz? Filtre] [Reset Grid]    Înreg/pag: [10 ?]  ? ?
? ??????????????????????????????????????????????????????????? ?
???????????????????????????????????????????????????????????????
? Data Grid Card                                             ?
? ??????????????????????????????????????????????????????????? ?
? ? ?Nume?CNP?Data?Vârst??Gen?Telefon?Email?Jude??Status?Ac?iuni? ?
? ? ???????????????????????????????????????????????????????????? ?
? ? ?...?...?... ? ...  ?...?  ...  ? ... ? ... ? ...  ? [...] ? ?
? ? ???????????????????????????????????????????????????????????? ?
? ? [<] [1] [2] [3] [>] ? [10 ?] ? Pagina 1 din 5 (47 înreg.) ? ?
? ??????????????????????????????????????????????????????????? ?
???????????????????????????????????????????????????????????????
```

### 6.2 Sec?iuni Interface

#### 6.2.1 Zon? Filtrare
- **C?utare global?** (nume, prenume, email, telefon, CNP)
- **Dropdown Jude?** (cu c?utare, date din BD)
- **Dropdown Localitate** (cascade, dependent de jude?)
- **Dropdown Status** (Activa/Inactiva)

#### 6.2.2 Zona Controale
- **Buton "Reseteaz? filtre"** (Warning style, elimin? toate filtrele)
- **Buton "Reset Grid"** (Secondary style, reseteaz? set?rile grid)
- **Selector dimensiune pagin?** (5, 10, 20, 50, 100)

#### 6.2.3 DataGrid
- **Coloane sortabile**: Nume complet, CNP, Data na?terii, etc.
- **Coloana Status**: Badge colorat (verde/ro?u)
- **Coloana Ac?iuni**: Frozen la dreapta cu butoane View/Edit
- **Detalii expandabile**: Click pe rând pentru informa?ii complete

### 6.3 St?ri Interfa??

#### 6.3.1 Loading State
```razor
@if (_isLoading)
{
    <RadzenAlert AlertStyle="AlertStyle.Info" Variant="Radzen.Variant.Filled">
        Se încarc? datele...
    </RadzenAlert>
}
```

#### 6.3.2 Badge-uri Status
```razor
<!-- În DataGrid -->
<RadzenBadge Text="@persoana.StatusText" 
             BadgeStyle="@(persoana.EsteActiva ? Radzen.BadgeStyle.Success : Radzen.BadgeStyle.Danger)" 
             Variant="Radzen.Variant.Filled" />

<!-- În detalii expandabile -->
<RadzenBadge Text="@persoana.StatusText" 
             BadgeStyle="@(persoana.EsteActiva ? Radzen.BadgeStyle.Success : Radzen.BadgeStyle.Danger)" 
             Variant="Radzen.Variant.Filled" />
```

---

## 7. Logica de Business

### 7.1 Ciclul de Via?? al Componentei

```csharp
protected override async Task OnInitializedAsync()
{
    await LoadGridSettings();        // Înc?rcare set?ri grid
    await LoadJudeteAsync();         // Înc?rcare jude?e din BD
    await LoadDataAsync(new Radzen.LoadDataArgs()); // Înc?rcare date ini?iale
}
```

### 7.2 Construirea Query String-ului

```csharp
private string BuildQueryString()
{
    var queryParams = new List<string>
    {
        $"Page={_searchQuery.Page}",
        $"PageSize={_searchQuery.PageSize}"
    };

    if (!string.IsNullOrWhiteSpace(_searchQuery.Search))
        queryParams.Add($"Search={Uri.EscapeDataString(_searchQuery.Search)}");

    if (!string.IsNullOrWhiteSpace(_searchQuery.Judet))
        queryParams.Add($"Judet={Uri.EscapeDataString(_searchQuery.Judet)}");

    if (!string.IsNullOrWhiteSpace(_searchQuery.Localitate))
        queryParams.Add($"Localitate={Uri.EscapeDataString(_searchQuery.Localitate)}");

    if (_searchQuery.EsteActiv.HasValue)
        queryParams.Add($"EsteActiv={_searchQuery.EsteActiv.Value}");

    if (!string.IsNullOrWhiteSpace(_searchQuery.Sort))
        queryParams.Add($"Sort={_searchQuery.Sort}");

    return queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
}
```

### 7.3 Gestionarea Erorilor

```csharp
private void ShowErrorNotification(string message)
{
    NotificationService.Notify(new Radzen.NotificationMessage
    {
        Severity = Radzen.NotificationSeverity.Error,
        Summary = "Eroare",
        Detail = message,
        Duration = 4000
    });
}
```

---

## 8. Integrare API

### 8.1 Endpoint-uri Utilizate

| Opera?iune | Method | Endpoint | Scop |
|------------|---------|----------|------|
| Listare | GET | `/api/Persoane` | Înc?rcare list? paginat? cu filtrare |
| Jude?e | GET | `/api/Location/judete` | Înc?rcare jude?e pentru dropdown |
| Localit??i | GET | `/api/Location/localitati/judet/{judet}` | Înc?rcare localit??i pentru jude? |

### 8.2 Modele Request/Response

#### 8.2.1 Request Parameters
```
GET /api/Persoane?Page=1&PageSize=10&Search=ion&Judet=Alba&Localitate=Abrud&EsteActiv=true&Sort=nume
```

#### 8.2.2 PagedResult Response
```json
{
  "items": [
    {
      "id": 123,
      "nume": "Popescu",
      "prenume": "Ion",
      "numeComplet": "Popescu Ion",
      "cnp": "1234567890123",
      "dataNasterii": "1990-01-15T00:00:00",
      "varsta": 34,
      "gen": "Masculin",
      "telefon": "0721123456",
      "email": "ion.popescu@email.com",
      "judet": "Alba",
      "localitate": "Abrud",
      "adresa": "Str. Principal? nr. 123, Abrud jud. Alba",
      "esteActiva": true,
      "statusText": "Activa",
      "dataCreare": "2024-01-01T10:00:00"
    }
  ],
  "totalCount": 47,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

---

## 9. Stilizare ?i CSS

### 9.1 Clase CSS Principale

```css
/* Layout complet */
.p-4.compact {
    padding: 1rem;
}

/* Card-uri */
.mb-4 {
    margin-bottom: 1.5rem;
}

/* Grid frozen column */
.rz-frozen-column {
    position: sticky;
    right: 0;
    background: white;
    z-index: 10;
}

/* Badge-uri status */
.rz-badge-success {
    background-color: #28a745;
    color: white;
}

.rz-badge-danger {
    background-color: #dc3545;
    color: white;
}
```

### 9.2 Responsive Design

```css
/* Desktop */
@media (min-width: 768px) {
    .RadzenColumn[SizeMD="4"] {
        grid-column: span 4;
    }
    
    .RadzenColumn[SizeMD="3"] {
        grid-column: span 3;
    }
    
    .RadzenColumn[SizeMD="2"] {
        grid-column: span 2;
    }
}

/* Mobile */
@media (max-width: 767px) {
    .RadzenColumn[Size="12"] {
        grid-column: span 12;
    }
    
    .RadzenDataGrid {
        font-size: 0.875rem;
    }
}
```

---

## 10. Performance ?i Optimiz?ri

### 10.1 Debounced Search

```csharp
// Evit? request-uri excesive la tastare
private async Task DelayedSearch()
{
    _searchTimer?.Dispose();
    _searchTimer = new Timer(async _ => {
        // Execut? c?utarea dup? 300ms de inactivitate
    }, null, 300, Timeout.Infinite);
}
```

### 10.2 Lazy Loading ?i Paginare

```csharp
// Încarc? doar datele necesare pentru pagina curent?
_searchQuery.Page = ((args.Skip ?? 0) / _searchQuery.PageSize) + 1;
```

### 10.3 Cleanup Resources

```csharp
public void Dispose()
{
    _searchTimer?.Dispose();
}
```

---

## 11. Testing ?i Debugging

### 11.1 Console Logging

```csharp
// Debug pentru filtrare
Console.WriteLine($"[DEBUG] Status changed to: {_searchQuery.EsteActiv}");
Console.WriteLine($"[DEBUG] Page size changed to: {_searchQuery.PageSize}");
Console.WriteLine($"[DEBUG] Making API call to: api/Persoane{queryString}");

// Debug pentru rezultate
Console.WriteLine($"[DEBUG] Received {_data.Count()} items, total count: {_totalCount}");
```

### 11.2 Error Tracking

```csharp
catch (HttpRequestException ex)
{
    Console.WriteLine($"[DEBUG] HTTP Exception: {ex.Message}");
    ShowErrorNotification("Nu se poate conecta la server");
}
catch (Exception ex)
{
    Console.WriteLine($"[DEBUG] General Exception: {ex.Message}");
    ShowErrorNotification($"Eroare nespecificat?: {ex.Message}");
}
```

---

## 12. Configura?ie ?i Set?ri

### 12.1 Constante ?i Configur?ri

```csharp
// Cheie pentru persisten?a set?rilor
private const string GRID_SETTINGS_KEY = "persoane_grid_settings";

// Op?iuni dimensiune pagin?
private readonly int[] _pageSizeOptions = new[] { 5, 10, 20, 50, 100 };

// Timeout debounce search
private const int SEARCH_DEBOUNCE_MS = 300;
```

### 12.2 Grid Settings

```json
{
  "columns": [
    {
      "property": "NumeComplet",
      "width": "200px",
      "visible": true,
      "sortDirection": "asc"
    }
  ],
  "pageSize": 10,
  "density": "Compact"
}
```

---

## 13. Securitate ?i Validare

### 13.1 Sanitizare Input

```csharp
// Escape pentru query parameters
if (!string.IsNullOrWhiteSpace(_searchQuery.Search))
    queryParams.Add($"Search={Uri.EscapeDataString(_searchQuery.Search)}");
```

### 13.2 Protec?ii Client-Side

- **XSS Prevention**: Escapare automat? în Blazor
- **Input Validation**: Verific?ri de lungime ?i format
- **Error Boundaries**: Gestionare gracioas? a erorilor

---

## 14. Evolu?ie ?i Manutenabilitate

### 14.1 Istoric Modific?ri

| Data | Versiune | Modific?ri |
|------|----------|------------|
| Dec 2024 | 1.0 | Implementare ini?ial? |
| Dec 2024 | 1.1 | Ad?ugare dropdown-uri cascade |
| Dec 2024 | 1.2 | Implementare badge-uri status |
| Dec 2024 | 1.3 | Optimizare paginare ?i filtrare |
| Dec 2024 | 1.4 | Finalizare frozen columns ?i UX |

### 14.2 Roadmap Func?ionalit??i

#### **Versiunea 2.0 (Q1 2025)**
- [ ] Export Excel/PDF
- [ ] Filtrare avansat? multi-select
- [ ] Grupare dinamic? pe coloane
- [ ] C?utare fuzzy ?i autocomplete

#### **Versiunea 2.1 (Q2 2025)**
- [ ] Bulk operations (select multiple)
- [ ] Column hiding/showing UI
- [ ] Saved search filters
- [ ] Real-time updates via SignalR

### 14.3 Dependin?e de Actualizat

```json
{
  "radzen.blazor": "^5.5.4",
  "microsoft.aspnetcore.components.webassembly": "^9.0.0",
  "system.text.json": "^9.0.0"
}
```

---

## 15. Troubleshooting

### 15.1 Probleme Comune

#### **Filtrarea nu func?ioneaz?**
```csharp
// Verific? parametrii în browser console
Console.WriteLine($"[DEBUG] Final query string: {queryString}");

// Verific? r?spunsul API
Console.WriteLine($"[DEBUG] API Error: {response.StatusCode}");
```

#### **Dropdown-urile nu se încarc?**
```csharp
// Verific? serviciul de loca?ii
if (LocationService == null) 
{
    Console.WriteLine("LocationService is null - check DI registration");
}

// Verific? înc?rcarea jude?elor
var judete = await LocationService.GetJudeteAsync();
Console.WriteLine($"Loaded {judete.Count} counties");
```

#### **Grid-ul nu se reîncarc?**
```csharp
// For?eaz? reload
if (_dataGrid != null)
    await _dataGrid.Reload();
else
    await LoadDataAsync(new Radzen.LoadDataArgs());
```

### 15.2 Performance Issues

#### **Înc?rcare lent?**
```csharp
// M?surare timpi de r?spuns
var stopwatch = Stopwatch.StartNew();
var response = await Http.GetAsync(url);
Console.WriteLine($"API call took {stopwatch.ElapsedMilliseconds}ms");
```

#### **Memory leaks**
```csharp
// Verific? disposal corect
public void Dispose()
{
    _searchTimer?.Dispose();
    _judeteOptions?.Clear();
    _localitatiOptions?.Clear();
}
```

---

## 16. Contact ?i Suport

### 16.1 Echipa de Dezvoltare
- **Lead Developer**: Aurelian Iancu
- **UI/UX Designer**: [TBD]
- **Tester**: [TBD]

### 16.2 Documenta?ie Adi?ional?
- [Radzen Blazor Documentation](https://blazor.radzen.com/)
- [Blazor WebAssembly Guide](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [DataGrid Advanced Features](https://blazor.radzen.com/datagrid)

### 16.3 Issue Tracking
Pentru raportarea problemelor sau solicitarea de func?ionalit??i noi:
1. Crea?i un issue în GitHub repository
2. Include?i informa?ii complete despre browser ?i versiune
3. Ad?uga?i screenshot-uri sau video pentru probleme UI
4. Specifica?i pa?ii pentru reproducerea problemei

---

**Ultima actualizare:** December 2024  
**Versiunea documenta?ie:** 1.0  
**Status:** Complet? ?i validat?  

---

*Aceast? documenta?ie este men?inut? ?i actualizat? constant pentru a reflecta cel mai recent status al paginii Persoane din sistemul ValyanMed.*