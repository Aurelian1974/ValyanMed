# ValyanMed - Troubleshooting Guide

## Problema: "Cannot read properties of undefined (reading 'invokeAfterStartedCallbacks')"

Aceast? eroare apare de obicei la startup-ul aplica?iei Blazor WebAssembly. Iat? pa?ii de rezolvare:

### 1. Verific?ri Browser
- Deschide?i Developer Tools (F12)
- Verifica?i Console pentru erori suplimentare
- Verifica?i Network tab pentru failed requests

### 2. Verifica?i API-ul
```bash
# Porne?te API-ul separat
cd API
dotnet run
```
API-ul trebuie s? ruleze pe: `https://localhost:7294`

### 3. Verifica?i Client-ul
```bash
# Porne?te Client-ul separat
cd Client
dotnet run
```

### 4. Clear Browser Cache
- Ap?sa?i Ctrl + F5 pentru hard refresh
- Sau ?terge?i cache-ul browser-ului complet

### 5. Clear .NET Cache
```bash
dotnet clean
dotnet restore
dotnet build
```

### 6. Verific?ri Port
- API: `https://localhost:7294`
- Client: variabil (se afi?eaz? în consol?)

### 7. Logs Important
Verifica?i în Console:
- "Loading: assembly manifest" - trebuie s? apar?
- "Blazor started successfully" - confirm? startup
- Erori de CORS sau network

### 8. Fallback - Browser Compatibility
Încerca?i:
- Chrome/Edge (recommended)
- Firefox
- Incognito/Private mode

### 9. Manual Testing
Accesa?i direct: `https://localhost:7294/api/test` (dac? exist? endpoint de test)

### 10. Database Connection
Verifica?i c? baza de date este configurat? corect în `API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;..."
  }
}
```

## Quick Start Script
Folosi?i `start-valyanmed.bat` pentru pornire automat?.

## Common Solutions

### CORS Issues
În `API/Program.cs`, verifica?i:
```csharp
app.UseCors("AllowAll");
```

### SSL Certificate Issues
```bash
dotnet dev-certs https --trust
```

### Port Conflicts
Verifica?i c? porturile 7294 (API) ?i 5173 (Client) sunt libere.