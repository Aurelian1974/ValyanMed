/*
    Stored Procedures for DispozitiveMedicale table
    CRUD operations: Create, Read, Update, Delete, GetPaged
*/

-- GetAll procedure
IF OBJECT_ID('dbo.sp_DispozitiveMedicale_GetAll','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_DispozitiveMedicale_GetAll AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_DispozitiveMedicale_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Guid,
        Denumire,
        Categorie,
        ClasaRisc,
        Producator,
        ModelTip,
        NumarSerie,
        CertificareCE,
        DataExpirare,
        Specificatii,
        DataCreare,
        DataModificare
    FROM dbo.DispozitiveMedicale
    ORDER BY Denumire ASC;
END
GO

-- GetById procedure
IF OBJECT_ID('dbo.sp_DispozitiveMedicale_GetById','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_DispozitiveMedicale_GetById AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_DispozitiveMedicale_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Guid,
        Denumire,
        Categorie,
        ClasaRisc,
        Producator,
        ModelTip,
        NumarSerie,
        CertificareCE,
        DataExpirare,
        Specificatii,
        DataCreare,
        DataModificare
    FROM dbo.DispozitiveMedicale
    WHERE Id = @Id;
END
GO

-- Create procedure
IF OBJECT_ID('dbo.sp_DispozitiveMedicale_Create','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_DispozitiveMedicale_Create AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_DispozitiveMedicale_Create
    @Denumire NVARCHAR(255),
    @Categorie NVARCHAR(100) = NULL,
    @ClasaRisc NVARCHAR(10) = NULL,
    @Producator NVARCHAR(255) = NULL,
    @ModelTip NVARCHAR(100) = NULL,
    @NumarSerie NVARCHAR(100) = NULL,
    @CertificareCE BIT = 0,
    @DataExpirare DATE = NULL,
    @Specificatii NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @NewId INT;
        
        INSERT INTO dbo.DispozitiveMedicale (
            Denumire,
            Categorie,
            ClasaRisc,
            Producator,
            ModelTip,
            NumarSerie,
            CertificareCE,
            DataExpirare,
            Specificatii,
            DataCreare,
            DataModificare
        )
        VALUES (
            @Denumire,
            @Categorie,
            @ClasaRisc,
            @Producator,
            @ModelTip,
            @NumarSerie,
            @CertificareCE,
            @DataExpirare,
            @Specificatii,
            GETDATE(),
            GETDATE()
        );
        
        SET @NewId = SCOPE_IDENTITY();
        SELECT @NewId AS Id;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- Update procedure
IF OBJECT_ID('dbo.sp_DispozitiveMedicale_Update','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_DispozitiveMedicale_Update AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_DispozitiveMedicale_Update
    @Id INT,
    @Denumire NVARCHAR(255),
    @Categorie NVARCHAR(100) = NULL,
    @ClasaRisc NVARCHAR(10) = NULL,
    @Producator NVARCHAR(255) = NULL,
    @ModelTip NVARCHAR(100) = NULL,
    @NumarSerie NVARCHAR(100) = NULL,
    @CertificareCE BIT = 0,
    @DataExpirare DATE = NULL,
    @Specificatii NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.DispozitiveMedicale WHERE Id = @Id)
        BEGIN
            RAISERROR('Dispozitivul medical nu a fost gasit.', 16, 1);
            RETURN;
        END
        
        UPDATE dbo.DispozitiveMedicale 
        SET 
            Denumire = @Denumire,
            Categorie = @Categorie,
            ClasaRisc = @ClasaRisc,
            Producator = @Producator,
            ModelTip = @ModelTip,
            NumarSerie = @NumarSerie,
            CertificareCE = @CertificareCE,
            DataExpirare = @DataExpirare,
            Specificatii = @Specificatii,
            DataModificare = GETDATE()
        WHERE Id = @Id;
        
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

-- Delete procedure
IF OBJECT_ID('dbo.sp_DispozitiveMedicale_Delete','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_DispozitiveMedicale_Delete AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_DispozitiveMedicale_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.DispozitiveMedicale WHERE Id = @Id)
        BEGIN
            RAISERROR('Dispozitivul medical nu a fost gasit.', 16, 1);
            RETURN;
        END
        
        DELETE FROM dbo.DispozitiveMedicale WHERE Id = @Id;
        
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

-- GetPaged procedure
IF OBJECT_ID('dbo.sp_DispozitiveMedicale_GetPaged','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_DispozitiveMedicale_GetPaged AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_DispozitiveMedicale_GetPaged
    @Search NVARCHAR(255) = NULL,
    @Categorie NVARCHAR(100) = NULL,
    @ClasaRisc NVARCHAR(10) = NULL,
    @Page INT = 1,
    @PageSize INT = 25,
    @Sort NVARCHAR(400) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page IS NULL OR @Page < 1) SET @Page = 1;
    IF (@PageSize IS NULL OR @PageSize < 1) SET @PageSize = 25;
    IF (@PageSize > 5000) SET @PageSize = 5000;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    DECLARE @OrderBy NVARCHAR(MAX) = N'Denumire ASC';

    -- Build ORDER BY from Sort parameter
    IF (@Sort IS NOT NULL AND LEN(@Sort) > 0)
    BEGIN
        IF (@Sort LIKE '%desc%')
            SET @OrderBy = N'Denumire DESC';
        ELSE IF (@Sort LIKE '%asc%')
            SET @OrderBy = N'Denumire ASC';
    END

    DECLARE @sql NVARCHAR(MAX) = N'
    SELECT Id, Guid, Denumire, Categorie, ClasaRisc, Producator, ModelTip, NumarSerie, CertificareCE, DataExpirare, Specificatii, DataCreare, DataModificare
    FROM dbo.DispozitiveMedicale
    WHERE (@Search IS NULL OR @Search = '''' OR 
           UPPER(Denumire) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(Categorie) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(Producator) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(ModelTip) LIKE ''%'' + UPPER(@Search) + ''%'')
      AND (@Categorie IS NULL OR @Categorie = '''' OR @Categorie = ''toate'' OR Categorie = @Categorie)
      AND (@ClasaRisc IS NULL OR @ClasaRisc = '''' OR @ClasaRisc = ''toate'' OR ClasaRisc = @ClasaRisc)
    ORDER BY ' + @OrderBy + '
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(1)
    FROM dbo.DispozitiveMedicale
    WHERE (@Search IS NULL OR @Search = '''' OR 
           UPPER(Denumire) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(Categorie) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(Producator) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(ModelTip) LIKE ''%'' + UPPER(@Search) + ''%'')
      AND (@Categorie IS NULL OR @Categorie = '''' OR @Categorie = ''toate'' OR Categorie = @Categorie)
      AND (@ClasaRisc IS NULL OR @ClasaRisc = '''' OR @ClasaRisc = ''toate'' OR ClasaRisc = @ClasaRisc);';

    EXEC sp_executesql
        @sql,
        N'@Search NVARCHAR(255), @Categorie NVARCHAR(100), @ClasaRisc NVARCHAR(10), @Offset INT, @PageSize INT',
        @Search = @Search, @Categorie = @Categorie, @ClasaRisc = @ClasaRisc, @Offset = @Offset, @PageSize = @PageSize;
END
GO

PRINT 'DispozitiveMedicale stored procedures created successfully!';