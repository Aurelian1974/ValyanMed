@echo off
echo Starting ValyanMed API and Client...
echo.

echo [1/3] Building solution...
dotnet build
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo [2/3] Starting API in background...
start "ValyanMed API" cmd /k "cd API && dotnet run"

echo.
echo [3/3] Waiting for API to start, then starting Client...
timeout /t 5 /nobreak > nul

start "ValyanMed Client" cmd /k "cd Client && dotnet run"

echo.
echo Both applications should be starting...
echo API: https://localhost:7294
echo Client: https://localhost:5173 (or similar)
echo.
echo Press any key to exit...
pause > nul