-- =============================================
-- SCRIPT ACTUALIZARE STORED PROCEDURES PERSONAL MEDICAL
-- Actualizarea procedurilor pentru suportul ierarhiei de departamente
-- =============================================

USE [ValyanMed];
GO

PRINT '=== ACTUALIZARE STORED PROCEDURES PERSONAL MEDICAL ===';
PRINT '';

-- =============================================
-- SP 1: sp_PersonalMedical_GetPaged - ACTUALIZAT
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PersonalMedical_GetPaged]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_PersonalMedical_GetPaged];
GO

CREATE PROCEDURE [dbo].[sp_PersonalMedical_GetPaged]
    @Search NVARCHAR(255) = NULL,
    @CategorieID UNIQUEIDENTIFIER = NULL,
    @SpecializareID UNIQUEIDENTIFIER = NULL,
    @SubspecializareID UNIQUEIDENTIFIER = NULL,
    @Departament NVARCHAR(100) = NULL, -- Pentru compatibilitate cu datele vechi
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = NULL,
    @Nume NVARCHAR(100) = NULL,
    @Prenume NVARCHAR(100) = NULL,
    @Specializare NVARCHAR(150) = NULL, -- Pentru compatibilitate cu datele vechi
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

    -- Parse Sort into @OrderCol and @OrderDir (whitelisted)
    DECLARE @OrderCol NVARCHAR(100) = N'Nume';
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
        SET @col = CASE @col
            WHEN N'Nume' THEN N'Nume'
            WHEN N'Prenume' THEN N'Prenume'
            WHEN N'Specializare' THEN N'SpecializareNume'
            WHEN N'NumarLicenta' THEN N'NumarLicenta'
            WHEN N'Departament' THEN N'CategorieNume'
            WHEN N'Pozitie' THEN N'Pozitie'
            WHEN N'Telefon' THEN N'Telefon'
            WHEN N'Email' THEN N'Email'
            WHEN N'EsteActiv' THEN N'EsteActiv'
            WHEN N'DataCreare' THEN N'DataCreare'
            ELSE N'Nume' END;
        SET @OrderCol = @col;
        SET @OrderDir = CASE WHEN LOWER(@dir) = 'desc' THEN N'DESC' ELSE N'ASC' END;
    END

    -- Main query cu JOIN-uri pentru datele ierarhice
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
      -- Compatibilitate cu c?ut?rile pe textul vechi
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
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- Count query cu acelea?i filtre
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

PRINT '? sp_PersonalMedical_GetPaged actualizat cu suport pentru ierarhie';

-- =============================================
-- SP 2: sp_PersonalMedical_Create - ACTUALIZAT
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PersonalMedical_Create]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_PersonalMedical_Create];
GO

CREATE PROCEDURE [dbo].[sp_PersonalMedical_Create]
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @CategorieID UNIQUEIDENTIFIER = NULL,
    @SpecializareID UNIQUEIDENTIFIER = NULL,
    @SubspecializareID UNIQUEIDENTIFIER = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @Telefon NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = 1,
    -- Compatibilitate cu datele vechi
    @Departament NVARCHAR(100) = NULL,
    @Specializare NVARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validare date obligatorii
        IF (@Nume IS NULL OR LTRIM(RTRIM(@Nume)) = '')
        BEGIN
            RAISERROR('Numele este obligatoriu.', 16, 1);
            RETURN;
        END
        
        IF (@Prenume IS NULL OR LTRIM(RTRIM(@Prenume)) = '')
        BEGIN
            RAISERROR('Prenumele este obligatoriu.', 16, 1);
            RETURN;
        END
        
        -- Validare num?r licen?? duplicat
        IF @NumarLicenta IS NOT NULL AND EXISTS (SELECT 1 FROM PersonalMedical WHERE NumarLicenta = @NumarLicenta AND EsteActiv = 1)
        BEGIN
            RAISERROR('Un membru al personalului cu acest num?r de licen?? exist? deja în sistem.', 16, 1);
            RETURN;
        END
        
        -- Validare ierarhie departamente
        IF @CategorieID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Departamente WHERE DepartamentID = @CategorieID AND Tip = 'Categorie')
        BEGIN
            RAISERROR('Categoria specificat? nu exist?.', 16, 1);
            RETURN;
        END
        
        IF @SpecializareID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Departamente WHERE DepartamentID = @SpecializareID AND Tip = 'Specialitate')
        BEGIN
            RAISERROR('Specializarea specificat? nu exist?.', 16, 1);
            RETURN;
        END
        
        IF @SubspecializareID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Departamente WHERE DepartamentID = @SubspecializareID AND Tip = 'Subspecialitate')
        BEGIN
            RAISERROR('Subspecializarea specificat? nu exist?.', 16, 1);
            RETURN;
        END
        
        DECLARE @NewPersonalID UNIQUEIDENTIFIER = NEWID();
        
        INSERT INTO PersonalMedical (
            PersonalID,
            Nume,
            Prenume,
            CategorieID,
            SpecializareID, 
            SubspecializareID,
            NumarLicenta,
            Telefon,
            Email,
            Pozitie,
            EsteActiv,
            DataCreare,
            -- Compatibilitate cu datele vechi
            Departament,
            Specializare
        )
        VALUES (
            @NewPersonalID,
            LTRIM(RTRIM(@Nume)),
            LTRIM(RTRIM(@Prenume)),
            @CategorieID,
            @SpecializareID,
            @SubspecializareID,
            @NumarLicenta,
            @Telefon,
            @Email,
            @Pozitie,
            @EsteActiv,
            GETDATE(),
            @Departament,
            @Specializare
        );
        
        SELECT @NewPersonalID AS PersonalID;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

