# ?? ValyanMed - Quick Start Guide

## ?? Running the Application

### Option 1: Enhanced Launcher (Recommended)
```bash
debug-valyanmed.bat
```
This script will:
- ? Check prerequisites (.NET 9 SDK)
- ? Clean and build the solution
- ? Start API server (https://localhost:7294)
- ? Start Client app (https://localhost:7169)
- ? Open browser to login page

### Option 2: Simple Launcher
```bash
simple-launch.bat
```
Basic launcher without extensive checks.

### Option 3: Manual Launch
```bash
# Terminal 1 - API
cd API
dotnet run

# Terminal 2 - Client  
cd Client
dotnet run
```

## ?? Troubleshooting

### Health Check
```bash
health-check.bat
```
Checks API/Client status and opens login page automatically.

### Directory Check
```bash
check-directory.bat
```
Verifies you're in the correct directory with all projects.

## ?? Application URLs

- **?? Login Page**: https://localhost:7169/login
- **?? API Swagger**: https://localhost:7294/swagger  
- **?? API Base**: https://localhost:7294

## ?? Common Issues

### "Cannot read properties of undefined"
- **Solution**: Wait 60 seconds for full startup
- **Solution**: Press Ctrl+F5 (hard browser refresh)
- **Solution**: Clear browser cache

### "API not responding"  
- **Solution**: Check API console window for errors
- **Solution**: Verify https://localhost:7294 responds
- **Solution**: Check Windows Firewall/antivirus

### "Build failed"
- **Solution**: Run `dotnet clean` then restart
- **Solution**: Check .NET 9 SDK is installed
- **Solution**: Run `dotnet restore` manually

### "Directory not found"
- **Solution**: Run `check-directory.bat`
- **Solution**: Navigate to ValyanMed root folder
- **Solution**: Ensure all project folders exist

## ?? Development Workflow

1. **Start**: Run `debug-valyanmed.bat`
2. **Wait**: 30-60 seconds for complete startup  
3. **Login**: Open https://localhost:7169/login
4. **Debug**: F12 ? Console tab for errors
5. **Test**: Check functionality
6. **Stop**: Close console windows when done

## ?? Technical Details

- **Framework**: .NET 9
- **Frontend**: Blazor WebAssembly
- **Backend**: ASP.NET Core Web API
- **Database**: SQL Server with Dapper
- **UI**: MudBlazor components
- **Auth**: JWT Bearer tokens

## ?? Project Structure

```
ValyanMed/
??? API/           # Web API backend
??? Client/        # Blazor WebAssembly frontend  
??? Shared/        # Shared models and DTOs
??? Application/   # Business logic layer
??? Infrastructure/# Data access layer
??? Core/          # Domain models
??? *.bat          # Launcher scripts
```

## ?? Success Indicators

? **API Console**: "Now listening on: https://localhost:7294"  
? **Client Console**: "Now listening on: https://localhost:7169"  
? **Browser**: Login page loads without errors  
? **F12 Console**: "Blazor started successfully"

---

**Happy coding! ??**