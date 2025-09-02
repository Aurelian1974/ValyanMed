-- =============================================
-- STORED PROCEDURES PENTRU DEPARTAMENTE IERARHICE
-- Funcționalități practice pentru lucrul cu ierarhia
-- =============================================

USE [ValyanMed];
GO

-- =============================================
-- SP 1: Obține toți descendenții unui departament
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetDescendenti]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_GetDescendenti];
GO

CREATE PROCEDURE [dbo].[sp_GetDescendenti]
    @NumeDepartament NVARCHAR(200),
    @TipDepartament NVARCHAR(50) = NULL,
    @IncludeNivelulSpecificat BIT = 1,
    @NivelMaxim INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DepartamentID UNIQUEIDENTIFIER;
    
    -- Găsește ID-ul departamentului
    IF @TipDepartament IS NULL
    BEGIN
        SELECT TOP 1 @DepartamentID = DepartamentID 
        FROM Departamente 
        WHERE Nume = @NumeDepartament
        ORDER BY 
            CASE Tip 
                WHEN 'Categorie' THEN 1 
                WHEN 'Specialitate' THEN 2 
                WHEN 'Subspecialitate' THEN 3 
                ELSE 4 
            END;
    END
    ELSE
    BEGIN
        SELECT @DepartamentID = DepartamentID 
        FROM Departamente 
        WHERE Nume = @NumeDepartament AND Tip = @TipDepartament;
    END
    
    IF @DepartamentID IS NULL
    BEGIN
        RAISERROR('Departamentul specificat nu exista!', 16, 1);
        RETURN;
    END
    
    -- Construiește query-ul pentru descendenți
    ;WITH DescendentiCuCai AS (
        SELECT 
            d.DepartamentID,
            d.Nume,
            d.Tip,
            h.Nivel,
            CAST(d.Nume AS NVARCHAR(1000)) AS CaleDeLaParinte
        FROM DepartamenteIerarhie h
        INNER JOIN Departamente d ON h.DescendantID = d.DepartamentID
        WHERE h.AncestorID = @DepartamentID
        AND (@IncludeNivelulSpecificat = 1 OR h.Nivel > 0)
        AND (@NivelMaxim IS NULL OR h.Nivel <= @NivelMaxim)
    )
    SELECT 
        DepartamentID,
        REPLICATE('  ', Nivel) + Nume AS NumeIndentat,
        Nume,
        Tip,
        Nivel,
        CaleDeLaParinte
    FROM DescendentiCuCai
    ORDER BY Nivel, Nume;
END
GO

-- =============================================
-- SP 2: Obține toți strămoșii unui departament
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetStramosi]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_GetStramosi];
GO

CREATE PROCEDURE [dbo].[sp_GetStramosi]
    @NumeDepartament NVARCHAR(200),
    @TipDepartament NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DepartamentID UNIQUEIDENTIFIER;
    
    -- Găsește ID-ul departamentului
    IF @TipDepartament IS NULL
    BEGIN
        SELECT TOP 1 @DepartamentID = DepartamentID 
        FROM Departamente 
        WHERE Nume = @NumeDepartament
        ORDER BY 
            CASE Tip 
                WHEN 'Subspecialitate' THEN 1 
                WHEN 'Specialitate' THEN 2 
                WHEN 'Categorie' THEN 3 
                ELSE 4 
            END;
    END
    ELSE
    BEGIN
        SELECT @DepartamentID = DepartamentID 
        FROM Departamente 
        WHERE Nume = @NumeDepartament AND Tip = @TipDepartament;
    END
    
    IF @DepartamentID IS NULL
    BEGIN
        RAISERROR('Departamentul specificat nu exista!', 16, 1);
        RETURN;
    END
    
    -- Construiește calea completă către rădăcină
    ;WITH CaleCatreDepartament AS (
        SELECT 
            d.DepartamentID,
            d.Nume,
            d.Tip,
            h.Nivel,
            CAST(d.Nume AS NVARCHAR(1000)) AS Cale
        FROM DepartamenteIerarhie h
        INNER JOIN Departamente d ON h.AncestorID = d.DepartamentID
        WHERE h.DescendantID = @DepartamentID
        AND h.Nivel > 0
    )
    SELECT 
        DepartamentID,
        Nume,
        Tip,
        Nivel AS DistantaNivele,
        Cale
    FROM CaleCatreDepartament
    ORDER BY Nivel;
    
    -- Afișează și calea completă ca string
    ;WITH CalecompleteaString AS (
        SELECT 
            d.Nume,
            h.Nivel
        FROM DepartamenteIerarhie h
        INNER JOIN Departamente d ON h.AncestorID = d.DepartamentID
        WHERE h.DescendantID = @DepartamentID
        AND h.Nivel >= 0
    )
    SELECT 
        STRING_AGG(Nume, ' > ') WITHIN GROUP (ORDER BY Nivel DESC) AS CalieCompleta
    FROM CalecompleteaString;