PRINT '? sp_PersonalMedical_Create actualizat cu suport pentru ierarhie';

-- =============================================
-- SP 3: sp_PersonalMedical_Update - ACTUALIZAT
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PersonalMedical_Update]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_PersonalMedical_Update];
GO

CREATE PROCEDURE [dbo].[sp_PersonalMedical_Update]
    @PersonalID UNIQUEIDENTIFIER,
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @CategorieID UNIQUEIDENTIFIER = NULL,
    @SpecializareID UNIQUEIDENTIFIER = NULL,
    @SubspecializareID UNIQUEIDENTIFIER = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @Telefon NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = 1,
    -- Compatibilitate cu datele vechi
    @Departament NVARCHAR(100) = NULL,
    @Specializare NVARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verific? c? personalul exist?
        IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE PersonalID = @PersonalID)
        BEGIN
            RAISERROR('Personalul medical specificat nu exist?.', 16, 1);
            RETURN;
        END
        
        -- Validare date obligatorii
        IF (@Nume IS NULL OR LTRIM(RTRIM(@Nume)) = '')
        BEGIN
            RAISERROR('Numele este obligatoriu.', 16, 1);
            RETURN;
        END
        
        IF (@Prenume IS NULL OR LTRIM(RTRIM(@Prenume)) = '')
        BEGIN
            RAISERROR('Prenumele este obligatoriu.', 16, 1);
            RETURN;
        END
        
        -- Validare num?r licen?? duplicat (exclus înregistrarea curent?)
        IF @NumarLicenta IS NOT NULL AND EXISTS (
            SELECT 1 FROM PersonalMedical 
            WHERE NumarLicenta = @NumarLicenta 
            AND PersonalID != @PersonalID 
            AND EsteActiv = 1
        )
        BEGIN
            RAISERROR('Un alt membru al personalului cu acest num?r de licen?? exist? deja în sistem.', 16, 1);
            RETURN;
        END
        
        -- Validare ierarhie departamente
        IF @CategorieID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Departamente WHERE DepartamentID = @CategorieID AND Tip = 'Categorie')
        BEGIN
            RAISERROR('Categoria specificat? nu exist?.', 16, 1);
            RETURN;
        END
        
        IF @SpecializareID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Departamente WHERE DepartamentID = @SpecializareID AND Tip = 'Specialitate')
        BEGIN
            RAISERROR('Specializarea specificat? nu exist?.', 16, 1);
            RETURN;
        END
        
        IF @SubspecializareID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Departamente WHERE DepartamentID = @SubspecializareID AND Tip = 'Subspecialitate')
        BEGIN
            RAISERROR('Subspecializarea specificat? nu exist?.', 16, 1);
            RETURN;
        END
        
        UPDATE PersonalMedical 
        SET 
            Nume = LTRIM(RTRIM(@Nume)),
            Prenume = LTRIM(RTRIM(@Prenume)),
            CategorieID = @CategorieID,
            SpecializareID = @SpecializareID,
            SubspecializareID = @SubspecializareID,
            NumarLicenta = @NumarLicenta,
            Telefon = @Telefon,
            Email = @Email,
            Pozitie = @Pozitie,
            EsteActiv = @EsteActiv,
            -- Compatibilitate cu datele vechi
            Departament = @Departament,
            Specializare = @Specializare
        WHERE PersonalID = @PersonalID;
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Nu s-au putut actualiza datele personalului medical.', 16, 1);
            RETURN;
        END
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

PRINT '? sp_PersonalMedical_Update actualizat cu suport pentru ierarhie';

-- =============================================
-- SP 4: sp_PersonalMedical_GetById - ACTUALIZAT
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PersonalMedical_GetById]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_PersonalMedical_GetById];
GO

