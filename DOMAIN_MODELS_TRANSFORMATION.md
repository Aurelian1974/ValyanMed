# Transformare: De la Modele Anemice la Modele de Domeniu Bogate

## Problem? Ini?ial?: Modele Anemice

Proiectul avea modele anemice care con?ineau doar date, f?r? logic? de business:

```csharp
// ÎNAINTE - Model anemic
public class PersoanaModel
{
    public string Nume { get; set; }
    public DateTime DataNasterii { get; set; }
    public string CNP { get; set; }
    // Doar propriet??i, zero logic?
}

// Logic? împr??tiat? prin servicii
public class PatientService 
{
    public int CalculateAge(DateTime birthDate)
    {
        // Logic? duplicat? în mai multe locuri
        var today = DateTime.Today;
        return today.Year - birthDate.Year;
    }
    
    public bool IsMinor(DateTime birthDate)
    {
        return CalculateAge(birthDate) < 18;
    }
}
```

### Probleme cu Modelele Anemice

1. **Separarea artificial?** între date ?i comportament
2. **Duplicarea logicii** în mai multe servicii
3. **Dificultatea în testare** - logica este împr??tiat?
4. **Înc?lcarea principiului Tell Don't Ask**
5. **Codul procedural** în loc de orientat obiect

## Solu?ia: Modele de Domeniu Bogate

Am creat modele de domeniu care încapsuleaz? atât datele cât ?i logica de business:

```csharp
// DUP? - Model de domeniu bogat
public class Patient
{
    private Patient() { } // Pentru EF Core
    
    public Patient(string nume, string prenume, string cnp, DateTime? dataNasterii)
    {
        // Validare în constructor
        if (string.IsNullOrWhiteSpace(nume))
            throw new ArgumentException("Numele este obligatoriu");
            
        Nume = nume.Trim();
        CNP = cnp.Trim();
        DataNasterii = dataNasterii;
        ValidateCNP(); // Validare automat?
    }
    
    // Propriet??i cu encapsulare
    public string Nume { get; private set; }
    public DateTime? DataNasterii { get; private set; }
    public string CNP { get; private set; }
    
    // LOGIC? DE BUSINESS INTEGRAT?
    public int CalculateAge()
    {
        if (!DataNasterii.HasValue) return 0;
        var today = DateTime.Today;
        var age = today.Year - DataNasterii.Value.Year;
        if (DataNasterii.Value.Date > today.AddYears(-age)) age--;
        return age;
    }
    
    public bool IsMinor() => CalculateAge() < 18;
    public bool IsSenior() => CalculateAge() >= 65;
    public string GetFullName() => $"{Nume} {Prenume}";
    
    public bool IsEligibleForTreatment(Treatment treatment)
    {
        var age = CalculateAge();
        return treatment.TipTratament switch
        {
            TreatmentType.Pediatric => age < 18,
            TreatmentType.Adult => age >= 18 && age < 65,
            TreatmentType.Geriatric => age >= 65,
            TreatmentType.Universal => true,
            _ => false
        };
    }
    
    public ValidationResult ValidateForAppointment()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(TipActIdentitate)) 
            errors.Add("Document de identitate necesar");
        if (CalculateAge() < 0) 
            errors.Add("Data na?terii nu poate fi în viitor");
        return new ValidationResult(errors.Count == 0, errors);
    }
    
    // Metode de actualizare cu logic? de business
    public void UpdateContactInfo(string judet, string localitate, ...)
    {
        Judet = judet?.Trim();
        // Logic? de validare ?i normalizare
        DataModificare = DateTime.UtcNow;
    }
}
```

## Structura Implement?rii

### 1. Entit??i de Domeniu (`Core/Entities/`)

- **`Patient.cs`** - Model bogat pentru pacien?i cu logic? medical?
- **`Medication.cs`** - Model pentru medicamente cu valid?ri farmaceutice
- **`MedicalDevice.cs`** - Dispozitive medicale cu logic? de siguran??
- **`MedicalSupply.cs`** - Materiale sanitare cu reguli de utilizare
- **`Partner.cs`** - Parteneri de business cu valid?ri fiscale

### 2. Extensii de Mapare (`Core/Extensions/`)

- **`DomainMappingExtensions.cs`** - Conversii între domain models ?i DTOs

### 3. Servicii de Domeniu (`Core/Services/`)

- **`PatientDomainService.cs`** - Opera?iuni complexe pentru pacien?i
- **`MedicationDomainService.cs`** - Logic? de business pentru medicamente

### 4. Modele Client Enhanced (`Client/Models/`)

- **`EnhancedModels.cs`** - Wrappere pentru UI cu logic? de afi?are

## Beneficiile Transform?rii

### 1. **Encapsularea Logicii de Business**
```csharp
// ÎNAINTE
var age = patientService.CalculateAge(patient.DataNasterii);
var isMinor = patientService.IsMinor(patient.DataNasterii);

// DUP?
var age = patient.CalculateAge();
var isMinor = patient.IsMinor();
```

