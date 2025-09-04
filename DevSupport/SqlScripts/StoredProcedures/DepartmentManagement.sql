-- Proceduri stocate pentru gestionarea departamentelor medicale
-- Data: 2025-01-04
-- Autor: DevSupport

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ==============================================
-- Procedur? pentru vizualizarea complet? a ierarhiei
-- ==============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_VizualizeazaIerarhieCompleta]
AS
BEGIN
    SET NOCOUNT ON;
    
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
            -- G?sim root-urile (categoriile)
            SELECT DepartamentID FROM Departamente WHERE Tip = 'Categorie'
        )
    )
    SELECT 
        NumeIndentat AS [Structura Ierarhica],
        Tip,
        Nivel
    FROM HierarhieIndentata
    ORDER BY NumeIndentat;
END
GO

-- ==============================================
-- Procedur? pentru vizualizarea unei categorii specifice
-- ==============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_VizualizeazaCategorie]
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
        PRINT 'Categoria specificat? nu exist?!';
        RETURN;
    END
    
    WITH Subarborele AS (
        SELECT 
            d.DepartamentID,
            d.Nume,
            d.Tip,
            h.Nivel,
            REPLICATE('?  ', h.Nivel) + 
            CASE 
                WHEN h.Nivel = 0 THEN '?? '
                WHEN h.Nivel = 1 THEN '?? '
                ELSE '?? '
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
END
GO

-- ==============================================
-- Procedur? pentru g?sirea rela?iilor unui departament
-- ==============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GasesteRelatiiDepartament]
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
        PRINT 'Departamentul specificat nu exist?!';
        RETURN;
    END
    
    -- P?rintele direct
    PRINT '=== P?RINTELE DIRECT ===';
    SELECT 
        p.Nume AS ParinteDirect,
        p.Tip
    FROM DepartamenteIerarhie h
    INNER JOIN Departamente p ON h.AncestorID = p.DepartamentID
    WHERE h.DescendantID = @DeptID AND h.Nivel = 1;
    
    -- Copiii direc?i
    PRINT '=== COPIII DIREC?I ===';
    SELECT 
        c.Nume AS CopilDirect,
        c.Tip
    FROM DepartamenteIerarhie h
    INNER JOIN Departamente c ON h.DescendantID = c.DepartamentID
    WHERE h.AncestorID = @DeptID AND h.Nivel = 1;
    
    -- To?i str?mo?ii
    PRINT '=== TO?I STR?MO?II ===';
    SELECT 
        s.Nume AS Stramos,
        s.Tip,
        h.Nivel AS DistantaNivele
    FROM DepartamenteIerarhie h
    INNER JOIN Departamente s ON h.AncestorID = s.DepartamentID
    WHERE h.DescendantID = @DeptID AND h.Nivel > 0
    ORDER BY h.Nivel DESC;
    
    -- To?i descenden?ii
    PRINT '=== TO?I DESCENDEN?II ===';
    SELECT 
        d.Nume AS Descendent,
        d.Tip,
        h.Nivel AS DistantaNivele
    FROM DepartamenteIerarhie h
    INNER JOIN Departamente d ON h.DescendantID = d.DepartamentID
    WHERE h.AncestorID = @DeptID AND h.Nivel > 0
    ORDER BY h.Nivel, d.Nume;
END
GO

-- ==============================================
-- Procedur? pentru ad?ugarea unui departament nou
-- ==============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_AdaugaDepartament]
    @Nume NVARCHAR(200),
    @Tip NVARCHAR(50),
    @ParinteNume NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @NoiDepartamentID UNIQUEIDENTIFIER = NEWID();
        DECLARE @ParinteID UNIQUEIDENTIFIER = NULL;
        
        -- G?sim p?rintele dac? este specificat
        IF @ParinteNume IS NOT NULL
        BEGIN
            SELECT @ParinteID = DepartamentID 
            FROM Departamente 
            WHERE Nume = @ParinteNume;
            
            IF @ParinteID IS NULL
            BEGIN
                RAISERROR('Departamentul p?rinte specificat nu exist?!', 16, 1);
                RETURN;
            END
        END
        
        -- Inser?m noul departament
        INSERT INTO Departamente (DepartamentID, Nume, Tip)
        VALUES (@NoiDepartamentID, @Nume, @Tip);
        
        -- Inser?m rela?ia cu sine (nivel 0)
        INSERT INTO DepartamenteIerarhie (AncestorID, DescendantID, Nivel)
        VALUES (@NoiDepartamentID, @NoiDepartamentID, 0);
        
        -- Dac? are p?rinte, inser?m toate rela?iile ierarhice
        IF @ParinteID IS NOT NULL
        BEGIN
            INSERT INTO DepartamenteIerarhie (AncestorID, DescendantID, Nivel)
            SELECT 
                AncestorID, 
                @NoiDepartamentID, 
                Nivel + 1
            FROM DepartamenteIerarhie
            WHERE DescendantID = @ParinteID;
        END
        
        COMMIT TRANSACTION;
        PRINT 'Departamentul a fost ad?ugat cu succes!';
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ==============================================
-- Procedur? pentru ?tergerea unui departament
-- ==============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_StergeDepartament]
    @NumeDepartament NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @DeptID UNIQUEIDENTIFIER;
        
        SELECT @DeptID = DepartamentID 
        FROM Departamente 
        WHERE Nume = @NumeDepartament;
        
        IF @DeptID IS NULL
        BEGIN
            RAISERROR('Departamentul specificat nu exist?!', 16, 1);
            RETURN;
        END
        
        -- Verific?m dac? departamentul are copii
        IF EXISTS (
            SELECT 1 FROM DepartamenteIerarhie 
            WHERE AncestorID = @DeptID AND Nivel = 1
        )
        BEGIN
            RAISERROR('Nu se poate ?terge departamentul deoarece are sub-departamente!', 16, 1);
            RETURN;
        END
        
        -- ?tergem toate rela?iile ierarhice
        DELETE FROM DepartamenteIerarhie 
        WHERE AncestorID = @DeptID OR DescendantID = @DeptID;
        
        -- ?tergem departamentul
        DELETE FROM Departamente WHERE DepartamentID = @DeptID;
        
        COMMIT TRANSACTION;
        PRINT 'Departamentul a fost ?ters cu succes!';
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ==============================================
-- Procedur? pentru c?utarea medicilor dintr-un departament
-- ==============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetMediciDinDepartament]
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
        PRINT 'Departamentul specificat nu exist?!';
        RETURN;
    END
    
    SELECT 
        p.Nume + ' ' + p.Prenume AS NumeComplet,
        pm.Functie,
        pm.NumarLicenta,
        pm.DataAngajare,
        CASE WHEN pm.Activ = 1 THEN 'Da' ELSE 'Nu' END AS Activ
    FROM PersonalMedical pm
    INNER JOIN Persoana p ON pm.PersoanaId = p.Id
    WHERE pm.DepartamentId = @DeptID
    ORDER BY pm.Functie, p.Nume, p.Prenume;
END
GO

PRINT 'Procedurile stocate pentru departamente au fost create cu succes!'