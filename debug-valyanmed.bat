@echo off
setlocal enabledelayedexpansion

echo ===============================================
echo       ValyanMed Enhanced Debug Launcher
echo ===============================================
echo.

echo ?? Current directory: %CD%
echo.

echo [1/6] Checking prerequisites...
where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo ? .NET SDK not found! Please install .NET 9 SDK.
    echo ?? Download from: https://dotnet.microsoft.com/download/dotnet/9.0
    pause
    exit /b 1
)
echo ? .NET SDK found

echo.
echo [2/6] Verifying project structure...
set MISSING_PROJECTS=0
if not exist "API" (
    echo ? API directory missing
    set /a MISSING_PROJECTS+=1
)
if not exist "Client" (
    echo ? Client directory missing
    set /a MISSING_PROJECTS+=1
)
if not exist "Shared" (
    echo ? Shared directory missing  
    set /a MISSING_PROJECTS+=1
)

if !MISSING_PROJECTS! gtr 0 (
    echo.
    echo ? Missing critical project directories!
    echo ?? Make sure you're in the ValyanMed root directory
    echo ?? Expected structure:
    echo    ValyanMed\
    echo    ??? API\
    echo    ??? Client\
    echo    ??? Shared\
    echo    ??? ... other projects
    echo.
    pause
    exit /b 1
)
echo ? Project structure verified

echo.
echo [3/6] Clearing previous builds and cache...
if exist "ValyanMed.sln" (
    dotnet clean ValyanMed.sln --verbosity quiet >nul 2>&1
    echo ? Solution cleaned
) else (
    echo ??  No solution file found, cleaning projects individually...
    for %%d in (API Client Shared Application Infrastructure Core) do (
        if exist "%%d" (
            dotnet clean "%%d\%%d.csproj" --verbosity quiet >nul 2>&1
        )
    )
    echo ? Projects cleaned
)

echo.
echo [4/6] Restoring packages...
if exist "ValyanMed.sln" (
    dotnet restore ValyanMed.sln --verbosity quiet
) else (
    dotnet restore --verbosity quiet
)

if %errorlevel% neq 0 (
    echo ? Package restore failed!
    echo ?? Try running: dotnet restore --verbosity normal
    pause
    exit /b 1
)
echo ? Packages restored

echo.
echo [5/6] Building solution...
if exist "ValyanMed.sln" (
    dotnet build ValyanMed.sln --configuration Debug --verbosity minimal --no-restore
) else (
    dotnet build --configuration Debug --verbosity minimal --no-restore
)

if %errorlevel% neq 0 (
    echo ? Build failed!
    echo.
    echo ?? Try these solutions:
    echo    1. Run: dotnet build --verbosity normal (for detailed errors)
    echo    2. Clean and rebuild manually
    echo    3. Check for missing dependencies
    echo.
    pause
    exit /b 1
)
echo ? Build successful

echo.
echo [6/6] Starting applications...
echo.

echo ?? Starting API server...
echo ?? API URL: https://localhost:7294
echo ?? Swagger: https://localhost:7294/swagger
start "ValyanMed API" cmd /k "cd /d "%CD%\API" && title ValyanMed API && echo. && echo ? ValyanMed API Starting... && echo ?? https://localhost:7294 && echo ?? https://localhost:7294/swagger && echo. && dotnet run --no-build"

echo ? Waiting for API initialization...
timeout /t 12 /nobreak > nul

echo.
echo ?? Starting Client application...
echo ???  Client URL: https://localhost:7169
echo ?? Login URL: https://localhost:7169/login
start "ValyanMed Client" cmd /k "cd /d "%CD%\Client" && title ValyanMed Client && echo. && echo ? ValyanMed Client Starting... && echo ??? https://localhost:7169 && echo ?? https://localhost:7169/login && echo. && dotnet run --no-build"

echo.
echo ===============================================
echo ?? ValyanMed Applications Starting Successfully!
echo ===============================================
echo.
echo ?? URLS TO OPEN:
echo    ?? Login Page:      https://localhost:7169/login
echo    ?? API Swagger:     https://localhost:7294/swagger
echo    ?? API Base:        https://localhost:7294
echo.
echo ?? DEBUGGING GUIDE:
echo    1. Wait 30-60 seconds for full startup
echo    2. Open: https://localhost:7169/login
echo    3. Press F12 ? Console tab (check for errors)
echo    4. Press F12 ? Network tab (check failed requests)
echo.
echo ?? TROUBLESHOOTING:
echo    ? Can't access sites? Check Windows Firewall
echo    ? Build errors? Try: dotnet clean then restart
echo    ? Browser errors? Try: Ctrl+F5 (hard refresh)
echo    ? API not responding? Check API console window
echo.
echo ? Please wait 30-60 seconds for complete initialization...
echo.
echo ?? Ready to test! Open: https://localhost:7169/login
echo ===============================================
echo.
echo Press any key to close this launcher window...
pause > nul

endlocal