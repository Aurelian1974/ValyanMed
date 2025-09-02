using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using System.ComponentModel;

namespace Client.Shared;

public partial class MainLayout : LayoutComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    private List<MenuModel> menuData = new();

    protected override void OnInitialized()
    {
        InitializeMenu();
    }

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