END
GO

-- =============================================
-- SP 3: Caută departamente după pattern
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_CautaDepartamente]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_CautaDepartamente];
GO

CREATE PROCEDURE [dbo].[sp_CautaDepartamente]
    @SearchPattern NVARCHAR(200),
    @TipDepartament NVARCHAR(50) = NULL,
    @IncludeCaleCompleta BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @IncludeCaleCompleta = 1
    BEGIN
        -- Căutare cu cale completă
        ;WITH DepartamenteCuCai AS (
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
            FROM DepartamenteCuCai parinte
            INNER JOIN DepartamenteIerarhie h ON parinte.DepartamentID = h.AncestorID AND h.Nivel = 1
            INNER JOIN Departamente copil ON h.DescendantID = copil.DepartamentID
        )
        SELECT 
            DepartamentID,
            Nume,
            Tip,
            Nivel,
            Cale AS CalieCompleta
        FROM DepartamenteCuCai
        WHERE 
            Nume LIKE '%' + @SearchPattern + '%'
            AND (@TipDepartament IS NULL OR Tip = @TipDepartament)
        ORDER BY Nivel, Cale;
    END
    ELSE
    BEGIN
        -- Căutare simplă
        SELECT 
            DepartamentID,
            Nume,
            Tip
        FROM Departamente
        WHERE 
            Nume LIKE '%' + @SearchPattern + '%'
            AND (@TipDepartament IS NULL OR Tip = @TipDepartament)
        ORDER BY Tip, Nume;
    END
END
GO

-- =============================================
-- SP 4: Obține statistici pentru un departament
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetStatisticiDepartament]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_GetStatisticiDepartament];
GO

CREATE PROCEDURE [dbo].[sp_GetStatisticiDepartament]
    @NumeDepartament NVARCHAR(200),
    @TipDepartament NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DepartamentID UNIQUEIDENTIFIER;
    
    -- Găsește ID-ul departamentului
    IF @TipDepartament IS NULL
    BEGIN
        SELECT TOP 1 @DepartamentID = DepartamentID 
        FROM Departamente 
        WHERE Nume = @NumeDepartament
        ORDER BY 
            CASE Tip 
                WHEN 'Categorie' THEN 1 
                WHEN 'Specialitate' THEN 2 
                WHEN 'Subspecialitate' THEN 3 
                ELSE 4 
            END;
    END
    ELSE
    BEGIN
        SELECT @DepartamentID = DepartamentID 
        FROM Departamente 
        WHERE Nume = @NumeDepartament AND Tip = @TipDepartament;
    END
    
    IF @DepartamentID IS NULL
    BEGIN
        RAISERROR('Departamentul specificat nu exista!', 16, 1);
        RETURN;
    END
    
    -- Informații de bază
    SELECT 
        d.Nume AS NumeDepartament,
        d.Tip AS TipDepartament,
        d.DepartamentID
    FROM Departamente d
    WHERE d.DepartamentID = @DepartamentID;
    
    -- Statistici descendenți
    SELECT 
        'Descendenti' AS TipStatistica,
        d.Tip AS TipDescendent,
        COUNT(*) AS Numar
    FROM DepartamenteIerarhie h
    INNER JOIN Departamente d ON h.DescendantID = d.DepartamentID
    WHERE h.AncestorID = @DepartamentID
    AND h.Nivel > 0
    GROUP BY d.Tip
    ORDER BY 
        CASE d.Tip 
            WHEN 'Specialitate' THEN 1 
            WHEN 'Subspecialitate' THEN 2 
            ELSE 3 
        END;
    
    -- Strămoși direcți
    SELECT 
        'Stramosi' AS TipStatistica,
        s.Nume AS NumeStramos,
        s.Tip AS TipStramos,
        h.Nivel AS DistantaNivele
    FROM DepartamenteIerarhie h
    INNER JOIN Departamente s ON h.AncestorID = s.DepartamentID
    WHERE h.DescendantID = @DepartamentID
    AND h.Nivel > 0
    ORDER BY h.Nivel;
END
GO

-- =============================================
-- SP 5: Exportă structura pentru un departament (JSON-like)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_ExportaStructura]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_ExportaStructura];
GO

