-- =============================================
-- SCRIPT POPULARE DEPARTAMENTE IERARHICE
-- Bazat pe structura din forinsertintoDepartment.txt
-- =============================================

USE [ValyanMed];
GO

-- Cur??? datele existente
DELETE FROM DepartamenteIerarhie;
DELETE FROM Departamente;

-- =============================================
-- FAZA 1: INSERARE CATEGORII (DOMENII)
-- =============================================
DECLARE @CategoriiTemp TABLE (
    TempID INT IDENTITY(1,1),
    Nume NVARCHAR(200),
    DepartamentID UNIQUEIDENTIFIER
);

INSERT INTO @CategoriiTemp (Nume, DepartamentID)
VALUES 
    (N'Medicale', NEWID()),
    (N'Chirurgicale', NEWID()),
    (N'Pediatrice', NEWID()),
    (N'Critice si urgente', NEWID()),
    (N'Suport si diagnostic', NEWID()),
    (N'Speciale', NEWID());

-- Inserare categorii în tabela principal?
INSERT INTO Departamente (DepartamentID, Nume, Tip)
SELECT DepartamentID, Nume, N'Categorie'
FROM @CategoriiTemp;

-- Inserare self-references în ierarhie pentru categorii
INSERT INTO DepartamenteIerarhie (AncestorID, DescendantID, Nivel)
SELECT DepartamentID, DepartamentID, 0
FROM @CategoriiTemp;

-- =============================================
-- FAZA 2: INSERARE SPECIALIT??I (SEC?II)
-- =============================================
DECLARE @SpecialitatiTemp TABLE (
    TempID INT IDENTITY(1,1),
    Nume NVARCHAR(200),
    DepartamentID UNIQUEIDENTIFIER,
    CategorieNume NVARCHAR(200)
);

INSERT INTO @SpecialitatiTemp (Nume, DepartamentID, CategorieNume)
VALUES 
    -- Medicale
    (N'Medicina interna', NEWID(), N'Medicale'),
    (N'Neurologie', NEWID(), N'Medicale'),
    (N'Psihiatrie adulti', NEWID(), N'Medicale'),
    (N'Psihiatrie pediatrica', NEWID(), N'Medicale'),
    (N'Dermatologie', NEWID(), N'Medicale'),
    (N'Boli infectioase', NEWID(), N'Medicale'),
    (N'Geriatrie', NEWID(), N'Medicale'),
    (N'Medic. fizica si de reabilitare', NEWID(), N'Medicale'),
    (N'Medic. paliativa', NEWID(), N'Medicale'),
    (N'Medic. ocupationala', NEWID(), N'Medicale'),
    (N'Medic. preventiva si sanatate publica', NEWID(), N'Medicale'),
    
    -- Chirurgicale
    (N'Chirurgie generala', NEWID(), N'Chirurgicale'),
    (N'Chirurgie cardiovasculara', NEWID(), N'Chirurgicale'),
    (N'Chirurgie toracica', NEWID(), N'Chirurgicale'),
    (N'Chirurgie vasculara', NEWID(), N'Chirurgicale'),
    (N'Neurochirurgie', NEWID(), N'Chirurgicale'),
    (N'Ortopedie si Traumatologie', NEWID(), N'Chirurgicale'),
    (N'Chirurgie plastica, estetica si reparatorie', NEWID(), N'Chirurgicale'),
    (N'Chirurgie pediatrica', NEWID(), N'Chirurgicale'),
    (N'Chirurgie oro-maxilo-faciala', NEWID(), N'Chirurgicale'),
    (N'Otorinolaringologie (ORL)', NEWID(), N'Chirurgicale'),
    (N'Oftalmologie', NEWID(), N'Chirurgicale'),
    (N'Urologie', NEWID(), N'Chirurgicale'),
    (N'Obstetrica si Ginecologie', NEWID(), N'Chirurgicale'),
    (N'Chirurgie oncologica', NEWID(), N'Chirurgicale'),
    
    -- Pediatrice
    (N'Pediatrie generala', NEWID(), N'Pediatrice'),
    (N'Neonatologie', NEWID(), N'Pediatrice'),
    (N'Cardiologie pediatrica', NEWID(), N'Pediatrice'),
    (N'Neurologie pediatrica', NEWID(), N'Pediatrice'),
    (N'Nefrologie pediatrica', NEWID(), N'Pediatrice'),
    (N'Oncologie si hematologie pediatrica', NEWID(), N'Pediatrice'),
    (N'Chirurgie pediatrica', NEWID(), N'Pediatrice'),
    (N'Terapie intensiva pediatrica (PICU)', NEWID(), N'Pediatrice'),
    
    -- Critice si urgente
    (N'Unitate Primiri Urgente (UPU / ER)', NEWID(), N'Critice si urgente'),
    (N'Terapie Intensiva adulti (ICU)', NEWID(), N'Critice si urgente'),
    (N'Terapie Intensiva cardiaca (CCU)', NEWID(), N'Critice si urgente'),
    (N'Terapie Intensiva neonatala (NICU)', NEWID(), N'Critice si urgente'),
    (N'Terapie Intensiva pediatrica (PICU)', NEWID(), N'Critice si urgente'),
    (N'Anestezie si Terapie Intensiva', NEWID(), N'Critice si urgente'),
    
    -- Suport si diagnostic
    (N'Radiologie si imagistica medicala', NEWID(), N'Suport si diagnostic'),
    (N'Medicina nucleara', NEWID(), N'Suport si diagnostic'),
    (N'Laborator analize medicale', NEWID(), N'Suport si diagnostic'),
    (N'Anatomie patologica si citologie', NEWID(), N'Suport si diagnostic'),
    (N'Transfuzii sanguine', NEWID(), N'Suport si diagnostic'),
    (N'Farmacie spitaliceasca', NEWID(), N'Suport si diagnostic'),
    (N'Nutritie clinica si dietetica', NEWID(), N'Suport si diagnostic'),
    (N'Kinetoterapie si recuperare medicala', NEWID(), N'Suport si diagnostic'),
    (N'Psihologie clinica si psihoterapie', NEWID(), N'Suport si diagnostic'),
    
    -- Speciale
    (N'Transplant', NEWID(), N'Speciale'),
    (N'Fertilizare si medicina reproductiva (IVF)', NEWID(), N'Speciale'),
    (N'Medicina sportiva', NEWID(), N'Speciale'),
    (N'Genetica medicala', NEWID(), N'Speciale'),
    (N'Terapia durerii (Pain Management)', NEWID(), N'Speciale');

