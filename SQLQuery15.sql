-- Script pentru corectarea stored procedure sp_PersonalMedical_GetPaged
-- Problema: mappingul coloanelor pentru sortare nu corespunde cu alias-urile din SELECT
-- Script pentru corectarea completă a stored procedures PersonalMedical
-- Problema: mappingul coloanelor pentru sortare și posibile duplicate în ORDER BY

USE [ValyanMed]
GO

-- =====================================================
-- 1. Corectare sp_PersonalMedical_GetPaged
-- =====================================================
ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetPaged]
    @Search NVARCHAR(255) = NULL,
    @CategorieID UNIQUEIDENTIFIER = NULL,
    @SpecializareID UNIQUEIDENTIFIER = NULL,
    @SubspecializareID UNIQUEIDENTIFIER = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = NULL,
    @Nume NVARCHAR(100) = NULL,
    @Prenume NVARCHAR(100) = NULL,
    @Specializare NVARCHAR(150) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @Telefon NVARCHAR(30) = NULL,
    @Email NVARCHAR(150) = NULL,
    @Page INT = 1,
    @PageSize INT = 25,
    @Sort NVARCHAR(400) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page IS NULL OR @Page < 1) SET @Page = 1;
    IF (@PageSize IS NULL OR @PageSize < 1) SET @PageSize = 25;
    IF (@PageSize > 100000) SET @PageSize = 100000;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Parse Sort into @OrderCol and @OrderDir (CORECTEZ MAPPINGUL)
    --DECLARE @OrderCol NVARCHAR(100) = N'pm.Nume';
    -- Parse Sort - FIX pentru duplicate în ORDER BY
    DECLARE @OrderCol NVARCHAR(200) = N'pm.Nume';
    DECLARE @OrderDir NVARCHAR(4) = N'ASC';
    
    IF (@Sort IS NOT NULL AND LEN(@Sort) > 0)
    BEGIN
        DECLARE @pos INT = CHARINDEX(':', @Sort);
        DECLARE @col NVARCHAR(100) = LTRIM(RTRIM(@Sort));
        DECLARE @dir NVARCHAR(10) = N'asc';
        
        IF @pos > 0
        BEGIN
            SET @col = LTRIM(RTRIM(SUBSTRING(@Sort, 1, @pos - 1)));
            SET @dir = LTRIM(RTRIM(SUBSTRING(@Sort, @pos + 1, 10)));
        END
        
        -- CORECTEZ MAPPINGUL COLOANELOR - acum corespunde cu alias-urile din SELECT
        SET @col = CASE @col
            WHEN N'Nume' THEN N'pm.Nume'
            WHEN N'Prenume' THEN N'pm.Prenume'
            WHEN N'Specializare' THEN N'ISNULL(spec.Nume, pm.Specializare)'
            WHEN N'SpecializareNume' THEN N'spec.Nume'
            WHEN N'NumarLicenta' THEN N'pm.NumarLicenta'
            WHEN N'Departament' THEN N'ISNULL(cat.Nume, pm.Departament)'
            WHEN N'CategorieNume' THEN N'cat.Nume'
            WHEN N'Pozitie' THEN N'pm.Pozitie'
            WHEN N'Telefon' THEN N'pm.Telefon'
            WHEN N'Email' THEN N'pm.Email'
            WHEN N'EsteActiv' THEN N'pm.EsteActiv'
            WHEN N'DataCreare' THEN N'pm.DataCreare'
        -- MAPPINGUL CORECT - evităm duplicate
        SET @col = CASE LOWER(@col)
            WHEN N'nume' THEN N'pm.Nume'
            WHEN N'prenume' THEN N'pm.Prenume'
            WHEN N'specializare' THEN N'ISNULL(spec.Nume, pm.Specializare)'
            WHEN N'specializarenume' THEN N'spec.Nume'
            WHEN N'numarlicenta' THEN N'pm.NumarLicenta'
            WHEN N'departament' THEN N'ISNULL(cat.Nume, pm.Departament)'
            WHEN N'categorienume' THEN N'cat.Nume'
            WHEN N'pozitie' THEN N'pm.Pozitie'
            WHEN N'telefon' THEN N'pm.Telefon'
            WHEN N'email' THEN N'pm.Email'
            WHEN N'esteactiv' THEN N'pm.EsteActiv'
            WHEN N'datacreare' THEN N'pm.DataCreare'
            ELSE N'pm.Nume' END;
        
        SET @OrderCol = @col;
        SET @OrderDir = CASE WHEN LOWER(@dir) = 'desc' THEN N'DESC' ELSE N'ASC' END;
    END

    -- Main query cu JOIN-uri pentru datele ierarhice
    -- Query principal - SIMPLIFICAT pentru a evita problemele
    DECLARE @sql NVARCHAR(MAX) = N'
    SELECT 
        pm.PersonalID,
        pm.Nume,
        pm.Prenume,
        ISNULL(spec.Nume, pm.Specializare) AS Specializare,
        pm.NumarLicenta,
        pm.Telefon,
        pm.Email,
        ISNULL(cat.Nume, pm.Departament) AS Departament,
        pm.Pozitie,
        pm.EsteActiv,
        pm.DataCreare,
        -- Coloane noi pentru ierarhie
        pm.CategorieID,
        pm.SpecializareID,
        pm.SubspecializareID,
        cat.Nume AS CategorieNume,
        spec.Nume AS SpecializareNume,
        sub.Nume AS SubspecializareNume
    FROM PersonalMedical pm WITH (NOLOCK)
    LEFT JOIN Departamente cat ON pm.CategorieID = cat.DepartamentID AND cat.Tip = ''Categorie''
    LEFT JOIN Departamente spec ON pm.SpecializareID = spec.DepartamentID AND spec.Tip = ''Specialitate''
    LEFT JOIN Departamente sub ON pm.SubspecializareID = sub.DepartamentID AND sub.Tip = ''Subspecialitate''
    WHERE 1 = 1
      AND (@EsteActiv IS NULL OR pm.EsteActiv = @EsteActiv)
      AND (@CategorieID IS NULL OR pm.CategorieID = @CategorieID)
      AND (@SpecializareID IS NULL OR pm.SpecializareID = @SpecializareID)
      AND (@SubspecializareID IS NULL OR pm.SubspecializareID = @SubspecializareID)
      -- Compatibilitate cu căutările pe textul vechi
      AND (@Departament IS NULL OR @Departament = '''' OR @Departament = ''toate'' 
           OR UPPER(ISNULL(cat.Nume, pm.Departament)) LIKE ''%'' + UPPER(@Departament) + ''%'')
      AND (@Pozitie IS NULL OR @Pozitie = '''' OR @Pozitie = ''toate'' OR UPPER(pm.Pozitie) LIKE ''%'' + UPPER(@Pozitie) + ''%'')
      AND (@Nume IS NULL OR @Nume = '''' OR UPPER(pm.Nume) LIKE ''%'' + UPPER(@Nume) + ''%'')
      AND (@Prenume IS NULL OR @Prenume = '''' OR UPPER(pm.Prenume) LIKE ''%'' + UPPER(@Prenume) + ''%'')
      AND (@Specializare IS NULL OR @Specializare = '''' 
           OR UPPER(ISNULL(spec.Nume, pm.Specializare)) LIKE ''%'' + UPPER(@Specializare) + ''%'')
      AND (@NumarLicenta IS NULL OR @NumarLicenta = '''' OR UPPER(pm.NumarLicenta) LIKE ''%'' + UPPER(@NumarLicenta) + ''%'')
      AND (@Telefon IS NULL OR @Telefon = '''' OR pm.Telefon LIKE ''%'' + @Telefon + ''%'')
      AND (@Email IS NULL OR @Email = '''' OR UPPER(pm.Email) LIKE ''%'' + UPPER(@Email) + ''%'')
      AND (
            @Search IS NULL OR @Search = ''''
            OR UPPER(pm.Nume) LIKE ''%'' + UPPER(@Search) + ''%''
            OR UPPER(pm.Prenume) LIKE ''%'' + UPPER(@Search) + ''%''
            OR UPPER(CONCAT(pm.Nume, '' '', pm.Prenume)) LIKE ''%'' + UPPER(@Search) + ''%''
            OR (ISNULL(spec.Nume, pm.Specializare) IS NOT NULL AND UPPER(ISNULL(spec.Nume, pm.Specializare)) LIKE ''%'' + UPPER(@Search) + ''%'')
            OR (pm.NumarLicenta IS NOT NULL AND UPPER(pm.NumarLicenta) LIKE ''%'' + UPPER(@Search) + ''%'')
            OR (ISNULL(cat.Nume, pm.Departament) IS NOT NULL AND UPPER(ISNULL(cat.Nume, pm.Departament)) LIKE ''%'' + UPPER(@Search) + ''%'')
            OR (pm.Pozitie IS NOT NULL AND UPPER(pm.Pozitie) LIKE ''%'' + UPPER(@Search) + ''%'')
            OR (pm.Telefon IS NOT NULL AND pm.Telefon LIKE ''%'' + @Search + ''%'')
            OR (pm.Email IS NOT NULL AND UPPER(pm.Email) LIKE ''%'' + UPPER(@Search) + ''%'')
          )
    ORDER BY ' + @OrderCol + ' ' + @OrderDir + ', pm.Nume ASC, pm.Prenume ASC
    ORDER BY ' + @OrderCol + ' ' + @OrderDir + '
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- Count query cu aceleași filtre
    SELECT COUNT(1)
    FROM PersonalMedical pm WITH (NOLOCK)
    LEFT JOIN Departamente cat ON pm.CategorieID = cat.DepartamentID AND cat.Tip = ''Categorie''
    LEFT JOIN Departamente spec ON pm.SpecializareID = spec.DepartamentID AND spec.Tip = ''Specialitate''
    LEFT JOIN Departamente sub ON pm.SubspecializareID = sub.DepartamentID AND sub.Tip = ''Subspecialitate''
    WHERE 1 = 1
      AND (@EsteActiv IS NULL OR pm.EsteActiv = @EsteActiv)
      AND (@CategorieID IS NULL OR pm.CategorieID = @CategorieID)
      AND (@SpecializareID IS NULL OR pm.SpecializareID = @SpecializareID)
      AND (@SubspecializareID IS NULL OR pm.SubspecializareID = @SubspecializareID)
      AND (@Departament IS NULL OR @Departament = '''' OR @Departament = ''toate'' 
           OR UPPER(ISNULL(cat.Nume, pm.Departament)) LIKE ''%'' + UPPER(@Departament) + ''%'')
      AND (@Pozitie IS NULL OR @Pozitie = '''' OR @Pozitie = ''toate'' OR UPPER(pm.Pozitie) LIKE ''%'' + UPPER(@Pozitie) + ''%'')
      AND (@Nume IS NULL OR @Nume = '''' OR UPPER(pm.Nume) LIKE ''%'' + UPPER(@Nume) + ''%'')
      AND (@Prenume IS NULL OR @Prenume = '''' OR UPPER(pm.Prenume) LIKE ''%'' + UPPER(@Prenume) + ''%'')
      AND (@Specializare IS NULL OR @Specializare = '''' 
           OR UPPER(ISNULL(spec.Nume, pm.Specializare)) LIKE ''%'' + UPPER(@Specializare) + ''%'')
      AND (@NumarLicenta IS NULL OR @NumarLicenta = '''' OR UPPER(pm.NumarLicenta) LIKE ''%'' + UPPER(@NumarLicenta) + ''%'')
      AND (@Telefon IS NULL OR @Telefon = '''' OR pm.Telefon LIKE ''%'' + @Telefon + ''%'')
      AND (@Email IS NULL OR @Email = '''' OR UPPER(pm.Email) LIKE ''%'' + UPPER(@Search) + ''%'')
      AND (@Email IS NULL OR @Email = '''' OR UPPER(pm.Email) LIKE ''%'' + UPPER(@Email) + ''%'')
      AND (
            @Search IS NULL OR @Search = ''''
            OR UPPER(pm.Nume) LIKE ''%'' + UPPER(@Search) + ''%''
            OR UPPER(pm.Prenume) LIKE ''%'' + UPPER(@Search) + ''%''
            OR UPPER(CONCAT(pm.Nume, '' '', pm.Prenume)) LIKE ''%'' + UPPER(@Search) + ''%''
            OR (ISNULL(spec.Nume, pm.Specializare) IS NOT NULL AND UPPER(ISNULL(spec.Nume, pm.Specializare)) LIKE ''%'' + UPPER(@Search) + ''%'')
            OR (pm.NumarLicenta IS NOT NULL AND UPPER(pm.NumarLicenta) LIKE ''%'' + UPPER(@Search) + ''%'')
            OR (ISNULL(cat.Nume, pm.Departament) IS NOT NULL AND UPPER(ISNULL(cat.Nume, pm.Departament)) LIKE ''%'' + UPPER(@Search) + ''%'')
            OR (pm.Pozitie IS NOT NULL AND UPPER(pm.Pozitie) LIKE ''%'' + UPPER(@Search) + ''%'')
            OR (pm.Telefon IS NOT NULL AND pm.Telefon LIKE ''%'' + @Search + ''%'')
            OR (pm.Email IS NOT NULL AND UPPER(pm.Email) LIKE ''%'' + UPPER(@Search) + ''%'')
          );';

    EXEC sp_executesql
        @sql,
        N'@Search NVARCHAR(255), @CategorieID UNIQUEIDENTIFIER, @SpecializareID UNIQUEIDENTIFIER, @SubspecializareID UNIQUEIDENTIFIER, @Departament NVARCHAR(100), @Pozitie NVARCHAR(50), @EsteActiv BIT, @Nume NVARCHAR(100), @Prenume NVARCHAR(100), @Specializare NVARCHAR(150), @NumarLicenta NVARCHAR(50), @Telefon NVARCHAR(30), @Email NVARCHAR(150), @Offset INT, @PageSize INT',
        @Search = @Search, @CategorieID = @CategorieID, @SpecializareID = @SpecializareID, @SubspecializareID = @SubspecializareID, @Departament = @Departament, @Pozitie = @Pozitie, @EsteActiv = @EsteActiv, @Nume = @Nume, @Prenume = @Prenume, @Specializare = @Specializare, @NumarLicenta = @NumarLicenta, @Telefon = @Telefon, @Email = @Email, @Offset = @Offset, @PageSize = @PageSize;
END
GO

-- =====================================================
-- 2. Verifică și corectează sp_PersonalMedical_GetById (dacă există)
-- =====================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_GetById')
BEGIN
    ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetById]
        @PersonalID UNIQUEIDENTIFIER
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT 
            pm.PersonalID,
            pm.Nume,
            pm.Prenume,
            ISNULL(spec.Nume, pm.Specializare) AS Specializare,
            pm.NumarLicenta,
            pm.Telefon,
            pm.Email,
            ISNULL(cat.Nume, pm.Departament) AS Departament,
            pm.Pozitie,
            pm.EsteActiv,
            pm.DataCreare,
            pm.CategorieID,
            pm.SpecializareID,
            pm.SubspecializareID,
            cat.Nume AS CategorieNume,
            spec.Nume AS SpecializareNume,
            sub.Nume AS SubspecializareNume
        FROM PersonalMedical pm WITH (NOLOCK)
        LEFT JOIN Departamente cat ON pm.CategorieID = cat.DepartamentID AND cat.Tip = 'Categorie'
        LEFT JOIN Departamente spec ON pm.SpecializareID = spec.DepartamentID AND spec.Tip = 'Specialitate'
        LEFT JOIN Departamente sub ON pm.SubspecializareID = sub.DepartamentID AND sub.Tip = 'Subspecialitate'
        WHERE pm.PersonalID = @PersonalID;
    END
END
ELSE
BEGIN
    -- Creează stored procedure dacă nu există
    EXEC('
    CREATE PROCEDURE [dbo].[sp_PersonalMedical_GetById]
        @PersonalID UNIQUEIDENTIFIER
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT 
            pm.PersonalID,
            pm.Nume,
            pm.Prenume,
            ISNULL(spec.Nume, pm.Specializare) AS Specializare,
            pm.NumarLicenta,
            pm.Telefon,
            pm.Email,
            ISNULL(cat.Nume, pm.Departament) AS Departament,
            pm.Pozitie,
            pm.EsteActiv,
            pm.DataCreare,
            pm.CategorieID,
            pm.SpecializareID,
            pm.SubspecializareID,
            cat.Nume AS CategorieNume,
            spec.Nume AS SpecializareNume,
            sub.Nume AS SubspecializareNume
        FROM PersonalMedical pm WITH (NOLOCK)
        LEFT JOIN Departamente cat ON pm.CategorieID = cat.DepartamentID AND cat.Tip = ''Categorie''
        LEFT JOIN Departamente spec ON pm.SpecializareID = spec.DepartamentID AND spec.Tip = ''Specialitate''
        LEFT JOIN Departamente sub ON pm.SubspecializareID = sub.DepartamentID AND sub.Tip = ''Subspecialitate''
        WHERE pm.PersonalID = @PersonalID;
    END')
END
GO

-- =====================================================
-- 3. Verifică și corectează sp_PersonalMedical_Create (dacă există)
-- =====================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_Create')
BEGIN
    ALTER PROCEDURE [dbo].[sp_PersonalMedical_Create]
        @Nume NVARCHAR(100),
        @Prenume NVARCHAR(100),
        @CategorieID UNIQUEIDENTIFIER = NULL,
        @SpecializareID UNIQUEIDENTIFIER = NULL,
        @SubspecializareID UNIQUEIDENTIFIER = NULL,
        @NumarLicenta NVARCHAR(50) = NULL,
        @Telefon NVARCHAR(30) = NULL,
        @Email NVARCHAR(150) = NULL,
        @Pozitie NVARCHAR(50),
        @EsteActiv BIT = 1,
        @Departament NVARCHAR(100) = NULL,  -- Pentru compatibilitate
        @Specializare NVARCHAR(150) = NULL  -- Pentru compatibilitate
    AS
    BEGIN
        SET NOCOUNT ON;
        
        DECLARE @PersonalID UNIQUEIDENTIFIER = NEWID();
        
        INSERT INTO PersonalMedical (
            PersonalID, Nume, Prenume, CategorieID, SpecializareID, SubspecializareID,
            NumarLicenta, Telefon, Email, Pozitie, EsteActiv, DataCreare,
            Departament, Specializare  -- Pentru compatibilitate
        )
        VALUES (
            @PersonalID, @Nume, @Prenume, @CategorieID, @SpecializareID, @SubspecializareID,
            @NumarLicenta, @Telefon, @Email, @Pozitie, @EsteActiv, GETDATE(),
            @Departament, @Specializare
        );
        
        SELECT @PersonalID AS PersonalID;
    END
END
ELSE
BEGIN
    -- Creează stored procedure dacă nu există
    EXEC('
    CREATE PROCEDURE [dbo].[sp_PersonalMedical_Create]
        @Nume NVARCHAR(100),
        @Prenume NVARCHAR(100),
        @CategorieID UNIQUEIDENTIFIER = NULL,
        @SpecializareID UNIQUEIDENTIFIER = NULL,
        @SubspecializareID UNIQUEIDENTIFIER = NULL,
        @NumarLicenta NVARCHAR(50) = NULL,
        @Telefon NVARCHAR(30) = NULL,
        @Email NVARCHAR(150) = NULL,
        @Pozitie NVARCHAR(50),
        @EsteActiv BIT = 1,
        @Departament NVARCHAR(100) = NULL,
        @Specializare NVARCHAR(150) = NULL
    AS
    BEGIN
        SET NOCOUNT ON;
        
        DECLARE @PersonalID UNIQUEIDENTIFIER = NEWID();
        
        INSERT INTO PersonalMedical (
            PersonalID, Nume, Prenume, CategorieID, SpecializareID, SubspecializareID,
            NumarLicenta, Telefon, Email, Pozitie, EsteActiv, DataCreare,
            Departament, Specializare
        )
        VALUES (
            @PersonalID, @Nume, @Prenume, @CategorieID, @SpecializareID, @SubspecializareID,
            @NumarLicenta, @Telefon, @Email, @Pozitie, @EsteActiv, GETDATE(),
            @Departament, @Specializare
        );
        
        SELECT @PersonalID AS PersonalID;
    END')
END
GO

-- =====================================================
-- 4. Verifică și corectează sp_PersonalMedical_Update (dacă există)
-- =====================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PersonalMedical_Update')
BEGIN
    ALTER PROCEDURE [dbo].[sp_PersonalMedical_Update]
        @PersonalID UNIQUEIDENTIFIER,
        @Nume NVARCHAR(100),
        @Prenume NVARCHAR(100),
        @CategorieID UNIQUEIDENTIFIER = NULL,
        @SpecializareID UNIQUEIDENTIFIER = NULL,
        @SubspecializareID UNIQUEIDENTIFIER = NULL,
        @NumarLicenta NVARCHAR(50) = NULL,
        @Telefon NVARCHAR(30) = NULL,
        @Email NVARCHAR(150) = NULL,
        @Pozitie NVARCHAR(50),
        @EsteActiv BIT,
        @Departament NVARCHAR(100) = NULL,  -- Pentru compatibilitate
        @Specializare NVARCHAR(150) = NULL  -- Pentru compatibilitate
    AS
    BEGIN
        SET NOCOUNT ON;
        
        UPDATE PersonalMedical 
        SET 
            Nume = @Nume,
            Prenume = @Prenume,
            CategorieID = @CategorieID,
            SpecializareID = @SpecializareID,
            SubspecializareID = @SubspecializareID,
            NumarLicenta = @NumarLicenta,
            Telefon = @Telefon,
            Email = @Email,
            Pozitie = @Pozitie,
            EsteActiv = @EsteActiv,
            Departament = @Departament,
            Specializare = @Specializare
        WHERE PersonalID = @PersonalID;
    END
END
ELSE
BEGIN
    -- Creează stored procedure dacă nu există
    EXEC('
    CREATE PROCEDURE [dbo].[sp_PersonalMedical_Update]
        @PersonalID UNIQUEIDENTIFIER,
        @Nume NVARCHAR(100),
        @Prenume NVARCHAR(100),
        @CategorieID UNIQUEIDENTIFIER = NULL,
        @SpecializareID UNIQUEIDENTIFIER = NULL,
        @SubspecializareID UNIQUEIDENTIFIER = NULL,
        @NumarLicenta NVARCHAR(50) = NULL,
        @Telefon NVARCHAR(30) = NULL,
        @Email NVARCHAR(150) = NULL,
        @Pozitie NVARCHAR(50),
        @EsteActiv BIT,
        @Departament NVARCHAR(100) = NULL,
        @Specializare NVARCHAR(150) = NULL
    AS
    BEGIN
        SET NOCOUNT ON;
        
        UPDATE PersonalMedical 
        SET 
            Nume = @Nume,
            Prenume = @Prenume,
            CategorieID = @CategorieID,
            SpecializareID = @SpecializareID,
            SubspecializareID = @SubspecializareID,
            NumarLicenta = @NumarLicenta,
            Telefon = @Telefon,
            Email = @Email,
            Pozitie = @Pozitie,
            EsteActiv = @EsteActiv,
            Departament = @Departament,
            Specializare = @Specializare
        WHERE PersonalID = @PersonalID;
    END')
END
GO

PRINT 'Stored procedures pentru PersonalMedical au fost corectate cu succes!'