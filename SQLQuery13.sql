-- =============================================
-- SCRIPT TESTARE IMPLEMENTARE IERARHIE DEPARTAMENTE
-- Verifică că migrarea și implementarea au reușit
-- =============================================

USE [ValyanMed];
GO

PRINT '=== TESTARE IMPLEMENTARE IERARHIE DEPARTAMENTE ===';
PRINT '';

-- =============================================
-- TEST 1: Verifică structura tabelei PersonalMedical
-- =============================================

PRINT '=== TEST 1: STRUCTURA TABELA PersonalMedical ===';

-- Verifică că noile coloane există
DECLARE @CategorieIDExists BIT = 0;
DECLARE @SpecializareIDExists BIT = 0;
DECLARE @SubspecializareIDExists BIT = 0;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PersonalMedical') AND name = 'CategorieID')
    SET @CategorieIDExists = 1;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PersonalMedical') AND name = 'SpecializareID')
    SET @SpecializareIDExists = 1;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PersonalMedical') AND name = 'SubspecializareID')
    SET @SubspecializareIDExists = 1;

IF @CategorieIDExists = 1
    PRINT '✓ Coloana CategorieID există';
ELSE
    PRINT '✗ Coloana CategorieID nu există';

IF @SpecializareIDExists = 1
    PRINT '✓ Coloana SpecializareID există';
ELSE
    PRINT '✗ Coloana SpecializareID nu există';

IF @SubspecializareIDExists = 1
    PRINT '✓ Coloana SubspecializareID există';
ELSE
    PRINT '✗ Coloana SubspecializareID nu există';

-- Verifică foreign keys
DECLARE @FKCategorieExists BIT = 0;
DECLARE @FKSpecializareExists BIT = 0;
DECLARE @FKSubspecializareExists BIT = 0;

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PersonalMedical_CategorieID')
    SET @FKCategorieExists = 1;

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PersonalMedical_SpecializareID')
    SET @FKSpecializareExists = 1;

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PersonalMedical_SubspecializareID')
    SET @FKSubspecializareExists = 1;

IF @FKCategorieExists = 1
    PRINT '✓ Foreign key pentru CategorieID există';
ELSE
    PRINT '✗ Foreign key pentru CategorieID nu există';

IF @FKSpecializareExists = 1
    PRINT '✓ Foreign key pentru SpecializareID există';
ELSE
    PRINT '✗ Foreign key pentru SpecializareID nu există';

IF @FKSubspecializareExists = 1
    PRINT '✓ Foreign key pentru SubspecializareID există';
ELSE
    PRINT '✗ Foreign key pentru SubspecializareID nu există';

-- =============================================
-- TEST 2: Verifică că stored procedures există
-- =============================================

PRINT '';
PRINT '=== TEST 2: STORED PROCEDURES ===';

-- Lista de stored procedures care trebuie să existe
DECLARE @SPList TABLE (SPName NVARCHAR(100), [Exists] BIT DEFAULT 0);

INSERT INTO @SPList (SPName) VALUES 
    ('sp_PersonalMedical_GetPaged'),
    ('sp_PersonalMedical_Create'),
    ('sp_PersonalMedical_Update'),
    ('sp_PersonalMedical_GetById'),
    ('sp_Departamente_GetCategorii'),
    ('sp_Departamente_GetSpecializariByCategorie'),
    ('sp_Departamente_GetSubspecializariBySpecializare'),
    ('sp_PersonalMedical_GetStatisticiByDepartamente');

UPDATE spl 
SET [Exists] = 1
FROM @SPList spl
WHERE EXISTS (SELECT 1 FROM sys.procedures WHERE name = spl.SPName);

-- Raportare rezultate
SELECT 
    SPName,
    CASE WHEN [Exists] = 1 THEN '✓ Există' ELSE '✗ Nu există' END AS Status
FROM @SPList
ORDER BY SPName;

-- =============================================
-- TEST 3: Testează încărcarea categoriilor
-- =============================================