CREATE PROCEDURE [dbo].[sp_ExportaStructura]
    @NumeDepartament NVARCHAR(200) = NULL,
    @TipDepartament NVARCHAR(50) = NULL,
    @FormatOutput NVARCHAR(20) = 'TREE' -- 'TREE', 'LIST', 'FLAT'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DepartamentID UNIQUEIDENTIFIER = NULL;
    
    -- Dacă e specificat un departament, găsește ID-ul
    IF @NumeDepartament IS NOT NULL
    BEGIN
        IF @TipDepartament IS NULL
        BEGIN
            SELECT TOP 1 @DepartamentID = DepartamentID 
            FROM Departamente 
            WHERE Nume = @NumeDepartament
            ORDER BY 
                CASE Tip 
                    WHEN 'Categorie' THEN 1 
                    WHEN 'Specialitate' THEN 2 
                    WHEN 'Subspecialitate' THEN 3 
                    ELSE 4 
                END;
        END
        ELSE
        BEGIN
            SELECT @DepartamentID = DepartamentID 
            FROM Departamente 
            WHERE Nume = @NumeDepartament AND Tip = @TipDepartament;
        END
        
        IF @DepartamentID IS NULL
        BEGIN
            RAISERROR('Departamentul specificat nu exista!', 16, 1);
            RETURN;
        END
    END
    
    -- Export bazat pe format
    IF @FormatOutput = 'TREE'
    BEGIN
        -- Format arbore vizual
        ;WITH StructuraVisuala AS (
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
                END + d.Nume AS StructuraVizuala
            FROM DepartamenteIerarhie h
            INNER JOIN Departamente d ON h.DescendantID = d.DepartamentID
            WHERE (@DepartamentID IS NULL OR h.AncestorID = @DepartamentID)
        )
        SELECT 
            StructuraVizuala AS [Structura Arbore],
            Tip,
            Nivel
        FROM StructuraVisuala
        ORDER BY 
            CASE WHEN @DepartamentID IS NULL THEN Nivel ELSE 0 END,
            Nume;
    END
    ELSE IF @FormatOutput = 'LIST'
    BEGIN
        -- Format listă cu căi complete
        ;WITH CaiComplete AS (
            SELECT 
                d.DepartamentID,
                d.Nume,
                d.Tip,
                0 as Nivel,
                CAST(d.Nume AS NVARCHAR(1000)) AS Cale
            FROM Departamente d
            WHERE d.Tip = 'Categorie' AND (@DepartamentID IS NULL OR d.DepartamentID = @DepartamentID)
            
            UNION ALL
            
            SELECT 
                copil.DepartamentID,
                copil.Nume,
                copil.Tip,
                parinte.Nivel + 1,
                CAST(parinte.Cale + ' > ' + copil.Nume AS NVARCHAR(1000))
            FROM CaiComplete parinte
            INNER JOIN DepartamenteIerarhie h ON parinte.DepartamentID = h.AncestorID AND h.Nivel = 1
            INNER JOIN Departamente copil ON h.DescendantID = copil.DepartamentID
        )
        SELECT 
            Cale AS [Calea Completa],
            Tip,
            Nivel,
            DepartamentID
        FROM CaiComplete
        ORDER BY Cale;
    END
    ELSE -- FLAT
    BEGIN
        -- Format plat pentru integrare
        SELECT 
            d.DepartamentID,
            d.Nume,
            d.Tip,
            h.Nivel,
            h.AncestorID,
            anc.Nume AS NumeParinte,
            anc.Tip AS TipParinte
        FROM DepartamenteIerarhie h
        INNER JOIN Departamente d ON h.DescendantID = d.DepartamentID
        LEFT JOIN Departamente anc ON h.AncestorID = anc.DepartamentID AND h.Nivel = 1
        WHERE (@DepartamentID IS NULL OR h.AncestorID = @DepartamentID OR h.DescendantID = @DepartamentID)
        ORDER BY h.Nivel, d.Nume;
    END
END
GO

-- =============================================
-- EXEMPLE DE UTILIZARE
-- =============================================

PRINT '=== STORED PROCEDURES CREATE CU SUCCES! ===';
PRINT 'Exemple de utilizare:';
PRINT '';
PRINT '-- Descendentii categoriei Medicale:';
PRINT 'EXEC sp_GetDescendenti @NumeDepartament = N''Medicale'', @TipDepartament = N''Categorie'';';
PRINT '';
PRINT '-- Stramosii subspecialitatii Cardiologie:';
PRINT 'EXEC sp_GetStramosi @NumeDepartament = N''Cardiologie'', @TipDepartament = N''Subspecialitate'';';
PRINT '';
PRINT '-- Cautare departamente cu "cardio":';
PRINT 'EXEC sp_CautaDepartamente @SearchPattern = N''cardio'';';
PRINT '';
PRINT '-- Statistici pentru Medicina interna:';
PRINT 'EXEC sp_GetStatisticiDepartament @NumeDepartament = N''Medicina interna'';';
PRINT '';
PRINT '-- Export structura completa in format arbore:';
PRINT 'EXEC sp_ExportaStructura @FormatOutput = N''TREE'';';
PRINT '';
PRINT '-- Export structura completa in format lista:';
PRINT 'EXEC sp_ExportaStructura @FormatOutput = N''LIST'';';
PRINT '';
PRINT '-- Export structura plata pentru o categorie:';
PRINT 'EXEC sp_ExportaStructura @NumeDepartament = N''Medicale'', @FormatOutput = N''FLAT'';';


EXEC sp_ExportaStructura @FormatOutput = N'TREE';