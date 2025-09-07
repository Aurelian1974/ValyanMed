<#
.SYNOPSIS
    Script pentru deployment-ul scripturilor SQL din proiectul ValyanMed
.DESCRIPTION
    Execut? scripturile SQL din DevSupport\SqlScripts în ordinea corect?
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("All", "Setup", "Tables", "Functions", "StoredProcedures", "Views", "SeedData", "Migrations")]
    [string]$Category = "All",
    
    [Parameter(Mandatory=$false)]
    [switch]$Force = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun = $false
)

$scriptsBasePath = Join-Path $PSScriptRoot "..\SqlScripts"
$adminScriptPath = Join-Path $PSScriptRoot "Admin-ValyanMedDatabase.ps1"

# Ordinea de execu?ie pentru deployment complet
$deploymentOrder = @(
    "Setup",
    "Tables", 
    "Functions",
    "StoredProcedures",
    "Views",
    "SeedData",
    "Migrations"
)

function Get-SqlScripts {
    param([string]$CategoryPath)
    
    if (Test-Path $CategoryPath) {
        return Get-ChildItem -Path $CategoryPath -Filter "*.sql" | Sort-Object Name
    }
    return @()
}

function Execute-SqlScript {
    param([string]$ScriptPath, [bool]$IsDryRun)
    
    if (-not (Test-Path $ScriptPath)) {
        Write-Host "? Script nu a fost g?sit: $ScriptPath" -ForegroundColor Red
        return $false
    }
    
    $scriptContent = Get-Content -Path $ScriptPath -Raw
    $scriptName = Split-Path $ScriptPath -Leaf
    
    if ($IsDryRun) {
        Write-Host "?? [DRY RUN] $scriptName" -ForegroundColor Yellow
        Write-Host "Con?inut:" -ForegroundColor Gray
        Write-Host $scriptContent.Substring(0, [Math]::Min(200, $scriptContent.Length)) -ForegroundColor Gray
        if ($scriptContent.Length -gt 200) {
            Write-Host "... (truncat)" -ForegroundColor Gray
        }
        Write-Host ""
        return $true
    }
    
    Write-Host "? Executare: $scriptName" -ForegroundColor Cyan
    
    try {
        & $adminScriptPath -Query $scriptContent -Force:$Force
        Write-Host "? Succes: $scriptName" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "? Eroare în $scriptName : $_" -ForegroundColor Red
        return $false
    }
}

function Deploy-Category {
    param([string]$CategoryName, [bool]$IsDryRun)
    
    $categoryPath = Join-Path $scriptsBasePath $CategoryName
    
    if (-not (Test-Path $categoryPath)) {
        Write-Host "??  Categoria '$CategoryName' nu exist? în $categoryPath" -ForegroundColor Yellow
        return @{ Success = 0; Failed = 0; Skipped = 1 }
    }
    
    Write-Host ""
    Write-Host "?? === CATEGORIA: $CategoryName ===" -ForegroundColor Magenta
    
    $scripts = Get-SqlScripts -CategoryPath $categoryPath
    
    if ($scripts.Count -eq 0) {
        Write-Host "?? Nu s-au g?sit scripturi în categoria '$CategoryName'" -ForegroundColor Yellow
        return @{ Success = 0; Failed = 0; Skipped = $scripts.Count }
    }
    
    Write-Host "?? G?site $($scripts.Count) scripturi în categoria '$CategoryName'" -ForegroundColor Cyan
    
    $results = @{ Success = 0; Failed = 0; Skipped = 0 }
    
    foreach ($script in $scripts) {
        $success = Execute-SqlScript -ScriptPath $script.FullName -IsDryRun $IsDryRun
        
        if ($success) {
            $results.Success++
        } else {
            $results.Failed++
            
            # Opre?te la prima eroare pentru categoriile critice
            if ($CategoryName -in @("Setup", "Tables", "Functions") -and -not $Force) {
                Write-Host "?? Oprire la prima eroare în categoria critic? '$CategoryName'" -ForegroundColor Red
                Write-Host "Folose?te -Force pentru a continua în ciuda erorilor" -ForegroundColor Yellow
                break
            }
        }
    }
    
    return $results
}

# Main execution
Write-Host "?? DEPLOYMENT SCRIPTURI SQL VALYANMED" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Categoria: $Category" -ForegroundColor Cyan
Write-Host "Loca?ie scripturi: $scriptsBasePath" -ForegroundColor Cyan

if ($DryRun) {
    Write-Host "?? MOD DRY RUN - Nu se execut? nimic, doar se afi?eaz? ce ar fi executat" -ForegroundColor Yellow
}

if ($Force) {
    Write-Host "? MOD FOR?AT - Se continu? în ciuda erorilor" -ForegroundColor Yellow
}

$totalResults = @{ Success = 0; Failed = 0; Skipped = 0 }

if ($Category -eq "All") {
    Write-Host ""
    Write-Host "?? DEPLOYMENT COMPLET - Toate categoriile în ordinea corect?" -ForegroundColor Green
    
    foreach ($cat in $deploymentOrder) {
        $results = Deploy-Category -CategoryName $cat -IsDryRun $DryRun
        
        $totalResults.Success += $results.Success
        $totalResults.Failed += $results.Failed
        $totalResults.Skipped += $results.Skipped
        
        # Opre?te la erori în categoriile critice
        if ($results.Failed -gt 0 -and $cat -in @("Setup", "Tables") -and -not $Force) {
            Write-Host "?? Deployment oprit din cauza erorilor în categoria critic? '$cat'" -ForegroundColor Red
            break
        }
    }
} else {
    $results = Deploy-Category -CategoryName $Category -IsDryRun $DryRun
    
    $totalResults.Success += $results.Success
    $totalResults.Failed += $results.Failed
    $totalResults.Skipped += $results.Skipped
}

# Rezumat final
Write-Host ""
Write-Host "?? === REZUMAT DEPLOYMENT ===" -ForegroundColor Green
Write-Host "? Scripturi executate cu succes: $($totalResults.Success)" -ForegroundColor Green
Write-Host "? Scripturi cu erori: $($totalResults.Failed)" -ForegroundColor Red
Write-Host "??  Scripturi omise: $($totalResults.Skipped)" -ForegroundColor Yellow
Write-Host "?? Total scripturi procesate: $($totalResults.Success + $totalResults.Failed + $totalResults.Skipped)" -ForegroundColor Cyan

if ($totalResults.Failed -gt 0) {
    Write-Host ""
    Write-Host "??  ATEN?IE: Au existat erori în timpul deployment-ului!" -ForegroundColor Red
    Write-Host "Verific? mesajele de eroare de mai sus ?i corecteaz? problemele." -ForegroundColor Yellow
    exit 1
} else {
    Write-Host ""
    Write-Host "?? DEPLOYMENT FINALIZAT CU SUCCES!" -ForegroundColor Green
    
    if (-not $DryRun) {
        Write-Host ""
        Write-Host "?? Pentru verificare, ruleaz?:" -ForegroundColor Cyan
        Write-Host ".\Quick-ValyanMedQuery.ps1 -Type stats" -ForegroundColor White
    }
    exit 0
}