### 2. **Validare Integrat?**
```csharp
// ÎNAINTE - validare separat?
if (string.IsNullOrEmpty(patient.Nume)) 
    throw new Exception("Nume obligatoriu");

// DUP? - validare în constructor ?i metode
var patient = new Patient(nume, prenume, cnp, dataNasterii); // Validare automat?
var validation = patient.ValidateForAppointment(); // Validare de business
```

### 3. **Invariante de Business**
```csharp
// DUP? - propriet??i protejate
public string Nume { get; private set; }

public void UpdateName(string nume)
{
    if (string.IsNullOrWhiteSpace(nume))
        throw new ArgumentException("Numele este obligatoriu");
    Nume = nume.Trim();
    DataModificare = DateTime.UtcNow;
}
```

### 4. **Logic? Contextual Relevant?**
```csharp
// Medicament cu logic? farmaceutic? integrat?
public bool IsExpired() => DateTime.Today > DataExpirare;
public bool IsStockLow() => Stoc <= StocSiguranta;
public decimal? CalculatePriceWithVAT() => Pret * (1 + TVA / 100);

public ValidationResult ValidateForDispensing(int quantity, bool hasPrescription)
{
    var errors = new List<string>();
    if (IsExpired()) errors.Add("Medicament expirat");
    if (quantity > Stoc) errors.Add("Stoc insuficient");
    if (PrescriptieMedicala && !hasPrescription) errors.Add("Necesit? prescrip?ie");
    return new ValidationResult(errors.Count == 0, errors);
}
```

## Demo Interactive

Pagina `/enhanced-demo` demonstreaz?:

1. **Compara?ia Înainte/Dup?** - vizualizare side-by-side
2. **Logic? de Business în Ac?iune** - teste interactive
3. **Valid?ri Integrate** - demonstra?ie valid?ri domain model
4. **Opera?iuni Complexe** - interac?iuni medicamente, eligibilitate tratamente

### Testarea Logicii de Business

```csharp
// Test eligibilitate tratament
var treatment = new Treatment { TipTratament = TreatmentType.Pediatric };
var eligible = patient.IsEligibleForTreatment(treatment);

// Test validare medicament
var validation = medication.ValidateForDispensing(quantity: 5, hasPrescription: false);

// Test validare partner business
var businessValidation = partner.ValidateForBusinessOperations();
```

## Migra?ia Existentului

Pentru a migra de la modelele vechi la cele noi:

```csharp
// Extensii pentru migra?ie
public static EnhancedPersoanaModel ToEnhanced(this PersoanaModel oldModel)
{
    var dto = oldModel.ToDTO();
    return new EnhancedPersoanaModel(dto);
}

// Utilizare
var enhancedPatients = oldPatients.ToEnhanced();
```

## Principii Aplicate

1. **Domain-Driven Design (DDD)**
   - Modelele reflect? domeniul de business
   - Limbajul ubiquu între dezvoltatori ?i exper?i domeniu

2. **Tell Don't Ask**
   - În loc s? întreb?m pentru date ?i s? aplic?m logica extern
   - Spunem obiectului ce s? fac?

3. **Single Responsibility**
   - Fiecare model are responsabilitatea pentru propria logic?

4. **Encapsulation**
   - Datele sunt protejate, accesibile prin metode controlate

5. **Immutability (par?ial?)**
   - Propriet??ile importante sunt `private set`
   - Modific?rile se fac prin metode cu validare

## Testarea Unitar?

Modelele bogate sunt mult mai u?or de testat:

```csharp
[Test]
public void Patient_CalculateAge_ReturnsCorrectAge()
{
    // Arrange
    var birthDate = new DateTime(1990, 1, 1);
    var patient = new Patient("Popescu", "Ion", "1900101123456", birthDate);
    
    // Act
    var age = patient.CalculateAge();
    
    // Assert
    Assert.AreEqual(34, age); // Presupunând c? testul ruleaz? în 2024
}

[Test]
public void Medication_ValidateForDispensing_WhenExpired_ReturnsInvalid()
{
    // Arrange
    var expiredMedication = new Medication(
        "Aspirin", "ASA", "500mg", "Comprimate", 
        "Pharma", DateTime.Today.AddDays(-1)); // Expirat ieri
    
    // Act
    var result = expiredMedication.ValidateForDispensing(1, false);
    
    // Assert
    Assert.IsFalse(result.IsValid);
    Assert.Contains("expirat", result.Errors[0].ToLower());
}
```

## Concluzie

Transformarea de la modele anemice la modele de domeniu bogate aduce:

- **Cod mai expressiv** ?i u?or de în?eles
- **Logic? centralizat?** ?i reutilizabil?
- **Valid?ri integrate** ?i consistente
- **Testare simplificat?** ?i mai sigur?
- **Mentenan?? redus?** prin eliminarea duplic?rii

Aceast? abordare respect? principiile Clean Architecture ?i Domain-Driven Design, rezultând într-o aplica?ie mai robust? ?i mai u?or de dezvoltat.