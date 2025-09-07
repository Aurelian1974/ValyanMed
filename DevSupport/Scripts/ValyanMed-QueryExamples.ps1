<#
.SYNOPSIS
    Exemple de query-uri pentru baza de date ValyanMed
.DESCRIPTION
    Script cu exemple de interog?ri utile pentru diferite tabele
#>

# Set?ri comune
$server = "TS1828\ERP"
$database = "ValyanMed"

Write-Host "=== EXEMPLE DE QUERY-URI PENTRU VALYANMED ===" -ForegroundColor Cyan
Write-Host ""

# 1. PERSOANE
Write-Host "1. PERSOANE" -ForegroundColor Yellow
Write-Host "Liste persoane active:"
Write-Host '.\Query-ValyanMedDatabase.ps1 -Query "SELECT TOP 10 Id, Nume, Prenume, CNP, Judet, Localitate, EsteActiva FROM Persoane WHERE EsteActiva = 1"'
Write-Host ""

Write-Host "C?utare persoane dup? nume:"
Write-Host '.\Query-ValyanMedDatabase.ps1 -Query "SELECT Id, Nume, Prenume, Telefon, Email FROM Persoane WHERE Nume LIKE ''%Popescu%''"'
Write-Host ""

Write-Host "Statistici persoane per jude?:"
Write-Host '.\Query-ValyanMedDatabase.ps1 -Query "SELECT Judet, COUNT(*) as Total FROM Persoane WHERE EsteActiva = 1 GROUP BY Judet ORDER BY Total DESC"'
Write-Host ""

# 2. PERSONAL MEDICAL
Write-Host "2. PERSONAL MEDICAL" -ForegroundColor Yellow
Write-Host "Personal medical activ cu specializ?ri:"
Write-Host '.\Query-ValyanMedDatabase.ps1 -Query "SELECT pm.PersonalID, pm.Nume, pm.Prenume, cat.Nume as Categorie, spec.Nume as Specializare FROM PersonalMedical pm LEFT JOIN Departamente cat ON pm.CategorieID = cat.DepartamentID LEFT JOIN Departamente spec ON pm.SpecializareID = spec.DepartamentID WHERE pm.EsteActiv = 1"'
Write-Host ""

Write-Host "Personal medical - format JSON:"
Write-Host '.\Query-ValyanMedDatabase.ps1 -Query "SELECT TOP 5 PersonalID, Nume, Prenume, NumarLicenta, Pozitie FROM PersonalMedical WHERE EsteActiv = 1" -Format Json'
Write-Host ""

# 3. DEPARTAMENTE IERARHICE
Write-Host "3. DEPARTAMENTE IERARHICE" -ForegroundColor Yellow
Write-Host "Toate categoriile:"
Write-Host '.\Query-ValyanMedDatabase.ps1 -Query "SELECT DepartamentID, Nume FROM Departamente WHERE Tip = ''Categorie'' ORDER BY Nume"'
Write-Host ""

Write-Host "Specializ?ri pentru categoria Medicale:"
Write-Host '.\Query-ValyanMedDatabase.ps1 -Query "SELECT d.Nume FROM Departamente d INNER JOIN DepartamenteIerarhie h ON d.DepartamentID = h.DescendantID WHERE h.AncestorID = (SELECT DepartamentID FROM Departamente WHERE Nume = ''Medicale'' AND Tip = ''Categorie'') AND h.Nivel = 1 AND d.Tip = ''Specialitate''"'
Write-Host ""

# 4. PACIEN?I
Write-Host "4. PACIEN?I" -ForegroundColor Yellow
Write-Host "Pacien?i recen?i:"
Write-Host '.\Query-ValyanMedDatabase.ps1 -Query "SELECT TOP 10 PacientID, Nume, Prenume, CNP, DataCreare FROM Pacienti WHERE EsteActiv = 1 ORDER BY DataCreare DESC"'
Write-Host ""

# 5. UTILIZATORI
Write-Host "5. UTILIZATORI" -ForegroundColor Yellow
Write-Host "Utilizatori activi:"
Write-Host '.\Query-ValyanMedDatabase.ps1 -Query "SELECT UserID, Nume, Prenume, Email, Departament, Pozitie FROM Utilizatori WHERE EsteActiv = 1"'
Write-Host ""

# 6. STATISTICI GENERALE
Write-Host "6. STATISTICI GENERALE" -ForegroundColor Yellow
Write-Host "Num?r total înregistr?ri per tabel:"
Write-Host @'
.\Query-ValyanMedDatabase.ps1 -Query "
SELECT 
    'Persoane' as Tabel, COUNT(*) as Total FROM Persoane WHERE EsteActiva = 1
UNION ALL
SELECT 
    'PersonalMedical', COUNT(*) FROM PersonalMedical WHERE EsteActiv = 1
UNION ALL
SELECT 
    'Pacienti', COUNT(*) FROM Pacienti WHERE EsteActiv = 1
UNION ALL
SELECT 
    'Utilizatori', COUNT(*) FROM Utilizatori WHERE EsteActiv = 1
UNION ALL
SELECT 
    'Departamente', COUNT(*) FROM Departamente
"
'@
Write-Host ""

# 7. EXPORT CSV
Write-Host "7. EXPORT DATE ÎN CSV" -ForegroundColor Yellow
Write-Host "Export persoane în CSV:"
Write-Host '.\Query-ValyanMedDatabase.ps1 -Query "SELECT Nume, Prenume, CNP, Telefon, Email, Judet, Localitate FROM Persoane WHERE EsteActiva = 1" -Format Csv > persoane_export.csv'
Write-Host ""

Write-Host "=== NOT? ===" -ForegroundColor Green
Write-Host "Toate query-urile sunt doar SELECT pentru siguran??."
Write-Host "Pentru query-uri complexe, folosi?i ghilimele simple în interiorul string-ului SQL."
Write-Host "Exemplu: WHERE Nume LIKE ''%text%''"