-- Inserare specialit??i în tabela principal?
INSERT INTO Departamente (DepartamentID, Nume, Tip)
SELECT DepartamentID, Nume, N'Specialitate'
FROM @SpecialitatiTemp;

-- Inserare self-references în ierarhie pentru specialit??i
INSERT INTO DepartamenteIerarhie (AncestorID, DescendantID, Nivel)
SELECT DepartamentID, DepartamentID, 0
FROM @SpecialitatiTemp;

-- Inserare rela?ii parent-child (Categorie -> Specialitate)
INSERT INTO DepartamenteIerarhie (AncestorID, DescendantID, Nivel)
SELECT 
    c.DepartamentID AS AncestorID,
    s.DepartamentID AS DescendantID,
    1 AS Nivel
FROM @CategoriiTemp c
INNER JOIN @SpecialitatiTemp s ON c.Nume = s.CategorieNume;

-- =============================================
-- FAZA 3: INSERARE SUBSPECIALIT??I
-- =============================================
DECLARE @SubspecialitatiTemp TABLE (
    TempID INT IDENTITY(1,1),
    Nume NVARCHAR(200),
    DepartamentID UNIQUEIDENTIFIER,
    SpecialitateNume NVARCHAR(200)
);

INSERT INTO @SubspecialitatiTemp (Nume, DepartamentID, SpecialitateNume)
VALUES 
    -- Medicina interna - subspecialit??i
    (N'Cardiologie', NEWID(), N'Medicina interna'),
    (N'Pneumologie / Medicina respiratorie', NEWID(), N'Medicina interna'),
    (N'Gastroenterologie si Hepatologie', NEWID(), N'Medicina interna'),
    (N'Nefrologie', NEWID(), N'Medicina interna'),
    (N'Endocrinologie si Diabet', NEWID(), N'Medicina interna'),
    (N'Hematologie clinica', NEWID(), N'Medicina interna'),
    (N'Oncologie medicala', NEWID(), N'Medicina interna'),
    (N'Reumatologie', NEWID(), N'Medicina interna'),
    (N'Alergologie si Imunologie clinica', NEWID(), N'Medicina interna'),
    
    -- Radiologie si imagistica medicala - subspecialit??i
    (N'CT', NEWID(), N'Radiologie si imagistica medicala'),
    (N'RMN', NEWID(), N'Radiologie si imagistica medicala'),
    (N'Ecografie', NEWID(), N'Radiologie si imagistica medicala'),
    (N'Radiografie digitala', NEWID(), N'Radiologie si imagistica medicala'),
    
    -- Laborator analize medicale - subspecialit??i
    (N'Biochimie', NEWID(), N'Laborator analize medicale'),
    (N'Hematologie', NEWID(), N'Laborator analize medicale'),
    (N'Microbiologie', NEWID(), N'Laborator analize medicale'),
    (N'Imunologie', NEWID(), N'Laborator analize medicale'),
    
    -- Transplant - subspecialit??i
    (N'Renal', NEWID(), N'Transplant'),
    (N'Hepatic', NEWID(), N'Transplant'),
    (N'Cardiac', NEWID(), N'Transplant'),
    (N'Medular', NEWID(), N'Transplant');

