<#
.SYNOPSIS
    Script rapid pentru query-uri comune ValyanMed
.DESCRIPTION
    Shortcuts pentru cele mai folosite query-uri
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("persoane", "personal", "pacienti", "departamente", "stats")]
    [string]$Type,
    
    [Parameter(Mandatory=$false)]
    [string]$Search = "",
    
    [Parameter(Mandatory=$false)]
    [int]$Top = 10
)

$scriptPath = Join-Path $PSScriptRoot "Query-ValyanMedDatabase.ps1"

switch ($Type) {
    "persoane" {
        if ($Search) {
            $query = "SELECT TOP $Top Id, Nume, Prenume, CNP, Telefon, Email, Judet, Localitate FROM Persoane WHERE EsteActiva = 1 AND (Nume LIKE '%$Search%' OR Prenume LIKE '%$Search%')"
        } else {
            $query = "SELECT TOP $Top Id, Nume, Prenume, CNP, Telefon, Email, Judet, Localitate FROM Persoane WHERE EsteActiva = 1 ORDER BY DataCreare DESC"
        }
    }
    
    "personal" {
        if ($Search) {
            $query = "SELECT TOP $Top PersonalID, Nume, Prenume, NumarLicenta, Pozitie, Telefon, Email FROM PersonalMedical WHERE EsteActiv = 1 AND (Nume LIKE '%$Search%' OR Prenume LIKE '%$Search%')"
        } else {
            $query = "SELECT TOP $Top pm.PersonalID, pm.Nume, pm.Prenume, pm.NumarLicenta, pm.Pozitie, ISNULL(cat.Nume, pm.Departament) as Departament FROM PersonalMedical pm LEFT JOIN Departamente cat ON pm.CategorieID = cat.DepartamentID WHERE pm.EsteActiv = 1"
        }
    }
    
    "pacienti" {
        if ($Search) {
            $query = "SELECT TOP $Top PacientID, Nume, Prenume, CNP, DataNasterii, Telefon FROM Pacienti WHERE EsteActiv = 1 AND (Nume LIKE '%$Search%' OR Prenume LIKE '%$Search%' OR CNP LIKE '%$Search%')"
        } else {
            $query = "SELECT TOP $Top PacientID, Nume, Prenume, CNP, DataNasterii, Telefon, DataCreare FROM Pacienti WHERE EsteActiv = 1 ORDER BY DataCreare DESC"
        }
    }
    
    "departamente" {
        $query = "SELECT TOP 50 d.Nume as Departament, d.Tip FROM Departamente d ORDER BY d.Tip, d.Nume"
    }
    
    "stats" {
        $query = @"
SELECT 
    'Total Persoane' as Indicator, COUNT(*) as Valoare FROM Persoane WHERE EsteActiva = 1
UNION ALL
SELECT 'Total Personal Medical', COUNT(*) FROM PersonalMedical WHERE EsteActiv = 1
UNION ALL
SELECT 'Total Pacienti', COUNT(*) FROM Pacienti WHERE EsteActiv = 1
UNION ALL
SELECT 'Total Utilizatori', COUNT(*) FROM Utilizator
UNION ALL
SELECT 'Total Departamente', COUNT(*) FROM Departamente
UNION ALL
SELECT 'Categorii', COUNT(*) FROM Departamente WHERE Tip = 'Categorie'
UNION ALL
SELECT 'Specialitati', COUNT(*) FROM Departamente WHERE Tip = 'Specialitate'
UNION ALL
SELECT 'Subspecialitati', COUNT(*) FROM Departamente WHERE Tip = 'Subspecialitate'
"@
    }
}

& $scriptPath -Query $query -Server "TS1828\ERP"

# Exemple simple pentru Claude/ChatGPT:
# .\Query-ValyanMedDatabase.ps1 -Query "SELECT TOP 10 * FROM Persoane WHERE EsteActiva = 1"
# .\Query-ValyanMedDatabase.ps1 -Query "SELECT Nume, Prenume, Email FROM PersonalMedical WHERE Pozitie LIKE '%Medic%'" -Format Json

# Folosind script-ul rapid:
# .\Quick-ValyanMedQuery.ps1 -Type persoane -Search "Ion"
# .\Quick-ValyanMedQuery.ps1 -Type stats
# .\Quick-ValyanMedQuery.ps1 -Type departamente