@echo off
echo Testing ValyanMed API connection...
echo.

echo [1/2] Testing API health...
curl -k -s -o nul -w "API Status: %%{http_code}" https://localhost:7294/
echo.
echo.

echo [2/2] Testing specific endpoints...
curl -k -s -o nul -w "Swagger UI: %%{http_code}" https://localhost:7294/swagger
echo.

echo.
echo If you see "000" - API is not running
echo If you see "200" - API is healthy  
echo If you see "404" - API is running but endpoint missing
echo.
pause