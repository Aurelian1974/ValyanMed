@echo off
echo ===============================================
echo         ValyanMed Directory Checker
echo ===============================================
echo.

echo ?? Current working directory: %CD%
echo ?? Script location: %~dp0
echo.

echo [1/3] Checking for solution file...
if exist "ValyanMed.sln" (
    echo ? ValyanMed.sln found
) else (
    echo ? ValyanMed.sln NOT found
    echo ?? You might be in the wrong directory
)

echo.
echo [2/3] Checking for project directories...
set PROJECT_COUNT=0

if exist "API" (
    echo ? API directory found
    set /a PROJECT_COUNT+=1
) else (
    echo ? API directory NOT found
)

if exist "Client" (
    echo ? Client directory found
    set /a PROJECT_COUNT+=1
) else (
    echo ? Client directory NOT found
)

if exist "Shared" (
    echo ? Shared directory found
    set /a PROJECT_COUNT+=1
) else (
    echo ? Shared directory NOT found
)

if exist "Application" (
    echo ? Application directory found
    set /a PROJECT_COUNT+=1
) else (
    echo ? Application directory NOT found
)

if exist "Infrastructure" (
    echo ? Infrastructure directory found
    set /a PROJECT_COUNT+=1
) else (
    echo ? Infrastructure directory NOT found
)

if exist "Core" (
    echo ? Core directory found
    set /a PROJECT_COUNT+=1
) else (
    echo ? Core directory NOT found
)

echo.
echo [3/3] Summary...
echo ?? Found %PROJECT_COUNT% out of 6 expected project directories

if %PROJECT_COUNT% geq 4 (
    echo ? Directory structure looks good!
    echo ?? You can run debug-valyanmed.bat
) else (
    echo ? Missing critical directories!
    echo.
    echo ?? Possible solutions:
    echo    1. Navigate to the correct ValyanMed root directory
    echo    2. Make sure you've cloned the repository completely
    echo    3. Check if the directories were renamed
    echo.
    echo Expected structure:
    echo    ValyanMed\
    echo    ??? API\
    echo    ??? Client\
    echo    ??? Shared\
    echo    ??? Application\
    echo    ??? Infrastructure\
    echo    ??? Core\
    echo    ??? ValyanMed.sln
)

echo.
echo ?? Current directory contents:
dir /b /a:d 2>nul

echo.
echo ===============================================
echo Press any key to continue...
pause > nul