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

-- Creeaz? tabel? temporar? pentru backup
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
PRINT 'Backup creat cu ' + CAST(@BackupCount AS NVARCHAR(10)) + ' înregistr?ri';

-- =============================================
-- PASUL 2: AD?UGARE COLOANE NOI
-- =============================================

PRINT '';
PRINT '=== PASUL 2: AD?UGARE COLOANE NOI ===';

-- Verific? ?i adaug? coloana CategorieID
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PersonalMedical') AND name = 'CategorieID')
BEGIN
    ALTER TABLE PersonalMedical ADD CategorieID UNIQUEIDENTIFIER NULL;
    PRINT '? Coloana CategorieID ad?ugat?';
END
ELSE
    PRINT '? Coloana CategorieID exist? deja';

-- Verific? ?i adaug? coloana SpecializareID  
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PersonalMedical') AND name = 'SpecializareID')
BEGIN
    ALTER TABLE PersonalMedical ADD SpecializareID UNIQUEIDENTIFIER NULL;
    PRINT '? Coloana SpecializareID ad?ugat?';
END
ELSE
    PRINT '? Coloana SpecializareID exist? deja';

-- Verific? ?i adaug? coloana SubspecializareID
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PersonalMedical') AND name = 'SubspecializareID')
BEGIN
    ALTER TABLE PersonalMedical ADD SubspecializareID UNIQUEIDENTIFIER NULL;
    PRINT '? Coloana SubspecializareID ad?ugat?';
END
ELSE
    PRINT '? Coloana SubspecializareID exist? deja';

-- =============================================
-- PASUL 3: AD?UGARE FOREIGN KEYS
-- =============================================

PRINT '';
PRINT '=== PASUL 3: AD?UGARE FOREIGN KEYS ===';

-- Verific? dac? tabelele Departamente ?i DepartamenteIerarhie exist?
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Departamente' AND xtype='U')
BEGIN
    PRINT '? EROARE: Tabelul Departamente nu exist?!';
    PRINT 'Rula?i mai întâi scripturile pentru crearea tabelelor de departamente.';
    RETURN;
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DepartamenteIerarhie' AND xtype='U')
BEGIN
    PRINT '? EROARE: Tabelul DepartamenteIerarhie nu exist?!';
    PRINT 'Rula?i mai întâi scripturile pentru crearea tabelelor de departamente.';
    RETURN;
END

-- Adaug? foreign key pentru CategorieID
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PersonalMedical_CategorieID')
BEGIN
    ALTER TABLE PersonalMedical 
    ADD CONSTRAINT FK_PersonalMedical_CategorieID 
    FOREIGN KEY (CategorieID) REFERENCES Departamente(DepartamentID);
    PRINT '? Foreign key FK_PersonalMedical_CategorieID ad?ugat';
END
ELSE
    PRINT '? Foreign key FK_PersonalMedical_CategorieID exist? deja';

-- Adaug? foreign key pentru SpecializareID
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PersonalMedical_SpecializareID')
BEGIN
    ALTER TABLE PersonalMedical 
    ADD CONSTRAINT FK_PersonalMedical_SpecializareID 
    FOREIGN KEY (SpecializareID) REFERENCES Departamente(DepartamentID);
    PRINT '? Foreign key FK_PersonalMedical_SpecializareID ad?ugat';
END
ELSE
    PRINT '? Foreign key FK_PersonalMedical_SpecializareID exist? deja';

-- Adaug? foreign key pentru SubspecializareID
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PersonalMedical_SubspecializareID')
BEGIN
    ALTER TABLE PersonalMedical 
    ADD CONSTRAINT FK_PersonalMedical_SubspecializareID 
    FOREIGN KEY (SubspecializareID) REFERENCES Departamente(DepartamentID);
    PRINT '? Foreign key FK_PersonalMedical_SubspecializareID ad?ugat';
END
ELSE
    PRINT '? Foreign key FK_PersonalMedical_SubspecializareID exist? deja';

-- =============================================
-- PASUL 4: MIGRAREA DATELOR EXISTENTE
-- =============================================

PRINT '';
PRINT '=== PASUL 4: MIGRAREA DATELOR EXISTENTE ===';

-- Verific? dac? tabelele de departamente au date
DECLARE @CategoriiCount INT = (SELECT COUNT(*) FROM Departamente WHERE Tip = 'Categorie');
DECLARE @SpecializariCount INT = (SELECT COUNT(*) FROM Departamente WHERE Tip = 'Specialitate');

IF @CategoriiCount = 0
BEGIN
    PRINT '? ATEN?IE: Nu exist? categorii în tabelul Departamente!';
    PRINT 'Rula?i scripturile de populare departamente înainte de migrare.';
END
ELSE
    PRINT '? G?site ' + CAST(@CategoriiCount AS NVARCHAR(10)) + ' categorii de departamente';

IF @SpecializariCount = 0
BEGIN
    PRINT '? ATEN?IE: Nu exist? specializ?ri în tabelul Departamente!';
    PRINT 'Rula?i scripturile de populare departamente înainte de migrare.';
END
ELSE
    PRINT '? G?site ' + CAST(@SpecializariCount AS NVARCHAR(10)) + ' specializ?ri';

