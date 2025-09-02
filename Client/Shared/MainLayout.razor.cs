using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using System.ComponentModel;

namespace Client.Shared;

public partial class MainLayout : LayoutComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;

    private List<MenuModel> menuData = new();
    private string searchTerm = string.Empty;
    private bool isDarkMode = false;

    protected override void OnInitialized()
    {
        InitializeMenu();
    }

    #region Search Functionality
    private async Task OnGlobalSearch()
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return;

        // Implement global search logic
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "C?utare",
            Detail = $"Se caut?: {searchTerm}",
            Duration = 3000
        });

        // Navigate to search results
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
        // Show notifications panel/dialog
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
        // Default quick action - most common: add patient
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
            // Save theme preference
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "theme", isDarkMode ? "dark" : "light");
            
            // Simple CSS class toggle instead of complex JavaScript
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
            
            // Fallback - just toggle the state and show notification
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Tem?",
                Detail = $"Modul {(isDarkMode ? "întunecat" : "luminos")} va fi disponibil în curând",
                Duration = 2000
            });
        }
        
        // Force re-render to update the icon
        StateHasChanged();
    }
    #endregion

    #region Original Menu Logic
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
            // For?eaz? stilurile pentru butonul Quick Actions prin JavaScript direct
            await JSRuntime.InvokeVoidAsync("eval", @"
                setTimeout(() => {
                    console.log('Forcing Quick Actions button styles...');
                    
                    const quickBtn = document.querySelector('.quick-actions-btn-flat');
                    if (quickBtn) {
                        // For?eaz? stilurile direct pe element
                        quickBtn.style.background = 'rgba(0,0,0,0.4)';
                        quickBtn.style.border = '2px solid rgba(255,255,255,0.8)';
                        quickBtn.style.color = 'white';
                        quickBtn.style.fontWeight = '800';
                        quickBtn.style.boxShadow = '0 3px 10px rgba(0,0,0,0.3)';
                        
                        // For?eaz? pe text
                        const btnText = quickBtn.querySelector('.rz-button-text');
                        if (btnText) {
                            btnText.style.color = 'white';
                            btnText.style.fontWeight = '800';
                            btnText.style.textShadow = '0 2px 4px rgba(0,0,0,0.6)';
                        }
                        
                        // For?eaz? pe icon
                        const btnIcon = quickBtn.querySelector('.rz-button-icon');
                        if (btnIcon) {
                            btnIcon.style.color = 'white';
                            btnIcon.style.textShadow = '0 2px 4px rgba(0,0,0,0.6)';
                        }
                        
                        // For?eaz? pe butonul principal ?i cel de dropdown
                        const allButtons = quickBtn.querySelectorAll('.rz-button, .rz-splitbutton-button');
                        allButtons.forEach(btn => {
                            btn.style.background = 'rgba(0,0,0,0.4)';
                            btn.style.color = 'white';
                            btn.style.border = 'none';
                        });
                        
                        console.log('Quick Actions button styles applied successfully');
                    } else {
                        console.log('Quick Actions button not found');
                    }
                }, 1000);
            ");
        }
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