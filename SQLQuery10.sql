begin tran
-- =============================================
-- SCRIPT MIGRARE DEPARTAMENTE IERARHICE
-- Modificarea tabelei PersonalMedical pentru suportul ierarhiei de departamente
-- =============================================

USE [ValyanMed];
GO

PRINT '=== ÎNCEPERE MIGRARE DEPARTAMENTE IERARHICE ===';
PRINT '';

-- =============================================
-- PASUL 1: BACKUP DATE EXISTENTE
-- =============================================

PRINT '=== PASUL 1: BACKUP DATE EXISTENTE ===';

-- Creează tabelă temporară pentru backup
IF OBJECT_ID('PersonalMedical_Backup_Migration', 'U') IS NOT NULL
    DROP TABLE PersonalMedical_Backup_Migration;

-- Backup datelor existente
SELECT 
    PersonalID,
    Nume,
    Prenume,
    Specializare,
    NumarLicenta,
    Telefon,
    Email,
    Departament,
    Pozitie,
    EsteActiv,
    DataCreare
INTO PersonalMedical_Backup_Migration
FROM PersonalMedical;

DECLARE @BackupCount INT = (SELECT COUNT(*) FROM PersonalMedical_Backup_Migration);
PRINT 'Backup creat cu ' + CAST(@BackupCount AS NVARCHAR(10)) + ' înregistrări';

-- =============================================
-- PASUL 2: ADĂUGARE COLOANE NOI
-- =============================================

PRINT '';
PRINT '=== PASUL 2: ADĂUGARE COLOANE NOI ===';

-- Verifică și adaugă coloana CategorieID
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PersonalMedical') AND name = 'CategorieID')
BEGIN
    ALTER TABLE PersonalMedical ADD CategorieID UNIQUEIDENTIFIER NULL;
    PRINT '✓ Coloana CategorieID adăugată';
END
ELSE
    PRINT '⚠ Coloana CategorieID există deja';

-- Verifică și adaugă coloana SpecializareID  
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PersonalMedical') AND name = 'SpecializareID')
BEGIN
    ALTER TABLE PersonalMedical ADD SpecializareID UNIQUEIDENTIFIER NULL;
    PRINT '✓ Coloana SpecializareID adăugată';
END
ELSE
    PRINT '⚠ Coloana SpecializareID există deja';

-- Verifică și adaugă coloana SubspecializareID
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PersonalMedical') AND name = 'SubspecializareID')
BEGIN
    ALTER TABLE PersonalMedical ADD SubspecializareID UNIQUEIDENTIFIER NULL;
    PRINT '✓ Coloana SubspecializareID adăugată';
END
ELSE
    PRINT '⚠ Coloana SubspecializareID există deja';

-- =============================================
-- PASUL 3: ADĂUGARE FOREIGN KEYS
-- =============================================

PRINT '';
PRINT '=== PASUL 3: ADĂUGARE FOREIGN KEYS ===';

-- Verifică dacă tabelele Departamente și DepartamenteIerarhie există
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Departamente' AND xtype='U')
BEGIN
    PRINT '❌ EROARE: Tabelul Departamente nu există!';
    PRINT 'Rulați mai întâi scripturile pentru crearea tabelelor de departamente.';
    RETURN;
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DepartamenteIerarhie' AND xtype='U')
BEGIN
    PRINT '❌ EROARE: Tabelul DepartamenteIerarhie nu există!';
    PRINT 'Rulați mai întâi scripturile pentru crearea tabelelor de departamente.';
    RETURN;
END

-- Adaugă foreign key pentru CategorieID
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PersonalMedical_CategorieID')
BEGIN
    ALTER TABLE PersonalMedical 
    ADD CONSTRAINT FK_PersonalMedical_CategorieID 
    FOREIGN KEY (CategorieID) REFERENCES Departamente(DepartamentID);
    PRINT '✓ Foreign key FK_PersonalMedical_CategorieID adăugat';
END
ELSE
    PRINT '⚠ Foreign key FK_PersonalMedical_CategorieID există deja';

-- Adaugă foreign key pentru SpecializareID
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PersonalMedical_SpecializareID')
BEGIN
    ALTER TABLE PersonalMedical 
    ADD CONSTRAINT FK_PersonalMedical_SpecializareID 
    FOREIGN KEY (SpecializareID) REFERENCES Departamente(DepartamentID);
    PRINT '✓ Foreign key FK_PersonalMedical_SpecializareID adăugat';