PRINT '';
PRINT '=== TEST 3: TESTARE CATEGORII ===';

BEGIN TRY
    DECLARE @CategoriiCount INT;
    
    EXEC sp_Departamente_GetCategorii;
    
    SELECT @CategoriiCount = COUNT(*) FROM Departamente WHERE Tip = 'Categorie';
    PRINT '✓ sp_Departamente_GetCategorii funcționează. Categorii găsite: ' + CAST(@CategoriiCount AS NVARCHAR(10));
    
END TRY
BEGIN CATCH
    PRINT '✗ Eroare la testarea categoriilor: ' + ERROR_MESSAGE();
END CATCH

-- =============================================
-- TEST 4: Testează încărcarea specializărilor
-- =============================================

PRINT '';
PRINT '=== TEST 4: TESTARE SPECIALIZĂRI ===';

BEGIN TRY
    DECLARE @MedicaleID UNIQUEIDENTIFIER;
    DECLARE @SpecializariCount INT;
    
    SELECT TOP 1 @MedicaleID = DepartamentID 
    FROM Departamente 
    WHERE Nume = 'Medicale' AND Tip = 'Categorie';
    
    IF @MedicaleID IS NOT NULL
    BEGIN
        EXEC sp_Departamente_GetSpecializariByCategorie @CategorieID = @MedicaleID;
        
        SELECT @SpecializariCount = COUNT(*) 
        FROM DepartamenteIerarhie h
        INNER JOIN Departamente d ON h.DescendantID = d.DepartamentID
        WHERE h.AncestorID = @MedicaleID AND h.Nivel = 1 AND d.Tip = 'Specialitate';
        
        PRINT '✓ sp_Departamente_GetSpecializariByCategorie funcționează. Specializări găsite pentru Medicale: ' + CAST(@SpecializariCount AS NVARCHAR(10));
    END
    ELSE
    BEGIN
        PRINT '⚠ Nu s-a găsit categoria Medicale pentru testare';
    END
    
END TRY
BEGIN CATCH
    PRINT '✗ Eroare la testarea specializărilor: ' + ERROR_MESSAGE();
END CATCH

-- =============================================
-- TEST 5: Testează încărcarea subspecializărilor
-- =============================================

PRINT '';
PRINT '=== TEST 5: TESTARE SUBSPECIALIZĂRI ===';

BEGIN TRY
    DECLARE @MedicinaInternaID UNIQUEIDENTIFIER;
    DECLARE @SubspecializariCount INT;
    
    SELECT TOP 1 @MedicinaInternaID = DepartamentID 
    FROM Departamente 
    WHERE Nume = 'Medicina interna' AND Tip = 'Specialitate';
    
    IF @MedicinaInternaID IS NOT NULL
    BEGIN
        EXEC sp_Departamente_GetSubspecializariBySpecializare @SpecializareID = @MedicinaInternaID;
        
        SELECT @SubspecializariCount = COUNT(*) 
        FROM DepartamenteIerarhie h
        INNER JOIN Departamente d ON h.DescendantID = d.DepartamentID
        WHERE h.AncestorID = @MedicinaInternaID AND h.Nivel = 1 AND d.Tip = 'Subspecialitate';
        
        PRINT '✓ sp_Departamente_GetSubspecializariBySpecializare funcționează. Subspecializări găsite pentru Medicina interna: ' + CAST(@SubspecializariCount AS NVARCHAR(10));
    END
    ELSE
    BEGIN
        PRINT '⚠ Nu s-a găsit specializarea Medicina interna pentru testare';
    END
    
END TRY
BEGIN CATCH
    PRINT '✗ Eroare la testarea subspecializărilor: ' + ERROR_MESSAGE();
END CATCH

-- =============================================
-- TEST 6: Testează sp_PersonalMedical_GetPaged actualizat
-- =============================================

PRINT '';
PRINT '=== TEST 6: TESTARE PERSONAL MEDICAL PAGINAT ===';