CREATE PROCEDURE [dbo].[sp_PersonalMedical_GetById]
    @PersonalID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pm.PersonalID,
        pm.Nume,
        pm.Prenume,
        pm.CategorieID,
        pm.SpecializareID,
        pm.SubspecializareID,
        pm.NumarLicenta,
        pm.Telefon,
        pm.Email,
        pm.Pozitie,
        pm.EsteActiv,
        pm.DataCreare,
        -- Compatibilitate cu datele vechi
        pm.Departament,
        pm.Specializare,
        -- Nume din ierarhie pentru afi?are
        cat.Nume AS CategorieNume,
        spec.Nume AS SpecializareNume,
        sub.Nume AS SubspecializareNume
    FROM PersonalMedical pm WITH (NOLOCK)
    LEFT JOIN Departamente cat ON pm.CategorieID = cat.DepartamentID AND cat.Tip = 'Categorie'
    LEFT JOIN Departamente spec ON pm.SpecializareID = spec.DepartamentID AND spec.Tip = 'Specialitate'
    LEFT JOIN Departamente sub ON pm.SubspecializareID = sub.DepartamentID AND sub.Tip = 'Subspecialitate'
    WHERE pm.PersonalID = @PersonalID;
END
GO

PRINT '? sp_PersonalMedical_GetById actualizat cu suport pentru ierarhie';

-- =============================================
-- SP 5: sp_Departamente_GetCategorii - NOU
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Departamente_GetCategorii]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Departamente_GetCategorii];
GO

CREATE PROCEDURE [dbo].[sp_Departamente_GetCategorii]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        DepartamentID,
        Nume,
        Tip
    FROM Departamente WITH (NOLOCK)
    WHERE Tip = 'Categorie'
    ORDER BY Nume;
END
GO

PRINT '? sp_Departamente_GetCategorii creat';

-- =============================================
-- SP 6: sp_Departamente_GetSpecializariByCategorie - NOU
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Departamente_GetSpecializariByCategorie]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Departamente_GetSpecializariByCategorie];
GO

CREATE PROCEDURE [dbo].[sp_Departamente_GetSpecializariByCategorie]
    @CategorieID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        spec.DepartamentID,
        spec.Nume,
        spec.Tip
    FROM Departamente spec WITH (NOLOCK)
    INNER JOIN DepartamenteIerarhie h ON spec.DepartamentID = h.DescendantID
    WHERE h.AncestorID = @CategorieID
      AND h.Nivel = 1
      AND spec.Tip = 'Specialitate'
    ORDER BY spec.Nume;
END
GO

PRINT '? sp_Departamente_GetSpecializariByCategorie creat';

-- =============================================
-- SP 7: sp_Departamente_GetSubspecializariBySpecializare - NOU
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Departamente_GetSubspecializariBySpecializare]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Departamente_GetSubspecializariBySpecializare];
GO

CREATE PROCEDURE [dbo].[sp_Departamente_GetSubspecializariBySpecializare]
    @SpecializareID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        sub.DepartamentID,
        sub.Nume,
        sub.Tip
    FROM Departamente sub WITH (NOLOCK)
    INNER JOIN DepartamenteIerarhie h ON sub.DepartamentID = h.DescendantID
    WHERE h.AncestorID = @SpecializareID
      AND h.Nivel = 1
      AND sub.Tip = 'Subspecialitate'
    ORDER BY sub.Nume;
END
GO

PRINT '? sp_Departamente_GetSubspecializariBySpecializare creat';

-- =============================================
-- SP 8: sp_PersonalMedical_GetStatisticiByDepartamente - NOU
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PersonalMedical_GetStatisticiByDepartamente]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_PersonalMedical_GetStatisticiByDepartamente];
GO

CREATE PROCEDURE [dbo].[sp_PersonalMedical_GetStatisticiByDepartamente]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        cat.Nume AS Categorie,
        COUNT(pm.PersonalID) AS TotalPersonal,
        COUNT(CASE WHEN pm.EsteActiv = 1 THEN 1 END) AS PersonalActiv,
        COUNT(CASE WHEN pm.EsteActiv = 0 THEN 1 END) AS PersonalInactiv
    FROM Departamente cat WITH (NOLOCK)
    LEFT JOIN PersonalMedical pm ON cat.DepartamentID = pm.CategorieID
    WHERE cat.Tip = 'Categorie'
    GROUP BY cat.DepartamentID, cat.Nume
    ORDER BY cat.Nume;
END
GO

PRINT '? sp_PersonalMedical_GetStatisticiByDepartamente creat';

PRINT '';
PRINT '=== ACTUALIZARE STORED PROCEDURES COMPLET? ===';
PRINT '';
PRINT 'Proceduri actualizate:';
PRINT '? sp_PersonalMedical_GetPaged - suport pentru filtrare pe ierarhie';
PRINT '? sp_PersonalMedical_Create - accept? noile coloane ierarhice';
PRINT '? sp_PersonalMedical_Update - actualizeaz? noile coloane ierarhice';
PRINT '? sp_PersonalMedical_GetById - returneaz? datele ierarhice';
PRINT '';
PRINT 'Proceduri noi create:';
PRINT '? sp_Departamente_GetCategorii';
PRINT '? sp_Departamente_GetSpecializariByCategorie';
PRINT '? sp_Departamente_GetSubspecializariBySpecializare';
PRINT '? sp_PersonalMedical_GetStatisticiByDepartamente';
PRINT '';
PRINT 'Urm?torul pas: Actualizarea aplica?iei Blazor pentru dropdown-urile ierarhice.';