END
ELSE
    PRINT '⚠ Foreign key FK_PersonalMedical_SpecializareID există deja';

-- Adaugă foreign key pentru SubspecializareID
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PersonalMedical_SubspecializareID')
BEGIN
    ALTER TABLE PersonalMedical 
    ADD CONSTRAINT FK_PersonalMedical_SubspecializareID 
    FOREIGN KEY (SubspecializareID) REFERENCES Departamente(DepartamentID);
    PRINT '✓ Foreign key FK_PersonalMedical_SubspecializareID adăugat';
END
ELSE
    PRINT '⚠ Foreign key FK_PersonalMedical_SubspecializareID există deja';

-- =============================================
-- PASUL 4: MIGRAREA DATELOR EXISTENTE
-- =============================================

PRINT '';
PRINT '=== PASUL 4: MIGRAREA DATELOR EXISTENTE ===';

-- Verifică dacă tabelele de departamente au date
DECLARE @CategoriiCount INT = (SELECT COUNT(*) FROM Departamente WHERE Tip = 'Categorie');
DECLARE @SpecializariCount INT = (SELECT COUNT(*) FROM Departamente WHERE Tip = 'Specialitate');

IF @CategoriiCount = 0
BEGIN
    PRINT '❌ ATENȚIE: Nu există categorii în tabelul Departamente!';
    PRINT 'Rulați scripturile de populare departamente înainte de migrare.';
END
ELSE
    PRINT '✓ Găsite ' + CAST(@CategoriiCount AS NVARCHAR(10)) + ' categorii de departamente';

IF @SpecializariCount = 0
BEGIN
    PRINT '❌ ATENȚIE: Nu există specializări în tabelul Departamente!';
    PRINT 'Rulați scripturile de populare departamente înainte de migrare.';
END
ELSE
    PRINT '✓ Găsite ' + CAST(@SpecializariCount AS NVARCHAR(10)) + ' specializări';

-- Funcție de mapare departamente vechi -> categorii noi
PRINT '';
PRINT 'Începerea migrării datelor...';

UPDATE pm
SET CategorieID = (
    CASE 
        WHEN UPPER(pm.Departament) LIKE '%CARDIO%' OR UPPER(pm.Departament) LIKE '%INTERNA%' OR UPPER(pm.Departament) LIKE '%MEDICINA%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Medicale' AND Tip = 'Categorie')
        
        WHEN UPPER(pm.Departament) LIKE '%CHIRURG%' OR UPPER(pm.Departament) LIKE '%OPERATORI%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Chirurgicale' AND Tip = 'Categorie')
        
        WHEN UPPER(pm.Departament) LIKE '%PEDIATR%' OR UPPER(pm.Departament) LIKE '%COPII%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Pediatrice' AND Tip = 'Categorie')
        
        WHEN UPPER(pm.Departament) LIKE '%URGENTA%' OR UPPER(pm.Departament) LIKE '%TERAPIE%' OR UPPER(pm.Departament) LIKE '%ICU%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Critice si urgente' AND Tip = 'Categorie')
        
        WHEN UPPER(pm.Departament) LIKE '%RADIOLOG%' OR UPPER(pm.Departament) LIKE '%LABORATOR%' OR UPPER(pm.Departament) LIKE '%IMAGIST%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Suport si diagnostic' AND Tip = 'Categorie')
        
        WHEN UPPER(pm.Departament) LIKE '%TRANSPLANT%' OR UPPER(pm.Departament) LIKE '%SPECIAL%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Speciale' AND Tip = 'Categorie')
        
        ELSE (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Medicale' AND Tip = 'Categorie') -- Default
    END
)
FROM PersonalMedical pm
WHERE pm.CategorieID IS NULL AND pm.Departament IS NOT NULL;

