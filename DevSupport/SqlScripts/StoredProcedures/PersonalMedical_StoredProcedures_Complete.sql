-- ===================================================================
-- PROCEDURI STOCATE SUPLIMENTARE PENTRU PERSONAL MEDICAL
-- Completare pentru func?ionalit??ile CRUD lips?
-- ===================================================================

USE [ValyanMed]
GO

-- Ob?inere personal medical dup? ID
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PersonalMedical_GetById]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_PersonalMedical_GetById]
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
        pm.Specializare,
        pm.NumarLicenta,
        pm.Telefon,
        pm.Email,
        pm.Departament,
        pm.Pozitie,
        pm.EsteActiv,
        pm.DataCreare
    FROM PersonalMedical pm
    WHERE pm.PersonalID = @PersonalID;
END
GO

-- Creare personal medical nou
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PersonalMedical_Create]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_PersonalMedical_Create]
GO

CREATE PROCEDURE [dbo].[sp_PersonalMedical_Create]
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Specializare NVARCHAR(100) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @Telefon NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50),
    @EsteActiv BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validare num?r licen?? duplicat
        IF @NumarLicenta IS NOT NULL AND EXISTS (
            SELECT 1 FROM PersonalMedical 
            WHERE NumarLicenta = @NumarLicenta AND EsteActiv = 1
        )
        BEGIN
            RAISERROR('Un membru al personalului cu acest num?r de licen?? exist? deja în sistem.', 16, 1);
            RETURN;
        END
        
        -- Validare email duplicat
        IF @Email IS NOT NULL AND EXISTS (
            SELECT 1 FROM PersonalMedical 
            WHERE Email = @Email AND EsteActiv = 1
        )
        BEGIN
            RAISERROR('Un membru al personalului cu acest email exist? deja în sistem.', 16, 1);
            RETURN;
        END
        
        DECLARE @NewPersonalID UNIQUEIDENTIFIER = NEWID();
        
        INSERT INTO PersonalMedical (
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
        )
        VALUES (
            @NewPersonalID,
            @Nume,
            @Prenume,
            @Specializare,
            @NumarLicenta,
            @Telefon,
            @Email,
            @Departament,
            @Pozitie,
            @EsteActiv,
            GETDATE()
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

-- Actualizare personal medical
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PersonalMedical_Update]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_PersonalMedical_Update]
GO

CREATE PROCEDURE [dbo].[sp_PersonalMedical_Update]
    @PersonalID UNIQUEIDENTIFIER,
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Specializare NVARCHAR(100) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @Telefon NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50),
    @EsteActiv BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verific? dac? personalul exist?
        IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE PersonalID = @PersonalID)
        BEGIN
            RAISERROR('Personalul medical nu a fost g?sit.', 16, 1);
            RETURN;
        END
        
        -- Validare num?r licen?? duplicat (excluzând înregistrarea curent?)
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
        
        -- Validare email duplicat (excluzând înregistrarea curent?)
        IF @Email IS NOT NULL AND EXISTS (
            SELECT 1 FROM PersonalMedical 
            WHERE Email = @Email 
              AND PersonalID != @PersonalID 
              AND EsteActiv = 1
        )
        BEGIN
            RAISERROR('Un alt membru al personalului cu acest email exist? deja în sistem.', 16, 1);
            RETURN;
        END
        
        UPDATE PersonalMedical 
        SET 
            Nume = @Nume,
            Prenume = @Prenume,
            Specializare = @Specializare,
            NumarLicenta = @NumarLicenta,
            Telefon = @Telefon,
            Email = @Email,
            Departament = @Departament,
            Pozitie = @Pozitie,
            EsteActiv = @EsteActiv
        WHERE PersonalID = @PersonalID;
        
        SELECT @@ROWCOUNT AS RowsAffected;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- ?tergere (dezactivare) personal medical
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PersonalMedical_Delete]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_PersonalMedical_Delete]
GO

CREATE PROCEDURE [dbo].[sp_PersonalMedical_Delete]
    @PersonalID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verific? dac? personalul exist?
        IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE PersonalID = @PersonalID)
        BEGIN
            RAISERROR('Personalul medical nu a fost g?sit.', 16, 1);
            RETURN;
        END
        
        -- TODO: Verific? dac? are program?ri active
        -- Pentru acum doar dezactiv?m (soft delete)
        
        UPDATE PersonalMedical 
        SET EsteActiv = 0 
        WHERE PersonalID = @PersonalID;
        
        SELECT @@ROWCOUNT AS RowsAffected;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- Actualizare procedura existent? sp_PersonalMedical_GetPaged pentru a returna ?i num?rul total
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_PersonalMedical_GetPaged]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_PersonalMedical_GetPaged]
GO

