@echo off
echo ===============================================
echo        ValyanMed Simple Launcher
echo ===============================================
echo.

echo ?? Working from: %CD%
echo.

echo [1/4] Building API...
if exist "API" (
    cd API
    echo Building API project...
    dotnet build --verbosity minimal
    if %errorlevel% neq 0 (
        echo ? API build failed!
        cd ..
        pause
        exit /b 1
    )
    cd ..
    echo ? API built successfully
) else (
    echo ? API directory not found!
    echo Make sure you're in the ValyanMed root directory
    pause
    exit /b 1
)

echo.
echo [2/4] Building Client...
if exist "Client" (
    cd Client
    echo Building Client project...
    dotnet build --verbosity minimal
    if %errorlevel% neq 0 (
        echo ? Client build failed!
        cd ..
        pause
        exit /b 1
    )
    cd ..
    echo ? Client built successfully
) else (
    echo ? Client directory not found!
    pause
    exit /b 1
)

echo.
echo [3/4] Starting API...
echo ?? API URL: https://localhost:7294
start "ValyanMed API" cmd /k "cd /d "%CD%\API" && echo Starting API on https://localhost:7294 && dotnet run"

echo Waiting for API startup...
timeout /t 8 /nobreak > nul

echo.
echo [4/4] Starting Client...
echo ???  Client URL: https://localhost:7169/login
start "ValyanMed Client" cmd /k "cd /d "%CD%\Client" && echo Starting Client on https://localhost:7169 && dotnet run"

echo.
echo ===============================================
echo ?? Both applications are starting...
echo.
echo ?? API:    https://localhost:7294
echo ?? Client: https://localhost:7169/login
echo.
echo ? Wait ~30 seconds then open: https://localhost:7169/login
echo ===============================================
echo.
pause