-- Inserare subspecialit??i în tabela principal?
INSERT INTO Departamente (DepartamentID, Nume, Tip)
SELECT DepartamentID, Nume, N'Subspecialitate'
FROM @SubspecialitatiTemp;

-- Inserare self-references în ierarhie pentru subspecialit??i
INSERT INTO DepartamenteIerarhie (AncestorID, DescendantID, Nivel)
SELECT DepartamentID, DepartamentID, 0
FROM @SubspecialitatiTemp;

-- Inserare rela?ii parent-child (Specialitate -> Subspecialitate)
INSERT INTO DepartamenteIerarhie (AncestorID, DescendantID, Nivel)
SELECT 
    spec.DepartamentID AS AncestorID,
    sub.DepartamentID AS DescendantID,
    1 AS Nivel
FROM @SpecialitatiTemp spec
INNER JOIN @SubspecialitatiTemp sub ON spec.Nume = sub.SpecialitateNume;

-- Inserare rela?ii grand-parent (Categorie -> Subspecialitate)
INSERT INTO DepartamenteIerarhie (AncestorID, DescendantID, Nivel)
SELECT 
    cat.DepartamentID AS AncestorID,
    sub.DepartamentID AS DescendantID,
    2 AS Nivel
FROM @CategoriiTemp cat
INNER JOIN @SpecialitatiTemp spec ON cat.Nume = spec.CategorieNume
INNER JOIN @SubspecialitatiTemp sub ON spec.Nume = sub.SpecialitateNume;

-- =============================================
-- VERIFICARE REZULTATE
-- =============================================

-- Statistici generale
SELECT 
    Tip,
    COUNT(*) AS NumarDepartamente
FROM Departamente
GROUP BY Tip
ORDER BY 
    CASE Tip 
        WHEN 'Categorie' THEN 1 
        WHEN 'Specialitate' THEN 2 
        WHEN 'Subspecialitate' THEN 3 
        ELSE 4 
    END;

-- Verificare structura ierarhic?
;WITH StructuraCompleta AS (
    -- CTE recursiv pentru construirea c?ilor complete
    SELECT 
        d.DepartamentID,
        d.Nume,
        d.Tip,
        0 as Nivel,
        CAST(d.Nume AS NVARCHAR(1000)) AS Cale
    FROM Departamente d
    WHERE d.Tip = 'Categorie'
    
    UNION ALL
    
    SELECT 
        copil.DepartamentID,
        copil.Nume,
        copil.Tip,
        parinte.Nivel + 1,
        CAST(parinte.Cale + ' > ' + copil.Nume AS NVARCHAR(1000))
    FROM StructuraCompleta parinte
    INNER JOIN DepartamenteIerarhie h ON parinte.DepartamentID = h.AncestorID AND h.Nivel = 1
    INNER JOIN Departamente copil ON h.DescendantID = copil.DepartamentID
)
SELECT TOP 20
    REPLICATE('  ', Nivel) + Nume AS [Structura Ierarhica],
    Tip,
    Nivel,
    Cale AS [Calea Completa]
FROM StructuraCompleta
ORDER BY Cale;

-- Afi?are statistici pe categorii
SELECT 
    cat.Nume AS Categorie,
    COUNT(CASE WHEN d.Tip = 'Specialitate' THEN 1 END) AS NumarSpecialitati,
    COUNT(CASE WHEN d.Tip = 'Subspecialitate' THEN 1 END) AS NumarSubspecialitati,
    COUNT(*) - 1 AS TotalDescendenti
FROM Departamente cat
LEFT JOIN DepartamenteIerarhie h ON cat.DepartamentID = h.AncestorID
LEFT JOIN Departamente d ON h.DescendantID = d.DepartamentID AND h.Nivel > 0
WHERE cat.Tip = 'Categorie'
GROUP BY cat.DepartamentID, cat.Nume
ORDER BY cat.Nume;

PRINT '=== POPULARE DEPARTAMENTE COMPLET?! ===';
PRINT 'Structura ierarhic? a fost creat? cu succes.';
PRINT 'Verifica?i rezultatele în output-ul de mai sus.';