-- Funcție de mapare specializări vechi -> specializări noi  
UPDATE pm
SET SpecializareID = (
    CASE 
        WHEN UPPER(pm.Specializare) LIKE '%CARDIO%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Medicina interna' AND Tip = 'Specialitate')
        
        WHEN UPPER(pm.Specializare) LIKE '%CHIRURG%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Chirurgie generala' AND Tip = 'Specialitate')
        
        WHEN UPPER(pm.Specializare) LIKE '%PEDIATR%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Pediatrie generala' AND Tip = 'Specialitate')
        
        WHEN UPPER(pm.Specializare) LIKE '%RADIOLOG%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Radiologie si imagistica medicala' AND Tip = 'Specialitate')
        
        WHEN UPPER(pm.Specializare) LIKE '%LABORATOR%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Laborator analize medicale' AND Tip = 'Specialitate')
        
        WHEN UPPER(pm.Specializare) LIKE '%ANESTEZIE%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Anestezie si Terapie Intensiva' AND Tip = 'Specialitate')
        
        WHEN UPPER(pm.Specializare) LIKE '%NEUROLOG%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Neurologie' AND Tip = 'Specialitate')
        
        WHEN UPPER(pm.Specializare) LIKE '%INTERN%' OR UPPER(pm.Specializare) LIKE '%MEDICINA%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Medicina interna' AND Tip = 'Specialitate')
        
        ELSE (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Medicina interna' AND Tip = 'Specialitate') -- Default
    END
)
FROM PersonalMedical pm
WHERE pm.SpecializareID IS NULL AND pm.Specializare IS NOT NULL;

-- Mapare subspecializări pentru cazurile specifice
UPDATE pm
SET SubspecializareID = (
    CASE 
        WHEN UPPER(pm.Specializare) LIKE '%CARDIO%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Cardiologie' AND Tip = 'Subspecialitate')
        
        WHEN UPPER(pm.Specializare) LIKE '%CT%' OR UPPER(pm.Specializare) LIKE '%COMPUTER%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'CT' AND Tip = 'Subspecialitate')
        
        WHEN UPPER(pm.Specializare) LIKE '%RMN%' OR UPPER(pm.Specializare) LIKE '%REZONANTA%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'RMN' AND Tip = 'Subspecialitate')
        
        WHEN UPPER(pm.Specializare) LIKE '%BIOCHIM%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Biochimie' AND Tip = 'Subspecialitate')
        
        WHEN UPPER(pm.Specializare) LIKE '%HEMATOLOG%'
            THEN (SELECT TOP 1 DepartamentID FROM Departamente WHERE Nume = 'Hematologie' AND Tip = 'Subspecialitate')
        
        ELSE NULL -- Marea majoritate nu vor avea subspecializare
    END
)
FROM PersonalMedical pm
WHERE pm.Specializare IS NOT NULL;

-- Raportare rezultate migrare
DECLARE @MigratedCategories INT = (SELECT COUNT(*) FROM PersonalMedical WHERE CategorieID IS NOT NULL);
DECLARE @MigratedSpecializations INT = (SELECT COUNT(*) FROM PersonalMedical WHERE SpecializareID IS NOT NULL);
DECLARE @MigratedSubspecializations INT = (SELECT COUNT(*) FROM PersonalMedical WHERE SubspecializareID IS NOT NULL);
DECLARE @TotalRecords INT = (SELECT COUNT(*) FROM PersonalMedical);

PRINT '';
PRINT 'Rezultatele migrării:';
PRINT '- Total înregistrări: ' + CAST(@TotalRecords AS NVARCHAR(10));
PRINT '- Categorii mapate: ' + CAST(@MigratedCategories AS NVARCHAR(10));
PRINT '- Specializări mapate: ' + CAST(@MigratedSpecializations AS NVARCHAR(10));
PRINT '- Subspecializări mapate: ' + CAST(@MigratedSubspecializations AS NVARCHAR(10));

-- =============================================
-- PASUL 5: INDEXURI PENTRU PERFORMANȚĂ
-- =============================================

PRINT '';
PRINT '=== PASUL 5: ADĂUGARE INDEXURI ===';

-- Index pentru CategorieID
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_PersonalMedical_CategorieID')
BEGIN
    CREATE INDEX IX_PersonalMedical_CategorieID ON PersonalMedical(CategorieID);
    PRINT '✓ Index IX_PersonalMedical_CategorieID creat';
END
ELSE
    PRINT '⚠ Index IX_PersonalMedical_CategorieID există deja';

-- Index pentru SpecializareID
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_PersonalMedical_SpecializareID')
BEGIN
    CREATE INDEX IX_PersonalMedical_SpecializareID ON PersonalMedical(SpecializareID);
    PRINT '✓ Index IX_PersonalMedical_SpecializareID creat';
END
ELSE
    PRINT '⚠ Index IX_PersonalMedical_SpecializareID există deja';

