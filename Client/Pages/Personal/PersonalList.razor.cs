using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client.Pages.Personal
{
    public partial class PersonalList : ComponentBase
    {
        [Inject]
        private HttpClient HttpClient { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        private async Task DeletePerson(int id)
        {
            try
            {
                var response = await HttpClient.DeleteAsync($"api/personal/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    // Success handling
                    await LoadData();
                    Snackbar.Add("Persoana a fost ștearsă cu succes.", Severity.Success);
                }
                else
                {
                    // Read the error message from the response
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        // Display the specific conflict error message
                        Snackbar.Add(errorContent, Severity.Warning);
                    }
                    else
                    {
                        // Handle other error cases
                        Snackbar.Add($"Eroare: {errorContent}", Severity.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Eroare: {ex.Message}", Severity.Error);
            }
        }

        private async Task LoadData()
        {
            // Implement your data loading logic here
        }
    }
}