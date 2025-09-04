---------------------------------------------------
-- 0. Creare tabele
---------------------------------------------------
IF OBJECT_ID('DepartamenteIerarhie', 'U') IS NOT NULL DROP TABLE DepartamenteIerarhie;
IF OBJECT_ID('Departamente', 'U') IS NOT NULL DROP TABLE Departamente;

CREATE TABLE Departamente (
    DepartamentID UNIQUEIDENTIFIER NOT NULL 
        DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    Nume NVARCHAR(200) NOT NULL,
    Tip NVARCHAR(50) NOT NULL   -- ex: Categorie, Specialitate, Subspecialitate
);

CREATE TABLE DepartamenteIerarhie (
    AncestorID UNIQUEIDENTIFIER NOT NULL,
    DescendantID UNIQUEIDENTIFIER NOT NULL,
    Nivel INT NOT NULL,   -- 0 = self, 1 = copil direct, 2+ = descendent la distanța

    CONSTRAINT PK_DepartamenteIerarhie PRIMARY KEY (AncestorID, DescendantID),
    CONSTRAINT FK_DepartamenteIerarhie_Ancestor FOREIGN KEY (AncestorID) REFERENCES Departamente(DepartamentID),
    CONSTRAINT FK_DepartamenteIerarhie_Descendant FOREIGN KEY (DescendantID) REFERENCES Departamente(DepartamentID)
);
---------------------------------------------------
-- 1. VIZUALIZARE SIMPLĂ - Structura completă cu indentare
---------------------------------------------------
WITH HierarhieIndentata AS (
    SELECT 
        d.DepartamentID,
        d.Nume,
        d.Tip,
        h.Nivel,
        REPLICATE('  ', h.Nivel) + d.Nume AS NumeIndentat
    FROM Departamente d
    INNER JOIN DepartamenteIerarhie h ON d.DepartamentID = h.DescendantID
    WHERE h.AncestorID IN (
        -- Găsim root-urile (categoriile)
        SELECT DepartamentID FROM Departamente WHERE Tip = 'Categorie'
    )
)
SELECT 
    NumeIndentat AS [Structura Ierarhica],
    Tip,
    Nivel
FROM HierarhieIndentata
ORDER BY NumeIndentat;

---------------------------------------------------
-- 2. VIZUALIZARE CU CĂILE COMPLETE
---------------------------------------------------
WITH CaiIerarhice AS (
    -- CTE recursiv pentru a construi căile
    SELECT 
        d.DepartamentID,
        d.Nume,
        d.Tip,
        0 as Nivel,
        CAST(d.Nume AS NVARCHAR(1000)) AS Cale
    FROM Departamente d
    WHERE d.Tip = 'Categorie' -- Start de la categorii (root)
    
    UNION ALL
    
    SELECT 
        copil.DepartamentID,
        copil.Nume,
        copil.Tip,
        parinte.Nivel + 1,
        CAST(parinte.Cale + ' > ' + copil.Nume AS NVARCHAR(1000))
    FROM CaiIerarhice parinte
    INNER JOIN DepartamenteIerarhie h ON parinte.DepartamentID = h.AncestorID AND h.Nivel = 1
    INNER JOIN Departamente copil ON h.DescendantID = copil.DepartamentID
)
SELECT 
    Cale AS [Calea Completa],
    Tip,
    Nivel
FROM CaiIerarhice
ORDER BY Cale;

---------------------------------------------------
-- 3. VIZUALIZARE PENTRU O CATEGORIE SPECIFICĂ
---------------------------------------------------
CREATE PROCEDURE VizualizeazaCategorie
    @NumeCategorie NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CategorieID UNIQUEIDENTIFIER;
    
    SELECT @CategorieID = DepartamentID 
    FROM Departamente 
    WHERE Nume = @NumeCategorie AND Tip = 'Categorie';
    
    IF @CategorieID IS NULL
    BEGIN
        PRINT 'Categoria specificată nu există!';
        RETURN;
    END
    
    WITH Subarborele AS (
        SELECT 
            d.DepartamentID,
            d.Nume,
            d.Tip,
            h.Nivel,
            REPLICATE('│  ', h.Nivel) + 
            CASE 
                WHEN h.Nivel = 0 THEN '┌─ '
                WHEN h.Nivel = 1 THEN '├─ '
                ELSE '└─ '
            END + d.Nume AS StructuraVisuala
        FROM Departamente d
        INNER JOIN DepartamenteIerarhie h ON d.DepartamentID = h.DescendantID
        WHERE h.AncestorID = @CategorieID
    )
    SELECT 
        StructuraVisuala AS [Structura Vizuala],
        Tip,
        Nivel
    FROM Subarborele
    ORDER BY Nivel, Nume;