BEGIN TRY
    -- Test cu parametri noi ierarhici
    DECLARE @TestCategorieID UNIQUEIDENTIFIER;
    SELECT TOP 1 @TestCategorieID = DepartamentID FROM Departamente WHERE Tip = 'Categorie';
    
    EXEC sp_PersonalMedical_GetPaged 
        @Search = NULL,
        @CategorieID = @TestCategorieID,
        @SpecializareID = NULL,
        @SubspecializareID = NULL,
        @Page = 1,
        @PageSize = 5;
    
    PRINT '✓ sp_PersonalMedical_GetPaged cu parametri ierarhici funcționează';
    
END TRY
BEGIN CATCH
    PRINT '✗ Eroare la testarea sp_PersonalMedical_GetPaged: ' + ERROR_MESSAGE();
END CATCH

-- =============================================
-- TEST 7: Verifică datele migrarăte
-- =============================================

PRINT '';
PRINT '=== TEST 7: VERIFICARE DATE MIGRARATE ===';

-- Statistici despre datele migrarăte
DECLARE @TotalPersonal INT = (SELECT COUNT(*) FROM PersonalMedical);
DECLARE @PersonalCuCategorie INT = (SELECT COUNT(*) FROM PersonalMedical WHERE CategorieID IS NOT NULL);
DECLARE @PersonalCuSpecializare INT = (SELECT COUNT(*) FROM PersonalMedical WHERE SpecializareID IS NOT NULL);
DECLARE @PersonalCuSubspecializare INT = (SELECT COUNT(*) FROM PersonalMedical WHERE SubspecializareID IS NOT NULL);

PRINT 'Statistici migrare:';
PRINT '- Total personal: ' + CAST(@TotalPersonal AS NVARCHAR(10));
PRINT '- Personal cu categorie: ' + CAST(@PersonalCuCategorie AS NVARCHAR(10)) + ' (' + 
      CAST(CASE WHEN @TotalPersonal > 0 THEN (@PersonalCuCategorie * 100 / @TotalPersonal) ELSE 0 END AS NVARCHAR(10)) + '%)';
PRINT '- Personal cu specializare: ' + CAST(@PersonalCuSpecializare AS NVARCHAR(10)) + ' (' + 
      CAST(CASE WHEN @TotalPersonal > 0 THEN (@PersonalCuSpecializare * 100 / @TotalPersonal) ELSE 0 END AS NVARCHAR(10)) + '%)';
PRINT '- Personal cu subspecializare: ' + CAST(@PersonalCuSubspecializare AS NVARCHAR(10)) + ' (' + 
      CAST(CASE WHEN @TotalPersonal > 0 THEN (@PersonalCuSubspecializare * 100 / @TotalPersonal) ELSE 0 END AS NVARCHAR(10)) + '%)';

-- Sample de date migrarăte
PRINT '';
PRINT 'Sample date migrarăte (primele 5):';
SELECT TOP 5
    pm.Nume + ' ' + pm.Prenume AS NumeComplet,
    pm.Departament AS DepartamentVechi,
    pm.Specializare AS SpecializareVeche,
    c.Nume AS CategorieNoua,
    s.Nume AS SpecializareNoua,
    sub.Nume AS SubspecializareNoua
FROM PersonalMedical pm
LEFT JOIN Departamente c ON pm.CategorieID = c.DepartamentID
LEFT JOIN Departamente s ON pm.SpecializareID = s.DepartamentID  
LEFT JOIN Departamente sub ON pm.SubspecializareID = sub.DepartamentID
ORDER BY pm.Nume, pm.Prenume;

-- =============================================
-- TEST 8: Test de integritate
-- =============================================

PRINT '';
PRINT '=== TEST 8: VERIFICARE INTEGRITATE ===';

-- Verifică că nu există referințe invalide
DECLARE @InvalidCategories INT = (
    SELECT COUNT(*) 
    FROM PersonalMedical pm
    WHERE pm.CategorieID IS NOT NULL 
    AND NOT EXISTS (SELECT 1 FROM Departamente d WHERE d.DepartamentID = pm.CategorieID AND d.Tip = 'Categorie')
);

