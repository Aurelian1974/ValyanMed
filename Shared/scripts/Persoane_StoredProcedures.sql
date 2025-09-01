-- =============================================
-- Author: System
-- Create date: 2024
-- Description: Stored procedures pentru entitatea Persoana
-- Includes: CRUD operations + Paged search
-- =============================================

-- =============================================
-- Create Persoana
-- =============================================
IF OBJECT_ID('sp_CreatePersoana', 'P') IS NOT NULL
    DROP PROCEDURE sp_CreatePersoana;
GO

CREATE PROCEDURE sp_CreatePersoana
    @Guid UNIQUEIDENTIFIER,
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Judet NVARCHAR(50) = NULL,
    @Localitate NVARCHAR(100) = NULL,
    @Strada NVARCHAR(100) = NULL,
    @NumarStrada NVARCHAR(10) = NULL,
    @CodPostal NVARCHAR(10) = NULL,
    @PozitieOrganizatie NVARCHAR(100) = NULL,
    @DataNasterii DATE = NULL,
    @CNP NVARCHAR(13) = NULL,
    @TipActIdentitate NVARCHAR(20) = NULL,
    @SerieActIdentitate NVARCHAR(10) = NULL,
    @NumarActIdentitate NVARCHAR(20) = NULL,
    @StareCivila NVARCHAR(20) = NULL,
    @Gen NVARCHAR(20) = NULL,
    @Telefon NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @EsteActiva BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NewId INT;
    
    INSERT INTO Persoane (
        Guid, Nume, Prenume, Judet, Localitate, Strada, NumarStrada, 
        CodPostal, PozitieOrganizatie, DataNasterii, CNP, 
        TipActIdentitate, SerieActIdentitate, NumarActIdentitate, 
        StareCivila, Gen, Telefon, Email, EsteActiva, DataCreare
    )
    VALUES (
        @Guid, @Nume, @Prenume, @Judet, @Localitate, @Strada, @NumarStrada,
        @CodPostal, @PozitieOrganizatie, @DataNasterii, @CNP,
        @TipActIdentitate, @SerieActIdentitate, @NumarActIdentitate,
        @StareCivila, @Gen, @Telefon, @Email, @EsteActiva, GETDATE()
    );
    
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId as Id;
END
GO