END;
GO

---------------------------------------------------
-- 4. STATISTICI IERARHIE
---------------------------------------------------
CREATE VIEW StatisticiIerarhie AS
SELECT 
    cat.Nume AS Categorie,
    COUNT(CASE WHEN d.Tip = 'Specialitate' THEN 1 END) AS NumarSpecialitati,
    COUNT(CASE WHEN d.Tip = 'Subspecialitate' THEN 1 END) AS NumarSubspecialitati,
    COUNT(*) - 1 AS TotalDescendenti  -- -1 pentru a exclude categoria însăși
FROM Departamente cat
LEFT JOIN DepartamenteIerarhie h ON cat.DepartamentID = h.AncestorID
LEFT JOIN Departamente d ON h.DescendantID = d.DepartamentID AND h.Nivel > 0
WHERE cat.Tip = 'Categorie'
GROUP BY cat.DepartamentID, cat.Nume;
GO

---------------------------------------------------
-- 5. GĂSIRE PĂRINȚI ȘI COPII
---------------------------------------------------
CREATE PROCEDURE GasesteRelatii
    @NumeDepartament NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeptID UNIQUEIDENTIFIER;
    
    SELECT @DeptID = DepartamentID 
    FROM Departamente 
    WHERE Nume = @NumeDepartament;
    
    IF @DeptID IS NULL
    BEGIN
        PRINT 'Departamentul specificat nu există!';
        RETURN;
    END
    
    -- Părintele direct
    PRINT '=== PĂRINTELE DIRECT ===';
    SELECT 
        p.Nume AS ParinteDirect,
        p.Tip
    FROM DepartamenteIerarhie h
    INNER JOIN Departamente p ON h.AncestorID = p.DepartamentID
    WHERE h.DescendantID = @DeptID AND h.Nivel = 1;
    
    -- Copiii direcți
    PRINT '=== COPIII DIRECȚI ===';
    SELECT 
        c.Nume AS CopilDirect,
        c.Tip
    FROM DepartamenteIerarhie h
    INNER JOIN Departamente c ON h.DescendantID = c.DepartamentID
    WHERE h.AncestorID = @DeptID AND h.Nivel = 1;
    
    -- Toți strămoșii
    PRINT '=== TOȚI STRĂMOȘII ===';
    SELECT 
        s.Nume AS Stramos,
        s.Tip,
        h.Nivel AS DistantaNivele
    FROM DepartamenteIerarhie h
    INNER JOIN Departamente s ON h.AncestorID = s.DepartamentID
    WHERE h.DescendantID = @DeptID AND h.Nivel > 0
    ORDER BY h.Nivel DESC;
    
    -- Toți descendenții
    PRINT '=== TOȚI DESCENDENȚII ===';
    SELECT 
        d.Nume AS Descendent,
        d.Tip,
        h.Nivel AS DistantaNivele
    FROM DepartamenteIerarhie h
    INNER JOIN Departamente d ON h.DescendantID = d.DepartamentID
    WHERE h.AncestorID = @DeptID AND h.Nivel > 0
    ORDER BY h.Nivel, d.Nume;
END;
GO

---------------------------------------------------
-- 6. EXEMPLE DE UTILIZARE
---------------------------------------------------

-- Vizualizare completă
SELECT '=== STRUCTURA COMPLETĂ CU INDENTARE ===' AS Info;
WITH HierarhieIndentata AS (
    SELECT 
        d.DepartamentID,
        d.Nume,
        d.Tip,
        h.Nivel,
        REPLICATE('  ', h.Nivel) + d.Nume AS NumeIndentat
    FROM Departamente d
    INNER JOIN DepartamenteIerarhie h ON d.DepartamentID = h.DescendantID
    WHERE h.AncestorID IN (
        SELECT DepartamentID FROM Departamente WHERE Tip = 'Categorie'
    )
)
SELECT 
    NumeIndentat AS [Structura Ierarhica],
    Tip,
    Nivel
FROM HierarhieIndentata
ORDER BY NumeIndentat;

-- Statistici
SELECT '=== STATISTICI IERARHIE ===' AS Info;
SELECT * FROM StatisticiIerarhie;

-- Exemplu de utilizare proceduri
PRINT '=== EXEMPLU VIZUALIZARE CATEGORIE ===';
EXEC VizualizeazaCategorie N'Medicale';

PRINT '=== EXEMPLU GĂSIRE RELAȚII ===';
EXEC GasesteRelatii N'Medicina internă';