-- Func?ie de mapare departamente vechi -> categorii noi
PRINT '';
PRINT 'Începerea migr?rii datelor...';

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

-- Func?ie de mapare specializ?ri vechi -> specializ?ri noi  
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

-- Mapare subspecializ?ri pentru cazurile specifice
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
PRINT 'Rezultatele migr?rii:';
PRINT '- Total înregistr?ri: ' + CAST(@TotalRecords AS NVARCHAR(10));
PRINT '- Categorii mapate: ' + CAST(@MigratedCategories AS NVARCHAR(10));
PRINT '- Specializ?ri mapate: ' + CAST(@MigratedSpecializations AS NVARCHAR(10));
PRINT '- Subspecializ?ri mapate: ' + CAST(@MigratedSubspecializations AS NVARCHAR(10));

-- =============================================
-- PASUL 5: INDEXURI PENTRU PERFORMAN??
-- =============================================

PRINT '';
PRINT '=== PASUL 5: AD?UGARE INDEXURI ===';

-- Index pentru CategorieID
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_PersonalMedical_CategorieID')
BEGIN
    CREATE INDEX IX_PersonalMedical_CategorieID ON PersonalMedical(CategorieID);
    PRINT '? Index IX_PersonalMedical_CategorieID creat';
END
ELSE
    PRINT '? Index IX_PersonalMedical_CategorieID exist? deja';

-- Index pentru SpecializareID
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_PersonalMedical_SpecializareID')
BEGIN
    CREATE INDEX IX_PersonalMedical_SpecializareID ON PersonalMedical(SpecializareID);
    PRINT '? Index IX_PersonalMedical_SpecializareID creat';
END
ELSE
    PRINT '? Index IX_PersonalMedical_SpecializareID exist? deja';

-- Index pentru SubspecializareID
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_PersonalMedical_SubspecializareID')
BEGIN
    CREATE INDEX IX_PersonalMedical_SubspecializareID ON PersonalMedical(SubspecializareID);
    PRINT '? Index IX_PersonalMedical_SubspecializareID creat';
END
ELSE
    PRINT '? Index IX_PersonalMedical_SubspecializareID exist? deja';

-- Index compus pentru c?ut?ri rapide
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_PersonalMedical_Ierarhie_Activ')
BEGIN
    CREATE INDEX IX_PersonalMedical_Ierarhie_Activ ON PersonalMedical(CategorieID, SpecializareID, EsteActiv);
    PRINT '? Index IX_PersonalMedical_Ierarhie_Activ creat';
END
ELSE
    PRINT '? Index IX_PersonalMedical_Ierarhie_Activ exist? deja';

-- =============================================
-- PASUL 6: VALID?RI FINALE
-- =============================================

PRINT '';
PRINT '=== PASUL 6: VALID?RI FINALE ===';

-- Verific? c? nu am pierdut date
DECLARE @OriginalCount INT = (SELECT COUNT(*) FROM PersonalMedical_Backup_Migration);
DECLARE @CurrentCount INT = (SELECT COUNT(*) FROM PersonalMedical);

IF @OriginalCount = @CurrentCount
    PRINT '? Num?rul de înregistr?ri este acela?i (' + CAST(@CurrentCount AS NVARCHAR(10)) + ')';
ELSE
    PRINT '? ATEN?IE: Num?rul de înregistr?ri s-a schimbat! Original: ' + CAST(@OriginalCount AS NVARCHAR(10)) + ', Curent: ' + CAST(@CurrentCount AS NVARCHAR(10));

-- Verific? integritatea foreign keys
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
    PRINT '? Toate categoriile sunt valide';
ELSE
    PRINT '? ATEN?IE: ' + CAST(@InvalidCategories AS NVARCHAR(10)) + ' categorii invalide g?site';

IF @InvalidSpecializations = 0
    PRINT '? Toate specializ?rile sunt valide';
ELSE
    PRINT '? ATEN?IE: ' + CAST(@InvalidSpecializations AS NVARCHAR(10)) + ' specializ?ri invalide g?site';

IF @InvalidSubspecializations = 0
    PRINT '? Toate subspecializ?rile sunt valide';
ELSE
    PRINT '? ATEN?IE: ' + CAST(@InvalidSubspecializations AS NVARCHAR(10)) + ' subspecializ?ri invalide g?site';

-- =============================================
-- PASUL 7: RECOMAND?RI FINALE
-- =============================================

PRINT '';
PRINT '=== RECOMAND?RI FINALE ===';
PRINT '1. Verifica?i datele migrar?te în tabelul PersonalMedical';
PRINT '2. Testa?i aplica?ia pentru a vedea dac? totul func?ioneaz? corect';
PRINT '3. Doar dup? testarea complet?, ?terge?i tabelul PersonalMedical_Backup_Migration';
PRINT '4. Actualiza?i stored procedures ?i queries pentru a folosi noile coloane';
PRINT '5. Actualiza?i aplica?ia Blazor pentru a folosi dropdownurile ierarhice';

PRINT '';
PRINT '=== MIGRARE COMPLET? CU SUCCES! ===';

-- Query pentru verificarea rapid? a datelor migrar?te
PRINT '';
PRINT 'Pentru verificarea rapid? a datelor migrar?te, rula?i:';
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