-- Index pentru SubspecializareID
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_PersonalMedical_SubspecializareID')
BEGIN
    CREATE INDEX IX_PersonalMedical_SubspecializareID ON PersonalMedical(SubspecializareID);
    PRINT '✓ Index IX_PersonalMedical_SubspecializareID creat';
END
ELSE
    PRINT '⚠ Index IX_PersonalMedical_SubspecializareID există deja';

-- Index compus pentru căutări rapide
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_PersonalMedical_Ierarhie_Activ')
BEGIN
    CREATE INDEX IX_PersonalMedical_Ierarhie_Activ ON PersonalMedical(CategorieID, SpecializareID, EsteActiv);
    PRINT '✓ Index IX_PersonalMedical_Ierarhie_Activ creat';
END
ELSE
    PRINT '⚠ Index IX_PersonalMedical_Ierarhie_Activ există deja';

-- =============================================
-- PASUL 6: VALIDĂRI FINALE
-- =============================================

PRINT '';
PRINT '=== PASUL 6: VALIDĂRI FINALE ===';

-- Verifică că nu am pierdut date
DECLARE @OriginalCount INT = (SELECT COUNT(*) FROM PersonalMedical_Backup_Migration);
DECLARE @CurrentCount INT = (SELECT COUNT(*) FROM PersonalMedical);

IF @OriginalCount = @CurrentCount
    PRINT '✓ Numărul de înregistrări este același (' + CAST(@CurrentCount AS NVARCHAR(10)) + ')';
ELSE
    PRINT '❌ ATENȚIE: Numărul de înregistrări s-a schimbat! Original: ' + CAST(@OriginalCount AS NVARCHAR(10)) + ', Curent: ' + CAST(@CurrentCount AS NVARCHAR(10));

-- Verifică integritatea foreign keys
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
    PRINT '✓ Toate categoriile sunt valide';
ELSE
    PRINT '❌ ATENȚIE: ' + CAST(@InvalidCategories AS NVARCHAR(10)) + ' categorii invalide găsite';

IF @InvalidSpecializations = 0
    PRINT '✓ Toate specializările sunt valide';
ELSE
    PRINT '❌ ATENȚIE: ' + CAST(@InvalidSpecializations AS NVARCHAR(10)) + ' specializări invalide găsite';

IF @InvalidSubspecializations = 0
    PRINT '✓ Toate subspecializările sunt valide';
ELSE
    PRINT '❌ ATENȚIE: ' + CAST(@InvalidSubspecializations AS NVARCHAR(10)) + ' subspecializări invalide găsite';

-- =============================================
-- PASUL 7: RECOMANDĂRI FINALE
-- =============================================

PRINT '';
PRINT '=== RECOMANDĂRI FINALE ===';
PRINT '1. Verificați datele migrarăte în tabelul PersonalMedical';
PRINT '2. Testați aplicația pentru a vedea dacă totul funcționează corect';
PRINT '3. Doar după testarea completă, ștergeți tabelul PersonalMedical_Backup_Migration';
PRINT '4. Actualizați stored procedures și queries pentru a folosi noile coloane';
PRINT '5. Actualizați aplicația Blazor pentru a folosi dropdownurile ierarhice';

PRINT '';
PRINT '=== MIGRARE COMPLETĂ CU SUCCES! ===';

-- Query pentru verificarea rapidă a datelor migrarăte
PRINT '';
PRINT 'Pentru verificarea rapidă a datelor migrarăte, rulați:';
PRINT '';
PRINT 'SELECT TOP 10';
PRINT '    pm.Nume + '' '' + pm.Prenume AS NumeComplet,';
PRINT '    pm.Departament AS DepartamentVechi,';
PRINT '    pm.Specializare AS SpecializareVeche,';
PRINT '    c.Nume AS CategorieNoua,';
PRINT '    s.Nume AS SpecializareNoua,';
PRINT '    sub.Nume AS SubspecializareNoua';
PRINT 'FROM PersonalMedical pm';
PRINT 'LEFT JOIN Departamente c ON pm.CategorieID = c.DepartamentID';
PRINT 'LEFT JOIN Departamente s ON pm.SpecializareID = s.DepartamentID';  
PRINT 'LEFT JOIN Departamente sub ON pm.SubspecializareID = sub.DepartamentID';
PRINT 'ORDER BY pm.Nume, pm.Prenume;';

rollback