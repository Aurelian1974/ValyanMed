@echo off
echo ===============================================
echo        ValyanMed Quick Health Check
echo ===============================================
echo.

echo ?? Working from: %CD%
echo ? Check time: %DATE% %TIME%
echo.

echo [1/5] Testing API connection...
curl -k -s -o nul -w "API Status: %%{http_code}" https://localhost:7294/ 2>nul
if %errorlevel% equ 0 (
    echo ? API is responding
) else (
    echo ? API not responding (may not be started)
)

echo.
echo [2/5] Testing Client connection...
curl -k -s -o nul -w "Client Status: %%{http_code}" https://localhost:7169/ 2>nul
if %errorlevel% equ 0 (
    echo ? Client is responding
) else (
    echo ? Client not responding (may not be started)
)

echo.
echo [3/5] Checking project files...
set FILE_ISSUES=0

if exist "API\API.csproj" (
    echo ? API project file exists
) else (
    echo ? API project file missing
    set /a FILE_ISSUES+=1
)

if exist "Client\Client.csproj" (
    echo ? Client project file exists
) else (
    echo ? Client project file missing
    set /a FILE_ISSUES+=1
)

if exist "Client\wwwroot\index.html" (
    echo ? Client index.html exists
) else (
    echo ? Client index.html missing
    set /a FILE_ISSUES+=1
)

if exist "Client\wwwroot\css\app.css" (
    echo ? Client CSS files exist
) else (
    echo ? Client CSS files missing
    set /a FILE_ISSUES+=1
)

echo.
echo [4/5] Checking build outputs...
if exist "API\bin\Debug\net9.0\API.dll" (
    echo ? API compiled successfully
) else (
    echo ? API not compiled (run debug-valyanmed.bat)
)

if exist "Client\bin\Debug\net9.0\wwwroot" (
    echo ? Client compiled successfully
) else (
    echo ? Client not compiled (run debug-valyanmed.bat)
)

echo.
echo [5/5] Quick build test...
echo Testing solution build...
dotnet build --verbosity quiet --no-restore >nul 2>&1
if %errorlevel% equ 0 (
    echo ? Solution builds successfully
) else (
    echo ? Build errors detected (run: dotnet build)
)

echo.
echo ===============================================
echo ?? HEALTH CHECK SUMMARY
echo ===============================================

if %FILE_ISSUES% equ 0 (
    echo ? All critical files present
) else (
    echo ? %FILE_ISSUES% file issues detected
)

echo.
echo ?? QUICK ACCESS LINKS:
echo    ?? Client Login:    https://localhost:7169/login
echo    ?? API Swagger:     https://localhost:7294/swagger
echo    ?? API Health:      https://localhost:7294/
echo.

echo ?? NEXT STEPS:
if %FILE_ISSUES% gtr 0 (
    echo    ? Fix missing files first
) else (
    echo    1. Run: debug-valyanmed.bat (if not started)
    echo    2. Wait 60 seconds for startup
    echo    3. Open: https://localhost:7169/login
)

echo.
echo ?? COMMON ISSUES:
echo    • API/Client not responding = Not started yet
echo    • 404 errors = Normal during startup
echo    • SSL warnings = Expected in development
echo.

echo ?? AUTO-OPENING LOGIN PAGE...
timeout /t 3 /nobreak > nul
start https://localhost:7169/login

echo ===============================================
echo Press any key to close...
pause > nul