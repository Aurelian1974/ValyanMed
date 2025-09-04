# DevSupport Project

Acest proiect con?ine toate resursele de dezvoltare ?i suport pentru aplica?ia ValyanMed:

## Structura Proiectului

### ?? Documentation/
Documenta?ia complet? a proiectului:
- **ProjectStructure/**: Diagrame ?i descrieri ale arhitecturii
- **ApiDocumentation/**: Documenta?ia API-urilor
- **UserGuides/**: Ghiduri pentru utilizatori
- **TechnicalSpecs/**: Specifica?ii tehnice

### ?? SqlScripts/
Toate scripturile SQL organizate pe categorii:

#### Migrations/
- **001_InitialSchema.sql**: Schema ini?ial? a bazei de date
- **002_AddMedicalStaffAndAppointments.sql**: Tabele pentru personal medical ?i program?ri
- **003_CompleteTableStructure.sql**: Structura complet? a bazei de date ValyanMed

#### StoredProcedures/
- **DepartmentManagement.sql**: Proceduri pentru gestionarea departamentelor medicale
- **MedicationManagement.sql**: Proceduri pentru gestionarea medicamentelor ?i stocului

#### Views/
- **ReportingViews.sql**: View-uri pentru raportare ?i analiz?

#### Functions/
- **MedicalCalculations.sql**: Func?ii pentru calcul?ri medicale (vârst?, IMC, dozaje)

#### SeedData/
- **InitialData.sql**: Date ini?iale pentru popularea bazei de date (jude?e, localit??i, utilizatori de test)

### ?? TestFiles/
Fi?iere de test organizate pe tipuri:
- **UnitTests/**: Teste unitare (.cs)
- **BlazorComponents/**: Teste pentru componente Blazor (.razor, .razor.cs)
- **IntegrationTests/**: Teste de integrare

## Mutarea Scripturilor din Shared/

Scripturile originale din `Shared/scripts/` au fost organizate ?i mutate în DevSupport:

### Fi?iere procesate:
1. **script.sql** (544KB) - Script mare cu structura complet? ?i datele de baz?
   - Împ?r?it în Migrations/003_CompleteTableStructure.sql
   - Date mutate în SeedData/InitialData.sql

2. **scriptsforDepartment.sql** (7.5KB) - Scripturi pentru departamente ierarhice
   - Proceduri mutate în StoredProcedures/DepartmentManagement.sql
   - View-uri pentru ierarhie în Views/ReportingViews.sql

### Structura original? prelucrat?:
- ? **Tabele principale**: Medicament, DispozitiveMedicale, MaterialeSanitare, Persoana, etc.
- ? **Date de referin??**: Jude?e, localit??i, tipuri de localit??i
- ? **Departamente ierarhice**: Sistem closure table pentru organizare
- ? **Proceduri ?i func?ii**: Pentru gestionare ?i calcul?ri
- ? **View-uri de raportare**: Pentru analiz? ?i monitoring

## Utilizare

Acest proiect serve?te ca:
1. **Repository central** pentru documenta?ie
2. **Bibliotec? de scripturi SQL** reutilizabile ?i organizate
3. **Suite de teste** comprehensive
4. **Resurse de dezvoltare** comune

## Configurare

Proiectul este configurat s?:
- Includ? toate scripturile SQL ca resurse embedded
- Suporte teste Blazor cu bunit
- Referen?ieze toate proiectele din solu?ie
- Func?ioneze cu .NET 9

## Execu?ia Scripturilor

Scripturile sunt organizate în ordinea execu?iei:
1. **Migrations/** - În ordine numeric? pentru schema DB
2. **SeedData/** - Pentru popularea datelor ini?iale
3. **StoredProcedures/** - Pentru logica de business
4. **Views/** - Pentru raportare
5. **Functions/** - Pentru calcul?ri ?i valid?ri

Toate scripturile includ verific?ri de existen?? ?i sunt sigure pentru re-execu?ie.