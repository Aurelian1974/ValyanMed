-- View-uri pentru raportare ?i analiz? date medicale
-- Data: 2025-01-04
-- Autor: DevSupport

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ==============================================
-- View pentru statistici departamente
-- ==============================================
CREATE OR ALTER VIEW [dbo].[vw_StatisticiDepartamente]
AS
SELECT 
    cat.Nume AS Categorie,
    COUNT(CASE WHEN d.Tip = 'Specialitate' THEN 1 END) AS NumarSpecialitati,
    COUNT(CASE WHEN d.Tip = 'Subspecialitate' THEN 1 END) AS NumarSubspecialitati,
    COUNT(*) - 1 AS TotalDescendenti,  -- -1 pentru a exclude categoria îns??i
    COUNT(DISTINCT pm.Id) AS NumarMedici,
    COUNT(CASE WHEN pm.Activ = 1 THEN 1 END) AS MediciActivi
FROM Departamente cat
LEFT JOIN DepartamenteIerarhie h ON cat.DepartamentID = h.AncestorID
LEFT JOIN Departamente d ON h.DescendantID = d.DepartamentID AND h.Nivel > 0
LEFT JOIN PersonalMedical pm ON d.DepartamentID = pm.DepartamentId
WHERE cat.Tip = 'Categorie'
GROUP BY cat.DepartamentID, cat.Nume;
GO

-- ==============================================
-- View pentru medicamente cu alert? stoc
-- ==============================================
CREATE OR ALTER VIEW [dbo].[vw_AlerteStocMedicamente]
AS
SELECT 
    m.MedicamentID,
    m.Nume AS Medicament,
    m.Concentratie,
    m.FormaFarmaceutica,
    m.Producator,
    m.Stoc AS StocCurent,
    m.StocSiguranta AS StocMinim,
    (m.StocSiguranta - m.Stoc) AS DeficitStoc,
    m.Pret,
    (m.StocSiguranta - m.Stoc) * ISNULL(m.Pret, 0) AS CostReaprovizionare,
    CASE 
        WHEN m.Stoc = 0 THEN 'EPUIZAT'
        WHEN m.Stoc < (m.StocSiguranta * 0.5) THEN 'CRITIC'
        WHEN m.Stoc <= m.StocSiguranta THEN 'SC?ZUT'
        ELSE 'OK'
    END AS StatusStoc,
    m.DataActualizare AS UltimaActualizare
FROM Medicament m
WHERE 
    m.Stoc <= m.StocSiguranta 
    AND m.Activ = 1
    AND m.Status = 'Activ';
GO

-- ==============================================
-- View pentru medicamente cu expirare apropiat?
-- ==============================================
CREATE OR ALTER VIEW [dbo].[vw_AlerteExpirariMedicamente]
AS
SELECT 
    m.MedicamentID,
    m.Nume AS Medicament,
    m.Concentratie,
    m.FormaFarmaceutica,
    m.Producator,
    m.DataExpirare,
    DATEDIFF(DAY, GETDATE(), m.DataExpirare) AS ZileRamase,
    m.Stoc,
    ISNULL(m.Pret, 0) AS Pret,
    (m.Stoc * ISNULL(m.Pret, 0)) AS ValoareStoc,
    CASE 
        WHEN m.DataExpirare <= GETDATE() THEN 'EXPIRAT'
        WHEN m.DataExpirare <= DATEADD(MONTH, 1, GETDATE()) THEN 'CRITIC'
        WHEN m.DataExpirare <= DATEADD(MONTH, 3, GETDATE()) THEN 'ATEN?IE'
        WHEN m.DataExpirare <= DATEADD(MONTH, 6, GETDATE()) THEN 'MONITORIZARE'
        ELSE 'OK'
    END AS StatusExpirare
FROM Medicament m
WHERE 
    m.DataExpirare <= DATEADD(MONTH, 6, GETDATE())
    AND m.Stoc > 0
    AND m.Activ = 1;
GO