-- =============================================
-- Get Persoana By ID
-- =============================================
IF OBJECT_ID('sp_GetPersoanaById', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetPersoanaById;
GO

CREATE PROCEDURE sp_GetPersoanaById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Guid, Nume, Prenume, Judet, Localitate, Strada, NumarStrada,
        CodPostal, PozitieOrganizatie, DataNasterii, CNP,
        TipActIdentitate, SerieActIdentitate, NumarActIdentitate,
        StareCivila, Gen, Telefon, Email, EsteActiva,
        DataCreare, DataModificare
    FROM Persoane 
    WHERE Id = @Id;
END
GO

-- =============================================
-- Update Persoana
-- =============================================
IF OBJECT_ID('sp_UpdatePersoana', 'P') IS NOT NULL
    DROP PROCEDURE sp_UpdatePersoana;
GO

CREATE PROCEDURE sp_UpdatePersoana
    @Id INT,
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Judet NVARCHAR(50) = NULL,
    @Localitate NVARCHAR(100) = NULL,
    @Strada NVARCHAR(100) = NULL,
    @NumarStrada NVARCHAR(10) = NULL,
    @CodPostal NVARCHAR(10) = NULL,
    @PozitieOrganizatie NVARCHAR(100) = NULL,
    @DataNasterii DATE = NULL,
    @CNP NVARCHAR(13) = NULL,
    @TipActIdentitate NVARCHAR(20) = NULL,
    @SerieActIdentitate NVARCHAR(10) = NULL,
    @NumarActIdentitate NVARCHAR(20) = NULL,
    @StareCivila NVARCHAR(20) = NULL,
    @Gen NVARCHAR(20) = NULL,
    @Telefon NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @EsteActiva BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Persoane 
    SET 
        Nume = @Nume,
        Prenume = @Prenume,
        Judet = @Judet,
        Localitate = @Localitate,
        Strada = @Strada,
        NumarStrada = @NumarStrada,
        CodPostal = @CodPostal,
        PozitieOrganizatie = @PozitieOrganizatie,
        DataNasterii = @DataNasterii,
        CNP = @CNP,
        TipActIdentitate = @TipActIdentitate,
        SerieActIdentitate = @SerieActIdentitate,
        NumarActIdentitate = @NumarActIdentitate,
        StareCivila = @StareCivila,
        Gen = @Gen,
        Telefon = @Telefon,
        Email = @Email,
        EsteActiva = @EsteActiva,
        DataModificare = GETDATE()
    WHERE Id = @Id;
END
GO

-- =============================================
-- Delete Persoana
-- =============================================
IF OBJECT_ID('sp_DeletePersoana', 'P') IS NOT NULL
    DROP PROCEDURE sp_DeletePersoana;
GO

CREATE PROCEDURE sp_DeletePersoana
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM Persoane 
    WHERE Id = @Id;
END
GO

-- =============================================
-- Get All Persoane
-- =============================================
IF OBJECT_ID('sp_GetAllPersoane', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetAllPersoane;
GO

CREATE PROCEDURE sp_GetAllPersoane
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Guid, Nume, Prenume, Judet, Localitate, Strada, NumarStrada,
        CodPostal, PozitieOrganizatie, DataNasterii, CNP,
        TipActIdentitate, SerieActIdentitate, NumarActIdentitate,
        StareCivila, Gen, Telefon, Email, EsteActiva,
        DataCreare, DataModificare
    FROM Persoane 
    ORDER BY Nume, Prenume;
END
GO

-- =============================================
-- Get Persoane Paged With Search - MAIN PROCEDURE
-- =============================================
IF OBJECT_ID('sp_GetPersoanePagedWithSearch', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetPersoanePagedWithSearch;
GO

CREATE PROCEDURE sp_GetPersoanePagedWithSearch
    @Search NVARCHAR(255) = NULL,
    @Judet NVARCHAR(50) = NULL,
    @Localitate NVARCHAR(100) = NULL,
    @EsteActiva BIT = NULL,
    @Page INT = 1,
    @PageSize INT = 10,
    @Sort NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate parameters
    IF @Page < 1 SET @Page = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    IF @PageSize > 100 SET @PageSize = 100; -- Max page size
    
    -- Calculate offset
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    
    -- Build WHERE clause conditions
    DECLARE @WhereClause NVARCHAR(MAX) = N'WHERE 1=1';
    
    -- Search in multiple fields
    IF @Search IS NOT NULL AND LTRIM(RTRIM(@Search)) != ''
    BEGIN
        SET @WhereClause = @WhereClause + N' AND (
            Nume LIKE N''%' + REPLACE(@Search, '''', '''''') + N'%'' OR
            Prenume LIKE N''%' + REPLACE(@Search, '''', '''''') + N'%'' OR
            CONCAT(Nume, '' '', Prenume) LIKE N''%' + REPLACE(@Search, '''', '''''') + N'%'' OR
            ISNULL(Email, '''') LIKE N''%' + REPLACE(@Search, '''', '''''') + N'%'' OR
            ISNULL(Telefon, '''') LIKE N''%' + REPLACE(@Search, '''', '''''') + N'%'' OR
            ISNULL(CNP, '''') LIKE N''%' + REPLACE(@Search, '''', '''''') + N'%''
        )';
    END
    
    -- Filter by Judet
    IF @Judet IS NOT NULL AND LTRIM(RTRIM(@Judet)) != ''
    BEGIN
        SET @WhereClause = @WhereClause + N' AND Judet = N''' + REPLACE(@Judet, '''', '''''') + N'''';
    END
    
    -- Filter by Localitate
    IF @Localitate IS NOT NULL AND LTRIM(RTRIM(@Localitate)) != ''
    BEGIN
        SET @WhereClause = @WhereClause + N' AND Localitate = N''' + REPLACE(@Localitate, '''', '''''') + N'''';
    END
    
    -- Filter by Status
    IF @EsteActiva IS NOT NULL
    BEGIN
        SET @WhereClause = @WhereClause + N' AND EsteActiva = ' + CAST(@EsteActiva AS NVARCHAR(1));
    END
    
    -- Build ORDER BY clause
    DECLARE @OrderByClause NVARCHAR(200) = N'ORDER BY ';
    
    IF @Sort IS NOT NULL AND LTRIM(RTRIM(@Sort)) != ''
    BEGIN
        -- Parse sort parameter (e.g., "nume asc" or "nume desc")
        DECLARE @SortField NVARCHAR(50) = LTRIM(RTRIM(@Sort));
        DECLARE @SortDirection NVARCHAR(10) = 'ASC';
        
        -- Extract direction if provided
        IF @SortField LIKE '% desc' OR @SortField LIKE '% DESC'
        BEGIN
            SET @SortDirection = 'DESC';
            SET @SortField = LTRIM(RTRIM(REPLACE(REPLACE(@SortField, ' desc', ''), ' DESC', '')));
        END
        ELSE IF @SortField LIKE '% asc' OR @SortField LIKE '% ASC'
        BEGIN
            SET @SortDirection = 'ASC';
            SET @SortField = LTRIM(RTRIM(REPLACE(REPLACE(@SortField, ' asc', ''), ' ASC', '')));
        END
        
        -- Map sort fields to actual column names
        SET @SortField = CASE LOWER(@SortField)
            WHEN 'numecomplet' THEN 'Nume, Prenume'
            WHEN 'nume' THEN 'Nume'
            WHEN 'prenume' THEN 'Prenume'
            WHEN 'judet' THEN 'Judet'
            WHEN 'localitate' THEN 'Localitate'
            WHEN 'email' THEN 'Email'
            WHEN 'telefon' THEN 'Telefon'
            WHEN 'datacreare' THEN 'DataCreare'
            WHEN 'datacrearii' THEN 'DataCreare'
            WHEN 'varsta' THEN 'DataNasterii'
            WHEN 'datanasterii' THEN 'DataNasterii'
            WHEN 'esteactiva' THEN 'EsteActiva'
            WHEN 'gen' THEN 'Gen'
            ELSE 'Nume, Prenume'
        END;
        
        SET @OrderByClause = @OrderByClause + @SortField + N' ' + @SortDirection;
    END
    ELSE
    BEGIN
        SET @OrderByClause = @OrderByClause + N'Nume, Prenume ASC';
    END
    
    -- Build final query for data
    DECLARE @DataQuery NVARCHAR(MAX) = N'
        SELECT 
            Id,
            Guid,
            Nume,
            Prenume,
            CONCAT(Nume, '' '', Prenume) as NumeComplet,
            CNP,
            DataNasterii,
            CASE 
                WHEN DataNasterii IS NOT NULL 
                THEN DATEDIFF(YEAR, DataNasterii, GETDATE()) - 
                     CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, DataNasterii, GETDATE()), DataNasterii) > GETDATE() 
                          THEN 1 ELSE 0 END
                ELSE 0 
            END as Varsta,
            Gen,
            Telefon,
            Email,
            Judet,
            Localitate,
            CASE 
                WHEN Strada IS NOT NULL OR NumarStrada IS NOT NULL OR Localitate IS NOT NULL OR Judet IS NOT NULL
                THEN CONCAT(
                    ISNULL(Strada + '' '', ''''),
                    CASE WHEN NumarStrada IS NOT NULL THEN ''nr. '' + NumarStrada + '' '' ELSE '''' END,
                    ISNULL(Localitate + '' '', ''''),
                    CASE WHEN Judet IS NOT NULL THEN ''jud. '' + Judet ELSE '''' END
                )
                ELSE NULL 
            END as Adresa,
            EsteActiva,
            DataCreare
        FROM Persoane ' + @WhereClause + N'
        ' + @OrderByClause + N'
        OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + N' ROWS
        FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + N' ROWS ONLY';
    
    -- Build count query
    DECLARE @CountQuery NVARCHAR(MAX) = N'
        SELECT COUNT(*) 
        FROM Persoane ' + @WhereClause;
    
    -- Execute data query
    EXEC sp_executesql @DataQuery;
    
    -- Execute count query
    EXEC sp_executesql @CountQuery;
END
GO

-- =============================================
-- Check if CNP exists (for validation)
-- =============================================
IF OBJECT_ID('sp_CheckCNPExists', 'P') IS NOT NULL
    DROP PROCEDURE sp_CheckCNPExists;
GO

CREATE PROCEDURE sp_CheckCNPExists
    @CNP NVARCHAR(13),
    @ExcludeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Count INT = 0;
    
    -- Only check if CNP is not NULL or empty
    IF @CNP IS NOT NULL AND LTRIM(RTRIM(@CNP)) != ''
    BEGIN
        SELECT @Count = COUNT(*)
        FROM Persoane 
        WHERE CNP = @CNP 
          AND (@ExcludeId IS NULL OR Id != @ExcludeId);
    END
    
    SELECT CASE WHEN @Count > 0 THEN 1 ELSE 0 END as [ExistsCNP];
END
GO

PRINT 'Stored procedures pentru Persoane create cu succes!';