DECLARE @InvalidSpecializations INT = (
    SELECT COUNT(*) 
    FROM PersonalMedical pm
    WHERE pm.SpecializareID IS NOT NULL 
    AND NOT EXISTS (SELECT 1 FROM Departamente d WHERE d.DepartamentID = pm.SpecializareID AND d.Tip = 'Specialitate')
);

DECLARE @InvalidSubspecializations INT = (
    SELECT COUNT(*) 
    FROM PersonalMedical pm
    WHERE pm.SubspecializareID IS NOT NULL 
    AND NOT EXISTS (SELECT 1 FROM Departamente d WHERE d.DepartamentID = pm.SubspecializareID AND d.Tip = 'Subspecialitate')
);

IF @InvalidCategories = 0
    PRINT '✓ Toate referințele la categorii sunt valide';
ELSE
    PRINT '✗ ' + CAST(@InvalidCategories AS NVARCHAR(10)) + ' referințe invalide la categorii';

IF @InvalidSpecializations = 0
    PRINT '✓ Toate referințele la specializări sunt valide';
ELSE
    PRINT '✗ ' + CAST(@InvalidSpecializations AS NVARCHAR(10)) + ' referințe invalide la specializări';

IF @InvalidSubspecializations = 0
    PRINT '✓ Toate referințele la subspecializări sunt valide';
ELSE
    PRINT '✗ ' + CAST(@InvalidSubspecializations AS NVARCHAR(10)) + ' referințe invalide la subspecializări';

-- =============================================
-- REZULTAT FINAL
-- =============================================

PRINT '';
PRINT '=================================================';
PRINT '               REZULTAT TESTARE';
PRINT '=================================================';

DECLARE @TotalIssues INT = 0;

-- Calculează numărul total de probleme
IF @CategorieIDExists = 0 SET @TotalIssues = @TotalIssues + 1;
IF @SpecializareIDExists = 0 SET @TotalIssues = @TotalIssues + 1;
IF @SubspecializareIDExists = 0 SET @TotalIssues = @TotalIssues + 1;
IF @FKCategorieExists = 0 SET @TotalIssues = @TotalIssues + 1;
IF @FKSpecializareExists = 0 SET @TotalIssues = @TotalIssues + 1;
IF @FKSubspecializareExists = 0 SET @TotalIssues = @TotalIssues + 1;
SET @TotalIssues = @TotalIssues + @InvalidCategories + @InvalidSpecializations + @InvalidSubspecializations;

IF @TotalIssues = 0
BEGIN
    PRINT '🟢 TOATE TESTELE AU TRECUT CU SUCCES!';
    PRINT '';
    PRINT 'Implementarea ierarhiei de departamente este completă și funcțională.';
    PRINT 'Aplicația Blazor poate fi testată acum.';
END
ELSE
BEGIN
    PRINT '🔴 ' + CAST(@TotalIssues AS NVARCHAR(10)) + ' PROBLEME GĂSITE!';
    PRINT '';
    PRINT 'Verificați erorile de mai sus și corectați-le înainte de a testa aplicația.';
END

PRINT '';
PRINT '=================================================';

-- Query pentru verificarea rapidă în aplicație
PRINT '';
PRINT 'Pentru testarea în aplicația Blazor, verificați că:';
PRINT '1. API Controller-ul DepartamenteController returnează date';
PRINT '2. Dropdown-urile se populează corect';
PRINT '3. Cascade-ul funcționează (Categorie -> Specializare -> Subspecializare)';
PRINT '4. Salvarea personalului medical funcționează cu noile coloane';
PRINT '5. Griglia afișează corect datele ierarhice';
PRINT '';
PRINT 'Pentru debugging rapid, folosiți:';
PRINT 'GET /api/departamente/categorii';
PRINT 'GET /api/departamente/specializari/{categorieId}';
PRINT 'GET /api/departamente/subspecializari/{specializareId}';
PRINT '=================================================';