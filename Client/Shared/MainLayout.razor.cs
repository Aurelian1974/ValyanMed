using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using System.ComponentModel;
using Shared.DTOs.Authentication;
using Client.Services.Authentication;

namespace Client.Shared;

public partial class MainLayout : LayoutComponentBase, IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private IAuthenticationStateService AuthStateService { get; set; } = null!;

    private List<MenuModel> menuData = new();
    private string searchTerm = string.Empty;
    private bool isDarkMode = false;
    private AuthenticationResponse? currentUser;
    private string userDisplayName = "Utilizator";
    private string userRole = "Guest";
    
    // Simple dropdown state
    private bool showUserMenu = false;
    
    // Breadcrumbs dynamic
    private string currentPageTitle = "Dashboard";
    private string currentPageIcon = "dashboard";
    private string currentPagePath = "/dashboard";
    private string? parentPageTitle = null;
    private string? parentPagePath = null;

    protected override async Task OnInitializedAsync()
    {
        InitializeMenu();
        await LoadCurrentUserAsync();
        
        // Subscribe to authentication state changes
        AuthStateService.OnAuthenticationStateChanged += OnAuthenticationStateChanged;
        
        // Subscribe to navigation changes
        NavigationManager.LocationChanged += OnLocationChanged;
        
        // Set initial breadcrumb
        UpdateBreadcrumb(NavigationManager.Uri);
    }

    #region User Management
    public async Task RefreshCurrentUserAsync()
    {
        await LoadCurrentUserAsync();
    }

    private async Task OnAvatarClick(MouseEventArgs e)
    {
        showUserMenu = !showUserMenu;
        StateHasChanged();
    }

    private void CloseUserMenu()
    {
        showUserMenu = false;
        StateHasChanged();
    }

    private void NavigateToProfile()
    {
        showUserMenu = false;
        NavigationManager.NavigateTo("/profil");
    }

    private void NavigateToSettings()
    {
        showUserMenu = false;
        NavigationManager.NavigateTo("/setari");
    }

    private void NavigateToPreferences()
    {
        showUserMenu = false;
        NavigationManager.NavigateTo("/preferinte");
    }

    private void NavigateToHelp()
    {
        showUserMenu = false;
        NavigationManager.NavigateTo("/ajutor");
    }

    private string GetUserInitials()
    {
        if (currentUser != null)
        {
            var names = currentUser.NumeComplet.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return names.Length >= 2 
                ? $"{names[0][0]}{names[1][0]}"
                : names.Length == 1 
                    ? $"{names[0][0]}{(names[0].Length > 1 ? names[0][1] : 'X')}"
                    : "XX";
        }
        
        return "XX";
    }

    private async Task LoadCurrentUserAsync()
    {
        try
        {
            currentUser = await AuthStateService.GetCurrentUserAsync();
            UpdateUserDisplayInfo();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading current user: {ex.Message}");
            userDisplayName = "Utilizator";
            userRole = "Guest";
        }
    }

    private void UpdateUserDisplayInfo()
    {
        if (currentUser != null)
        {
            userDisplayName = currentUser.NumeComplet;
            userRole = DetermineUserRole();
        }
        else
        {
            userDisplayName = "Utilizator";
            userRole = "Guest";
        }
        
        StateHasChanged();
    }

    private string DetermineUserRole()
    {
        if (currentUser?.NumeComplet.StartsWith("Dr.") == true)
            return "Doctor";
        else if (currentUser?.Email.Contains("admin") == true)
            return "Administrator";
        else
            return "Personal Medical";
    }

    private void OnAuthenticationStateChanged()
    {
        InvokeAsync(async () =>
        {
            await LoadCurrentUserAsync();
        });
    }
    #endregion

    #region Disabled Features (For future implementation)
    /*
    #region Search Functionality
    private async Task OnGlobalSearch()
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return;

        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "C?utare",
            Detail = $"Se caut?: {searchTerm}",
            Duration = 3000
        });

        NavigationManager.NavigateTo($"/search?q={Uri.EscapeDataString(searchTerm)}");
    }

    private async Task OnSearchKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await OnGlobalSearch();
        }
    }
    #endregion

    #region Notifications
    private async Task ShowNotifications()
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Notific?ri",
            Detail = "Ave?i 3 notific?ri noi",
            Duration = 4000
        });
    }
    #endregion

    #region Quick Actions
    private async Task ShowQuickActions()
    {
        NavigationManager.NavigateTo("/medical/pacienti/nou");
    }

    private async Task OnQuickActionSelect(string action)
    {
        switch (action)
        {
            case "add-patient":
                NavigationManager.NavigateTo("/medical/pacienti/nou");
                break;
            case "schedule-consultation":
                NavigationManager.NavigateTo("/medical/programari/noua");
                break;
            case "manage-medication":
                NavigationManager.NavigateTo("/farmacie/medicamente");
                break;
            case "view-reports":
                NavigationManager.NavigateTo("/medical/rapoarte");
                break;
            case "add-staff":
                NavigationManager.NavigateTo("/medical/personal/nou");
                break;
            default:
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Ac?iune rapid?",
                    Detail = $"Ac?iunea {action} va fi disponibil? în curând",
                    Duration = 3000
                });
                break;
        }
    }
    #endregion

    #region Theme Management
    private async Task ToggleTheme()
    {
        isDarkMode = !isDarkMode;
        
        try
        {
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "theme", isDarkMode ? "dark" : "light");
            
            var script = isDarkMode 
                ? "document.body.classList.add('dark-theme'); document.body.classList.remove('light-theme');"
                : "document.body.classList.add('light-theme'); document.body.classList.remove('dark-theme');";
                
            await JSRuntime.InvokeVoidAsync("eval", script);
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Tem? schimbat?",
                Detail = $"Activat modul {(isDarkMode ? "întunecat" : "luminos")}",
                Duration = 2000
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Theme toggle error: {ex.Message}");
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Tem?",
                Detail = $"Modul {(isDarkMode ? "întunecat" : "luminos")} va fi disponibil în curând",
                Duration = 2000
            });
        }
        
        StateHasChanged();
    }
    #endregion
    */
    #endregion

    #region Menu Logic
    private void InitializeMenu()
    {
        menuData = new List<MenuModel>
        {
            new MenuModel(() => menuData)
            {
                Text = "Dashboard Medical",
                Icon = "dashboard",
                Click = EventCallback.Factory.Create(this, NavigateToDashboard)
            },
            new MenuModel(() => menuData)
            {
                Text = "Sistemul Medical",
                Icon = "medical_services",
                Expanded = true,
                Items = new List<MenuModel>
                {
                    new MenuModel(() => menuData)
                    {
                        Text = "Programari si Inregistrare",
                        Icon = "event_available",
                        Items = new List<MenuModel>
                        {
                            new MenuModel(() => menuData) { Text = "Programari", Icon = "event", Path = "/medical/programari" },
                            new MenuModel(() => menuData) { Text = "Pacienti", Icon = "people", Path = "/medical/pacienti" },
                            new MenuModel(() => menuData) { Text = "Cautare pacienti", Icon = "search", Path = "/medical/pacienti/cautare" },
                            new MenuModel(() => menuData) { Text = "Verificare asigurare", Icon = "verified_user", Path = "/medical/verificare-asigurare" },
                            new MenuModel(() => menuData) { Text = "Formulare consimtamant", Icon = "assignment", Path = "/medical/formulare-consimtamant" }
                        }
                    },
                    new MenuModel(() => menuData)
                    {
                        Text = "Triaj si Evaluare",
                        Icon = "priority_high",
                        Items = new List<MenuModel>
                        {
                            new MenuModel(() => menuData) { Text = "Triaj pacienti", Icon = "assignment", Path = "/medical/triaj" },
                            new MenuModel(() => menuData) { Text = "Semne vitale", Icon = "monitor_heart", Path = "/medical/semne-vitale" },
                            new MenuModel(() => menuData) { Text = "Chestionar simptome", Icon = "quiz", Path = "/medical/chestionar-simptome" },
                            new MenuModel(() => menuData) { Text = "Clasificare prioritate", Icon = "flag", Path = "/medical/clasificare-prioritate" }
                        }
                    },
                    new MenuModel(() => menuData)
                    {
                        Text = "Consultatie Medicala",
                        Icon = "medical_services",
                        Items = new List<MenuModel>
                        {
                            new MenuModel(() => menuData) { Text = "Consultatii", Icon = "medical_services", Path = "/medical/consultatii" },
                            new MenuModel(() => menuData) { Text = "Istoric medical", Icon = "history", Path = "/medical/istoric-medical" },
                            new MenuModel(() => menuData) { Text = "Examinare clinica", Icon = "person_search", Path = "/medical/examinare-clinica" },
                            new MenuModel(() => menuData) { Text = "Diagnostic preliminar", Icon = "medical_services", Path = "/medical/diagnostic-preliminar" },
                            new MenuModel(() => menuData) { Text = "Prescriptii", Icon = "local_pharmacy", Path = "/medical/prescriptii" }
                        }
                    },
                    new MenuModel(() => menuData)
                    {
                        Text = "Investigatii si Analize",
                        Icon = "science",
                        Items = new List<MenuModel>
                        {
                            new MenuModel(() => menuData) { Text = "Analize laborator", Icon = "biotech", Path = "/medical/analize-laborator" },
                            new MenuModel(() => menuData) { Text = "Imagistica", Icon = "medical_information", Path = "/medical/imagistica" },
                            new MenuModel(() => menuData) { Text = "Teste functionale", Icon = "favorite", Path = "/medical/teste-functionale" },
                            new MenuModel(() => menuData) { Text = "Rezultate teste", Icon = "assignment", Path = "/medical/rezultate-teste" },
                            new MenuModel(() => menuData) { Text = "Recoltare probe", Icon = "bloodtype", Path = "/medical/recoltare-probe" }
                        }
                    },
                    new MenuModel(() => menuData)
                    {
                        Text = "Monitorizare si Follow-up",
                        Icon = "insights",
                        Items = new List<MenuModel>
                        {
                            new MenuModel(() => menuData) { Text = "Follow-up", Icon = "schedule", Path = "/medical/follow-up" },
                            new MenuModel(() => menuData) { Text = "Monitorizare tratament", Icon = "monitor_heart", Path = "/medical/monitorizare-tratament" },
                            new MenuModel(() => menuData) { Text = "Comunicare medic familie", Icon = "email", Path = "/medical/comunicare-medic-familie" },
                            new MenuModel(() => menuData) { Text = "Evolutia pacientului", Icon = "trending_up", Path = "/medical/evolutia-pacientului" }
                        }
                    }
                }
            },
            new MenuModel(() => menuData)
            {
                Text = "Farmacie",
                Icon = "local_pharmacy",
                Items = new List<MenuModel>
                {
                    new MenuModel(() => menuData) { Text = "Medicamente", Icon = "medication", Path = "/farmacie/medicamente" },
                    new MenuModel(() => menuData) { Text = "Materiale sanitare", Icon = "medical_services", Path = "/farmacie/materiale-sanitare" },
                    new MenuModel(() => menuData) { Text = "Dispozitive medicale", Icon = "medical_information", Path = "/farmacie/dispozitive-medicale" },
                    new MenuModel(() => menuData) { Text = "Documente intrare", Icon = "input", Path = "/farmacie/documente-intrare" },
                    new MenuModel(() => menuData) { Text = "Documente iesire", Icon = "output", Path = "/farmacie/documente-iesire" }
                }
            },
            new MenuModel(() => menuData)
            {
                Text = "Rapoarte",
                Icon = "assessment",
                Click = EventCallback.Factory.Create(this, NavigateToRapoarte)
            },
            new MenuModel(() => menuData)
            {
                Text = "Setari clinica",
                Icon = "settings",
                Click = EventCallback.Factory.Create(this, NavigateToSetari)
            },
            new MenuModel(() => menuData)
            {
                Text = "Administrare",
                Icon = "admin_panel_settings",
                Items = new List<MenuModel>
                {
                    new MenuModel(() => menuData)
                    {
                        Text = "Persoane",
                        Icon = "people",
                        Items = new List<MenuModel>
                        {
                            new MenuModel(() => menuData) { Text = "Vizualizare persoane", Icon = "group", Path = "/administrare/persoane" },
                            new MenuModel(() => menuData) { Text = "Gestionare persoane", Icon = "manage_accounts", Path = "/administrare/gestionare-persoane" }
                        }
                    },
                    new MenuModel(() => menuData) { Text = "Utilizatori", Icon = "manage_accounts", Path = "/utilizatori" },
                    new MenuModel(() => menuData)
                    {
                        Text = "Personal Medical",
                        Icon = "badge",
                        Items = new List<MenuModel>
                        {
                            new MenuModel(() => menuData) { Text = "Personal medical", Icon = "group", Path = "/medical/personal" },
                            new MenuModel(() => menuData) { Text = "Gestionare personal", Icon = "manage_accounts", Path = "/medical/gestionare-personal" }
                        }
                    },
                    new MenuModel(() => menuData) { Text = "Parteneri", Icon = "business", Path = "/administrare/parteneri" },
                    new MenuModel(() => menuData) { Text = "Roluri", Icon = "group_work", Path = "/administrare/roluri" }
                }
            }
        };
    }

    private async Task Logout()
    {
        showUserMenu = false;
        try
        {
            await JSRuntime.InvokeVoidAsync("appLifecycle.logoutNow", "https://localhost:7294/api/auth/logout", 
                new[] { "valyanmed_auth_token", "valyanmed_user_info", "currentUser" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logout error: {ex.Message}");
        }
        finally
        {
            NavigationManager.NavigateTo("/login", forceLoad: true);
        }
    }

    void NavigateToRapoarte() => NavigationManager.NavigateTo("/medical/rapoarte");
    void NavigateToSetari() => NavigationManager.NavigateTo("/medical/setari-clinica");
    void NavigateToDashboard() => NavigationManager.NavigateTo("/medical/dashboard");
    #endregion

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadCurrentUserAsync();
        }
        
        // Simple styling for user profile section
        await JSRuntime.InvokeVoidAsync("eval", @"
            setTimeout(() => {
                // Style User Profile Section only
                const userProfileSection = document.querySelector('.user-profile-section');
                if (userProfileSection) {
                    console.log('?? Styling user profile section...');
                    
                    userProfileSection.style.setProperty('background', 'rgba(255,255,255,0.08)', 'important');
                    userProfileSection.style.setProperty('border', '1px solid rgba(255,255,255,0.15)', 'important');
                    userProfileSection.style.setProperty('border-radius', '20px', 'important');
                    userProfileSection.style.setProperty('padding', '6px 6px 6px 12px', 'important');
                    userProfileSection.style.setProperty('backdrop-filter', 'blur(10px)', 'important');
                    userProfileSection.style.setProperty('box-shadow', '0 2px 8px rgba(0,0,0,0.1)', 'important');
                    
                    console.log('? User profile section styled!');
                } else {
                    console.log('? User profile section not found');
                }
            }, 50);
        ");
    }

    public void Dispose()
    {
        AuthStateService.OnAuthenticationStateChanged -= OnAuthenticationStateChanged;
        NavigationManager.LocationChanged -= OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        UpdateBreadcrumb(e.Location);
        InvokeAsync(StateHasChanged);
    }

    private void UpdateBreadcrumb(string uri)
    {
        var relativePath = NavigationManager.ToBaseRelativePath(uri).ToLower();
        
        Console.WriteLine($"[BREADCRUMB DEBUG] URI: {uri}, RelativePath: {relativePath}");
        
        if (string.IsNullOrEmpty(relativePath) || relativePath == "dashboard" || relativePath == "medical/dashboard")
        {
            currentPageTitle = "Dashboard";
            currentPageIcon = "dashboard";
            currentPagePath = "/dashboard";
            parentPageTitle = null;
            parentPagePath = null;
        }
        else if (relativePath.Contains("administrare/gestionare-persoane"))
        {
            currentPageTitle = "Gestionare Persoane";
            currentPageIcon = "manage_accounts";
            currentPagePath = "/administrare/gestionare-persoane";
            parentPageTitle = "Persoane";
            parentPagePath = "/administrare/persoane";
        }
        else if (relativePath.Contains("persoane") || relativePath.Contains("administrare/persoane"))
        {
            currentPageTitle = "Persoane";
            currentPageIcon = "people";
            currentPagePath = "/administrare/persoane";
            parentPageTitle = null;
            parentPagePath = null;
        }
        else if (relativePath.Contains("utilizatori"))
        {
            currentPageTitle = "Utilizatori";
            currentPageIcon = "manage_accounts";
            currentPagePath = "/utilizatori";
            parentPageTitle = null;
            parentPagePath = null;
        }
        else if (relativePath.Contains("pacienti"))
        {
            currentPageTitle = "Pacien?i";
            currentPageIcon = "people";
            currentPagePath = "/medical/pacienti";
            parentPageTitle = null;
            parentPagePath = null;
        }
        else if (relativePath.Contains("personal"))
        {
            currentPageTitle = "Personal Medical";
            currentPageIcon = "badge";
            currentPagePath = "/medical/personal";
            parentPageTitle = null;
            parentPagePath = null;
        }
        else if (relativePath.Contains("farmacie"))
        {
            currentPageTitle = "Farmacie";
            currentPageIcon = "local_pharmacy";
            currentPagePath = "/farmacie";
            parentPageTitle = null;
            parentPagePath = null;
        }
        else if (relativePath.Contains("rapoarte"))
        {
            currentPageTitle = "Rapoarte";
            currentPageIcon = "assessment";
            currentPagePath = "/rapoarte";
            parentPageTitle = null;
            parentPagePath = null;
        }
        else
        {
            currentPageTitle = "Dashboard";
            currentPageIcon = "dashboard";
            currentPagePath = "/dashboard";
            parentPageTitle = null;
            parentPagePath = null;
        }
        
        Console.WriteLine($"[BREADCRUMB DEBUG] Set to: {currentPageTitle} with icon {currentPageIcon}");
        Console.WriteLine($"[BREADCRUMB DEBUG] Parent: {parentPageTitle}");
    }

    public class MenuModel : INotifyPropertyChanged
    {
        private readonly Func<List<MenuModel>> collection;
        private bool _expanded;

        public MenuModel(Func<List<MenuModel>> collection)
        {
            this.collection = collection;
            Items = new List<MenuModel>();
        }

        public string Text { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string? Path { get; set; }
        public EventCallback Click { get; set; }

        public bool Expanded 
        {
            get => _expanded;
            set
            {
                if (_expanded != value)
                {
                    _expanded = value;
                    OnPropertyChanged(nameof(Expanded));
                }
            }
        }

        public List<MenuModel> Items { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}