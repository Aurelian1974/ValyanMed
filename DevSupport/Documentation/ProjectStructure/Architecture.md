# Arhitectura Proiectului ValyanMed

## Prezentare General?

ValyanMed este o aplica?ie medical? dezvoltat? în .NET 9 folosind arhitectura Clean Architecture (Onion Architecture) cu Blazor WebAssembly pentru frontend.

## Structura Proiectului

```
ValyanMed/
??? Core/                    # Domain Layer - Entit??i, interfe?e, reguli de business
??? Application/             # Application Layer - Use cases, servicii aplica?ie
??? Infrastructure/          # Infrastructure Layer - Implement?ri concrete
??? Shared/                  # Shared Kernel - DTOs, modele comune
??? Client/                  # Presentation Layer - Blazor WebAssembly
??? API/                     # API Layer - Controllers, middleware
??? DevSupport/              # Suport dezvoltare - Documente, teste, scripturi
```

## Layere de Arhitectur?

### 1. Core (Domain Layer)
- **Responsabilit??i**: Entit??i de business, interfe?e repository, reguli de domeniu
- **Dependen?e**: Niciuna (centrul arhitecturii)
- **Con?inut**:
  - `Entities/`: Entit??i de domeniu (Patient, User, MedicalStaff, etc.)
  - `Interfaces/`: Contracte pentru repository-uri ?i servicii
  - `Enums/`: Enumer?ri de domeniu
  - `ValueObjects/`: Obiecte de valoare

### 2. Application (Application Layer)
- **Responsabilit??i**: Use cases, servicii de aplica?ie, orchestrarea logicii
- **Dependen?e**: Core
- **Con?inut**:
  - `Services/`: Servicii de aplica?ie
  - `DTOs/`: Data Transfer Objects
  - `Validators/`: Validatori pentru input
  - `Mappings/`: Profile-uri AutoMapper

### 3. Infrastructure (Infrastructure Layer)
- **Responsabilit??i**: Implement?ri concrete, acces la date, servicii externe
- **Dependen?e**: Core, Application
- **Con?inut**:
  - `Data/`: Context Entity Framework, repository implementations
  - `Services/`: Implement?ri servicii externe
  - `Configurations/`: Configur?ri Entity Framework

### 4. Shared (Shared Kernel)
- **Responsabilit??i**: Modele ?i utilit??i partajate între layere
- **Dependen?e**: Niciuna
- **Con?inut**:
  - `Models/`: Modele comune
  - `Extensions/`: Metode de extensie
  - `Constants/`: Constante aplica?ie

### 5. Client (Presentation Layer)
- **Responsabilit??i**: UI Blazor WebAssembly, componente, pagini
- **Dependen?e**: Shared
- **Con?inut**:
  - `Pages/`: Pagini Blazor
  - `Components/`: Componente reutilizabile
  - `Services/`: Servicii client (API calls)
  - `wwwroot/`: Resurse statice (CSS, JS, imagini)

### 6. API (API Layer)
- **Responsabilit??i**: Endpoints REST, autentificare, middleware
- **Dependen?e**: Application, Infrastructure
- **Con?inut**:
  - `Controllers/`: API Controllers
  - `Middleware/`: Middleware custom
  - `Filters/`: Action filters

## Fluxul de Date

```
Client (Blazor) 
    ? HTTP/API calls
API (Controllers) 
    ? Business logic
Application (Services) 
    ? Data access
Infrastructure (Repositories) 
    ? Database queries
Database (SQL Server)
```

## Principii Arhitecturale

### 1. Dependency Inversion
- Layerele exterioare depind de interfe?e definite în layerele interioare
- Core nu depinde de niciun layer exterior

### 2. Separation of Concerns
- Fiecare layer are responsabilit??i clare ?i distincte
- Business logic în Core ?i Application
- Infrastructure details în Infrastructure

### 3. Testability
- Interfe?e pentru toate serviciile externe
- Dependen?e injectabile
- Logica de business separat? de infrastructure

## Tehnologii Utilizate

- **.NET 9**: Framework principal
- **Blazor WebAssembly**: Frontend SPA
- **Entity Framework Core 9**: ORM pentru acces la date
- **SQL Server**: Baza de date
- **AutoMapper**: Mapare obiecte
- **FluentValidation**: Valid?ri
- **Radzen**: Componente UI
- **xUnit + bUnit**: Framework-uri de testare

## Configurare ?i Deployment

### Development
```bash
# Pornire API
cd API && dotnet run

# Pornire Client (în terminal separat)
cd Client && dotnet run
```

### Production
- API deployment pe IIS/Azure App Service
- Client deployment pe CDN/Azure Static Web Apps
- Database pe SQL Server/Azure SQL Database

## Conven?iile de Cod

1. **Naming**: PascalCase pentru publice, camelCase pentru private
2. **Async/Await**: Toate opera?iile I/O sunt asincrone
3. **Error Handling**: Result pattern pentru erori de business
4. **Validation**: FluentValidation în Application layer
5. **Logging**: Serilog pentru logging structurat