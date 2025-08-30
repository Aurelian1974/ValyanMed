using Microsoft.AspNetCore.Components;
using global::Shared.DTOs.Medical;

namespace Client.Pages.Medical;

public partial class PatientRegistration
{
    // TODO: Implementare cu Radzen Form si validari
    private CreatePacientRequest _pacientRequest = new();
    
    protected override void OnInitialized()
    {
        // Logic here
    }
}