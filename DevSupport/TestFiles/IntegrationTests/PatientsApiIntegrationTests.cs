using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using System.Net;
using DevSupport.Models;

namespace DevSupport.TestFiles.IntegrationTests;

public class PatientsApiIntegrationTests : IClassFixture<WebApplicationFactory<TestStartup>>
{
    private readonly WebApplicationFactory<TestStartup> _factory;
    private readonly HttpClient _client;

    public PatientsApiIntegrationTests(WebApplicationFactory<TestStartup> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Additional test services if needed
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Should_GetAllPatients_When_ValidRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var patients = await response.Content.ReadFromJsonAsync<List<PatientDto>>();
        patients.Should().NotBeNull();
        patients.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Should_CreatePatient_When_ValidDataProvided()
    {
        // Arrange
        var newPatient = new CreatePatientRequest
        {
            CNP = "1990101123456",
            FirstName = "Test",
            LastName = "Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Masculin",
            Phone = "0721111111",
            Email = "test.patient@email.ro",
            Address = "Adresa test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", newPatient);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdPatient = await response.Content.ReadFromJsonAsync<PatientDto>();
        createdPatient.Should().NotBeNull();
        createdPatient!.CNP.Should().Be(newPatient.CNP);
        createdPatient.FirstName.Should().Be(newPatient.FirstName);
        createdPatient.LastName.Should().Be(newPatient.LastName);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_InvalidCNPProvided()
    {
        // Arrange
        var invalidPatient = new CreatePatientRequest
        {
            CNP = "123", // CNP invalid
            FirstName = "Test",
            LastName = "Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Masculin"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", invalidPatient);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_UpdatePatient_When_ValidDataProvided()
    {
        // Arrange - Creeaz? mai întâi un pacient
        var newPatient = new CreatePatientRequest
        {
            CNP = "1990102123456",
            FirstName = "Original",
            LastName = "Name",
            DateOfBirth = new DateTime(1990, 1, 2),
            Gender = "Masculin"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/patients", newPatient);
        var createdPatient = await createResponse.Content.ReadFromJsonAsync<PatientDto>();

        var updatePatient = new UpdatePatientRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Phone = "0721222222"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/patients/{createdPatient!.Id}", updatePatient);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updatedPatient = await response.Content.ReadFromJsonAsync<PatientDto>();
        updatedPatient!.FirstName.Should().Be("Updated");
        updatedPatient.Phone.Should().Be("0721222222");
    }

    [Fact]
    public async Task Should_DeletePatient_When_ValidIdProvided()
    {
        // Arrange - Creeaz? mai întâi un pacient
        var newPatient = new CreatePatientRequest
        {
            CNP = "1990103123456",
            FirstName = "ToDelete",
            LastName = "Patient",
            DateOfBirth = new DateTime(1990, 1, 3),
            Gender = "Masculin"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/patients", newPatient);
        var createdPatient = await createResponse.Content.ReadFromJsonAsync<PatientDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/patients/{createdPatient!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verific? c? pacientul nu mai exist?
        var getResponse = await _client.GetAsync($"/api/patients/{createdPatient.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_SearchPatients_When_ValidSearchTermProvided()
    {
        // Arrange
        var searchTerm = "Popescu";

        // Act
        var response = await _client.GetAsync($"/api/patients/search?searchTerm={searchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var searchResults = await response.Content.ReadFromJsonAsync<List<PatientDto>>();
        searchResults.Should().NotBeNull();
        
        if (searchResults!.Any())
        {
            searchResults.Should().OnlyContain(p => 
                p.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_GettingNonExistentPatient()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/patients/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}