-- ==============================================
-- View pentru raportul complet al medicamentelor
-- ==============================================
CREATE OR ALTER VIEW [dbo].[vw_RaportMedicamente]
AS
SELECT 
    m.MedicamentID,
    m.Nume,
    m.DenumireComunaInternationala,
    m.Concentratie,
    m.FormaFarmaceutica,
    m.Producator,
    m.CodATC,
    CASE LEFT(m.CodATC, 1)
        WHEN 'A' THEN 'Tract alimentar ?i metabolism'
        WHEN 'B' THEN 'Sânge ?i organe hematopoietice'
        WHEN 'C' THEN 'Sistem cardiovascular'
        WHEN 'D' THEN 'Dermatologice'
        WHEN 'G' THEN 'Sistem genitourinar ?i hormoni sexuali'
        WHEN 'H' THEN 'Preparate hormonale sistemice'
        WHEN 'J' THEN 'Antiinfec?ioase generale pentru uz sistemic'
        WHEN 'L' THEN 'Antineoplazice ?i agen?i imunomodulatori'
        WHEN 'M' THEN 'Sistem musculo-scheletic'
        WHEN 'N' THEN 'Sistem nervos'
        WHEN 'P' THEN 'Produse antiparazitare'
        WHEN 'R' THEN 'Sistem respirator'
        WHEN 'S' THEN 'Organe de sim?'
        WHEN 'V' THEN 'Diverse'
        ELSE 'Necategorizat'
    END AS CategorieATC,
    m.Status,
    m.Stoc,
    m.StocSiguranta,
    ISNULL(m.Pret, 0) AS Pret,
    (m.Stoc * ISNULL(m.Pret, 0)) AS ValoareStoc,
    m.DataExpirare,
    DATEDIFF(DAY, GETDATE(), m.DataExpirare) AS ZileRamase,
    
    -- Status combinat pentru stoc
    CASE 
        WHEN m.Stoc = 0 THEN 'EPUIZAT'
        WHEN m.Stoc < (m.StocSiguranta * 0.5) THEN 'STOC CRITIC'
        WHEN m.Stoc <= m.StocSiguranta THEN 'STOC SC?ZUT'
        ELSE 'STOC OK'
    END AS StatusStoc,
    
    -- Status pentru expirare
    CASE 
        WHEN m.DataExpirare <= GETDATE() THEN 'EXPIRAT'
        WHEN m.DataExpirare <= DATEADD(MONTH, 1, GETDATE()) THEN 'EXPIR? CRITIC'
        WHEN m.DataExpirare <= DATEADD(MONTH, 3, GETDATE()) THEN 'EXPIR? CURÂND'
        WHEN m.DataExpirare <= DATEADD(MONTH, 6, GETDATE()) THEN 'MONITORIZARE'
        ELSE 'VALABIL'
    END AS StatusExpirare,
    
    -- Status general
    CASE 
        WHEN m.Activ = 0 THEN 'INACTIV'
        WHEN m.Status != 'Activ' THEN 'SUSPENDAT'
        WHEN m.DataExpirare <= GETDATE() THEN 'EXPIRAT'
        WHEN m.Stoc = 0 THEN 'EPUIZAT'
        WHEN m.Stoc <= m.StocSiguranta OR m.DataExpirare <= DATEADD(MONTH, 1, GETDATE()) THEN 'ALERT?'
        ELSE 'DISPONIBIL'
    END AS StatusGeneral,
    
    m.DataActualizare,
    m.UtilizatorActualizare
FROM Medicament m
WHERE m.Activ = 1;
GO

-- ==============================================
-- View pentru personal medical pe departamente
-- ==============================================
CREATE OR ALTER VIEW [dbo].[vw_PersonalMedicalDepartamente]
AS
SELECT 
    d.Nume AS Departament,
    d.Tip AS TipDepartament,
    p.Nume + ' ' + p.Prenume AS NumeComplet,
    p.CNP,
    pm.Functie,
    pm.NumarLicenta,
    pm.DataAngajare,
    DATEDIFF(YEAR, pm.DataAngajare, GETDATE()) AS AniExperienta,
    CASE WHEN pm.DataPlecare IS NULL THEN 'Activ' ELSE 'Plecat' END AS Status,
    pm.DataPlecare,
    p.Email,
    p.Telefon
FROM PersonalMedical pm
INNER JOIN Departamente d ON pm.DepartamentId = d.DepartamentID
INNER JOIN Persoana p ON pm.PersoanaId = p.Id;
GO

-- ==============================================
-- View pentru ierarhia complet? a departamentelor
-- ==============================================
CREATE OR ALTER VIEW [dbo].[vw_IerarhieDepartamente]
AS
WITH CaiIerarhice AS (
    -- CTE recursiv pentru a construi c?ile
    SELECT 
        d.DepartamentID,
        d.Nume,
        d.Tip,
        0 as Nivel,
        CAST(d.Nume AS NVARCHAR(1000)) AS Cale,
        d.DepartamentID AS RootID
    FROM Departamente d
    WHERE d.Tip = 'Categorie' -- Start de la categorii (root)
    
    UNION ALL
    
    SELECT 
        copil.DepartamentID,
        copil.Nume,
        copil.Tip,
        parinte.Nivel + 1,
        CAST(parinte.Cale + ' > ' + copil.Nume AS NVARCHAR(1000)),
        parinte.RootID
    FROM CaiIerarhice parinte
    INNER JOIN DepartamenteIerarhie h ON parinte.DepartamentID = h.AncestorID AND h.Nivel = 1
    INNER JOIN Departamente copil ON h.DescendantID = copil.DepartamentID
)
SELECT 
    DepartamentID,
    Nume AS NumeDepartament,
    Tip,
    Nivel,
    Cale AS CaleaCompleta,
    RootID AS CategorieID,
    REPLICATE('  ', Nivel) + Nume AS NumeIndentat
FROM CaiIerarhice;
GO

-- ==============================================
-- View pentru statistici generale
-- ==============================================
CREATE OR ALTER VIEW [dbo].[vw_StatisticiGenerale]
AS
SELECT 
    'Medicamente' AS Categoria,
    'Total active' AS Subcategorie,
    COUNT(*) AS Valoare
FROM Medicament 
WHERE Activ = 1

UNION ALL

SELECT 
    'Medicamente',
    'În stoc',
    COUNT(*)
FROM Medicament 
WHERE Activ = 1 AND Stoc > 0

UNION ALL

SELECT 
    'Medicamente',
    'Epuizate',
    COUNT(*)
FROM Medicament 
WHERE Activ = 1 AND Stoc = 0

UNION ALL

SELECT 
    'Departamente',
    'Total',
    COUNT(*)
FROM Departamente

UNION ALL

SELECT 
    'Personal Medical',
    'Total activ',
    COUNT(*)
FROM PersonalMedical 
WHERE Activ = 1

UNION ALL

SELECT 
    'Dispozitive Medicale',
    'Total',
    COUNT(*)
FROM DispozitiveMedicale

UNION ALL

SELECT 
    'Materiale Sanitare',
    'Total',
    COUNT(*)
FROM MaterialeSanitare;
GO

PRINT 'View-urile pentru raportare au fost create cu succes!'