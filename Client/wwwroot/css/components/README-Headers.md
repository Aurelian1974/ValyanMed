# Header-uri Globale - Ghid de Folosire

## ?? **Conceptul**

Am standardizat stilurile pentru header-uri în toat? aplica?ia folosind clasele CSS globale. Toate header-urile au acum acela?i aspect consistent ?i profesional.

## ?? **Tipuri de Header-uri Disponibile**

### **1. Header Standard (Albastru)**
```razor
<RadzenCard Class="page-header-card">
    <RadzenStack Orientation="Radzen.Orientation.Horizontal" AlignItems="Radzen.AlignItems.Center" Gap="1rem">
        <RadzenIcon Icon="dashboard" Class="page-header-icon" />
        <RadzenText TextStyle="TextStyle.H4" Class="page-header-title">
            Titlu Pagina
        </RadzenText>
    </RadzenStack>
</RadzenCard>
```

### **2. Header pentru Vizualizare (Mov)**
```razor
<RadzenCard Class="page-header-card view-mode">
    <RadzenStack Orientation="Radzen.Orientation.Horizontal" AlignItems="Radzen.AlignItems.Center" Gap="1rem">
        <RadzenIcon Icon="visibility" Class="page-header-icon view-mode" />
        <RadzenText TextStyle="TextStyle.H4" Class="page-header-title view-mode">
            Vizualizare Element
        </RadzenText>
    </RadzenStack>
</RadzenCard>
```

### **3. Header pentru Liste (Verde)**
```razor
<RadzenCard Class="page-header-card list-mode">
    <RadzenStack Orientation="Radzen.Orientation.Horizontal" AlignItems="Radzen.AlignItems.Center" Gap="1rem">
        <RadzenIcon Icon="list" Class="page-header-icon list-mode" />
        <RadzenText TextStyle="TextStyle.H4" Class="page-header-title list-mode">
            Lista Elemente
        </RadzenText>
    </RadzenStack>
</RadzenCard>
```

### **4. Header pentru Editare (Portocaliu)**
```razor
<RadzenCard Class="page-header-card edit-mode">
    <RadzenStack Orientation="Radzen.Orientation.Horizontal" AlignItems="Radzen.AlignItems.Center" Gap="1rem">
        <RadzenIcon Icon="edit" Class="page-header-icon edit-mode" />
        <RadzenText TextStyle="TextStyle.H4" Class="page-header-title edit-mode">
            Editare Element
        </RadzenText>
    </RadzenStack>
</RadzenCard>
```

## ?? **Clasele CSS**

### **Pentru Card:**
- `page-header-card` - standard (albastru)
- `page-header-card view-mode` - vizualizare (mov)
- `page-header-card list-mode` - liste (verde)
- `page-header-card edit-mode` - editare (portocaliu)

### **Pentru Icon:**
- `page-header-icon` - standard
- `page-header-icon view-mode` - vizualizare
- `page-header-icon list-mode` - liste
- `page-header-icon edit-mode` - editare

### **Pentru Titlu:**
- `page-header-title` - standard
- `page-header-title view-mode` - vizualizare
- `page-header-title list-mode` - liste
- `page-header-title edit-mode` - editare

## ?? **Fi?iere Afectate**

### **CSS:**
- `Client/wwwroot/css/components/headers.css` - defini?iile claselor
- `Client/wwwroot/css/app.css` - import-ul
- `Client/wwwroot/css/pages/adauga-personal-medical.css` - cur??at

### **Razor Pages Actualizate:**
- `Client/Pages/Medical/AdaugEditezPersonalMedical.razor`
- `Client/Pages/Medical/PersonalMedicalView.razor`
- `Client/Pages/Medical/GestionarePersonal.razor`

## ? **Avantaje**

1. **Consisten??** - toate header-urile arat? la fel
2. **Performan??** - un singur CSS global
3. **Mentenabilitate** - o singur? modificare pentru toate paginile
4. **Siguran?? tipului** - clasele sunt standardizate

## ?? **NU Mai Folosi?i**

```razor
<!-- ? NU folosi?i stiluri inline -->
<RadzenCard Style="background: linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%); border: 1px solid #2196f3; box-shadow: 0 4px 8px rgba(33, 150, 243, 0.2);">

<!-- ? NU crea?i CSS-uri custom pentru header-uri -->
.my-custom-header { ... }
```

## ? **Folosi?i**

```razor
<!-- ? Folosi?i clasele globale -->
<RadzenCard Class="page-header-card">
<RadzenCard Class="page-header-card view-mode">
<RadzenCard Class="page-header-card list-mode">
<RadzenCard Class="page-header-card edit-mode">
```

## ?? **Când S? Folosi?i Fiecare Tip**

- **Standard (albastru)**: pagini generale, dashboard-uri
- **View Mode (mov)**: pagini de vizualizare, detalii
- **List Mode (verde)**: pagini cu liste, tabele, grids
- **Edit Mode (portocaliu)**: pagini de ad?ugare/editare

## ?? **Exemplu Complet**

```razor
@page "/medical/exemplu"

<PageTitle>Exemplu Header - ValyanMed</PageTitle>

<!-- Header pentru pagin? de editare -->
<RadzenCard Class="page-header-card edit-mode">
    <RadzenStack Orientation="Radzen.Orientation.Horizontal" AlignItems="Radzen.AlignItems.Center" Gap="1rem">
        <RadzenIcon Icon="person_add" Class="page-header-icon edit-mode" />
        <RadzenText TextStyle="TextStyle.H4" Class="page-header-title edit-mode">
            Adauga Utilizator Nou
        </RadzenText>
    </RadzenStack>
</RadzenCard>

<!-- Restul con?inutului paginii -->
<RadzenCard>
    <!-- Form sau alt con?inut -->
</RadzenCard>
```

---

**Conform Plan_refactoring.txt**: Maxim 2 culori, CSS minimal, f?r? override-uri stufoase, Radzen native pe cât posibil.