CREATE PROCEDURE [dbo].[sp_PersonalMedical_GetPaged]
    @Search NVARCHAR(255) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = NULL,
    @Page INT = 1,
    @PageSize INT = 25,
    @Sort NVARCHAR(400) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page IS NULL OR @Page < 1) SET @Page = 1;
    IF (@PageSize IS NULL OR @PageSize < 1) SET @PageSize = 25;
    IF (@PageSize > 1000) SET @PageSize = 1000;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    DECLARE @OrderBy NVARCHAR(MAX) = N'Nume ASC, Prenume ASC';

    -- Build ORDER BY from Sort parameter
    IF (@Sort IS NOT NULL AND LEN(@Sort) > 0)
    BEGIN
        IF (@Sort LIKE '%nume%desc%')
            SET @OrderBy = N'Nume DESC, Prenume DESC';
        ELSE IF (@Sort LIKE '%nume%asc%')
            SET @OrderBy = N'Nume ASC, Prenume ASC';
        ELSE IF (@Sort LIKE '%pozitie%desc%')
            SET @OrderBy = N'Pozitie DESC';
        ELSE IF (@Sort LIKE '%pozitie%asc%')
            SET @OrderBy = N'Pozitie ASC';
        ELSE IF (@Sort LIKE '%departament%desc%')
            SET @OrderBy = N'Departament DESC';
        ELSE IF (@Sort LIKE '%departament%asc%')
            SET @OrderBy = N'Departament ASC';
        ELSE IF (@Sort LIKE '%datacreare%desc%')
            SET @OrderBy = N'DataCreare DESC';
        ELSE IF (@Sort LIKE '%datacreare%asc%')
            SET @OrderBy = N'DataCreare ASC';
    END

    DECLARE @sql NVARCHAR(MAX) = N'
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
    FROM PersonalMedical
    WHERE 1=1
      AND (@Search IS NULL OR @Search = '''' OR 
           UPPER(Nume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(Prenume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(ISNULL(Specializare, '''')) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(ISNULL(NumarLicenta, '''')) LIKE ''%'' + UPPER(@Search) + ''%'')
      AND (@Departament IS NULL OR @Departament = '''' OR Departament = @Departament)
      AND (@Pozitie IS NULL OR @Pozitie = '''' OR Pozitie = @Pozitie)
      AND (@EsteActiv IS NULL OR EsteActiv = @EsteActiv)
    ORDER BY ' + @OrderBy + '
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(1) AS TotalCount
    FROM PersonalMedical
    WHERE 1=1
      AND (@Search IS NULL OR @Search = '''' OR 
           UPPER(Nume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(Prenume) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(ISNULL(Specializare, '''')) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(ISNULL(NumarLicenta, '''')) LIKE ''%'' + UPPER(@Search) + ''%'')
      AND (@Departament IS NULL OR @Departament = '''' OR Departament = @Departament)
      AND (@Pozitie IS NULL OR @Pozitie = '''' OR Pozitie = @Pozitie)
      AND (@EsteActiv IS NULL OR EsteActiv = @EsteActiv);';

    EXEC sp_executesql
        @sql,
        N'@Search NVARCHAR(255), @Departament NVARCHAR(100), @Pozitie NVARCHAR(50), @EsteActiv BIT, @Offset INT, @PageSize INT',
        @Search = @Search, @Departament = @Departament, @Pozitie = @Pozitie, @EsteActiv = @EsteActiv, @Offset = @Offset, @PageSize = @PageSize;
END
GO

PRINT '';
PRINT '===============================================';
PRINT 'PROCEDURI STOCATE SUPLIMENTARE PERSONAL MEDICAL COMPLETE!';
PRINT '===============================================';
PRINT '';
PRINT 'PROCEDURI CREATE/ACTUALIZATE:';
PRINT '==============================';
PRINT '• sp_PersonalMedical_GetById - Ob?inere personal dup? ID';
PRINT '• sp_PersonalMedical_Create - Creare personal nou cu valid?ri';
PRINT '• sp_PersonalMedical_Update - Actualizare personal cu valid?ri';
PRINT '• sp_PersonalMedical_Delete - Dezactivare personal (soft delete)';
PRINT '• sp_PersonalMedical_GetPaged - Actualizat cu suport pentru toate filtrele';
PRINT '';
PRINT 'VALID?RI INCLUSE:';
PRINT '==================';
PRINT '• Validare num?r licen?? duplicat';
PRINT '• Validare email duplicat';
PRINT '• Validare existen?? personal pentru update/delete';
PRINT '• Error handling specific ?i informativ';
PRINT '• Suport pentru filtrare dup? EsteActiv';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '===========';
PRINT '1. Ruleaz? acest script pentru a crea procedurile lips?';
PRINT '2. Testeaz? aplica?ia Blazor cu opera?iunile CRUD';
PRINT '3. Verific? c? toate filtrele func?ioneaz? corect';
PRINT '4. Implementeaz? valid?ri suplimentare dup? necesit??i';
PRINT '';