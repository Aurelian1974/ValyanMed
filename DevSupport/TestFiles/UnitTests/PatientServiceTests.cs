using Xunit;
using Moq;
using System.Threading.Tasks;
using DevSupport.Services;
using DevSupport.Models;
using DevSupport.Interfaces;
using FluentAssertions;

namespace DevSupport.TestFiles.UnitTests;

public class PatientServiceTests
{
    private readonly Mock<IPatientRepository> _mockPatientRepository;
    private readonly PatientService _patientService;

    public PatientServiceTests()
    {
        _mockPatientRepository = new Mock<IPatientRepository>();
        _patientService = new PatientService(_mockPatientRepository.Object);
    }

    [Fact]
    public async Task Should_CreatePatient_When_ValidDataProvided()
    {
        // Arrange
        var patient = new Patient
        {
            CNP = "1234567890123",
            FirstName = "Ion",
            LastName = "Popescu",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Masculin",
            Phone = "0721234567",
            Email = "ion.popescu@email.ro"
        };

        _mockPatientRepository
            .Setup(x => x.ExistsAsync(patient.CNP))
            .ReturnsAsync(false);

        _mockPatientRepository
            .Setup(x => x.CreateAsync(It.IsAny<Patient>()))
            .ReturnsAsync(patient);

        // Act
        var result = await _patientService.CreateAsync(patient);

        // Assert
        result.Should().NotBeNull();
        result.CNP.Should().Be("1234567890123");
        result.FirstName.Should().Be("Ion");
        result.LastName.Should().Be("Popescu");
        
        _mockPatientRepository.Verify(x => x.CreateAsync(It.IsAny<Patient>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("123")] // CNP prea scurt
    [InlineData("12345678901234567890")] // CNP prea lung
    [InlineData("123456789012a")] // CNP cu litere
    public async Task Should_ThrowException_When_InvalidCNPProvided(string invalidCNP)
    {
        // Arrange
        var patient = new Patient
        {
            CNP = invalidCNP,
            FirstName = "Ion",
            LastName = "Popescu",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Masculin"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _patientService.CreateAsync(patient)
        );
    }

    [Fact]
    public async Task Should_ThrowException_When_CNPAlreadyExists()
    {
        // Arrange
        var patient = new Patient
        {
            CNP = "1234567890123",
            FirstName = "Ion",
            LastName = "Popescu",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Masculin"
        };

        _mockPatientRepository
            .Setup(x => x.ExistsAsync(patient.CNP))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _patientService.CreateAsync(patient)
        );
    }

    [Fact]
    public async Task Should_SearchPatients_When_ValidSearchTermProvided()
    {
        // Arrange
        var searchTerm = "Popescu";
        var expectedPatients = new List<Patient>
        {
            new Patient { FirstName = "Ion", LastName = "Popescu", CNP = "1234567890123", Gender = "Masculin", DateOfBirth = new DateTime(1990, 1, 1) },
            new Patient { FirstName = "Maria", LastName = "Popescu", CNP = "2234567890123", Gender = "Feminin", DateOfBirth = new DateTime(1992, 5, 15) }
        };

        _mockPatientRepository
            .Setup(x => x.SearchAsync(searchTerm, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedPatients);

        // Act
        var result = await _patientService.SearchAsync(searchTerm);

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.LastName.Contains(searchTerm)).Should().BeTrue();
    }

    [Fact]
    public async Task Should_UpdatePatient_When_ValidDataProvided()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var existingPatient = new Patient
        {
            Id = patientId,
            CNP = "1234567890123",
            FirstName = "Ion",
            LastName = "Popescu",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Masculin"
        };

        var updateRequest = new UpdatePatientRequest
        {
            FirstName = "Ioan",
            Phone = "0721111111"
        };

        _mockPatientRepository
            .Setup(x => x.GetByIdAsync(patientId))
            .ReturnsAsync(existingPatient);

        _mockPatientRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Patient>()))
            .ReturnsAsync((Patient p) => p);

        // Act
        var result = await _patientService.UpdateAsync(patientId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Ioan");
        result.Phone.Should().Be("0721111111");
        result.LastName.Should().Be("Popescu"); // Remains unchanged
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_ThrowException_When_UpdatingNonExistentPatient()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var updateRequest = new UpdatePatientRequest { FirstName = "Test" };

        _mockPatientRepository
            .Setup(x => x.GetByIdAsync(patientId))
            .ReturnsAsync((Patient?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _patientService.UpdateAsync(patientId, updateRequest)
        );
    }
}