/*
    Stored Procedures for MaterialeSanitare table
    CRUD operations: Create, Read, Update, Delete, GetPaged
*/

-- GetAll procedure
IF OBJECT_ID('dbo.sp_MaterialeSanitare_GetAll','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_MaterialeSanitare_GetAll AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_MaterialeSanitare_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Guid,
        Denumire,
        Categorie,
        Specificatii,
        UnitateaMasura,
        Sterile,
        UniUzinta,
        DataCreare,
        DataModificare
    FROM dbo.MaterialeSanitare
    ORDER BY Denumire ASC;
END
GO

-- GetById procedure
IF OBJECT_ID('dbo.sp_MaterialeSanitare_GetById','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_MaterialeSanitare_GetById AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_MaterialeSanitare_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Guid,
        Denumire,
        Categorie,
        Specificatii,
        UnitateaMasura,
        Sterile,
        UniUzinta,
        DataCreare,
        DataModificare
    FROM dbo.MaterialeSanitare
    WHERE Id = @Id;
END
GO

-- Create procedure
IF OBJECT_ID('dbo.sp_MaterialeSanitare_Create','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_MaterialeSanitare_Create AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_MaterialeSanitare_Create
    @Denumire NVARCHAR(255),
    @Categorie NVARCHAR(100) = NULL,
    @Specificatii NVARCHAR(MAX) = NULL,
    @UnitateaMasura NVARCHAR(50) = NULL,
    @Sterile BIT = 0,
    @UniUzinta BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @NewId INT;
        
        INSERT INTO dbo.MaterialeSanitare (
            Denumire,
            Categorie,
            Specificatii,
            UnitateaMasura,
            Sterile,
            UniUzinta,
            DataCreare,
            DataModificare
        )
        VALUES (
            @Denumire,
            @Categorie,
            @Specificatii,
            @UnitateaMasura,
            @Sterile,
            @UniUzinta,
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
IF OBJECT_ID('dbo.sp_MaterialeSanitare_Update','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_MaterialeSanitare_Update AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_MaterialeSanitare_Update
    @Id INT,
    @Denumire NVARCHAR(255),
    @Categorie NVARCHAR(100) = NULL,
    @Specificatii NVARCHAR(MAX) = NULL,
    @UnitateaMasura NVARCHAR(50) = NULL,
    @Sterile BIT = 0,
    @UniUzinta BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.MaterialeSanitare WHERE Id = @Id)
        BEGIN
            RAISERROR('Materialul sanitar nu a fost gasit.', 16, 1);
            RETURN;
        END
        
        UPDATE dbo.MaterialeSanitare 
        SET 
            Denumire = @Denumire,
            Categorie = @Categorie,
            Specificatii = @Specificatii,
            UnitateaMasura = @UnitateaMasura,
            Sterile = @Sterile,
            UniUzinta = @UniUzinta,
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
IF OBJECT_ID('dbo.sp_MaterialeSanitare_Delete','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_MaterialeSanitare_Delete AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_MaterialeSanitare_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.MaterialeSanitare WHERE Id = @Id)
        BEGIN
            RAISERROR('Materialul sanitar nu a fost gasit.', 16, 1);
            RETURN;
        END
        
        DELETE FROM dbo.MaterialeSanitare WHERE Id = @Id;
        
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
IF OBJECT_ID('dbo.sp_MaterialeSanitare_GetPaged','P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_MaterialeSanitare_GetPaged AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE dbo.sp_MaterialeSanitare_GetPaged
    @Search NVARCHAR(255) = NULL,
    @Categorie NVARCHAR(100) = NULL,
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
        DECLARE @sortParts TABLE (col NVARCHAR(100), dir NVARCHAR(4));
        
        -- Simple sort parsing (you can enhance this)
        IF (@Sort LIKE '%desc%')
            SET @OrderBy = N'Denumire DESC';
        ELSE IF (@Sort LIKE '%asc%')
            SET @OrderBy = N'Denumire ASC';
    END

    DECLARE @sql NVARCHAR(MAX) = N'
    SELECT Id, Guid, Denumire, Categorie, Specificatii, UnitateaMasura, Sterile, UniUzinta, DataCreare, DataModificare
    FROM dbo.MaterialeSanitare
    WHERE (@Search IS NULL OR @Search = '''' OR 
           UPPER(Denumire) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(Categorie) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(Specificatii) LIKE ''%'' + UPPER(@Search) + ''%'')
      AND (@Categorie IS NULL OR @Categorie = '''' OR @Categorie = ''toate'' OR Categorie = @Categorie)
    ORDER BY ' + @OrderBy + '
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(1)
    FROM dbo.MaterialeSanitare
    WHERE (@Search IS NULL OR @Search = '''' OR 
           UPPER(Denumire) LIKE ''%'' + UPPER(@Search) + ''%'' OR 
           UPPER(Categorie) LIKE ''%'' + UPPER(@Search) + ''%'' OR
           UPPER(Specificatii) LIKE ''%'' + UPPER(@Search) + ''%'')
      AND (@Categorie IS NULL OR @Categorie = '''' OR @Categorie = ''toate'' OR Categorie = @Categorie);';

    EXEC sp_executesql
        @sql,
        N'@Search NVARCHAR(255), @Categorie NVARCHAR(100), @Offset INT, @PageSize INT',
        @Search = @Search, @Categorie = @Categorie, @Offset = @Offset, @PageSize = @PageSize;
END
GO

PRINT 'MaterialeSanitare stored procedures created successfully!';